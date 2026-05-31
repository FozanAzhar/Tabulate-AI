# Assignment 1 — SpendSmart Project Pitch

**Course:** INFT6009 Mobile Application Development  
**App:** SpendSmart  
**Tagline:** Less clutter, More clarity.

Use this outline to build your 5 slides and 3-minute presentation script.

---

## Slide 1 — Problem

**Title:** Receipt chaos costs time and clarity

**Talking points:**
- Paper receipts get lost in drawers and wallets
- Manual expense entry is tedious and easy to postpone
- People lose track of where their money goes each month
- Existing tools often push ads, subscriptions, or cloud lock-in

**Script (~30 sec):**  
"How many of you have a wallet full of faded receipts or a shoebox of paper you'll 'organize later'? Manual tracking is slow, and most finance apps add subscriptions or ads. I wanted a simple, truly free way to snap a receipt and see spending clearly."

---

## Slide 2 — Solution

**Title:** SpendSmart — snap, read, archive, track

**Talking points:**
- Snap a photo of any receipt — no typing required
- On-device AI reads merchant, date, and total
- Secure local archive — your data stays on your device
- Dashboard shows spending by category

**Script (~35 sec):**  
"SpendSmart is a .NET MAUI expense tracker. You capture a receipt, the app reads it automatically, you confirm the details, and everything is stored in a searchable archive. The dashboard shows where your money went this month — no ads, no subscription."

---

## Slide 3 — Native features

**Title:** Built on real device capabilities

| Native feature | API | User benefit |
|----------------|-----|--------------|
| Camera / gallery | `MediaPicker` | Scan & Go workflow |
| Local storage | `FileSystem` + SQLite | Offline digital archive |
| On-device OCR | `Windows.Media.Ocr` | Automatic receipt reading |
| Share | `Share` | Export monthly summary |

**Script (~40 sec):**  
"The course requires native features, not just UI. SpendSmart uses MediaPicker for camera and gallery capture, SQLite and the file system for offline storage, and Windows native OCR to extract receipt text — all without a cloud API. Share lets users export their monthly summary through the OS share sheet."

---

## Slide 4 — UI wireframes

**Title:** Three-tab flow

```
[ Scan ]  →  Review  →  Save
[ Archive ]  →  Search / Edit / Delete
[ Dashboard ]  →  Total + Chart + Share
```

**Screens to sketch (simple boxes are fine):**
1. **Scan** — Capture / Gallery / Manual entry buttons
2. **Review** — Image preview, merchant, amount, date, category, Save
3. **Archive** — List with thumbnail, merchant, amount, date
4. **Dashboard** — Monthly total, donut chart, category list

**Script (~35 sec):**  
"The app uses Shell tab navigation: Scan for capture, Archive for history, Dashboard for insights. After capture, users always review OCR results before saving — this handles imperfect reads and keeps usability high."

---

## Slide 5 — Plan and scope

**Title:** 10-week delivery plan

| Phase | Weeks | Deliverable |
|-------|-------|-------------|
| Pitch | 3–4 | This presentation |
| MVP draft | 5–8 | Scan, OCR, archive, dashboard |
| Final prototype | 9–12 | Charts, search, polish, reflection |

**MVP (Week 8):** capture, OCR + edit, save, list, monthly total  
**Final (Week 12):** category chart, search, delete, share, usability fixes

**Risks & mitigations:**
- OCR accuracy varies → always show editable review screen + manual entry fallback
- Scope creep → no accounts, cloud sync, or bank linking in v1

**Script (~40 sec):**  
"MVP by Week 8 covers the full scan-to-save loop plus archive and dashboard. Weeks 9–12 add charts, search, and polish from usability testing. The main risk is OCR accuracy — we mitigate that with a review screen and manual entry. I'm targeting Windows with .NET MAUI in Visual Studio 2026."

---

## Submission checklist

- [ ] 5 content slides (export to PDF or PPTX)
- [ ] Presentation script or speaker notes
- [ ] Rehearse to stay under 3 minutes
- [ ] Submit on Canvas before Week 3 deadline
- [ ] Present live in Week 4
