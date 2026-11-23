# PLAN: Replace ReadyCanvas with the new ready-check panel

Goal: In `FantasyGameScene.unity`, the new panel under `GameInformation` replaces `ReadyCanvas`. Inside `ReadyContainer`, show one row for each player in the lobby. Use the same pattern as `SessionPlayerRow.prefab` in the main menu. Each row shows the player name and ready state.

## What I found
- `Assets/Matt Testing/Scripts/readyCheck.cs` (`NetworkBehaviour`) owns the old UI. Fields: `readyCanvas` (GameObject), `readyCheckImages[]`, `readyButton`. Only the server knows `readyClientIds`. `readyCheckImagesClientRpc(int index)` colours an image by ready order, not by player.
- Main-menu pattern: `Assets/Niki/Scripts/UI/MultiplayerLobbyPanel.cs`, `RefreshSession` (about lines 105-130). It keeps `Dictionary<string, LobbyPlayerRow> rows`, destroys rows for players who left, and calls `Instantiate(playerRowPrefab, playerContent)` for new players, then `Bind`. `LobbyPlayerRow.cs` binds an `ISession` + `IReadOnlyPlayer`. Name comes from `player.Properties[MultiplayerLobbyPanel.PlayerNameProperty]`. The row has `playerName`, `role` and `kickButton` fields.
- Scene: `ReadyContainer` (fileID 85223987) is under `GameInformation`. It has a RectTransform (parent 1268935279) and a uLayout `Layout` component (guid 47b91371...). It has no children. Old `ReadyCanvas` (1529033666) is under parent 2120494992. `readyCheck` references it through `readyCanvas: {fileID: 1529033666}` and `readyButton: {fileID: 642533976}` (scene lines about 494304-494310).
- `Assets/Niki/Scripts/UI/Editor/FantasySceneChecks.cs` already checks both Fantasy scenes (main + `Assets/Scenes/Juan/Juan_FantasyGameScene.unity`) in a preview scene. It has a "no missing scripts" check for the main scene. Its `Run()` returns a string and throws `InvalidOperationException` on failure.
- Unity version 6000.0.37f1. No AGENTS.md or CLAUDE.md exists in the repo.
- Working tree is dirty. `FantasyGameScene.unity` already has large uncommitted changes (+2206/-956), and so do `MainMenu.unity`, `Container_AbilityChoice.prefab` and many TMP font assets. New untracked: `PanelBackround.prefab`, `_---Divider---_.prefab`. **Do not touch, revert, or stage any of these.**

## Decisions (confirmed by the user)
1. Name source: each client sends its name to the server by RPC.
2. Ready button: `btn_Ready` under `ButtonPrompts` (existing). Task 4 assigns it to `readyCheck.readyButton`.
3. Old `ReadyCanvas`: deactivate first. Delete it only after the user approves.
4. Row look: a **prefab variant of `SessionPlayerRow.prefab`** (not a new prefab). Use a green check mark for ready. Hide Kick button and Host badge in the variant. Add `ReadyPlayerRow` to the variant instead of `LobbyPlayerRow`.
5. Juan scene: leave unchanged. Keep old serialized field names working in `readyCheck`.
6. Order: host first, then other clients by client id.

## Rules for every task
- Edit the scene only through a Unity Editor script (or the open editor), never by hand-editing YAML. The scene holds the user's uncommitted work.
- Do not run `git add`, `git commit`, `git checkout`, or `git stash`. Touch only the files listed per task. Do not edit `SessionPlayerRow.prefab` or `LobbyPlayerRow.cs`.
- Namespace `CupOHappiness.UI` for new UI scripts. Place new files under `Assets/Niki/` (scripts: `Assets/Niki/Scripts/UI/`, prefab: `Assets/Niki/Runtime/Prefabs/UI/Lobby/` next to the original).
- Test command base (set `UNITY` to the Unity 6000.0.37f1 editor exe; the project must NOT be open in another editor, or use the `unity` CLI from the unity-pipeline skill against the open editor):
  `"$UNITY" -batchmode -nographics -quit -projectPath "C:/Unity/Projects/INTERNSHIP/MonHunVroom" -executeMethod <Class.Method> -logFile -`
  Exit code 0 and no `InvalidOperationException` in the log = pass.

## Tasks

