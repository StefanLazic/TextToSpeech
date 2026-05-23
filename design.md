# Research: Implementing a Text-to-Speech Website with HTML & JavaScript (Free Options)

Below is a comparison of the realistic free options for building a browser-based Text-to-Speech (TTS) site, with documentation links supporting every claim, and a recommended approach.

---

## Option 1: Web Speech API — `SpeechSynthesis` (Recommended)

This is a **built-in browser API**. No account, no API key, no server, no network calls, no cost — ever. It uses the voices already installed on the user's operating system (or shipped with the browser).

### Why it fits the requirements
- **Free**: It is a W3C/WHATWG-aligned standard built into browsers. There is no service to pay for. Source: MDN — *Web Speech API* overview: https://developer.mozilla.org/en-US/docs/Web/API/Web_Speech_API
- **Pure HTML + JavaScript**: The entry point is `window.speechSynthesis`, a global object available in the browser. Source: MDN — *Window.speechSynthesis*: https://developer.mozilla.org/en-US/docs/Web/API/Window/speechSynthesis
- **Takes text as input, produces audio**: You create a `SpeechSynthesisUtterance` from a string and pass it to `speechSynthesis.speak()`. Source: MDN — *SpeechSynthesisUtterance*: https://developer.mozilla.org/en-US/docs/Web/API/SpeechSynthesisUtterance and *SpeechSynthesis.speak()*: https://developer.mozilla.org/en-US/docs/Web/API/SpeechSynthesis/speak

### Capabilities to expose in the UI
- **Choose a voice** from the OS/browser-provided list via `speechSynthesis.getVoices()`. Note: voices load asynchronously, so you must also listen for the `voiceschanged` event. Source: MDN — *SpeechSynthesis.getVoices()*: https://developer.mozilla.org/en-US/docs/Web/API/SpeechSynthesis/getVoices and *voiceschanged* event: https://developer.mozilla.org/en-US/docs/Web/API/SpeechSynthesis/voiceschanged_event
- **Adjust rate, pitch, volume, language** via properties on the utterance: `rate`, `pitch`, `volume`, `lang`. Source: MDN — *SpeechSynthesisUtterance* properties list: https://developer.mozilla.org/en-US/docs/Web/API/SpeechSynthesisUtterance#instance_properties
- **Pause / resume / cancel** via `speechSynthesis.pause()`, `.resume()`, `.cancel()`. Source: MDN — *SpeechSynthesis*: https://developer.mozilla.org/en-US/docs/Web/API/SpeechSynthesis
- **Lifecycle events** (`start`, `end`, `error`, `boundary`) on the utterance for UI feedback. Source: MDN — *SpeechSynthesisUtterance* events: https://developer.mozilla.org/en-US/docs/Web/API/SpeechSynthesisUtterance#events

### Browser support
- Supported in all current major browsers (Chrome, Edge, Firefox, Safari, Opera) on both desktop and mobile. Source: MDN compatibility table (bottom of the page) — https://developer.mozilla.org/en-US/docs/Web/API/SpeechSynthesis#browser_compatibility and Can I Use: https://caniuse.com/speech-synthesis

### Important caveats (documented)
1. **Available voices differ by OS/browser** because they come from the platform. The same code may sound different on Windows vs macOS vs Android. Source: MDN — `getVoices()` note: "The list of voices is populated asynchronously … and the available voices differ between browsers." https://developer.mozilla.org/en-US/docs/Web/API/SpeechSynthesis/getVoices
2. **You cannot easily download the audio as a file.** The spec only plays the speech through the audio output; it does not expose a `Blob`/`AudioBuffer`. This is a known limitation; the W3C Web Speech API spec defines no recording interface: https://wicg.github.io/speech-api/#tts-section . If downloadable audio is a requirement, see Option 2 or 3.
3. **Autoplay / user-gesture requirement**: Chrome requires a user interaction before audio plays. Source: Chrome autoplay policy — https://developer.chrome.com/blog/autoplay/ (also referenced from MDN's *Autoplay guide for media and Web Audio APIs*: https://developer.mozilla.org/en-US/docs/Web/Media/Autoplay_guide). Triggering speech from a button click satisfies this.
4. **Safari/iOS quirks**: iOS sometimes requires `speak()` to be called directly inside the user-gesture handler. Documented in the WebKit blog post introducing the API: https://webkit.org/blog/7184/introducing-the-web-speech-api/

---

## Option 2: Free-tier cloud TTS services (only if downloadable/high-quality audio is needed)

These run on a server, so the JS would `fetch` an endpoint that returns an audio file (MP3/WAV) playable via `<audio>` or saveable. They are not "free forever" — they have monthly free quotas.

