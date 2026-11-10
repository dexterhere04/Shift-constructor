---
name: Unity Input System
description: Use the new Input System in this project — project-wide actions, reading Move/Look/Jump, generated wrapper classes, PlayerInput, UI input, and rebinding. Load for any player input, controls, or UI navigation work.
---

# Input System 1.19 (new backend only)

The project is configured with `activeInputHandler: 1`, meaning **Input System Package (New) only**. Any call to legacy `Input.GetKey`, `Input.GetAxis`, `Input.mousePosition`, etc. throws `InvalidOperationException` at runtime. Use `UnityEngine.InputSystem` instead.

## The project's actions

`Assets/InputSystem_Actions.inputactions` (GUID `052faaac586de48259a63d0c4782560b`) is registered as the **project-wide actions** asset. Wrapper generation is currently **off**.

Two maps:

- `Player`: `Move` (Vector2), `Look` (Vector2), `Attack`, `Interact`, `Crouch`, `Jump`, `Previous`, `Next`, `Sprint`.
- `UI`: `Navigate`, `Submit`, `Cancel`, `Point`, `Click`, `RightClick`, `MiddleClick`, `ScrollWheel`, `TrackedDevicePosition`, `TrackedDeviceOrientation`.

Control schemes: `Keyboard&Mouse`, `Gamepad`, `Touch`, `Joystick`, `XR`. Bindings are already defined; prefer editing bindings in the Input Actions editor over rewriting the JSON by hand.

## Reading input in code

### Project-wide actions (simplest)

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

namespace ShiftConstructor.Gameplay
{
    public sealed class PlayerMover : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _speed = 5f;

        private InputAction _move;

        private void Awake()
        {
            _move = InputSystem.actions.FindAction("Player/Move", throwIfNotFound: true);
        }

        private void Update()
        {
            Vector2 input = _move.ReadValue<Vector2>();
            transform.Translate(new Vector3(input.x, 0f, input.y) * (_speed * Time.deltaTime));
        }
    }
}
```

- `InputSystem.actions` is the project-wide asset and is enabled for you.
- Use `"Map/Action"` (e.g. `"Player/Jump"`) to avoid ambiguity when names repeat across maps.
- If several components share an action, do **not** call `Disable()` from one of them. Only a single owner should manage the action map's enabled state.

### Event callbacks

```csharp
private InputAction _jump;

private void OnEnable()
{
    _jump = InputSystem.actions.FindAction("Player/Jump", throwIfNotFound: true);
    _jump.performed += OnJump;
    _jump.Enable();
}

private void OnDisable()
{
    _jump.performed -= OnJump;
    _jump.Disable();
}

private void OnJump(InputAction.CallbackContext context)
{
    // context.ReadValue<float>() for a button is 1 on press.
}
```

- Always unsubscribe in `OnDisable` and enable/disable symmetrically.
- `performed` fires once per press; use `started`/`canceled` for hold behavior and `IsPressed()` for polling.

### Direct devices (no action asset)

```csharp
using UnityEngine.InputSystem;

if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
{
    // Jump.
}

Vector2 look = Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;
```

Direct device access is fine for prototypes and debug input. For real gameplay use actions so bindings, rebinding, and devices stay centralized.

## Generated wrapper class

A wrapper gives compile-time names instead of strings:

1. Select `Assets/InputSystem_Actions.inputactions`.
2. In the Inspector, enable **Generate C# Class** and set the path (e.g. `Assets/Scripts/InputSystem_Actions.cs`), class name, and namespace.
3. This sets `generateWrapperCode: 1`, `wrapperCodePath`, `wrapperClassName`, and `wrapperCodeNamespace` in the asset's `.meta`. Unity regenerates the file whenever the asset changes.

Usage:

```csharp
private InputSystem_Actions _actions;

private void Awake()
{
    _actions = new InputSystem_Actions();
}

private void OnEnable() => _actions.Player.Enable();
private void OnDisable() => _actions.Player.Disable();

private void Update()
{
    Vector2 move = _actions.Player.Move.ReadValue<Vector2>();
}
```

The generated `.cs` is a build artifact of the asset. Never edit it by hand; edit the `.inputactions` asset or the generator settings instead.

## PlayerInput component

Use `PlayerInput` when Unity should manage maps and player joining for you (local multiplayer, device pairing, `SendMessages`, `Invoke Unity Events`, or `Invoke C# Events` behavior). For a single player, the project-wide actions above are lighter.

## UI input

uGUI event handling requires `InputSystemUIInputModule` on the `EventSystem`, not the legacy `StandaloneInputModule`. When creating UI (load the `ui` skill for layout work):

- `EventSystem` + `InputSystemUIInputModule` (assign the `UI` action map if asked).
- Assign `Point`, `Click`, `Navigate`, `Submit`, `Cancel`, `ScrollWheel` actions.
- Do not add the old `StandaloneInputModule`; it does nothing in this configuration.

## Rebinding

```csharp
var rebind = _jump.PerformInteractiveRebinding()
    .WithCancelingThrough("<Keyboard>/escape")
    .OnComplete(operation =>
    {
        operation.Dispose();
        _jump.Enable();
    })
    .Start();

_jump.Disable();
```

- Disable the action before rebinding; re-enable when done or canceled.
- Always `Dispose()` the operation.
- Save overrides with `action.SaveBindingOverridesAsJson()` if rebinds must persist.

## Editing the `.inputactions` asset

- It is JSON wrapped by a `ScriptedImporter`, so it is text-editable, but a malformed edit breaks the importer. Prefer the Input Actions editor for bindings, maps, and composites.
- Adding an action by hand requires a matching entry in the map's `actions` array and at least one binding in `bindings`; regenerate the wrapper afterward.
- New `.inputactions` files need a `.meta` with the Input System's `ScriptedImporter` (`script` GUID `8404be70184654265930450def6a9037`). Copying an existing `.meta` and changing the GUID is the safe route.
