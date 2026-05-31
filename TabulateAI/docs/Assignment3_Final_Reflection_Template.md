# Assignment 3 — Final Reflection Report (Template)

**App:** Tabulate-AI  
**Course:** INFT6009 Mobile Application Development  
**Platform:** .NET MAUI (Windows)  
**Student:** ___________________  

Submit with code files and demo video by **Week 12**.

---

## 1. Project summary (150–200 words)

Describe what Tabulate-AI does, who it is for, and the core value proposition ("Less clutter, More clarity").

---

## 2. Native features implemented

Explain each native feature, **why** it was chosen, and **where** it appears in the app:

### 2.1 Camera / MediaPicker
- **File(s):** `ViewModels/ScanViewModel.cs`
- **Behaviour:**  
- **Course relevance:**  

### 2.2 File storage + SQLite
- **File(s):** `Services/ImageStorageService.cs`, `Services/ReceiptRepository.cs`
- **Behaviour:**  
- **Course relevance:**  

### 2.3 Windows OCR
- **File(s):** `Platforms/Windows/WindowsOcrService.cs`, `Services/ReceiptParser.cs`
- **Behaviour:**  
- **Course relevance:**  

### 2.4 Share (optional)
- **File(s):** `ViewModels/DashboardViewModel.cs`
- **Behaviour:**  

---

## 3. Architecture and design decisions

- Why MVVM + dependency injection?
- Why on-device OCR instead of cloud API?
- How does navigation work (Shell tabs + ReviewReceipt route)?

---

## 4. Usability improvements (since Assignment 2)

| Issue from testing | Change made | Outcome |
|--------------------|-------------|---------|
| | | |
| | | |

---

## 5. Challenges and limitations

**Technical challenges:**
- OCR accuracy on faded or crumpled receipts
- Parsing ambiguous date/amount formats

**What you would do differently:**
-  

**Known limitations:**
- Windows-only OCR implementation (stub on other platforms)
- Category suggestion is rule-based, not ML

---

## 6. Future work

- Android port with ML Kit OCR
- Optional cloud backup
- Budget alerts via local notifications
- Multi-currency support

---

## 7. Self-assessment

| Criterion | Evidence | Self-rating (1–5) |
|-----------|----------|-------------------|
| App runs and demos reliably | | |
| Native features used meaningfully | | |
| Code organisation | | |
| UI clarity | | |
| Usability response | | |

---

## 8. Demo script (final submission)

1. Launch app on Windows
2. Scan tab → pick/capture receipt
3. Review → edit → save
4. Archive → search → edit/delete
5. Dashboard → chart → share summary
6. Mention data stored locally (offline, free)

---

## Submission checklist

- [ ] Code compiles in Visual Studio 2026
- [ ] Code files uploaded (zip or Git link per Canvas instructions)
- [ ] Demo video or live demo prepared
- [ ] This reflection completed
- [ ] Submitted by Week 12 deadline
