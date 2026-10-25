# Quickstart: verify 003-ux-feedback-polish

1. `dotnet test tests/SimplestCalendar.Core.Tests`
2. Build and launch the WinUI app.
3. Create entry 23:30 + 1h → confirm next-day end 00:30; delete one segment only.
4. Navigate Next → Prev → Today; Prev disabled on current month.
5. Star an entry → star on grid; today cell tinted; maximize → cells grow; 5 notes → in-cell scroll.
6. Editor: TimePicker, duration hours+minutes one row, emoji by note.
7. Day list middle-click delete; hover emphasizes entries.
8. Quit — no hang.
