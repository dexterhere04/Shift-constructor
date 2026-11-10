# Project facts

Verified against the project on disk. Re-check if `Packages/manifest.json` or `ProjectSettings/ProjectVersion.txt` changes.

## Engine and tools

| Item | Value |
| --- | --- |
| Unity Editor | `6000.4.10f1` (revision `feeafc12a938`) |
| Editor install | `C:\Program Files\Unity\Hub\Editor\6000.4.10f1\Editor\Unity.exe` |
| Render pipeline | Universal RP `17.4.0` |
| Input System | `1.19.0`, new backend only (`activeInputHandler: 1`) |
| UI | uGUI `2.0.0` (includes TextMeshPro package) |
| TextMeshPro essentials | **Not imported** (`Assets/TextMesh Pro/` does not exist) |
| Visual Scripting | `1.9.11` (package present, unused) |
| Test Framework | `1.6.0` |
| Unity CLI | `1.0.0-beta.17` (beta channel; experimental) |
| Pipeline package | `com.unity.pipeline` `0.10.0-exp.1` (live-Editor control, port `7800`) |
| Version control | Plastic SCM (`.plastic/`, `ignore.conf`) |
| Color space | Linear (`m_ActiveColorSpace: 1`) |

## Unity CLI

The Unity CLI is installed with Unity Hub and already signed in. It reaches both the project on disk and this running Editor.

| Item | Value |
| --- | --- |
| Binary | `C:\Users\Tharun\AppData\Local\Unity\bin\unity.exe` |
| Version | `1.0.0-beta.17` (`unity upgrade --check --channel beta`) |
| Skill bundle | 34 official skills in `C:\Users\Tharun\.config\opencode\skills` |
| Editor control | `unity command ...` via `com.unity.pipeline`, port `7800` |

```powershell
unity status                              # connected Editors: port, PID, project, state
unity recompile                           # compile check; prints error CS lines
unity test --mode EditMode                # run tests in a batch Editor, writes test-results.xml
unity build --target StandaloneWindows64  # batch-mode build
unity commands --grep <term>              # search CLI + Pipeline commands
unity command --query <term>              # search Pipeline (live Editor) commands
```

Useful live-Editor commands (all `unity command <name>`): `scene get_hierarchy`, `scene save`, `gameobject create|find|delete|set_parent`, `component set_field`, `prefab instantiate|save_contents|apply_overrides`, `asset create|write_text|set_import_settings`, `capture scene_view|game_view`, `console get`, `test run`, `editor save_all`, `job batch` (transactional multi-step edit).

Notes:
- `unity upgrade --check --channel stable` returns HTTP 404; only the beta channel publishes a manifest today.
- `unity build` and `unity test` spawn a second Editor and fail while this project is locked by the open one. Run them with the Editor closed, or stay on `unity recompile` / `unity command test run` / `unity command` against the live Editor.
- Global skills are the official Unity set; project-specific skills live in `.opencode/skills/`.

## C# build settings

- Target framework: `netstandard2.1`
- Language version: **C# 9.0** (from `Assembly-CSharp.csproj`)
- Nullable reference types: disabled
- Scripting backend: Mono (no backend override)
- Generated `.csproj`/`.slnx` are build artifacts; never edit or rely on edits to them.

## URP configuration

Render pipeline is set **per quality level** in `ProjectSettings/QualitySettings.asset`, not in `GraphicsSettings` (where `m_CustomRenderPipeline` is 0).

| Quality level | Index | Pipeline asset | GUID |
| --- | --- | --- | --- |
| Mobile | 0 | `Assets/Settings/Mobile_RPAsset.asset` | `5e6cbd92db86f4b18aec3ed561671858` |
| PC | 1 (**current**) | `Assets/Settings/PC_RPAsset.asset` | `4b83569d67af61e458304325a23e5dfd` |

Renderers:

| Renderer | GUID |
| --- | --- |
| `PC_Renderer.asset` | `f288ae1f4751b564a96ac7587541f7a2` |
| `Mobile_Renderer.asset` | `65bc7dbf4170f435aa868c779acfb082` |

PC pipeline asset settings: depth texture **on**, opaque texture **on**, HDR **on**, MSAA **1x**, render scale `1`, SRP Batcher **on**, default renderer index `0`.

Volume profiles:

| Asset | GUID |
| --- | --- |
| `DefaultVolumeProfile.asset` | `ab09877e2e707104187f6f83e2f62510` |
| `SampleSceneProfile.asset` | `10fc4df2da32a41aaa32d77bc913491c` |
| `UniversalRenderPipelineGlobalSettings.asset` | `18dc0cd2c080841dea60987a38ce93fa` |

## Scene

`Assets/Scenes/SampleScene.unity` — GUID `99c9720ab356a0642a771bea13969a05`. It is the only enabled scene in `EditorBuildSettings`.

The scene is under active development, so this list is a snapshot, not a contract. Ask the open Editor for the current truth with `unity command scene get_hierarchy` (and `unity command scene list_open` for dirty state) before editing.

Root objects at the time of writing:

- `Main Camera` (Camera + UniversalAdditionalCameraData)
- `ground` (MeshRenderer + MeshFilter + BoxCollider, `ground.mat`)
- `Directional Light` (Light + UniversalAdditionalLightData)
- `Global Volume` (Volume using `SampleSceneProfile`)
- 6 × `smallRock` prefab instances (GUID `107bed240dd26d64e94114931f52331a`)
- 6 × `rockRow` prefab instances (GUID `2682ccd49c2ce774785fe548ad8d49d6`)

## Input actions

`Assets/InputSystem_Actions.inputactions` — GUID `052faaac586de48259a63d0c4782560b`, registered as project-wide actions in `ProjectSettings/EditorBuildSettings.asset`. Wrapper code generation is **off**.

Two action maps. Control schemes: `Keyboard&Mouse`, `Gamepad`, `Touch`, `Joystick`, `XR`.

`Player` map:

| Action | Type | Main bindings |
| --- | --- | --- |
| `Move` | Value (Vector2) | WASD / arrows, gamepad left stick, XR primary 2D axis, joystick stick |
| `Look` | Value (Vector2) | gamepad right stick, `<Pointer>/delta`, joystick hat switch |
| `Attack` | Button | mouse left, gamepad west, touch tap, joystick trigger, XR primary, Enter |
| `Interact` | Button | `E`, gamepad north |
| `Crouch` | Button | `C`, gamepad east |
| `Jump` | Button | Space, gamepad south, XR secondary |
| `Previous` | Button | `1`, gamepad dpad-left |
| `Next` | Button | `2`, gamepad dpad-right |
| `Sprint` | Button | Left Shift, gamepad left-stick press, XR trigger |

`UI` map (all `PassThrough` except `Submit`/`Cancel` buttons): `Navigate`, `Submit`, `Cancel`, `Point`, `Click`, `RightClick`, `MiddleClick`, `ScrollWheel`, `TrackedDevicePosition`, `TrackedDeviceOrientation`.

## Editor settings

- Asset serialization: **Force Text**
- New script line endings: OS native (CRLF on this machine)
- Default behavior mode: 3D
- Enter Play Mode options: enabled but no options disabled, so a normal domain reload happens on entering Play Mode.

## Packaging

`Packages/manifest.json` notable entries: `com.unity.ai.assistant` (editor AI), `com.unity.ai.inference`, `com.unity.ai.navigation`, `com.unity.pipeline` `0.10.0-exp.1` (Unity CLI live-Editor control), `com.unity.multiplayer.center`. Do not remove packages without asking; many are pre-installed Unity 6 defaults.
