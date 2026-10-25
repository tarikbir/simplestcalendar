# Replaceable visual resources

Put app icons and other replaceable images here.

- `AppIcon.ico` — window, tray, and exe icon (placeholder calendar-style asset until you replace it)
- Add PNGs / other art here as needed; wire new files in `SimplestCalendar.csproj` as `Content` with `CopyToOutputDirectory`.

After replacing `AppIcon.ico`, rebuild so the next installer/publish picks up the new icon.
