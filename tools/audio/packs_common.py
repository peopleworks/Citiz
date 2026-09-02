"""Shared helpers for the audio tools: hashing, durations, and updating content/audio/packs.json."""
from __future__ import annotations

import hashlib
import json
import re
from pathlib import Path

from mutagen.mp3 import MP3

ROOT = Path(__file__).resolve().parents[2]
PACKS_PATH = ROOT / "content" / "audio" / "packs.json"
DIST = Path(__file__).resolve().parent / "dist"
PUBLIC_DOMAIN = "Public domain (U.S. Government work, 17 U.S.C. § 105)"


def slug(text: str) -> str:
    """A lower-case, hyphenated id fragment: 'Father of Our Country' -> 'father-of-our-country'."""
    text = text.lower().replace("&", " and ")
    text = re.sub(r"[^a-z0-9]+", "-", text).strip("-")
    return text or "x"


def describe(path: Path) -> dict:
    """Size, duration and digest of one MP3, as the manifest records them."""
    data = path.read_bytes()
    return {
        "bytes": len(data),
        "seconds": round(MP3(path).info.length, 1),
        "sha256": hashlib.sha256(data).hexdigest(),
    }


def load_packs() -> dict:
    if PACKS_PATH.exists():
        return json.loads(PACKS_PATH.read_text(encoding="utf-8"))
    return {"$schema": "../schemas/audio-packs.schema.json", "packs": []}


def save_pack(pack: dict) -> None:
    """Inserts or replaces the pack with the same id and writes the catalog and the pack's own manifest copy."""
    catalog = load_packs()
    catalog["packs"] = [p for p in catalog["packs"] if p["id"] != pack["id"]] + [pack]
    catalog["packs"].sort(key=lambda p: p["id"])
    PACKS_PATH.parent.mkdir(parents=True, exist_ok=True)
    PACKS_PATH.write_text(json.dumps(catalog, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    folder = DIST / pack["id"] / f"v{pack['version']}"
    folder.mkdir(parents=True, exist_ok=True)
    (folder / "manifest.json").write_text(json.dumps(pack, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")


def load_content(relative: str) -> dict:
    return json.loads((ROOT / "content" / relative).read_text(encoding="utf-8"))
