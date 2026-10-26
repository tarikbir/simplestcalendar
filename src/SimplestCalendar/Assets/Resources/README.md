# Replaceable visual resources

- `AppIcon.svg` — canonical artwork (prefer this when regenerating)
- `AppIcon-source.png` — high-res raster of the SVG (1024×1024)
- `AppIcon.ico` — window, tray, and exe icon

WinUI tile/splash PNGs under `Assets/` are generated from the SVG raster. After replacing icon assets, rebuild so the next installer/publish picks them up.
