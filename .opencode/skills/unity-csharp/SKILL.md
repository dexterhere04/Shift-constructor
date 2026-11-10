---
name: Unity C#
description: Write or review C# for this Unity 6.4 project — assembly layout, C# 9 limits, Unity 6 API changes, serialization, MonoBehaviour conventions, Awaitable, ScriptableObjects, and test assembly setup. Load when adding or changing gameplay or editor scripts.
---

# Unity C# conventions

## Environment

- **C# 9.0**, `netstandard2.1`, nullable reference types **disabled**. File-scoped namespaces (C# 10), `required` members (C# 11), and collection expressions (C# 12) do **not** compile.
- Runtime scripts live under `Assets/Scripts/` (create it) and compile into `Assembly-CSharp`.
- Editor-only scripts live under `Assets/Editor/` (or any folder named `Editor`) and compile into `Assembly-CSharp-Editor`. Never reference `UnityEditor` from runtime code; guard it with `#if UNITY_EDITOR` if unavoidable.
- **There are no `.asmdef` files.** Do not add one casually — it changes type visibility for `Assembly-CSharp` and can break every existing reference. Add assembly definitions only as a deliberate refactor.
- Put new code in `namespace ShiftConstructor` (feature sub-namespaces such as `ShiftConstructor.Gameplay`) to avoid collisions with packages.
- C# 9 supports: records, `init` accessors, target-typed `new`, pattern matching enhancements, static local functions, `nint`. Use them freely.

See `references/patterns.md` for copy-paste templates.

## Serialization

```csharp
[Header("Movement")]
[SerializeField, Min(0f)] private float _moveSpeed = 6f;
[SerializeField] private Rigidbody _body;

public float MoveSpeed => _moveSpeed;
```

- Use `[SerializeField] private` fields, not public fields. Expose read-only access through properties when needed.
- Unity serializes **fields**, not properties. For auto-properties use `[field: SerializeField]`.
- `[SerializeField]` works on private/protected fields; `[SerializeReference]` is only needed for polymorphic serialized data.
- Initialize serialized references in the Inspector. Do not silently fall back to `GetComponent` in `Awake` unless the field is optional — if you do, log a warning when it is missing.
- Clamp/validate in `OnValidate`, not at runtime only.

## Unity 6 API changes to prefer

| Deprecated | Use instead |
| --- | --- |
| `Rigidbody.velocity` | `Rigidbody.linearVelocity` |
| `Rigidbody.drag` | `Rigidbody.linearDamping` |
| `Rigidbody.angularDrag` | `Rigidbody.angularDamping` |
| `Object.FindObjectOfType<T>()` | `Object.FindFirstObjectByType<T>()` |
| `Object.FindObjectsOfType<T>()` | `Object.FindObjectsByType<T>(FindObjectsSortMode.None)` |
| `Object.FindObjectOfType<T>(true)` | `Object.FindFirstObjectByType<T>(FindObjectsInactive.Include)` |
| Legacy `Input.*` | New Input System (see `unity-input-system`) |

- Use `CompareTag("Player")` instead of `gameObject.tag == "Player"`.
- Use `TryGetComponent(out T component)` when the component is optional.
- `Debug.Log` is fine in development; avoid per-frame logging.

## MonoBehaviour rules

- Cache components and references in `Awake`; resolve cross-object references there too. Use `Start` for logic that depends on other objects' `Awake`.
- Keep `OnEnable` / `OnDisable` symmetric, especially for event subscriptions and input action enable/disable.
- Avoid `Update` when an event, coroutine, or `Awaitable` loop is enough. Never allocate in `Update` (no LINQ, no `string` concatenation, no `new WaitForSeconds`).
- Physics-driven movement belongs in `FixedUpdate`; read input in `Update` and apply it in `FixedUpdate`.
- Use `[RequireComponent(typeof(Rigidbody))]` when a component hard-depends on another.
- Do not use `DontDestroyOnLoad` or singleton patterns without a concrete need; if you must, make teardown explicit.

## Null checks on UnityEngine.Object

Destroyed Unity objects compare equal to `null`, but the C# null-conditional operator bypasses Unity's overloaded `==`:

```csharp
// Wrong: can run on a destroyed object.
_target?.TakeDamage(1);

// Right:
if (_target != null) _target.TakeDamage(1);
```

Use `?.` only for non-`UnityEngine.Object` types.

## Events

- Prefer a C# `event Action` for code-only signals. Always unsubscribe in `OnDisable`.
- Use `UnityEvent` (or `[SerializeField] UnityEvent`) when designers should wire callbacks in the Inspector.
- Raise events from the owner only; never let subscribers mutate the owner's state directly.

## Async and coroutines

Unity 6 provides `UnityEngine.Awaitable`, which is allocation-friendly and cancellation-aware.

```csharp
private async Awaitable Start()
{
    await Awaitable.WaitForSecondsAsync(0.5f, destroyCancellationToken);
    await Awaitable.NextFrameAsync(destroyCancellationToken);
    DoThing();
}
```

- Pass `destroyCancellationToken` so the await stops when the object is destroyed.
- Do not use `Task`/`async void` for gameplay flow; prefer `Awaitable` or coroutines.
- Keep the `Coroutine` handle and `StopCoroutine` it in `OnDisable` when the coroutine mutates state.

## ScriptableObjects

Use ScriptableObjects for shared configuration, not for per-instance runtime state:

```csharp
[CreateAssetMenu(menuName = "Shift Constructor/Rock Settings", fileName = "RockSettings")]
public sealed class RockSettings : ScriptableObject
{
    [SerializeField, Min(0f)] private float _mass = 10f;
    public float Mass => _mass;
}
```

## Style

- 4-space indentation, `_camelCase` for private fields, `PascalCase` for types/methods/properties, `camelCase` for locals and parameters.
- One public type per file; the file name matches the type name.
- `sealed` on classes not designed for inheritance.
- Explicit access modifiers everywhere.
- Braces on their own line (Allman), matching the Unity template code.

## Tests

Unity Test Framework 1.6 is installed. Tests need an assembly definition (unlike gameplay code) and only compile in the Editor when `UNITY_INCLUDE_TESTS` is defined. Use the templates in `references/patterns.md`, then run them with the CLI:

```powershell
unity test --mode EditMode
unity test --mode PlayMode --filter ShiftConstructor.Tests.RockSettingsTests
```

Reports default to `test-results.xml` (`--output` and `--report-format junit` change that). Add the `UNITY_INCLUDE_TESTS` define to Player builds only if you deliberately ship test assemblies.

## Verifying code changes

```powershell
unity recompile                        # recompile the open Editor and print any error CS lines
unity command console get --level error # read captured Editor console output
unity command script eval ...          # run a static method in the live Editor
```

With no Editor open, `unity recompile` has nothing to attach to: use `unity test --mode EditMode` or `unity build --target StandaloneWindows64` instead — both compile the scripts and fail on `error CS`. A change is not verified until one of these is clean.
