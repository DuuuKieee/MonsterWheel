# Monster Wheel

A 720x1280 Unity mini-game: a ring of 18 monster slots lights up clockwise around a center reel.

## Controls

| Press Space | What happens |
|---|---|
| 1st | Reel spins, ring light chases clockwise |
| 2nd | Reel decelerates onto a random monster, then the ring light decelerates onto a slot with the **same** monster (next match clockwise, after at least one more lap) |
| 3rd | New round |

Space is ignored while the wheel is stopping.

## Run

- Unity **6000.3.10f1**. Open `Assets/MonsterWheel/Scenes/MonsterWheel.unity` and press Play.
- Set the Game View to a 720x1280 (9:16) resolution. Any other aspect still shows the whole board (CanvasScaler *Expand*).
- Build: *File > Build Profiles > Windows*, the scene is already in the build list. The player opens a 720x1280 window (capped by the monitor's height).

## Project layout

```
Assets/MonsterWheel/
  Scripts/Core/     Pure C# rules + motion (no UnityEngine dependency)
    WheelEngine         state + ring/reel positions, Press(), Tick(dt)
    WheelStateMachine   Idle → Spinning → StoppingReel → StoppingRing → Idle
    StopPlanner         which ring slot to stop on
    DecelerationMotion  frame-rate independent ease-out that lands exactly on target
    RingLayout          slot i shows icon i mod 9 (each icon twice, point-symmetric)
  Scripts/View/     uGUI rendering + input (MonsterWheelController, RingView, ReelView, SlotView, WheelConfig)
  Scripts/Editor/   MonsterWheelSceneBuilder (menu: Tools > Monster Wheel > Build Scene)
  Tests/EditMode/   NUnit tests for Core
  Config/WheelConfig.asset   tweak speeds, glow, colors here
```

Stops start from the current spin speed (duration = curve slope × distance / speed), so there is no speed jump when stopping.

## Tests

*Window > General > Test Runner > EditMode > Run All* (29 tests), including a 1000-round simulation with random frame times asserting that ring and reel always stop on the same monster.

## Rebuild the scene

*Tools > Monster Wheel > Build Scene* regenerates the scene, config asset, halo texture, sprite import settings and player resolution from `Assets/assets`.

## Docs

- Design: `docs/superpowers/specs/2026-10-20-monster-wheel-design.md`
- Plan: `docs/superpowers/plans/2026-10-20-monster-wheel.md`