### Task 1: ReadyPlayerRow prefab variant + script
- Scope: One row component. The prefab is a variant of `SessionPlayerRow` (decision 4). It shows name and a green check mark when ready. Kick button and Host badge are hidden/removed in the variant.
- Files: new `Assets/Niki/Scripts/UI/ReadyPlayerRow.cs`; new `Assets/Niki/Runtime/Prefabs/UI/Lobby/ReadyPlayerRow.prefab` variant (+ .meta); new `Assets/Niki/Scripts/UI/Editor/ReadyPlayerRowChecks.cs`.
- API: `void Bind(string displayName, bool ready)`. Serialized fields: `TMP_Text playerName`, `GameObject readyMark` (green check).
- Dependencies: none.
- Acceptance: Variant opens with no missing scripts. All serialized fields assigned. `Bind("A", true)` shows name "A" and the check mark on; `Bind("A", false)` shows it off. Original `SessionPlayerRow.prefab` is unchanged (`git status` shows no change to it).
- Test: `ReadyPlayerRowChecks.Run()` loads the prefab, instantiates, binds, asserts. Run:
  `"$UNITY" -batchmode -nographics -quit -projectPath "C:/Unity/Projects/INTERNSHIP/MonHunVroom" -executeMethod CupOHappiness.UI.ReadyPlayerRowChecks.Run -logFile -`

### Task 2: Ready roster state in readyCheck
- Scope: The server keeps who is in the lobby, their names, and who is ready. Clients can read it. Replace the index-based `readyCheckImagesClientRpc(int)` flow.
- Files: `Assets/Matt Testing/Scripts/readyCheck.cs` only (plus a small struct in the same file or a new `ReadyEntry.cs`).
- Design: `NetworkList<ReadyEntry>` (`clientId`, `FixedString64Bytes name`, `bool ready`), server-write, created in field init/Awake. Each client sends its name once via `[ServerRpc(RequireOwnership=false)] RegisterNameServerRpc(FixedString64Bytes)` in `OnNetworkSpawn`. The server also adds entries for connected clients without a name ("Player"). `ReadyPressedServerRpc` sets `ready=true` for the sender. Remove entries on client disconnect. Order: host first, then by client id.
- Keep as-is: `readyClientIds` completion check, `ShowUpgradeChoices()` call, `SpawnPlayersAfterChoices`, jump-key ready input, old serialized field names (decision 5).
- Dependencies: none (Task 1 not required).
- Acceptance: Host and client both see the same list of N entries. Pressing ready flips only that player's `ready`. When all are ready, the old behaviour still fires (upgrade choices show). No change to spawn logic.
- Test: compile check:
  `"$UNITY" -batchmode -nographics -quit -projectPath "C:/Unity/Projects/INTERNSHIP/MonHunVroom" -logFile -` (no `error CS` lines). Manual: Multiplayer Play Mode, 2 players, press ready on each, check list contents. This cannot be fully automated.

### Task 3: ReadyRoster UI binder
- Scope: A component for `ReadyContainer`. It mirrors `MultiplayerLobbyPanel.RefreshSession`: `Dictionary<ulong, ReadyPlayerRow> rows`, destroy rows for removed entries, `Instantiate(rowPrefab, container)` for new ones, `Bind` all. Subscribes to `NetworkList.OnListChanged` and refreshes once on enable. Has a pure refresh method that takes `IReadOnlyList<ReadyEntry>`.
- Files: new `Assets/Niki/Scripts/UI/ReadyRoster.cs`; new `Assets/Niki/Scripts/UI/Editor/ReadyRosterChecks.cs`; small public accessor on `readyCheck` for the list (edit `readyCheck.cs`).
- Dependencies: Tasks 1, 2.
- Acceptance: Zero children at start. After list change, child count == entries. A ready flag change updates only that row. No duplicate rows after repeated refresh. Unsubscribes on destroy.
- Test: `ReadyRosterChecks.Run()` with a fake entry list: counts 0 -> 3 -> 2, no duplicates. Run `-executeMethod CupOHappiness.UI.ReadyRosterChecks.Run` (same base command).

### Task 4: Scene wiring (editor script, not YAML)
- Scope: In `FantasyGameScene.unity` only: add `ReadyRoster` to `ReadyContainer` and assign row prefab + container. Point `readyCheck` at the new panel root and `btn_Ready` (under `ButtonPrompts`). Remove the old `ReadyCanvas` references. Deactivate `ReadyCanvas`; do not delete it (decision 3).
- Files: new one-shot `Assets/Niki/Scripts/UI/Editor/ReadySceneWiring.cs` (delete after use); `Assets/Scenes/GAME SCENES/FantasyGameScene.unity` (modified by the script); extend `FantasySceneChecks.cs`.
- Dependencies: Tasks 1-3.
- Acceptance: `readyCheck` has no unassigned required refs. The new panel shows when the game scene starts and hides when everyone is ready (server and clients). The old canvas is not shown. The scene diff beyond the pre-existing diff only has changes for this feature.
- Test: before the task save `git diff -- "Assets/Scenes/GAME SCENES/FantasyGameScene.unity" > scene_before.diff` outside the repo. Extend `FantasySceneChecks.cs` (preview scene): `readyCheck` refs not null, `ReadyContainer` has `ReadyRoster`, no missing scripts. Run:
  `"$UNITY" -batchmode -nographics -quit -projectPath "C:/Unity/Projects/INTERNSHIP/MonHunVroom" -executeMethod CupOHappiness.UI.FantasySceneChecks.Run -logFile -`

