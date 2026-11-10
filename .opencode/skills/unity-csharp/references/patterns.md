# C# patterns for this project

## Component template

```csharp
using UnityEngine;

namespace ShiftConstructor.Gameplay
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class RockMover : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField, Min(0f)] private float _acceleration = 20f;

        private Rigidbody _body;
        private Vector2 _moveInput;

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            // Input reads happen once per frame.
            _moveInput = Vector2.zero; // Replace with Input System reads.
        }

        private void FixedUpdate()
        {
            var desired = new Vector3(_moveInput.x, 0f, _moveInput.y) * _acceleration;
            _body.AddForce(desired, ForceMode.Force);
        }

        private void OnValidate()
        {
            _acceleration = Mathf.Max(0f, _acceleration);
        }
    }
}
```

## Inspector-friendly auto-property

```csharp
[field: SerializeField, Min(1)] public int Level { get; private set; } = 1;
```

## Event owner

```csharp
using System;
using UnityEngine;

namespace ShiftConstructor.Gameplay
{
    public sealed class Health : MonoBehaviour
    {
        [SerializeField, Min(1)] private int _max = 3;

        public event Action<int> Changed;
        public event Action Died;

        public int Current { get; private set; }

        private void Awake() => Current = _max;

        public void TakeDamage(int amount)
        {
            if (amount <= 0 || Current <= 0) return;
            Current = Mathf.Max(0, Current - amount);
            Changed?.Invoke(Current);
            if (Current == 0) Died?.Invoke();
        }
    }
}
```

## Awaitable with cancellation

```csharp
using UnityEngine;

namespace ShiftConstructor.Gameplay
{
    public sealed class DelayedSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject _prefab;
        [SerializeField, Min(0f)] private float _delay = 1f;

        private async Awaitable Start()
        {
            while (this != null)
            {
                await Awaitable.WaitForSecondsAsync(_delay, destroyCancellationToken);
                if (this == null) return;
                Instantiate(_prefab, transform.position, transform.rotation);
            }
        }
    }
}
```

Note: the `this != null` checks are redundant with `destroyCancellationToken` but harmless; keep one mechanism, not both, in real code.

## ScriptableObject settings

```csharp
using UnityEngine;

namespace ShiftConstructor.Data
{
    [CreateAssetMenu(menuName = "Shift Constructor/Rock Settings", fileName = "RockSettings")]
    public sealed class RockSettings : ScriptableObject
    {
        [SerializeField, Min(0.01f)] private float _mass = 10f;
        [SerializeField, Min(0f)] private float _bounce = 0.2f;

        public float Mass => _mass;
        public float Bounce => _bounce;
    }
}
```

## GameObject prefab creation (editor script)

```csharp
using UnityEditor;
using UnityEngine;

namespace ShiftConstructor.EditorTools
{
    public static class PrefabFactory
    {
        [MenuItem("Shift Constructor/Create Prop Prefab")]
        public static void CreateProp()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "NewProp";
            const string path = "Assets/Prefabs/NewProp.prefab";
            System.IO.Directory.CreateDirectory("Assets/Prefabs");
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            AssetDatabase.Refresh();
        }
    }
}
```

## EditMode test assembly

`Assets/Tests/EditMode/ShiftConstructor.Tests.EditMode.asmdef`:

```json
{
  "name": "ShiftConstructor.Tests.EditMode",
  "rootNamespace": "ShiftConstructor.Tests",
  "references": [
    "UnityEngine.TestRunner",
    "UnityEditor.TestRunner"
  ],
  "includePlatforms": [
    "Editor"
  ],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": true,
  "precompiledReferences": [
    "nunit.framework.dll"
  ],
  "autoReferenced": false,
  "defineConstraints": [
    "UNITY_INCLUDE_TESTS"
  ],
  "versionDefines": [],
  "noEngineReferences": false
}
```

`Assets/Tests/PlayMode/ShiftConstructor.Tests.PlayMode.asmdef`:

```json
{
  "name": "ShiftConstructor.Tests.PlayMode",
  "rootNamespace": "ShiftConstructor.Tests",
  "references": [
    "UnityEngine.TestRunner"
  ],
  "includePlatforms": [],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": true,
  "precompiledReferences": [
    "nunit.framework.dll"
  ],
  "autoReferenced": false,
  "defineConstraints": [
    "UNITY_INCLUDE_TESTS"
  ],
  "versionDefines": [],
  "noEngineReferences": false
}
```

Test sample:

```csharp
using NUnit.Framework;
using UnityEngine;

namespace ShiftConstructor.Tests
{
    public sealed class VectorMathTests
    {
        [Test]
        public void Clamp_KeepsValueInsideRange()
        {
            Assert.AreEqual(1f, Mathf.Clamp(5f, 0f, 1f));
        }
    }
}
```

If gameplay code moves into its own asmdef later, test assemblies must add that asmdef to `references`.
