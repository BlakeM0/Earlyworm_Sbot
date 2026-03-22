# Configuration Reference

All configuration options for the Early Worm system. There are two separate config blocks — one in `EarlyWorm.cs` and one in `EarlyWormOverlay.html`. Neither requires any changes to work out of the box.

---

## EarlyWorm.cs

The config block is at the very top of the class, clearly marked. Everything below it is the working code and should not need to be changed.

---

### Chat Messages

All chat messages sent by the bot are defined as constants. Placeholders in `{curly braces}` are replaced automatically at runtime.

| Constant | Placeholders | Description |
|----------|-------------|-------------|
| `MSG_ALREADY_OPEN` | *(none)* | Entries are already open |
| `MSG_ENTRIES_OPEN` | *(none)* | Entries have opened |
| `MSG_ENTRIES_CLOSED` | `{count}` | Entries have closed |
| `MSG_ALREADY_ENTERED` | `{user}` | User has already entered |
| `MSG_JOINED` | `{user}`, `{count}` | User joined the draw |
| `MSG_NO_ENTRIES` | *(none)* | No entries when draw is called |
| `MSG_WINNER` | `{winner}`, `{total}` | Daily winner announcement |
| `MSG_NO_ENTRIES_COUNT` | *(none)* | No entries when count is checked |
| `MSG_ENTRY_COUNT_ONE` | *(none)* | Exactly one entry |
| `MSG_ENTRY_COUNT_MANY` | `{count}` | Multiple entries |
| `MSG_ODDS_NONE` | `{user}` | No entries exist when odds checked |
| `MSG_ODDS_NOT_ENTERED` | `{user}` | User not in draw when odds checked |
| `MSG_ODDS` | `{user}`, `{userEntries}`, `{total}`, `{odds}` | User's odds |
| `MSG_RESET` | *(none)* | Entries have been reset |
| `MSG_AUTO_CLOSING` | *(none)* | Auto mode started |
| `MSG_MONTHLY_WINNER` | `{winner}`, `{userEntries}`, `{total}` | Monthly winner announcement |
| `MSG_MONTHLY_ARCHIVED` | *(none)* | Monthly entries archived |
| `MSG_MONTHLY_NO_ENTRIES` | *(none)* | No monthly entries found |
| `MSG_MONTHLY_NOT_FOUND` | *(none)* | Monthly file not found |

---

### Behaviour Toggles

| Constant | Type | Default | Description |
|----------|------|---------|-------------|
| `USE_OVERLAY` | `bool` | `true` | Write JSON data file to trigger the OBS overlay animation. Set to `false` for chat-only announcements |
| `AUTO_DRAW` | `bool` | `true` | Automatically draw a winner when the auto-close timer fires. Set to `false` to close entries only — streamer calls the draw manually |

---

## EarlyWormOverlay.html

The config block is a `<script>` tag at the very top of the file, before all CSS and HTML. All constants are clearly labelled.

---

### On-Screen Text

| Constant | Default | Description |
|----------|---------|-------------|
| `TEXT_TITLE` | `'Early Worm Draw'` | Main heading shown on the overlay |
| `TEXT_SUBTITLE` | `"Selecting tonight's winner"` | Smaller text below the title |
| `TEXT_IDLE` | `'Waiting for entries...'` | Text shown on the idle screen |
| `TEXT_WINNER_LABEL` | `'Early Worm Winner'` | Label shown above the winner's name |

---

### Background

| Constant | Default | Description |
|----------|---------|-------------|
| `BACKGROUND_COLOR` | `'transparent'` | Background colour of the overlay. Use `'transparent'` for OBS overlays or any CSS colour e.g. `'#111111'` |
| `BACKGROUND_OPACITY` | `1.0` | Opacity of the background only (0.0–1.0). Does not affect text or other content. Only applies when `BACKGROUND_COLOR` is not `'transparent'` |

---

### Show / Hide

| Constant | Default | Description |
|----------|---------|-------------|
| `AUTO_HIDE` | `true` | When `true` the overlay is fully invisible while idle and fades in only when a draw fires. When `false` the idle screen is always visible |

---

### Optional Visual Features

| Constant | Default | Description |
|----------|---------|-------------|
| `SHOW_BORDER` | `false` | Outer border around the overlay |
| `SHOW_CORNER_MARKS` | `false` | Small L-shaped marks at each corner |
| `SHOW_SCANLINES` | `false` | Retro CRT scanline effect |
| `SHOW_DIRT_BAR` | `false` | Decorative bar along the bottom edge |
| `SHOW_FOOTER_STATS` | `false` | Entry count and status text at the bottom |

---

### Worm Image

| Constant | Default | Description |
|----------|---------|-------------|
| `WORM_IMAGE` | `'worm.png'` | Filename of the PNG image to use for the wiggling worm. File must be in the same folder as the HTML. Falls back to the 🐛 emoji if not found |

---

### Drum

| Constant | Default | Description |
|----------|---------|-------------|
| `DRUM_BACKGROUND` | `'rgba(0,0,0,0.6)'` | Background colour of the scrolling name window. Use `'transparent'` to remove the background entirely |

---

### Winner Name

| Constant | Default | Description |
|----------|---------|-------------|
| `WINNER_BOX_COLOR` | `'rgba(0,0,0,0.75)'` | Semi-transparent box behind the winner name for readability. Set to `''` to disable |
| `WINNER_OUTLINE_COLOR` | `'rgba(0,0,0,0.9)'` | Outline/stroke around the winner name text. Set to `''` to disable |

---

### Size and Layout

| Constant | Default | Description |
|----------|---------|-------------|
| `OVERLAY_WIDTH` | `800` | Width of the overlay in px — should match OBS browser source width |
| `OVERLAY_HEIGHT` | `400` | Height of the overlay in px — should match OBS browser source height |
| `DRUM_WIDTH` | `520` | Width of the scrolling name window in px |
| `DRUM_HEIGHT` | `220` | Height of the scrolling name window in px |
| `HEADER_GAP` | `12` | Gap in px between the header text and the drum/winner area |

---

### Timing

| Constant | Default | Description |
|----------|---------|-------------|
| `SPIN_DURATION` | `7000` | Length of the scroll animation in milliseconds. **Keep this in sync with `CPH.Wait` in `EarlyWorm.cs`** |
| `WINNER_DISPLAY` | `15000` | How long the winner screen is shown before auto-hiding, in milliseconds |
| `POLL_INTERVAL` | `1500` | How often the overlay checks `EarlyWormData.json` for a new trigger, in milliseconds |

---

## Keeping Timings in Sync

The C# code waits for the overlay animation to finish before posting the winner to chat. If you change `SPIN_DURATION` in the overlay you must also update the `CPH.Wait` value in `DrawEarlyWormWinner` and `DrawMonthlyWinner` in `EarlyWorm.cs` to match.

A good rule of thumb is to set `CPH.Wait` to `SPIN_DURATION + 500` to give a small buffer after the animation finishes before the chat message fires.

For example if you change `SPIN_DURATION` to `10000` (10 seconds):
```csharp
CPH.Wait(10500); // SPIN_DURATION (10000) + 500ms buffer
```
