using UnityEngine;
using UnityEngine.InputSystem;

namespace ShiftConstructor.Gameplay
{
    /// <summary>
    /// First-person mouse look. Yaw rotates the player body, pitch rotates the camera,
    /// so the camera (a child of the player) always looks where the mouse points.
    /// The cursor is locked during Play Mode so the mouse cannot leave the Game view.
    /// </summary>
    [DefaultExecutionOrder(-10)]
    [DisallowMultipleComponent]
    public sealed class PlayerLook : MonoBehaviour
    {
        [Header("Sensitivity")]
        [Tooltip("Degrees of rotation per pixel of mouse movement.")]
        [SerializeField, Min(0f)] private float _sensitivity = 0.12f;
        [SerializeField] private bool _invertVertical;

        [Header("Pitch limits")]
        [Tooltip("How far up or down the camera can look, in degrees.")]
        [SerializeField, Range(1f, 89f)] private float _pitchLimit = 85f;

        [Header("Cursor")]
        [SerializeField] private bool _lockCursorOnStart = true;
        [Tooltip("Press Escape to release the cursor, left-click to lock it again.")]
        [SerializeField] private bool _escapeReleasesCursor = true;

        [Header("Input actions")]
        [SerializeField] private string _lookAction = "Player/Look";

        [Header("References")]
        [SerializeField] private Transform _camera;

        private InputAction _look;
        private float _yaw;
        private float _pitch;

        public bool IsCursorLocked => Cursor.lockState == CursorLockMode.Locked;

        private void Awake()
        {
            if (_camera == null && Camera.main != null)
            {
                _camera = Camera.main.transform;
            }

            if (_camera == null)
            {
                Debug.LogError($"{nameof(PlayerLook)} needs a camera.", this);
                enabled = false;
                return;
            }

            InputActionAsset actions = InputSystem.actions;
            if (actions == null)
            {
                Debug.LogError($"{nameof(PlayerLook)} needs the project-wide input actions.", this);
                enabled = false;
                return;
            }

            _look = actions.FindAction(_lookAction);
            if (_look == null)
            {
                Debug.LogError($"{nameof(PlayerLook)} could not find '{_lookAction}' in the project-wide actions.", this);
                enabled = false;
                return;
            }

            _yaw = transform.eulerAngles.y;
            _pitch = NormalizeAngle(_camera.localEulerAngles.x);
        }

        private void Start()
        {
            if (_lockCursorOnStart)
            {
                SetCursorLocked(true);
            }
        }

        private void Update()
        {
            if (_escapeReleasesCursor
                && Keyboard.current != null
                && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                SetCursorLocked(false);
            }

            if (!IsCursorLocked)
            {
                if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                {
                    SetCursorLocked(true);
                }

                return;
            }

            Vector2 delta = _look != null ? _look.ReadValue<Vector2>() : Vector2.zero;
            ApplyLook(delta);
        }

        /// <summary>
        /// Rotates the player and camera by a raw look delta in pixels.
        /// Kept separate from <see cref="Update"/> so the math can be tested.
        /// </summary>
        private void ApplyLook(Vector2 delta)
        {
            _yaw += delta.x * _sensitivity;
            _pitch += (_invertVertical ? delta.y : -delta.y) * _sensitivity;
            _pitch = Mathf.Clamp(_pitch, -_pitchLimit, _pitchLimit);

            transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
            _camera.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        private void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        private void OnDisable()
        {
            SetCursorLocked(false);
        }

        private static float NormalizeAngle(float angle)
        {
            angle %= 360f;
            return angle > 180f ? angle - 360f : angle;
        }

        private void OnValidate()
        {
            _sensitivity = Mathf.Max(0f, _sensitivity);
        }
    }
}
