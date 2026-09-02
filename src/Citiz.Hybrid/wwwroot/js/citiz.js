// The whole JavaScript side of Citiz. Everything here stays on the device; there is no analytics,
// no beacon and no network call. See Docs/Privacy/LOCAL_VS_CLOUD.md.
(function () {
  'use strict';

  const safe = (fn, fallback) => {
    try { return fn(); } catch { return fallback; }
  };

  const storage = {
    get: key => safe(() => window.localStorage.getItem(key), null),
    set: (key, value) => safe(() => window.localStorage.setItem(key, value), undefined),
    remove: key => safe(() => window.localStorage.removeItem(key), undefined)
  };

  const speech = {
    available: () => typeof window.speechSynthesis !== 'undefined' && typeof window.SpeechSynthesisUtterance !== 'undefined',

    // Prefer a voice that runs on the device: privacy first, and it works offline.
    pick(lang) {
      if (!speech.available()) return null;
      const voices = window.speechSynthesis.getVoices();
      const primary = (lang || 'en').toLowerCase().split('-')[0];
      const matching = voices.filter(v => v.lang && v.lang.toLowerCase().startsWith(primary));
      return matching.find(v => v.localService && v.lang.toLowerCase() === (lang || '').toLowerCase())
        || matching.find(v => v.localService)
        || matching[0]
        || null;
    },

    isLocal: lang => {
      const voice = speech.pick(lang);
      return voice ? !!voice.localService : false;
    },

    speak(text, lang, rate) {
      if (!speech.available() || !text) return;
      window.speechSynthesis.cancel();
      const utterance = new SpeechSynthesisUtterance(text);
      utterance.lang = lang || 'en-US';
      utterance.rate = rate || 0.9;
      const voice = speech.pick(utterance.lang);
      if (voice) utterance.voice = voice;
      window.speechSynthesis.speak(utterance);
    },

    stop: () => { if (speech.available()) window.speechSynthesis.cancel(); }
  };

  // Audio packs: recordings downloaded once and kept in the browser's Cache Storage under one cache
  // per pack version, so playing a clip later is a local read and works offline. The only network
  // traffic is the download itself, started by the learner. See Docs/Privacy/LOCAL_VS_CLOUD.md.
  const audio = {
    cacheName: key => `citiz-audio-${key}`,
    supported: () => typeof window.caches !== 'undefined' && typeof window.Audio !== 'undefined',
    player: null,

    // "ready" when every file of the pack is cached, "none" otherwise.
    async state(key, files) {
      if (!audio.supported()) return 'none';
      try {
        const cache = await window.caches.open(audio.cacheName(key));
        for (const file of files) {
          if (!(await cache.match(file))) return 'none';
        }
        return 'ready';
      } catch {
        return 'none';
      }
    },

    // Fetches every file and stores it; reports progress in bytes through the .NET callback.
    // A failed file aborts the whole download and leaves the cache incomplete (state stays "none").
    async download(key, baseUrl, files, sizes, callback) {
      if (!audio.supported()) return false;
      const cache = await window.caches.open(audio.cacheName(key));
      let done = 0;
      audio.cancelled = false;
      for (let i = 0; i < files.length; i++) {
        if (audio.cancelled) return false;
        const file = files[i];
        if (!(await cache.match(file))) {
          const response = await fetch(baseUrl + file, { mode: 'cors', cache: 'no-store' });
          if (!response.ok) throw new Error(`HTTP ${response.status} for ${file}`);
          await cache.put(file, response);
        }
        done += sizes[i];
        if (callback) await callback.invokeMethodAsync('Report', done);
      }
      return true;
    },

    cancel() { audio.cancelled = true; },

    async remove(key) {
      if (!audio.supported()) return;
      await window.caches.delete(audio.cacheName(key));
    },

    // Plays one cached clip; returns false when it is not cached, so the caller can fall back.
    async play(key, file) {
      if (!audio.supported()) return false;
      const cache = await window.caches.open(audio.cacheName(key));
      const response = await cache.match(file);
      if (!response) return false;
      const blob = await response.blob();
      return audio.playBlob(blob);
    },

    // Plays audio handed over as base64 (the native hosts keep packs as files, not in Cache Storage).
    playBase64(base64, mediaType) {
      const bytes = Uint8Array.from(atob(base64), c => c.charCodeAt(0));
      return audio.playBlob(new Blob([bytes], { type: mediaType || 'audio/mpeg' }));
    },

    playBlob(blob) {
      audio.stop();
      speech.stop();
      const url = URL.createObjectURL(blob);
      const player = new Audio(url);
      audio.player = player;
      player.addEventListener('ended', () => audio.release(player, url));
      player.addEventListener('error', () => audio.release(player, url));
      const started = player.play();
      if (started && started.catch) started.catch(() => audio.release(player, url));
      return true;
    },

    stop() {
      const player = audio.player;
      if (player) {
        player.pause();
        audio.release(player, player.src);
      }
    },

    release(player, url) {
      if (audio.player === player) audio.player = null;
      try { URL.revokeObjectURL(url); } catch { /* already released */ }
    }
  };

  window.citiz = {
    storage,
    speech,
    audio,
    browserLanguage: () => navigator.language || 'en',

    setDocumentLanguage(lang, dir) {
      document.documentElement.lang = lang || 'en';
      document.documentElement.dir = dir || 'ltr';
    },

    // theme is "light", "dark", or anything else (including absent) to follow the system.
    // The same attribute is set synchronously by the inline script in index.html on first
    // paint, before Blazor boots, so there is no flash of the wrong theme; this just keeps it
    // in sync once the learner changes it in Settings.
    setTheme(theme) {
      if (theme === 'light' || theme === 'dark') {
        document.documentElement.setAttribute('data-theme', theme);
      } else {
        document.documentElement.removeAttribute('data-theme');
      }
    },

    announce(text) {
      const region = document.getElementById('citiz-live');
      if (!region) return;
      region.textContent = '';
      window.setTimeout(() => { region.textContent = text; }, 50);
    },

    download(fileName, text, mediaType) {
      const blob = new Blob([text], { type: mediaType || 'application/json' });
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = fileName;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.setTimeout(() => URL.revokeObjectURL(url), 1000);
    }
  };

  // Some browsers load voices asynchronously; touching the list early makes the first pick reliable.
  if (speech.available()) {
    window.speechSynthesis.getVoices();
    window.speechSynthesis.onvoiceschanged = () => window.speechSynthesis.getVoices();
  }
})();
