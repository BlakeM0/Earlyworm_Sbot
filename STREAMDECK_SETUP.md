# Stream Deck Setup Guide

This guide explains how to configure Elgato Stream Deck buttons to trigger Early Worm actions using the Streamer.bot plugin.

---

## Prerequisites

- Elgato Stream Deck software installed
- Streamer.bot running with the Early Worm actions set up (see [STREAMERBOT_SETUP.md](STREAMERBOT_SETUP.md))
- Streamer.bot Stream Deck plugin installed (see below)

---

## Installing the Streamer.bot Plugin

1. Open the Stream Deck software
2. Click the **grid/store icon** in the bottom right corner
3. Search for **Streamer.bot**
4. Click **Install**

---

## Connecting the Plugin to Streamer.bot

This only needs to be done once.

1. In Streamer.bot go to **Servers/Clients → HTTP Server**
2. Make sure the HTTP server is **enabled** and note the port number (default is `8080`)
3. In Stream Deck software, drag any **Streamer.bot** action from the right panel onto a button slot
4. Click the button to open its settings on the right
5. Under **Host** enter `127.0.0.1`
6. Set **Port** to match Streamer.bot (default `8080`)
7. Click **Connect** — the status should show as connected

---

## Setting Up a Button

1. Drag a **Streamer.bot** action from the right panel onto an empty button slot
2. In the button settings:
   - **Action** — select your `EarlyWorm` action from the dropdown
   - **Run Immediately** — leave this checked
3. Scroll down to the **Arguments** section and click **Add Argument**
4. Set the argument **name** to `input0`
5. Set the argument **value** to the sub-command for this button (see table below)
6. Optionally set a **Title** on the button so it is easy to identify on the deck

---

## Button Reference

| Button title | `input0` value | What it does |
|--------------|---------------|--------------|
| 🐛 Open | `start` | Opens entries |
| 🐛 Close | `stop` | Closes entries |
| 🐛 Draw | `draw` | Draws a winner |
| 🐛 Reset | `reset` | Resets entries without drawing |
| 🐛 Auto | `auto` | Opens entries and starts the 20 minute auto-close timer |

Add as many or as few buttons as the streamer needs — each one follows the same setup process with a different `input0` value.

---

## How It Works

The Stream Deck plugin triggers the `EarlyWorm` action in Streamer.bot and passes `input0` as an argument. The switch statement in the C# code reads `input0` and routes to the correct method — exactly the same as if the command had been typed in chat. No code changes are needed.

---

## Troubleshooting

**Button doesn't trigger anything**
- Check the plugin shows as connected in the button settings
- Make sure Streamer.bot is running and the HTTP server is enabled

**Wrong action fires**
- Double check the `input0` value matches exactly (lowercase, no spaces)

**Action fires but nothing happens in chat**
- Check Streamer.bot's action log to confirm the action ran
- Make sure the C# code has been saved and compiled without errors
