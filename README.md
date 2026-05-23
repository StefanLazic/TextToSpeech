# TextToSpeech

A small **Windows desktop application** that converts text to speech in **Serbian (sr-RS)**
using **Azure AI Speech**. Written in **C# / .NET 8 (WPF)**.

The app lets you:

- type or paste Serbian text into a text box,
- pick between the two Serbian neural voices (`sr-RS-SophieNeural`, `sr-RS-NicholasNeural`),
- click **Изговори** to hear the audio, or **Сачувај WAV…** to save it to disk.

The architecture deliberately separates a UI-free `TextToSpeech.Core` library from the WPF
shell so that the synthesizer can be replaced by a mock in tests — see
[Testing](#testing) below.

---

## Repository layout

```
TextToSpeech.slnx
├── src/
│   ├── TextToSpeech.Core/          # ISpeechSynthesizer, AzureSpeechSynthesizer, options, voices
│   └── TextToSpeech.App/           # WPF desktop app (net8.0-windows)
└── tests/
    ├── TextToSpeech.Core.Tests/        # Unit tests (validation, options, voice catalog)
    └── TextToSpeech.IntegrationTests/  # Integration tests using a mocked ISpeechSynthesizer
```

---

## Prerequisites

- **Windows 10 or 11** (the app uses WPF and `System.Media.SoundPlayer`).
- **.NET 8 SDK** — download from <https://dotnet.microsoft.com/download/dotnet/8.0>.
- An **Azure subscription** with a free **Azure AI Speech** resource (see next section).

---

## Creating a free Azure account and an Azure AI Speech resource

Azure AI Speech offers a permanent **free tier (F0)** that includes
**0.5 million characters per month** of neural text-to-speech — more than enough for
personal use. Pricing reference: <https://azure.microsoft.com/pricing/details/cognitive-services/speech-services/>.

### 1. Create a free Azure account

1. Go to <https://azure.microsoft.com/free/>.
2. Click **Start free** and sign in with a Microsoft account (or create one).
3. Complete identity verification (phone + credit card — the free tier does not charge,
   the card is only used to confirm you are a real person).
4. New accounts get **USD 200 of credit for 30 days** plus access to always-free services
   such as Azure AI Speech F0.

### 2. Create a Speech resource

1. Sign in to the Azure portal: <https://portal.azure.com/>.
2. Click **Create a resource** → search for **Speech** → choose **Speech** by Microsoft →
   **Create**.
3. Fill in:
   - **Subscription**: your free subscription.
   - **Resource group**: create a new one, e.g. `tts-rg`.
   - **Region**: pick one close to you, e.g. `West Europe` or `East US`.
     Remember the region — you will need it as `AZURE_SPEECH_REGION`.
   - **Name**: a unique name, e.g. `tts-srpski-demo`.
   - **Pricing tier**: **Free F0** (one free F0 per subscription).
4. Click **Review + create**, then **Create**.

### 3. Copy the key and region

1. After deployment, open the Speech resource.
2. In the left menu, click **Keys and Endpoint**.
3. Copy **KEY 1** (this becomes `AZURE_SPEECH_KEY`) and the **Location/Region**
   (this becomes `AZURE_SPEECH_REGION`, e.g. `westeurope`).

> ⚠️ Treat the subscription key like a password. **Never commit it to source control.**
> The repository's `.gitignore` already excludes `appsettings.json` for this reason.

---

## Configuring the app

The app reads the Azure credentials from (in order of precedence):

1. **Environment variables** — `AZURE_SPEECH_KEY` and `AZURE_SPEECH_REGION`.
2. **`appsettings.json`** next to the executable, with this shape:

   ```json
   {
     "AzureSpeech": {
       "SubscriptionKey": "<your-key>",
       "Region": "westeurope"
     }
   }
   ```

### Option A — environment variables (recommended)

PowerShell, for the current user (persistent):

```powershell
setx AZURE_SPEECH_KEY  "<your-key>"
setx AZURE_SPEECH_REGION "westeurope"
```

Open a new terminal afterwards so the variables are picked up.

### Option B — `appsettings.json`

Create `src/TextToSpeech.App/appsettings.json` with the JSON above and set the
file's **Copy to Output Directory** to *Copy if newer* in your IDE, or place the file
next to the built `TextToSpeech.App.exe`.

---

## Building and running

Clone the repo, then from the repository root:

```powershell
# Restore + build everything
dotnet build

# Run the WPF app
dotnet run --project src/TextToSpeech.App
```

The window will show **„Спремно. Регион: <your-region>.“** when configuration loaded
successfully. If the key/region are missing, the buttons stay disabled and the status
line explains what is wrong.

To produce a self-contained release build:

```powershell
dotnet publish src/TextToSpeech.App -c Release -r win-x64 --self-contained
```

The output is in `src/TextToSpeech.App/bin/Release/net8.0-windows/win-x64/publish/`.

---

## Testing

Tests **never call Azure** — the integration tests mock `ISpeechSynthesizer` using
[Moq](https://github.com/devlooped/moq) so the suite runs offline and no subscription
key is needed.

Run everything:

```powershell
dotnet test
```

You should see something like:

```
Passed!  - Failed: 0, Passed: 13, ... TextToSpeech.Core.Tests.dll
Passed!  - Failed: 0, Passed:  7, ... TextToSpeech.IntegrationTests.dll
```

### Unit tests — `tests/TextToSpeech.Core.Tests`

- `AzureSpeechOptionsTests` — validation of subscription key / region / Serbian defaults.
- `AzureSpeechSynthesizerTests` — argument-null checks, empty-text handling, options validation in the constructor.
- `SerbianVoicesTests` — the supported voice catalog is consistent and Serbian-only.

### Integration tests — `tests/TextToSpeech.IntegrationTests`

- `TextToSpeechServiceIntegrationTests` — drives `TextToSpeechService` end-to-end with a
  mocked `ISpeechSynthesizer` and asserts:
  - the text and the default Serbian voice (`sr-RS-SophieNeural`) reach the synthesizer,
  - an explicit voice selection (e.g. `sr-RS-NicholasNeural`) is honoured,
  - empty / whitespace text short-circuits without calling Azure,
  - errors returned by the synthesizer surface to the caller,
  - a cancellation token is forwarded to the synthesizer.

---

## Troubleshooting

| Symptom | Likely cause |
|---|---|
| Status line says "Конфигурација није подешена…" | `AZURE_SPEECH_KEY` / `AZURE_SPEECH_REGION` not set, or `appsettings.json` missing. |
| Status line says "Грешка: WebSocket Upgrade failed with HTTP status code: 401" | Wrong key or wrong region for that key. |
| Status line says "Грешка: Connection failed: ConnectionFailure" | No network, firewall blocking, or invalid region name. |
| No sound but status says "Готово (… бајтова)" | OS audio output muted / no default device; use **Сачувај WAV…** to verify the audio was generated. |
