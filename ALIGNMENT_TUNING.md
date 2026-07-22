# Aligning the monitor with the computer bezel

After a Gorilla Tag update the custom Computer Interface screen can appear shifted, tilted, or
floating outside the physical computer frame, because the screen's position/rotation were
calibrated for an older computer model.

You can re-align it **without recompiling** using two config values.

## Steps

1. Launch Gorilla Tag once with this build of Computer Interface so the config file is generated.
2. Open this file in a text editor:
   ```
   <Gorilla Tag>\BepInEx\config\tonimacaroni.computerinterface.cfg
   ```
   (On Steam, `<Gorilla Tag>` is usually
   `C:\Program Files (x86)\Steam\steamapps\common\Gorilla Tag`.)
3. Find the `[Appearance]` section and edit these two lines:
   ```ini
   [Appearance]

   MonitorPositionOffset = 0, 0, 0
   MonitorRotationOffset = 0, 0, 0
   ```
   The three numbers are **X, Y, Z**.
   - `MonitorPositionOffset` — slides the screen. Units are world meters, so small values matter
     (try steps of `0.05` to `0.1`).
   - `MonitorRotationOffset` — rotates the screen, in **degrees**.
4. Save the file and **restart Gorilla Tag** (BepInEx reads the config at launch).
5. Repeat until the screen sits inside the bezel.

## Which value to change

| The screen is… | Adjust |
|---|---|
| Too far left/right | `MonitorPositionOffset` **X** |
| Too high/low | `MonitorPositionOffset` **Y** |
| Too far forward/back (into/out of the frame) | `MonitorPositionOffset` **Z** |
| Tilted / sideways | `MonitorRotationOffset` (usually **Z** for roll, **X** for pitch) |

> Sign matters: if it moves the wrong way, flip the sign (e.g. `0.1` → `-0.1`).
>
> If the screen is wildly off (e.g. at the world origin, not near the computer at all), your build
> likely merged the in-game monitor mesh — Computer Interface logged a warning and is now parenting
> the screen to the computer itself. Use the offsets to bring it into the bezel.

## Note

The "correct" values are specific to your Gorilla Tag build's computer model, which is why they are
configurable instead of hard-coded. If you find values that work well, jot them down — a future GT
update may shift the model again.