### Task 5: End-to-end check and cleanup
- Scope: Run all checks. Play host + 1 client. Confirm rows appear for both, ready flips per player, panel closes when all are ready, upgrade choices appear. Delete one-shot wiring script.
- Files: remove `ReadySceneWiring.cs` (+ .meta).
- Dependencies: Task 4.
- Acceptance: all `*Checks.Run` pass; no console errors in play; `git status` shows no unrelated new changes from this work.
- Test: run each of `FantasySceneChecks.Run`, `ReadyPlayerRowChecks.Run`, `ReadyRosterChecks.Run`, `MultiplayerLobbyPanelChecks.Run` (all in `CupOHappiness.UI`) with the base command; then `git status --short`.

## Risks
1. **Dirty scene.** The scene already has big uncommitted edits. A wrong save could mix or lose them. Mitigation: editor-script only, capture the diff before/after, no git write commands.
2. **Unity project lock.** Batchmode fails if the editor has the project open. Mitigation: close the editor, or use the `unity` CLI against the live editor.
3. **Late join / disconnect.** The list must handle a client leaving during the ready check, or the "all ready" count can hang. Existing code compares to `ConnectedClientsIds.Count`; keep that.
4. **Juan scene.** `Juan_FantasyGameScene.unity` uses the same `readyCheck` script. Keep old serialized field names so its references stay valid.
5. **Netcode serialization.** `NetworkList` needs `INetworkSerializable`/`IEquatable` on the struct, and must be created in `Awake`/field init.
6. **uLayout.** `ReadyContainer` uses uLayout `Layout`. Instantiated rows may need sizing by it; verify spacing in play.
7. **Prefab variant.** Variant overrides must not change the original `SessionPlayerRow.prefab`. Its `LobbyPlayerRow` component cannot be removed in a variant; the variant must disable or keep it harmlessly (the Kick button is hidden by the variant, and `LobbyPlayerRow.Bind` is never called on it). Worker must pick the safest option and report it.

## Unresolved decisions
None open. Delete of old `ReadyCanvas` waits for later user approval.

## Worker flow
One task at a time through the omni-worker skill (owner id 2e16eda1626249b2b3a2cb2da0dbc987). This session reviews each result against its acceptance criteria and runs its test command before the next task starts.

## Progress notes
- Task 1: DONE. Worker `e186c738f0f0` wrote `ReadyPlayerRow.cs` and `ReadyPlayerRowChecks.cs`. The worker has NO Unity tools (build workers run with `--no-extensions`, so `codemode` and the AnkleBreaker tools do not load). The planner created the prefab variant in the open editor. `ReadyPlayerRowChecks.Run` passed. Compile errors: 0. `SessionPlayerRow.prefab` unchanged.
- Test commands: the editor is open, so batchmode fails on the project lock. Run checks through the AnkleBreaker `unity_execute_code` tool, for example `return CupOHappiness.UI.ReadyPlayerRowChecks.Run();`.
- Worker split for Tasks 2-3: the worker writes C# files only; the planner does every Unity editor step (prefab, scene wiring, running checks).
- `MultiplayerLobbyPanelChecks` is in `CupOHappiness.UI.Editor` and has no `Run()` (only `Capture`), so it is not a regression gate.
- Unexpected: `btn_UI_Display.prefab` shows as modified; it is not part of this plan.

- Task 2: DONE (planner). `readyCheck.cs` has `ReadyEntry` + `NetworkList<ReadyEntry> roster`, name RPC from `PlayerPrefs "Multiplayer.PlayerName"`, ready flag per client, add/remove on connect/disconnect. Old fields and index-based image RPC kept for the Juan scene. Compiles clean.
- Task 3: DONE (planner). `ReadyRoster.cs` + `ReadyRosterChecks.Run` passed (0 -> 3 -> 2 rows, host first, no duplicates, per-row ready flag).
- Task 4: DONE (planner). Scene wired in the open editor and saved: `ReadyRoster` on `ReadyContainer`; `readyCheck.readyCanvas` -> `GameInformation`; `readyCheck.readyButton` -> `btn_Ready`; old `ReadyCanvas` deactivated, not deleted. `FantasySceneChecks.Run` extended and passed.
- Task 5: PARTLY DONE. All editor checks pass. NOT done: a host + client play test (needs Multiplayer Play Mode) and deleting the old `ReadyCanvas`.

