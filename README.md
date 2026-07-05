# Expensely (Tabulate-AI)

Snap a receipt. AI extracts the details. Track spending and export reports — all on your phone.

## What it does

- Scan receipts with the camera or gallery
- AI reads merchant, date, amount, and line items
- Review and edit before saving
- Dashboard, history, budgets, and category breakdowns
- Export CSV/PDF or email a report with attachments
- Light/dark mode, backup & restore

## Requirements

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2022+ with the **.NET MAUI** workload
- Android emulator or device (app targets **Android**)

## Run the app

```powershell
git clone https://github.com/FozanAzhar/Tabulate-AI.git
cd Tabulate-AI
```

Open `Tabulate-AI.slnx` in Visual Studio, set **TabulateAI** as the startup project, pick an Android emulator, and press **F5**.

Or from the terminal:

```powershell
dotnet build TabulateAI/TabulateAI.csproj -f net9.0-android
```

## AI extraction (optional)

For best results, run the API locally. Keys stay on the server — never in the app.

```powershell
cd TabulateAI.Api
copy appsettings.Development.json.example appsettings.Development.json
# Add your Gemini + Mistral keys to appsettings.Development.json
dotnet run
```

The app talks to the API at `http://10.0.2.2:5299` on the Android emulator by default.

## Project structure

```
Tabulate-AI/
├── TabulateAI/       Mobile app (MAUI)
├── TabulateAI.Api/   AI receipt extraction API
└── Tabulate-AI.slnx
```

## Data

Everything is stored locally on the device — SQLite database and receipt images in the app folder. No account required.

## License

All rights reserved.
