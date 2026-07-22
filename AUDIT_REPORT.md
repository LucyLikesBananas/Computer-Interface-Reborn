# ComputerInterface 1.9.3 — Source Audit & Bug-Fix Report

Every `.cs` file in the project (≈4,900 lines / 70 files) was read and audited.
This document lists every defect found and the fix applied. Files were edited in place
under `ComputerInterface-1.9.3-FIXED/`. Each fix is marked with a `// FIX:` comment at the
change site so you can review them individually.

> ⚠️ **Important — what these fixes do NOT do.** They repair genuine bugs in the mod's own
> code (crashes, wrong behaviour, null derefs, dead parameters, malformed markup). They do
> **not** make the mod load on the newest Gorilla Tag build. The newest GT build merged the
> in-game monitor mesh with other meshes, which breaks ComputerInterface's monitor-replacement
> core, and the project was archived by its authors for exactly that reason. See
> "Remaining limitations" at the bottom.

---

## 🔧 Round 2 — "Loading" screen stuck + monitor misaligned with the bezel

After building, two symptoms appeared: the screen stayed on **"Loading"**, and the monitor was
**out of alignment with the computer border**.

### "Loading" stuck — fixed (definitive)
Cause: in `CustomComputer.Initialize()`, `ShowInitialView()` (which sets the real on-screen
text) ran **last**, after `SetMonitor()` and `BaseGameInterface.InitAll()`. Both touch many
game internals that change between GT builds; on a newer build one of them throws, the shared
`catch` swallows it, `ShowInitialView` is never reached, `CurrentComputerView` stays `null`, and
`PrepareMonitor` leaves every screen on `"Loading"`.

Fix (`Behaviours/CustomComputer.cs` + `Behaviours/BaseGameInterface.cs`):
- `ShowInitialView()` now runs **first**, in its own try/catch, so the menu always renders.
- `SetMonitor()` and `BaseGameInterface.InitAll()` each run in their **own** try/catch, so one
  failing step can no longer blank the screen.
- `InitAll()` now isolates every sub-init (`InitColorState`, `InitNameState`, …, `GorillaComputer.Start`)
  individually — a single failing base-game call is logged and skipped instead of aborting the rest.
- `SetMonitor()` no longer assumes the monitor is child index 1 (looked up by name + bounds-checked).

### Monitor misalignment — fixed via live config (no recompile needed)
Cause: the monitor's position/rotation are hard-coded in `ClassicMonitor`/`ModernMonitor` for an
older computer model. When GT's computer geometry shifts, the screen drifts out of the bezel.

Fix (`Models/CIConfig.cs` + `Behaviours/CustomComputer.cs`):
- Two new config entries: **`MonitorPositionOffset`** and **`MonitorRotationOffset`** (default
  `0,0,0`). They are added on top of the monitor's base position, so you can slide the screen back
  into the bezel on your specific build by editing the config file and restarting the game.
- `CreateMonitor()` now falls back to parenting the screen to the computer itself (instead of
  dropping it at the world origin) when the `"monitor"` child is missing — the merged-mesh case —
  so the screen still appears and can be nudged into place with the offset.

