#!/usr/bin/env bash
# Stores the ElevenLabs API key for this Mac in tools/audio/.env (git-ignored, permissions 600).
# The key is read hidden, checked with ElevenLabs, and never printed.
set -euo pipefail

env_file="$(cd "$(dirname "$0")" && pwd)/.env"

echo "Paste your ElevenLabs API key once with Cmd+V (Cmd, not Ctrl), then press Enter."
echo "Nothing appears while you paste; that is normal."
printf 'Key: '
IFS= read -rs key || true
echo

# Only visible characters: a Ctrl+V or a space would otherwise become part of the key.
key="$(printf '%s' "$key" | LC_ALL=C tr -cd '[:graph:]')"
# Pasted twice because nothing showed up the first time: keep one copy.
half=$(( ${#key} / 2 ))
if (( half > 0 && ${#key} % 2 == 0 )) && [[ "${key:0:half}" == "${key:half}" ]]; then
  key="${key:0:half}"
  echo "It was pasted twice; keeping one copy."
fi
if [[ -z "$key" ]]; then
  echo "Nothing entered; nothing changed." >&2
  exit 1
fi
if [[ "${key:1}" == *sk_* ]]; then
  echo "That looks like more than one key. Nothing changed; run this again and paste once." >&2
  exit 1
fi

# Ask ElevenLabs whether it knows the key. curl reads the header from stdin, so the key never
# appears on a command line.
body="$(mktemp)"
trap 'rm -f "$body"' EXIT
status="$(printf 'xi-api-key: %s\n' "$key" | curl -s -o "$body" -w '%{http_code}' -H @- https://api.elevenlabs.io/v1/voices || true)"
case "$status" in
  200) verdict="ElevenLabs accepts the key, and it can list voices." ;;
  000) verdict="Could not reach ElevenLabs to check the key. Check it later with: .venv/bin/python tools/audio/generate_elevenlabs.py --list-voices" ;;
  *)
    if grep -q missing_permissions "$body"; then
      verdict="ElevenLabs knows the key, but it lacks a permission: at elevenlabs.io → Developers → API Keys, edit the key and allow Text to Speech and Voices (read)."
    else
      echo "ElevenLabs refused that key (HTTP $status). Nothing changed; copy it again from elevenlabs.io → Developers → API Keys and paste once." >&2
      exit 1
    fi
    ;;
esac

umask 077
printf 'ELEVENLABS_API_KEY=%s\n' "$key" > "$env_file"
chmod 600 "$env_file"
echo "Saved to $env_file (permissions 600, listed in .gitignore)."
echo "$verdict"
