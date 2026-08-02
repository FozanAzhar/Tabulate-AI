# TabulateAI

Snap a receipt. AI pulls out the details. Track your spending — all on your phone.

## Screenshots

| Welcome | Dashboard |
|:---:|:---:|
| ![Welcome screen](docs/screenshots/welcome.png) | ![Dashboard overview](docs/screenshots/dashboard.png) |

| Receipt detail | Reports |
|:---:|:---:|
| ![Receipt detail with line items](docs/screenshots/receipt-detail.png) | ![Reports and export](docs/screenshots/reports.png) |

## What you can do

- **Scan receipts** with your camera or pick one from your gallery
- **Let AI read it** — merchant, date, amount, and each line item are filled in for you
- **Review and edit** anything before saving
- **See your spending** on a dashboard with category breakdowns and monthly budgets
- **Search your history** and edit or delete past receipts
- **Export reports** as CSV or PDF, or email a report with the PDF attached
- **Set monthly budgets** per category and track how you're going
- **Light and dark mode**, plus backup & restore of your data

## Your data stays yours

Everything is stored **on your device** — there's no account to create and no sign-in. Your receipts and images never leave your phone unless you choose to export or email a report.

## Getting started

1. Open the app and tap the **camera** button
2. Point it at a receipt (or choose one from your gallery)
3. Wait a moment while the AI reads it *(requires OCR setup below)*
4. Check the details, pick a category, and **save**
5. Head to the **Dashboard** and **Reports** tabs to see your spending

## Tips

- Add your **name and email** in Settings so exported reports are personalised
- Set **monthly budgets** in Settings to get category tracking on your dashboard
- Use **Reports → Email report** to send a PDF (with receipt thumbnails) to yourself, your manager, or your accountant

## Important for markers / assessors

You can open and explore most of the app **without any API keys** — dashboard, history, manual expenses, budgets, local notifications, backup/restore, and exports all work offline on the device.

**AI receipt OCR does not work out of the box.**  
API keys are intentionally **not** included in this repository (they are personal secrets and would be unsafe to commit).

If you want to **see the live OCR feature working** — scan/pick a receipt and have merchant, date, amount, and line items filled in automatically — you must set up **your own** Gemini and Mistral API keys and run the local API, as described below.

| What you want to do | Setup needed? |
|---------------------|---------------|
| Browse the app, add expenses manually, test budgets / notifications / backup | No |
| Watch AI extract details from a real receipt image | **Yes — complete the OCR setup** |

Without OCR setup:

- On **Android** (typical demo target): receipt fields stay empty after a scan; enter details manually on the review screen
- On **Windows**: a basic on-device OCR fallback may still fill some text

---

## OCR setup (required to see AI extraction live)

AI receipt reading uses a local ASP.NET API (`TabulateAI.Api`) that calls **Mistral OCR** and **Google Gemini**. Each person who wants live OCR must use **their own** keys from those providers.

### Prerequisites

- .NET 9 SDK with the MAUI workload
- Android emulator (or a physical device)
- Free accounts / API keys from Google AI Studio and Mistral (links below)

### 1. Get your own API keys

1. **Google Gemini** — create a key at [aistudio.google.com](https://aistudio.google.com)  
2. **Mistral** — create a key at [console.mistral.ai](https://console.mistral.ai)  

You will paste both into a local config file in the next step. Do **not** commit that file to git.

### 2. Configure the API

```bash
cd TabulateAI.Api
copy appsettings.Development.json.example appsettings.Development.json
```

(On macOS/Linux, use `cp` instead of `copy`.)

Open `appsettings.Development.json` and set:

| Setting | What to put |
|---------|-------------|
| `Ai:GeminiApiKey` | Your Gemini key |
| `Ai:MistralApiKey` | Your Mistral key |
| `Security:ClientApiKey` | Any long random string (shared secret between API and app) |

The example file already contains a sample `ClientApiKey` (`dev-local-change-me-use-a-long-random-string`) that matches the MAUI **Android Emulator** launch profile. You can keep that value for local testing, or change it in **both** places.

### 3. Run the API

In a terminal:

```bash
cd TabulateAI.Api
dotnet run
```

Leave this running. It listens on `http://localhost:5299` by default.  
The Android emulator reaches your PC via `http://10.0.2.2:5299` (already configured in the app launch profile).

### 4. Point the MAUI app at the API

**Easiest (Android emulator):** launch the app using the **Android Emulator** profile in Visual Studio / `launchSettings.json`. It already sets:

| Variable | Value |
|----------|--------|
| `TABULATE_AI_API_URL` | `http://10.0.2.2:5299` |
| `TABULATE_AI_API_KEY` | Same as `Security:ClientApiKey` in the example |

If you change `ClientApiKey`, update `TABULATE_AI_API_KEY` to match.

**Windows app:** use `http://localhost:5299` for `TABULATE_AI_API_URL`.

**Physical phone:** use your computer’s LAN IP (e.g. `http://192.168.x.x:5299`) instead of `10.0.2.2` / `localhost`, and ensure the phone and PC are on the same Wi‑Fi. You may also need to allow that IP for cleartext HTTP in `Platforms/Android/Resources/xml/network_security_config.xml`.

### 5. Verify OCR is working

1. Start `TabulateAI.Api` and confirm it is running (no missing-key errors in the console)
2. Launch the MAUI app (Android Emulator profile)
3. Tap scan → prefer **gallery / pick image** on an emulator (virtual camera is often unreliable)
4. Choose a clear receipt photo
5. After a few seconds, the review screen should show merchant, date, amount, and line items filled in

If fields stay empty, check that:

- The API process is still running
- Both Gemini and Mistral keys are set (not the `YOUR_...` placeholders)
- `TABULATE_AI_API_KEY` matches `Security:ClientApiKey`
- You used the Android Emulator profile (so `TABULATE_AI_API_URL` is set)
