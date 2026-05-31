# Tabulate-AI

Less clutter, More clarity — an AI-powered receipt expense tracker built with **.NET MAUI** for **INFT6009 Mobile Application Development**.

## Features

- **Scan & Go** — capture a receipt with the device camera or pick from gallery (`MediaPicker`)
- **Smart extraction** — cloud AI pipeline (Mistral OCR + Gemini) with Windows OCR fallback
- **Review before save** — edit extracted fields when extraction is imperfect
- **Digital archive** — receipt images and metadata stored locally (SQLite + file system)
- **Dashboard** — monthly spending total, category breakdown, and donut chart (Microcharts)
- **Search & manage** — search archive, edit, swipe-to-delete
- **Share summary** — native share sheet for monthly spending report

## Native features (course requirement)

| Feature | MAUI / platform API | Purpose |
|---------|---------------------|---------|
| Camera / media | `MediaPicker` | Capture or pick receipt photos |
| File storage | `FileSystem` + SQLite | Persist images and expense records offline |
| OCR | `Windows.Media.Ocr` | On-device fallback text extraction (Windows) |
| Share | `Share` | Export spending summary |

## Tech stack

- .NET 10 / .NET MAUI
- C# + XAML, MVVM (`CommunityToolkit.Mvvm`)
- SQLite (`sqlite-net-pcl`)
- Microcharts.Maui
- Optional cloud AI: Mistral OCR + Gemini 2.5 Flash (see [`docs/AI_Setup.md`](docs/AI_Setup.md))

## AI pipeline (optional)

Mistral OCR + Gemini 2.5 Flash run in a separate local API so keys stay off the device. The app tries cloud extraction first, then falls back to Windows OCR.

**Setup guide:** [`docs/AI_Setup.md`](docs/AI_Setup.md)

Quick start:

```powershell
# 1. Add keys (once)
cd TabulateAI.Api
dotnet user-secrets set "Ai:GeminiApiKey" "YOUR_GEMINI_KEY"
dotnet user-secrets set "Ai:MistralApiKey" "YOUR_MISTRAL_KEY"

# 2. Run API
dotnet run

# 3. Run MAUI app (launch profile already sets TABULATE_AI_API_URL)
```

## Run the app

1. Open `TabulateAI.slnx` or the project in **Visual Studio 2026**
2. Set target to **Windows Machine**
3. Press F5

Or from terminal:

```powershell
cd TabulateAI
dotnet build -f net10.0-windows10.0.19041.0
dotnet run -f net10.0-windows10.0.19041.0
```

## Project structure

```
TabulateAI/
├── Models/              Receipt, categories, summaries
├── Services/            Repository, image storage, OCR, parsing
├── Platforms/Windows/   WindowsOcrService
├── ViewModels/          MVVM for each page
├── Views/               Scan, Review, Archive, Dashboard
└── docs/                Assignment deliverable templates
```

## Demo script (Assignment 2 / 3)

1. Open **Scan** → **Pick from Gallery** (or **Capture Receipt** with webcam)
2. Wait for extraction → **Review Receipt** screen pre-fills fields
3. Edit if needed → **Save Receipt**
4. Open **Archive** → receipt appears with thumbnail; search by merchant
5. Open **Dashboard** → monthly total and category chart update
6. Tap **Share Summary** to demonstrate native share

## Data location

- Database: `%LOCALAPPDATA%\Packages\<app>\LocalState\tabulate.db3` (packaged) or app data directory
- Receipt images: `receipts/` subfolder under app data

## Course assessments

See [`docs/`](docs/) for pitch slides outline, usability testing report template, and final reflection template.
