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

  window.citiz = {
    storage,
    speech,
    browserLanguage: () => navigator.language || 'en',

    setDocumentLanguage(lang, dir) {
      document.documentElement.lang = lang || 'en';
      document.documentElement.dir = dir || 'ltr';
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
