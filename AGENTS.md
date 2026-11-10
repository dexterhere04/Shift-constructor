# Agent guide — Shift constructor

Unity **6000.4.10f1** · **URP 17.4** · **new Input System only** · **Plastic SCM** (not Git).

## Start here

Load the **`unity-project`** skill before your first change. It contains the project map, hard rules, and the change → verify loop. Then load the skill that matches the task:

| Skill | Where | Load it for |
| --- | --- | --- |
| `unity-project` | project | Orientation, layout, safe/unsafe paths, definition of done |
| `unity-csharp` | project | Gameplay or editor C#, Unity 6 API changes, serialization, tests |
| `unity-input-system` | project | Player input, controls, UI navigation, rebinding |
| `unity-cli` | global | Any `unity` command: status, build, test, compile check, logs, Editor/project management |
| `unity-pipeline` | global | Driving a running Editor: scenes, GameObjects, components, prefabs, screenshots, Play Mode |
| `urp-postprocessing` | global | Bloom, tonemapping, color adjustments, depth of field, vignette, Volume overrides |
| `ui` (→ `ui-ugui` / `ui-uitk` / `ui-imgui`) | global | Any menu, HUD, panel, or editor UI work |
| `physics-3d-collision` | global | Colliders, triggers, raycasts, collision that does not fire |
| `localization` | global | Locales, String/Asset Tables, CJK fonts |
| `optimize-audio`, `optimize-web`, `optimize-text-mesh-pro` | global | Performance and memory work in those areas |
| `unity-package-management` | global | Adding, removing, or upgrading UPM packages |

The global skills are installed at `~/.config/opencode/skills/`. Run `unity commands --grep <term>` to discover the rest.

## Non-negotiables

- Never edit `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `obj/`, `.plastic/`, or generated `*.csproj` / `*.slnx` files.
- Every asset and folder under `Assets/` needs a `.meta` file with a unique 32-hex GUID.
- Legacy `Input.*` is disabled. Use `UnityEngine.InputSystem`.
- Do not assume `git` exists. Version control is Plastic.
- Never claim a change works without a compile check, a test run, or explicit user confirmation.
- Prefer `unity command` (Pipeline) or an Editor script (`PrefabUtility`, `SerializedObject`, `EditorSceneManager`) over hand-editing YAML.

## Quick facts

- Editor: `C:\Program Files\Unity\Hub\Editor\6000.4.10f1\Editor\Unity.exe`
- CLI: `unity` (`C:\Users\Tharun\AppData\Local\Unity\bin\unity.exe`, 1.0.0-beta.17)
- Scene: `Assets/Scenes/SampleScene.unity` (only build scene)
- Current quality tier: **PC** (`Assets/Settings/PC_RPAsset.asset`)
- Runtime code → `Assets/Scripts/` (create it); editor-only code → `Assets/Editor/`
- No assembly definitions exist; C# 9, .NET Standard 2.1, nullable disabled
- C# gameplay scripts go in `Assembly-CSharp`; `Editor/` folders go in `Assembly-CSharp-Editor`

See `.opencode/skills/unity-project/references/project-facts.md` for GUIDs, package versions, scene contents, and CLI details.
