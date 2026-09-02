# Audio packs

The recordings Citiz can play come as *packs*: a folder of MP3 files plus a manifest, hosted on
a plain HTTPS server, listed in [`content/audio/packs.json`](../../content/audio/packs.json). The
app downloads a whole pack once, on the learner's request, and keeps it on the device; playing a
clip afterwards is a local read. The host sees one download, never what is studied.

Two kinds of pack, labelled differently everywhere they play:

| Pack | Kind | Contents | Built by |
| --- | --- | --- | --- |
| `uscis-2008` | official | USCIS's own MP3 track per question (question **and** answers, read by a person); public domain | `fetch_uscis_2008.py` |
| `citiz-voice-2025` | synthetic | prompt and every accepted answer of the 2025 test, one clip each | `generate_elevenlabs.py --set 2025` |
| `citiz-voice-2008` | synthetic | prompt of every 2008 question (the answers are in the official track) | `generate_elevenlabs.py --set 2008` |
| `citiz-voice-words` | synthetic | the reading and writing vocabulary | `generate_elevenlabs.py --set words` |

Synthetic packs are generated **once**, by the maintainer, from the verified text; the ElevenLabs
key never ships with the app and no learner ever calls ElevenLabs. USCIS has published no audio
for the 2025 test; when it does, an official pack replaces the synthetic voice for it.

## One-time setup (macOS)

```bash
python3 -m venv .venv && .venv/bin/pip install -r tools/audio/requirements.txt
```

The ElevenLabs key goes in an environment variable, exactly as on Windows, or in a git-ignored
file — pick one:

- **Environment variable, for this terminal only:** `export ELEVENLABS_API_KEY="sk_..."`
  (Windows' `set X=Y` is `export X=Y` in the Mac's zsh; the quotes keep special characters safe).
- **Environment variable, permanently:** add that same `export` line to the end of `~/.zshrc`
  (`nano ~/.zshrc`, paste, Ctrl+O, Enter, Ctrl+X), then open a new terminal.
- **File:** create `tools/audio/.env` containing one line, `ELEVENLABS_API_KEY=sk_...`. The file is
  listed in `.gitignore`, so it cannot be committed by accident.

Check it: `.venv/bin/python tools/audio/generate_elevenlabs.py --list-voices` prints your voices.

## Building the packs

```bash
# 1. The official 2008 recordings (30.5 MB, 100 tracks), straight from uscis.gov
.venv/bin/python tools/audio/fetch_uscis_2008.py --base-url https://YOUR-HOST/citiz-audio/uscis-2008/v1/

# 2. Pick a voice: samples of the same question with each candidate, to choose by ear
.venv/bin/python tools/audio/generate_elevenlabs.py --sample --voice <id-1> --voice <id-2>

# 3. How many characters a set costs (one ElevenLabs credit per character); calls nothing
.venv/bin/python tools/audio/generate_elevenlabs.py --set 2025 --dry-run

# 4. Generate (resumable; already generated clips are skipped)
.venv/bin/python tools/audio/generate_elevenlabs.py --set 2025 --voice <id> --base-url https://YOUR-HOST/citiz-audio/
.venv/bin/python tools/audio/generate_elevenlabs.py --set words --voice <id> --base-url https://YOUR-HOST/citiz-audio/
```

Every command writes the pack into `content/audio/packs.json` (commit that) and the files into
`tools/audio/dist/<pack-id>/v<version>/` (upload that; never committed). Synthetic packs land as
`needs-review`: listen to a few clips, then set `approved` in `packs.json`, like any content.

Bump `--version` when files change; the app keys its cache on pack id and version, so learners
re-download only what changed.

## Hosting (any HTTPS static host; the Windows Server + IIS recipe)

Upload `tools/audio/dist/` so that `https://YOUR-HOST/citiz-audio/uscis-2008/v1/q001.mp3` resolves.
The web app fetches across origins, so the folder must send CORS headers; the files are public
domain or licensed for exactly this, so a wildcard is fine. Put this `web.config` in the
`citiz-audio` folder:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <staticContent>
      <remove fileExtension=".mp3" />
      <mimeMap fileExtension=".mp3" mimeType="audio/mpeg" />
      <remove fileExtension=".json" />
      <mimeMap fileExtension=".json" mimeType="application/json" />
      <clientCache cacheControlMode="UseMaxAge" cacheControlMaxAge="365.00:00:00" />
    </staticContent>
    <httpProtocol>
      <customHeaders>
        <add name="Access-Control-Allow-Origin" value="*" />
        <add name="Access-Control-Allow-Methods" value="GET, HEAD, OPTIONS" />
      </customHeaders>
    </httpProtocol>
  </system.webServer>
</configuration>
```

A long cache lifetime is right because a changed file gets a new pack version, hence a new URL.
Check from a browser console on the live site: `fetch('https://YOUR-HOST/citiz-audio/uscis-2008/v1/manifest.json').then(r => r.status)` must print `200`.
