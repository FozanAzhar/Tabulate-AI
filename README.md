# Tabulate-AI

**Less clutter. More clarity.**

Tabulate-AI is a receipt expense tracker for Windows. Capture receipts, extract details with OCR and optional cloud AI, review and save expenses locally, then view spending on a dashboard and export reports.

## Features

- Scan receipts with the camera or photo gallery
- Extract merchant, date, amount, and line items (cloud AI or on-device Windows OCR)
- Review and edit fields before saving
- Store receipts offline (SQLite and local image files)
- Dashboard with monthly totals and category breakdown
- History search, edit, and delete
- Export spending summaries

## Tech stack

| Layer | Technologies |
|-------|----------------|
| Mobile app | .NET 9, .NET MAUI, C#, XAML, MVVM |
| Backend (optional) | ASP.NET Core (`TabulateAI.Api`) |
| Data | SQLite, local file storage |
| Charts | Microcharts.Maui |

## Requirements

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- Windows 10/11 (current app target)
- Visual Studio 2022 or later with the **.NET MAUI** workload

## Quick start

### 1. Clone and open

```powershell
git clone https://github.com/FozanAzhar/Tabulate-AI.git
cd Tabulate-AI
```

Open `Tabulate-AI.slnx` in Visual Studio, set the startup project to **TabulateAI**, choose **Windows Machine**, and press **F5**.

Or from the terminal:

```powershell
dotnet build Tabulate-AI.slnx
dotnet run --project TabulateAI -f net9.0-windows10.0.19041.0
```

The app runs without the API using Windows OCR only.

### 2. Optional — cloud AI (Mistral + Gemini)

For higher-quality extraction, run the API and add your keys (never commit keys to Git).

```powershell
cd TabulateAI.Api
dotnet user-secrets set "Ai:GeminiApiKey" "YOUR_GEMINI_KEY"
dotnet user-secrets set "Ai:MistralApiKey" "YOUR_MISTRAL_KEY"
dotnet run
```

In another terminal, run the MAUI app. The default API URL is `http://localhost:5299` (see `TabulateAI/Properties/launchSettings.json`).

You can also copy `TabulateAI.Api/appsettings.Development.json.example` to `appsettings.Development.json` and add keys there. That file is gitignored.

## Solution layout

```
Tabulate-AI/
├── TabulateAI/          MAUI client (UI, ViewModels, local data)
├── TabulateAI.Api/      Optional OCR + parsing API
└── Tabulate-AI.slnx     Solution file
```

## Data storage

- Database: app local data folder (`tabulate.db3`)
- Receipt images: `receipts/` under app data

## License

All rights reserved unless a license file is added later.
