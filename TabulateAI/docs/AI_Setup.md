# Tabulate-AI — Cloud Extraction Setup

Cloud receipt extraction uses a **local API** so your Gemini and Mistral keys stay off the phone/desktop app.

```
MAUI app  →  TabulateAI.Api  →  Mistral OCR  →  Gemini 2.5 Flash
(local)       (localhost)         (text)            (parse + validate)
```

If the API is unavailable, the app **falls back to Windows OCR** automatically.

---

## Step 1 — Get API keys

### Gemini (Google)
1. Go to [Google AI Studio](https://aistudio.google.com/)
2. Click **Get API key** → create a key
3. Copy the key (starts with `AIza...`)

### Mistral OCR
1. Go to [Mistral Console](https://console.mistral.ai/)
2. Open **API keys** → create a key
3. Copy the key

---

## Step 2 — Configure the API (keys live here only)

**Option A — User Secrets (recommended)**

From the `TabulateAI.Api` folder:

```powershell
cd TabulateAI.Api
dotnet user-secrets set "Ai:GeminiApiKey" "YOUR_GEMINI_KEY"
dotnet user-secrets set "Ai:MistralApiKey" "YOUR_MISTRAL_KEY"
```

**Option B — Development config file**

1. Copy `appsettings.Development.json.example` to `appsettings.Development.json`
2. Paste your keys into that file
3. Do **not** commit `appsettings.Development.json` (already gitignored)

---

## Step 3 — Run the API

```powershell
cd TabulateAI.Api
dotnet run
```

Default URL: **http://localhost:5299**

Test in browser: open `http://localhost:5299` — you should see `"service": "Tabulate-AI API"` and `"configured": true`.

---

## Step 4 — Point the MAUI app at the API

Set an environment variable so the app tries cloud extraction first:

**PowerShell (current session):**
```powershell
$env:TABULATE_AI_API_URL = "http://localhost:5299"
```

**Persist for your user account:**
```powershell
setx TABULATE_AI_API_URL "http://localhost:5299"
```
Restart Visual Studio after `setx`.

**Visual Studio launch profile** — edit `TabulateAI/Properties/launchSettings.json`:

```json
"environmentVariables": {
  "TABULATE_AI_API_URL": "http://localhost:5299"
}
```

If `TABULATE_AI_API_URL` is **not set**, the app uses **Windows OCR only** (offline, no keys needed).

---

## Step 5 — Run the app

1. Start **TabulateAI.Api** (terminal or second VS instance)
2. Start **Tabulate-AI** MAUI app (Windows Machine)
3. Scan tab → pick or capture a receipt
4. Review screen should show **Extracted via: MistralOcr+Gemini**

---

## Troubleshooting

| Problem | Fix |
|---------|-----|
| `configured: false` on API root | Set Gemini + Mistral keys in user secrets |
| Cloud fails, still works locally | API not running — start `dotnet run` in TabulateAI.Api |
| Mistral/Gemini 401 or 403 | Check keys; ensure billing/free tier active |
| Slow first scan | Normal — two cloud API calls; later scans similar |
| Submitting to Canvas | **Never** zip API keys; demo can use Windows OCR fallback |

---

## Course submission notes

- **Native features** unchanged: MediaPicker, SQLite/file storage, Share
- **AI pipeline** is an enhancement — document in reflection report
- Keys belong in **TabulateAI.Api user secrets**, not in the MAUI project
