# Tabulate-AI — Demo Script

Use this script for Assignment 2 pre-recorded update and Assignment 3 final demo.

## Setup (before recording)

- Build and run on **Windows Machine** target
- Have 1–2 sample receipt images ready (clear printed receipts work best)
- Optional: clear app data for a clean demo, or keep 2–3 receipts for dashboard chart
- Optional: start `TabulateAI.Api` for cloud AI demo (see `docs/AI_Setup.md`)

## Demo flow (~3 minutes)

### 1. Introduction (15 seconds)

> "This is Tabulate-AI — a .NET MAUI expense tracker for INFT6009. The tagline is 'Less clutter, More clarity.' You snap a receipt, the app reads it with on-device OCR or cloud AI, and stores everything locally — no subscription required."

### 2. Scan and extraction (45 seconds)

1. Open the **Scan** tab
2. Tap **Pick from Gallery** (or **Capture Receipt** if using webcam)
3. Select a receipt image
4. Point out the loading state: "Reading receipt..."
5. On **Review Receipt**, show pre-filled merchant, amount, date, category
6. Edit one field to show the fallback: "Extraction isn't perfect — users always confirm before saving"
7. Tap **Save Receipt**

### 3. Archive (30 seconds)

1. Switch to **Archive** tab
2. Show the saved receipt with thumbnail, merchant, category, date, amount
3. Type in **Search** to filter by merchant name
4. Tap a receipt to edit, or swipe to delete

### 4. Dashboard (30 seconds)

1. Open **Dashboard** tab
2. Show **Total spent this month**
3. Show **donut chart** and category breakdown list
4. Tap **Share Summary** — show Windows share UI

### 5. Native features (30 seconds)

> "Native features for the course: MediaPicker for camera and gallery, FileSystem plus SQLite for offline storage, Windows.Media.Ocr for on-device text extraction, and Share for exporting the monthly summary."

### 6. Close (15 seconds)

> "MVP is complete for Assignment 2. For the final submission I added search, charts, category suggestions, cloud AI extraction, and fixes from usability testing."

## Troubleshooting during demo

| Problem | Fallback |
|---------|----------|
| Extraction returns empty fields | Use **Manual Entry** from Scan tab |
| Camera not available | Use **Pick from Gallery** |
| Chart empty | Save at least one receipt for current month |
| Share does nothing | Mention it's OS-dependent; show summary text in code |

## Files to reference if asked

- OCR: `Platforms/Windows/WindowsOcrService.cs`, `Services/HybridOcrService.cs`
- Cloud AI: `TabulateAI.Api/Services/ReceiptAiPipeline.cs`
- Storage: `Services/ReceiptRepository.cs`, `Services/ImageStorageService.cs`
- Camera: `ViewModels/ScanViewModel.cs`
