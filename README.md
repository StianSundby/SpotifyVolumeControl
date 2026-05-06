
# 🔈 SpotifyVolumeControl 🔊

> A lightweight Windows tray app that lets a knob press toggle your volume knob between system audio and Spotify-only control.

Built for the Monsgeek M1 V5 with custom QMK firmware, but **works with any rotary encoder that can send keycodes**.

---
## ❔ How it works

The app runs silently in the system tray and installs a global low-level keyboard hook. In **System mode**, volume keys pass through to Windows as normal. In **Spotify mode**, they're intercepted and swallowed before Windows ever sees them — only Spotify's audio session is touched.

Spotify is located by enumerating the Windows Core Audio session list and matching by process name. No Spotify API or any of that nonsense.

```
Knob rotate  →  Volume Up / Down
Knob press   →  Toggle System ⟷ Spotify mode
```

By default the keypress that is intercepted is F24, this can be changed by right clicking the tray icon.

---
## 🔨 Requirements

- Windows 10 or 11
- [.NET 8 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- Spotify Desktop app
- A rotary encoder (knob)
---
## 🛠️ Getting Started

For all options I reccomend you do the following afterwards:

1. Press `Win + R`, type `shell:startup`
2. Drop a shortcut to the exe in that folder

### Option 1 - Download latest release
Download the latest release [here](https://github.com/StianSundby/SpotifyVolumeControl/releases).

### Option 2 - Clone and run

```bash
git clone https://github.com/yourname/SpotifyVolumeControl
cd SpotifyVolumeControl
dotnet restore
dotnet run
```
Exe file is in `/SpotifyVolumeControl/bin/Debug/net8.0-windows/`.

### Option 3 - publish the .exe yourself
```bash
git clone https://github.com/yourname/SpotifyVolumeControl
cd SpotifyVolumeControl
dotnet restore
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```
Exe file is in `bin/Release/net8.0-windows/win-x64/publish/`.

---
## ⌨️ QMK setup 

This was built alongside QMK firmware for the Monsgeek M1 V5 UK ISO. The relevant encoder map in `keymap.c`:

```c
#ifdef ENCODER_MAP_ENABLE
const uint16_t PROGMEM encoder_map[][NUM_ENCODERS][NUM_DIRECTIONS] = {
    [0] = {ENCODER_CCW_CW(KC_VOLD, KC_VOLU)},
    ...
};
#endif
```

The knob click is mapped to `KC_F24` on the base layer, which is what triggers the mode toggle. But bind it to whatever you want.

---
## ⚙️ Tray icon

| State | Tooltip |
|---|---|
| System mode | `Volume: System 🔊` |
| Spotify mode | `Volume: Spotify 🎵` |

Right-click the tray icon to toggle mode manually, change keybind, change volume increment or exit.

---
## ⚠️ Caveats

- Spotify must have played something at least once in the current session for its audio session to appear in the Windows mixer. The app will warn you with a balloon tip if you switch to Spotify mode and no session is found. This might require you to press the key again.
- If Spotify is muted at the OS level (in the volume mixer), adjustments will still apply to the underlying volume — unmute it first.

---
## Dependencies

| Package | Purpose |
|---|---|
| [NAudio](https://github.com/naudio/NAudio) | Windows Core Audio API wrapper |

---

*Made for a mechanical keyboard that deserved better firmware.*