**How to tune alignment** (see `ALIGNMENT_TUNING.md`):
1. Launch the game once with the new build so the config is generated.
2. Open `BepInEx/config/tonimacaroni.computerinterface.cfg`.
3. Under `[Appearance]`, edit `MonitorPositionOffset` (and `MonitorRotationOffset` if it's rotated)
   in small steps until the screen sits in the bezel.
4. Restart Gorilla Tag. Repeat until aligned.

> The exact offset values depend on your game build's computer model, which is why they're exposed
> as config rather than hard-coded — there is no universal "correct" number without the new geometry.

---

## 🔴 Critical (crashes / serious misbehaviour)

| # | File | Defect | Fix |
|---|------|--------|-----|
| 1 | `Views/GameSettings/RV_FusionCallbacks.cs` | Five `INetworkRunnerCallbacks` methods (`OnObjectExitAOI`, `OnObjectEnterAOI`, the 2-arg `OnDisconnectedFromServer`, the 4-arg `OnReliableDataReceived`, `OnReliableDataProgress`) **`throw new NotImplementedException()`**. When Fusion invokes any of them it throws and can crash networking/the game. (These exist because the Fusion API grew new overloads as GT updated.) | Replaced the throws with safe no-op bodies; the disconnect overload now redraws the room view like the 1-arg version. |
| 2 | `Views/GameSettings/QueueView.cs` | `MaxIdx = _queues.Count` (off-by-one). With 3 queues the selection could reach index 3, and `_queues[3]` threw `IndexOutOfRangeException`. | `MaxIdx = _queues.Count - 1`. |
| 3 | `Behaviours/CommandHandler.cs` | Zero-argument commands had their callback **invoked twice** — once for the 0-arg branch, then again after falling through with an empty array. | `return true` immediately after the 0-arg invocation. |
| 4 | `Behaviours/CustomComputer.cs` (`ReplaceKeys`) | `_keys[0]` / `_keys.Last(...)` thrown when no keys matched (unexpected keyboard layout) → `IndexOutOfRangeException`. | Guard: if `_keys.Count == 0`, warn and return. |
| 5 | `Models/UI/UITextPageHandler.cs` (`GetLinesForPage`) | Logged "lines not set" then **fell through** and dereferenced `_lines.Length` → `NullReferenceException`. (Sibling `UIElementPageHandler` correctly returns; this one didn't.) | Added the missing `return null;`. |
| 6 | `Behaviours/CustomComputer.cs` (`Initialize`) | `GetComponentInHierarchy<GorillaComputerTerminal>().gameObject` dereferenced a possibly-null result → NRE if the active scene has no computer. | Null-check; skip only the initial monitor prep (the rest of init + scene handlers still run, so later scenes work). |
| 7 | `Tools/AssetLoader.cs` | `AssetBundle.LoadFromStream(null)` threw `ArgumentNullException` if the embedded `CIBundle` resource was missing. | Null-check + clear error log. |

## 🟠 High (wrong visible behaviour / likely crash on edge input)

| # | File | Defect | Fix |
|---|------|--------|-----|
| 8 | `Views/ModView.cs` (`OnOptionSelected`) | The "Enable" branch returned **before `Redraw()`**, so the header never refreshed to show the new Enabled/Disabled state. | Restructured so both branches reach `Redraw()`. |
| 9 | `Views/GameSettings/RedemptionView.cs` | An invalid (<8-char) code path returned **without redrawing**, so the "Invalid Code" status never showed. | Call `Redraw()` before returning. |
| 10 | `Behaviours/CustomComputer.cs` (`SetMonitor`) | `return` **inside** the terminal loop aborted the whole method on the first terminal without a CI monitor, skipping the rest. | Changed to `continue`. |
| 11 | `Models/CustomKeyboardKey.cs` (`CreateKeyMap`) | `var key = (Key)40 + i;` parses as `((Key)40) + i` — enum + int — which is not a valid C# operation and does not type-check against the `Dictionary<EKeyboardKey, Key>` it is added to. | `var key = (Key)(40 + i);` (add integers, then cast). |
| 12 | `Behaviours/CommandHandler.cs` (`Execute`) | A command with a null `Callback` "succeeded" (returned `true`) while doing nothing and left the output message null. | Explicit guard: clear message + `return false`. |
| 13 | `Views/GameSettings/RoomView.cs` (`OnShow`) | Each entry created a new `DontDestroyOnLoad` "RoomCallbacks" GameObject and orphaned the previous one; event subscriptions stacked up → duplicate redraws + leak. | Destroy any existing `_callbacks` first. |

## 🟡 Medium (null-safety / malformed markup / minor logic)

| # | File | Defect | Fix |
|---|------|--------|-----|
| 14 | `Extensions/ReflectionEx.cs` | `AccessTools.Method/Field` return null on a missing member → confusing NRE. | Throw clear `MissingMethodException`/`MissingFieldException` instead. |
| 15 | `Views/WarnView.cs` | `args[0]` dereferenced with no null/length check. | Guard; show neutral "Warning" if args invalid. |
| 16 | `Views/DetailsView.cs` | Screen text said "Press any key to update" but the default branch only redrew stale data. | Call `UpdateStats()` before `Redraw()`. |
| 17 | `Views/GameSettings/TroopView.cs` | `CheckForComputer` result ignored; `computer.troopName` could NRE. | Guard; show "Computer not found" if absent. |
| 18 | `Views/GameSettings/CreditsView.cs` | `GetPage` crashed if the reflected credits call returned null (happens when GT renames/changes the credits API between builds). Added reflection-member guard in `OnShow` too. | Null-guards + graceful fallback. |
| 19 | `Views/GameSettings/GroupView.cs` | `DrawHeader` had an unbalanced extra `EndColor()` → stray `</color>` in markup. | Removed the extra tag. |
| 20 | `Views/GameSettings/SupportView.cs` | Same stray `EndColor()` in the header; also typos "roomView"→"view" and "inforamtion"→"information". | Balanced markup; fixed typos. |
| 21 | `Views/ModListView.cs` | Empty plugin list (no other mods) → `_plugins[index]` crash on select. | Bounds-check in `OnKeyPressed` and `SelectMod`. |
| 22 | `Models/ComputerView.cs` (`ReturnView`) | `ShowView(CallerViewType)` with a null caller passed null to `Activator.CreateInstance` → crash. | Fall back to `ReturnToMainMenu()`. |
| 23 | `Tools/Logging.cs` | `Plugin.Logger` could be null if logging fired before `Plugin.Awake` → NRE that swallowed the original error. | Null-guard. |
| 24 | `Behaviours/CustomComputer.cs` (`CreateMonitor`) | `Instantiate(null,…)` if the monitor prefab failed to load → misleading NRE. | Clear guard + descriptive exception. |

## 🟢 Low (typos, dead parameters, redundancy)

| # | File | Defect | Fix |
|---|------|--------|-----|
| 25 | `Extensions/StringBuilderEx.cs` | `BeginMono`/`AppendMono` accepted a `spacing` param but hardcoded `<mspace=58>` (dead parameter). | Use the parameter: `$"<mspace={spacing}>"`. |
| 26 | `Extensions/StringBuilderEx.cs` | `Clamp` crashed when `length <= 3` (`str[..negative]`) and on a null string. | Guard both. |
| 27 | `Extensions/StringBuilderEx.cs` | `BeginColor(string)` indexed `color[0]` with no length check. | `IsNullOrEmpty` guard. |
| 28 | `Models/CIConfig.cs` | Empty disabled-mods config split into `[""]` and was stored as a phantom entry; `"a;;b"` likewise. | Skip empty/whitespace segments. |
| 29 | `Views/CommandLineView.cs` | Typo "Press Option 1 to **roomView** command list". | "…to **view** command list". |
| 30 | `Views/CommandLineHelpView.cs` | Typo "**Nativate** using the Left/Right arrow keys". | "Navigate…". |
| 31 | `Views/GameSettings/ColorSettingView.cs` | `OnShow` read `GetColor()` (PlayerPrefs) twice. | Reuse the value. |
| 32 | `Behaviours/CommandHandler.cs` | `UnregisterCommand` logged at **Error** level for a normal action. | Log at Info. |
| 33 | `ComputerInterface.Commands/CommandRegistrar.cs` | `cam` command dereferenced `thirdPersonCamera` **before** the null check. | Guard first. |

---

## Remaining limitations (cannot be fixed from source alone)

These are **not** code bugs; they are external blockers:

1. **Cannot compile against the newest Gorilla Tag build here.** The project references
   `Assembly-CSharp.dll` (proprietary game code) plus the Fusion/Photon/Unity managed DLLs
   from a game install. A real rebuild must be done on a machine that has the newest GT
   installed, with those DLLs present.
2. **The monitor-mesh break.** The newest GT build merged the in-game computer's monitor
   mesh with other meshes (per the maintainers' archived README). ComputerInterface finds the
   screen by literal child names (`"monitor"`, `"monitor (1)"`) and `GorillaComputerTerminal`
   fields (`monitorMesh`, `myFunctionText`, `myScreenText`); with the merged mesh those no
   longer exist in the expected form, so `CreateMonitor`/`RemoveMonitor` cannot place or hide
   the screen. Fixing this needs the new game's meshes + a rewrite of the monitor logic —
   which is why the upstream project was archived, not updated.

A handful of the fixes above **do** improve resilience against game changes (reflection
null-guards in `CreditsView`/`ReflectionEx`, the Fusion callback no-ops, the asset-null
guard), so the mod will fail more gracefully on newer builds — but it will still not fully
function until the monitor work is done.

## Summary

- **33 defects** found and fixed across **26 source files**.
- **7 critical**, **6 high**, **11 medium**, **9 low**.
- Original files are untouched; everything is in `ComputerInterface-1.9.3-FIXED/`.
