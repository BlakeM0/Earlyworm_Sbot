# 🐛 Early Worm — Streamer.bot Giveaway System

A fully featured Twitch giveaway system for Streamer.bot with an animated OBS overlay, monthly prize draw, VIP reward handling, and Stream Deck support.

---

## Features

- Daily draw with secure random shuffle
- Monthly prize draw using accumulated daily entries
- Automatic VIP award for the daily winner
- Animated scrolling name overlay for OBS
- Stream Deck button support via the Streamer.bot plugin
- All chat messages configurable at the top of the code
- Auto show/hide overlay with stale data protection on refresh

---

## Repository Structure

```
EarlyWorm/
│
├── Early_worm/                        — Drag this entire folder into your Streamer.bot directory
│   ├── EarlyWorm.cs                   — Main C# inline script
│   ├── Early_Worm_List.txt            — Daily/monthly entry log (created automatically)
│   ├── VIP_To_Remove.txt              — VIP removal queue (created automatically)
│   ├── Overlay/
│   │   ├── EarlyWormOverlay.html      — OBS browser source overlay
│   │   ├── EarlyWormData.json         — Data file shared between C# and overlay
│   │   └── worm.png                   — Add your own worm image here
│   └── Archive/                       — Monthly entry archives (created automatically)
│
├── README.md                          — This file
├── STREAMERBOT_SETUP.md               — How to set up actions in Streamer.bot
├── STREAMDECK_SETUP.md                — How to configure Stream Deck buttons
├── OVERLAY_SETUP.md                   — How to set up the OBS overlay
└── CONFIGURATION.md                   — Full reference for all config options
```

---

## Quick Start

1. Drag the `Early_worm` folder into your Streamer.bot directory
2. Read [STREAMERBOT_SETUP.md](STREAMERBOT_SETUP.md) to create the required actions — or use the included import file
3. Read [OVERLAY_SETUP.md](OVERLAY_SETUP.md) to add the overlay to OBS
4. Optionally read [STREAMDECK_SETUP.md](STREAMDECK_SETUP.md) to set up Stream Deck buttons
5. Edit the configuration blocks at the top of `EarlyWorm.cs` and `EarlyWormOverlay.html` to customise messages and appearance

---

## File Placement

The `Early_worm` folder is pre-structured and ready to drop straight into your Streamer.bot directory:

```
Streamer.bot/
└── Early_worm/
    ├── EarlyWorm.cs
    ├── Early_Worm_List.txt
    ├── VIP_To_Remove.txt
    ├── Overlay/
    │   ├── EarlyWormOverlay.html
    │   ├── EarlyWormData.json
    │   └── worm.png              — add your own image here
    └── Archive/
```

> The `Early_Worm_List.txt`, `VIP_To_Remove.txt` and `Archive` folder are all created automatically by the code if they do not exist. They are included in the repository as empty placeholders to preserve the folder structure.

---

## Commands

| Command | Description |
|---------|-------------|
| `!earlyworm start` | Open entries |
| `!earlyworm stop` | Close entries |
| `!earlyworm draw` | Draw a winner |
| `!earlyworm reset` | Reset entries without drawing |
| `!earlyworm auto` | Open entries and start the auto-close timer |
| `!rub` | Enter the draw |
| `!rub odds` | Check your odds |
| `!rub count` | Show current entry count |

---

## Requirements

- [Streamer.bot](https://streamer.bot) v0.2.0 or later
- OBS Studio (for the overlay)
- Elgato Stream Deck software + Streamer.bot plugin (optional)

---

## Notes

- The monthly draw picks a winner from all accumulated daily entries — users who win daily draws more often have proportionally better monthly odds
- VIPs are automatically granted to daily winners and queued for removal at the next stream via `RemoveOldVips`
- All file paths are relative to the Streamer.bot base directory and are created automatically on first run
