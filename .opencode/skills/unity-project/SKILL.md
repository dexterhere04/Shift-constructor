---
name: Unity Project
description: Orientation for the "Shift constructor" Unity 6.4 URP project — engine and package facts, folder layout, files that must never be edited, Unity CLI workflow, and the change → verify loop. Load this before making any change in this repository.
---

# Unity Project — Shift constructor

This is a **Unity 6000.4.10f1** project using **URP 17.4**, the **new Input System**, and **Unity Version Control (Plastic SCM)**. It is a 3D game built from the URP "Empty Template" plus rock prefabs.

Read `references/project-facts.md` for exact versions, GUIDs, scene contents, quality settings, and CLI details.

## Hard rules

- **Only touch `Assets/`, `Packages/manifest.json`, and (rarely) `ProjectSettings/`.** Never edit `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `obj/`, `.plastic/`, or the generated `*.csproj` / `*.slnx` files.
- **Every asset needs a matching `.meta` file.** If you create a file under `Assets/` by hand, create or copy a `.meta` with a unique 32-hex GUID, or let Unity generate it on the next import. Never delete a `.meta` while its asset stays. The CLI's `asset create` / `asset create_folder` commands write the `.meta` for you.
- Unity serializes assets as **text YAML** (`m_SerializationMode: 2`). Prefer `unity command` Pipeline commands (`scene`, `gameobject`, `component`, `prefab`, `asset`) or an `[InitializeOnLoad]`/menu Editor script. Hand-edit YAML only for small, local changes — and re-read the file afterwards to confirm nothing broke.
- The project uses **only the new Input System** (`activeInputHandler: 1`). Legacy `Input.GetKey` / `Input.GetAxis` throw at runtime. Load the `unity-input-system` skill.
- The project is managed by **Plastic SCM, not Git**. Do not run `git` commands or assume `git diff` works. The CLI's `unity vcs ...` commands need a git repository and fail here ("No git repository"), and `cm` is not on `PATH`. Use the Unity Version Control client in the Hub for history, and ask the user before any destructive version-control action.
- `.vscode/settings.json` hides most Unity file types from the explorer. They still exist; read them by path.
- Treat `Assets/TutorialInfo/` and `Assets/Readme.asset` as template content. Leave them alone unless the task is explicitly about them.

## Layout

| Path | Purpose |
| --- | --- |
| `Assets/Scenes/SampleScene.unity` | Only scene; the only entry in the build list |
| `Assets/Settings/` | URP assets: PC/Mobile pipeline assets, renderers, volume profiles |
| `Assets/materials/ground.mat` | Ground material |
| `Assets/Settings/rockMat.mat` | Rock material |
| `Assets/rockRow.prefab`, `Assets/smallRock.prefab` | Props used by SampleScene |
| `Assets/Scripts/PlayerController.cs` | CharacterController-based player movement (walk/sprint/jump) |
| `Assets/InputSystem_Actions.inputactions` | Project-wide input actions (`Player` and `UI` maps) |
| `Assets/TutorialInfo/` | URP template readme UI; do not modify |
| `Assets/Editor/` | Does not exist yet — create it for editor-only tooling |

There are **no assembly definition files**. Runtime scripts compile into `Assembly-CSharp`; scripts under any `Editor/` folder compile into `Assembly-CSharp-Editor`.

## Workflow

1. **Check the Editor.** `unity status` lists connected instances (port, PID, project, state) and costs nothing. Never start a second batch-mode run while an instance holds this project.
2. **Orient.** Read the files you will change before editing. For large scenes and prefabs, `grep` or `unity command scene get_hierarchy` instead of reading the whole file.
3. **Implement** with the matching skill:
   - C# gameplay/editor code → `unity-csharp`
   - Scene, prefab, component, or asset edits → `unity-pipeline` (`unity command scene|gameobject|component|prefab|asset`)
   - Rendering, shaders, lighting, post-processing → `urp-postprocessing`
   - Player input → `unity-input-system`
   - Compiling, testing, building, logs → `unity-cli`
4. **Verify.** A change is not done until the project compiles. With the Editor open, `unity recompile` recompiles and reports `error CS` lines; `unity command console get --level error` reads the console; `unity command test run` runs tests in place. Never claim success without a compile check, a test run, or explicit user confirmation.
5. **Report.** List changed and added files (including new `.meta` files) and exactly how the result was verified.

## Working with a live Editor

`com.unity.pipeline` (`0.10.0-exp.1`) is installed, so the CLI can drive the running Editor over port `7800`:

```powershell
unity status                              # is an Editor open, and where?
unity command scene get_hierarchy         # what is in the open scene
unity command component set_field ...     # change a serialized field
unity command capture scene_view          # PNG of the Scene view
unity command editor save_all             # save dirty scenes
```

- `unity command --query <term>` finds a Pipeline command; `unity commands --grep <term>` searches both CLI and Pipeline commands.
- Mutating commands return structured results; use `dry-run` where offered, and `confirm=true` for destructive ones. `job batch` runs several edits as one undoable, rolled-back-on-failure transaction.
- Tests can run in the open Editor with `unity command test run`; `unity test` and `unity build` instead spawn a batch-mode Editor and fail while this project is locked by the open one.

## Definition of done

- [ ] Project compiles with no `error CS` lines (`unity recompile`).
- [ ] Every new asset has a `.meta` file with a unique GUID.
- [ ] Scene/prefab references resolve (no `Missing` / `m_Script: {fileID: 0}`).
- [ ] The scene was saved/imported successfully if its YAML changed.
- [ ] The user knows what changed and how it was verified.
