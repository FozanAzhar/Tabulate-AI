# SpendSmart — Final Submission Checklist

Use this before submitting Assignment 3 (Week 12).

## Code quality

- [x] Builds on Windows (`net10.0-windows10.0.19041.0`) without errors
- [x] MVVM separation (Views / ViewModels / Services / Models)
- [x] Dependency injection in `MauiProgram.cs`
- [x] No hard-coded secrets or API keys

## Features

- [x] Scan: camera, gallery, manual entry
- [x] OCR + editable review screen
- [x] SQLite + local image archive
- [x] Archive: list, search, edit, swipe delete
- [x] Dashboard: monthly total, category list, donut chart
- [x] Share monthly summary

## Native features (minimum 2)

- [x] MediaPicker (camera / gallery)
- [x] FileSystem + SQLite storage
- [x] Windows.Media.Ocr
- [x] Share API

## Assignment deliverables

| Item | Location |
|------|----------|
| Pitch outline | `docs/Assignment1_Pitch_Outline.md` |
| Usability report template | `docs/Assignment2_Usability_Report_Template.md` |
| Reflection template | `docs/Assignment3_Final_Reflection_Template.md` |
| Demo script | `docs/Demo_Script.md` |
| README | `README.md` |

## Before you submit

1. Run full demo from `docs/Demo_Script.md`
2. Fill in usability report after lab testing
3. Fill in reflection report with your own experience
4. Zip project or push to Git per Canvas instructions
5. Record or prepare live code demo

## Known limitations (document in reflection)

- OCR accuracy depends on receipt quality and lighting
- Windows OCR only; other platforms use stub service
- Categories use keyword rules, not machine learning