| Service | Free quota | Notes / Docs |
|---|---|---|
| **Google Cloud Text-to-Speech** | 0–1M chars/month free for standard voices, 0–1M for Neural2/WaveNet depending on type, per the pricing page | Pricing: https://cloud.google.com/text-to-speech/pricing • REST quickstart: https://cloud.google.com/text-to-speech/docs/quickstart-protocol |
| **Microsoft Azure AI Speech** | 0.5M chars/month free (Neural) on the F0 tier | Pricing: https://azure.microsoft.com/en-us/pricing/details/cognitive-services/speech-services/ • REST docs: https://learn.microsoft.com/en-us/azure/ai-services/speech-service/rest-text-to-speech |
| **Amazon Polly** | 5M chars/month standard / 1M Neural for the first 12 months only (AWS Free Tier) | https://aws.amazon.com/polly/pricing/ |
| **IBM Watson Text to Speech** | 10,000 chars/month on the Lite plan | https://www.ibm.com/products/text-to-speech/pricing |

**Why these are not the first recommendation:**
- All require creating an account and managing an API key.
- Embedding the API key in client-side JS exposes it; the documented best practice is to call them from a small backend/proxy. See e.g. Google's auth docs: https://cloud.google.com/docs/authentication/api-keys#securing — "API keys are generally not considered secure … treat them like passwords." That contradicts the "simple HTML + JS website" goal.
- They are only free up to a quota, and only the AWS one is permanently free (and only for 12 months).

---

## Option 3: Open-source / self-hosted TTS

Free in license, but they require running your own server (Python, etc.), so they don't fit "simple HTML + JavaScript site" without infrastructure. Listed for completeness:

- **Coqui TTS** (MPL-2.0) — https://github.com/coqui-ai/TTS
- **Mozilla TTS** (archived, superseded by Coqui) — https://github.com/mozilla/TTS
- **Piper** (MIT, runs locally, very fast) — https://github.com/rhasspy/piper
- **eSpeak NG** (GPLv3) — https://github.com/espeak-ng/espeak-ng

There is also **meSpeak.js**, a pure-JS port of eSpeak that runs entirely in the browser with no server. License GPLv3/proprietary dual. Docs: https://www.masswerk.at/mespeak/ . Voice quality is robotic compared to native OS voices, so it's mainly useful as a fallback when the Web Speech API is unavailable.

---

## Recommendation

Use **Option 1: the Web Speech API (`SpeechSynthesis`)**. It is the only option that is:
1. Free with no quota and no account,
2. Implementable as a single static HTML + JS page (no backend),
3. A web standard supported by all major browsers.

### Suggested architecture for the page
1. A `<textarea>` for input.
2. A `<select>` populated from `speechSynthesis.getVoices()` (re-populated on the `voiceschanged` event — required per MDN: https://developer.mozilla.org/en-US/docs/Web/API/SpeechSynthesis/voiceschanged_event).
3. Optional `<input type="range">` controls bound to the utterance's `rate` and `pitch` (ranges per spec: rate 0.1–10, pitch 0–2, volume 0–1 — https://developer.mozilla.org/en-US/docs/Web/API/SpeechSynthesisUtterance#instance_properties).
4. A "Speak" button (satisfies the user-gesture autoplay requirement — https://developer.chrome.com/blog/autoplay/) that builds a `SpeechSynthesisUtterance` and calls `speechSynthesis.speak()`.
5. "Pause", "Resume", and "Stop" buttons wired to `.pause()`, `.resume()`, `.cancel()`.
6. Optional: feature-detect `'speechSynthesis' in window` and show a graceful fallback message if absent (pattern shown in MDN's example: https://developer.mozilla.org/en-US/docs/Web/API/Web_Speech_API/Using_the_Web_Speech_API#speech_synthesis).

### If downloadable audio is later needed
Switch to **Azure** (largest persistent free monthly quota for neural voices) or **Google Cloud TTS**, and add a minimal backend proxy to keep the API key off the client (per Google's auth guidance linked above).

---

## Reference example to study
MDN ships a complete annotated demo (HTML + JS) implementing exactly this — voice picker, rate/pitch sliders, text input, speak button:
- Tutorial: https://developer.mozilla.org/en-US/docs/Web/API/Web_Speech_API/Using_the_Web_Speech_API#speech_synthesis
- Live demo source: https://github.com/mdn/dom-examples/tree/main/web-speech-api/speak-easy-synthesis

This is the most authoritative starting point and is published under MDN's permissive license (CC0 for code samples — https://developer.mozilla.org/en-US/docs/MDN/Writing_guidelines/Attrib_copyright_license).
