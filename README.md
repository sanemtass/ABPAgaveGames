# Art Block Puzzle — Agave Games Technical Case

Swap-based picture puzzle. Unity 6000.0.72f1, C#, built-in renderer. Portrait, targeting 9:19.5 (iPhone X class).

## How to Run

- Open with Unity 6000.0.72f1
- Open `Assets/Scenes/SampleScene.unity`, press Play
- Device Simulator with iPhone X recommended

## Gameplay

Swap the scattered pieces back into their correct positions within the move limit. Two adjacent pieces in correct relative positions connect and their shared border disappears. Connected groups move and swap as one unit. Complete the picture to win; run out of moves to lose.

The puzzle area keeps a fixed 9:16 aspect ratio, centered on screen, on all devices.

## Levels

Two levels, one per provided puzzle image. Win shows Play Next (loads next level), lose shows Try Again (restarts). After the last level the end card shows "All Levels Complete!" with no button — at that point the game is finished, as described in the case. The level flow is my interpretation, based on the two provided images, the level_number parameter and the Play Next wording.

To add a level: Create → Agave → Level Data, set the parameters, add it to the LevelSequence asset. The source image must have Read/Write Enabled in its import settings (required by runtime slicing with Sprite.Create).

## Architecture

- Game logic is plain C# (`BoardState`, `ConnectionEvaluator`, `GroupSwapService`, `MoveCounter`, `TutorialSwapFinder`), created and wired by `PuzzleBoard`. MonoBehaviours stay thin.
- UI and audio listen to gameplay events; gameplay code doesn't reference them.
- `LevelEndResult` and its subclasses (won / lost / finished) handle end card content; adding an outcome means adding a class, not editing existing code.
- Connections are not stored as state. They are derived from correct-position relationships each time, so the partial-overlap rule (displaced pieces losing their connections) works without special handling.

## Performance

- No Update loops; everything is event-driven.
- `GridCoordinate` implements `IEquatable<T>` to avoid boxing in HashSet/List operations.
- BFS buffers are allocated once and reused.
- Tweens use SetLink for cleanup; snap tweens skip pieces already in place.
- One reusable ParticleSystem instance for the connection burst. Canvas runs in Screen Space – Camera so particles can sort in front of the UI.
- Runtime-created sprites are destroyed on scene unload.
- targetFrameRate 60, vSync off, set at startup.

## Assets

- button.png ships as a vertical half (size optimization for symmetric art). Completed at runtime with RawImage UV Rect (W=2) + Mirror wrap, rotated 90° via RectTransform to use as a wide button — that's why ActionButton is rotated in the hierarchy.
- Background music: "Cozy Place" by Serjo De Lua (chosic.com), trimmed to 50s.
- Pickup/place sounds are synthesized; lose sound from a free SFX source.

## Known Trade-offs

- Board margins are fixed canvas units tuned for 9:19–9:21 screens. On squarer screens (iPhone SE) the board is slightly smaller; the case's target class is unaffected.