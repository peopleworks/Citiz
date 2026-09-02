#!/usr/bin/env bash
# Stores your ElevenLabs API key on this machine for the audio tools, without ever showing it:
# the key is typed hidden and written to tools/audio/.env (git-ignored, readable only by you).
# Run it once per machine; run it again to replace the key. Use a key created for this machine,
# so it can be revoked on its own at https://elevenlabs.io/app/settings/api-keys.
set -euo pipefail

env_file="$(cd "$(dirname "$0")" && pwd)/.env"
printf 'ElevenLabs API key for this Mac (typing is hidden): '
IFS= read -rs key
echo
key="${key//[[:space:]]/}"
if [[ -z "$key" ]]; then
  echo "Nothing entered; nothing changed." >&2
  exit 1
fi

umask 077
printf 'ELEVENLABS_API_KEY=%s\n' "$key" > "$env_file"
chmod 600 "$env_file"
echo "Saved to $env_file (permissions 600, listed in .gitignore)."
echo "Check it with: .venv/bin/python tools/audio/generate_elevenlabs.py --list-voices"
