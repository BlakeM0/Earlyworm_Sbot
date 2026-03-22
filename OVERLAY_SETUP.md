# OBS Overlay Setup Guide

This guide covers adding the Early Worm animated overlay to OBS Studio.

---

## File Placement

Both the overlay HTML file and the data JSON file must be in the same folder. Place them inside the `Early_worm` folder in your Streamer.bot directory:

```
Streamer.bot/
└── Early_worm/
    └── Overlay/
        ├── EarlyWormOverlay.html
        ├── EarlyWormData.json
        └── worm.png               — optional, see Worm Image below
```

> **Important:** The `EarlyWormOverlay.html` file polls for `EarlyWormData.json` in the same directory as itself. Both files must always be in the same folder.

---

## Adding the Browser Source in OBS

1. In OBS open the scene you want to add the overlay to
2. Click the **+** button in the Sources panel
3. Select **Browser**
4. Give it a name e.g. `Early Worm Overlay` and click OK
5. In the browser source settings:
   - Check **Local file**
   - Click **Browse** and navigate to `EarlyWormOverlay.html`
   - Set **Width** to match `OVERLAY_WIDTH` in the config (default `800`)
   - Set **Height** to match `OVERLAY_HEIGHT` in the config (default `400`)
   - Uncheck **Shutdown source when not visible**
   - Uncheck **Refresh browser when scene becomes active**
6. Click OK

> **Note:** Keeping **Refresh browser when scene becomes active** unchecked is important — refreshing the page mid-draw would interrupt the animation.

---

## Positioning the Overlay

- The overlay background is transparent by default so it sits cleanly over your stream layout
- Drag and resize the source in OBS to position it wherever suits your layout
- For a 1080p canvas with a facecam, a width of around 500px works well in a corner or along one side

---

## Worm Image

The overlay displays a worm image that wiggles on the idle screen and winner banner. To use your own image:

1. Place a PNG file in the same `Overlay` folder as the HTML file
2. Open `EarlyWormOverlay.html` in a text editor
3. At the top of the file find the config section and update:
```javascript
const WORM_IMAGE = 'worm.png'; // change to your filename
```

If the file is not found the overlay automatically falls back to the 🐛 emoji — nothing will break if the image is missing.

---

## Testing the Overlay

Open `EarlyWormOverlay.html` directly in a web browser to test it without OBS or Streamer.bot:

- Press **D** to run a demo spin with test entries
- Press **R** to reset back to the idle state

---

## Auto Show / Hide

When `AUTO_HIDE = true` in the overlay config the overlay is completely invisible while idle and only appears when a draw is triggered. This means you can leave the browser source active in OBS at all times without it showing on stream between draws.

When `AUTO_HIDE = false` the idle screen (worm image and waiting text) is always visible.

---

## Troubleshooting

**Overlay shows but doesn't animate when a draw fires**
- Make sure `EarlyWormData.json` is in the same folder as `EarlyWormOverlay.html`
- Check that Streamer.bot has write permission to that folder
- Open the browser source properties in OBS and click **Refresh** once to reload the page

**Overlay re-fires an old draw after OBS refreshes**
- Make sure **Refresh browser when scene becomes active** is unchecked in the browser source settings
- The overlay also has a built-in stale data guard that ignores any trigger written before the page loaded

**Winner name disappears too quickly**
- Increase `WINNER_DISPLAY` in the overlay config (value is in milliseconds)
- Make sure `CPH.Wait` in `EarlyWorm.cs` is set to approximately the same value as `SPIN_DURATION` in the overlay
