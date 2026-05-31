# SpendSmart

Less clutter, More clarity — an AI-powered receipt expense tracker built with **.NET MAUI** for **INFT6009 Mobile Application Development**.

## Features

- **Scan & Go** — capture a receipt with the device camera or pick from gallery (`MediaPicker`)
- **On-device OCR** — Windows `OcrEngine` reads receipt text and extracts merchant, date, and total
- **Review before save** — edit extracted fields when OCR is imperfect
- **Digital archive** — receipt images and metadata stored locally (SQLite + file system)
- **Dashboard** — monthly spending total, category breakdown, and donut chart (Microcharts)
- **Search & manage** — search archive, edit, swipe-to-delete
- **Share summary** — native share sheet for monthly spending report

## Native features (course requirement)

| Feature | MAUI / platform API | Purpose |
|---------|---------------------|---------|
| Camera / media | `MediaPicker` | Capture or pick receipt photos |
| File storage | `FileSystem` + SQLite | Persist images and expense records offline |
| OCR | `Windows.Media.Ocr` | Extract receipt text on-device (Windows) |
| Share | `Share` | Export spending summary |

## Tech stack

- .NET 10 / .NET MAUI
- C# + XAML, MVVM (`CommunityToolkit.Mvvm`)
- SQLite (`sqlite-net-pcl`)
- Microcharts.Maui

## Run the app

1. Open `SpendSmart.sln` or the project in **Visual Studio 2026**
2. Set target to **Windows Machine**
3. Press F5

Or from terminal:

```powershell
cd SpendSmart
dotnet build -f net10.0-windows10.0.19041.0
dotnet run -f net10.0-windows10.0.19041.0
```

## Project structure

```
SpendSmart/
├── Models/              Receipt, categories, summaries
├── Services/            Repository, image storage, OCR, parsing
├── Platforms/Windows/   WindowsOcrService
├── ViewModels/          MVVM for each page
├── Views/               Scan, Review, Archive, Dashboard
└── docs/                Assignment deliverable templates
```

## Demo script (Assignment 2 / 3)

1. Open **Scan** → **Pick from Gallery** (or **Capture Receipt** with webcam)
2. Wait for OCR → **Review Receipt** screen pre-fills fields
3. Edit if needed → **Save Receipt**
4. Open **Archive** → receipt appears with thumbnail; search by merchant
5. Open **Dashboard** → monthly total and category chart update
6. Tap **Share Summary** to demonstrate native share

## Data location

- Database: `%LOCALAPPDATA%\Packages\<app>\LocalState\spendsmart.db3` (packaged) or app data directory
- Receipt images: `receipts/` subfolder under app data

## Course assessments

See [`docs/`](docs/) for pitch slides outline, usability testing report template, and final reflection template.
