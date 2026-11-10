using UnityEngine;
using UnityEngine.InputSystem;

namespace ShiftConstructor.Gameplay
{
    /// <summary>
    /// Camera-relative walk/sprint/jump on top of a <see cref="CharacterController"/>.
    /// The controller is kinematic, so it is driven from Update rather than FixedUpdate.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField, Min(0f)] private float _walkSpeed = 4.5f;
        [SerializeField, Min(0f)] private float _sprintSpeed = 7f;
        [SerializeField, Min(0f)] private float _acceleration = 30f;
        [SerializeField, Min(0f)] private float _turnSpeed = 720f;

        [Tooltip("Rotate the player toward the movement direction. Turn this off when the camera is a child of the player, " +
                 "otherwise strafing makes the player circle, because the camera turns with it.")]
        [SerializeField] private bool _faceMovementDirection = true;

        [Header("Jump and gravity")]
        [SerializeField, Min(0f)] private float _jumpHeight = 1.2f;
        [SerializeField] private float _gravity = -22f;
        [SerializeField, Min(0f)] private float _groundStick = 2f;

        [Header("Input actions")]
        [SerializeField] private string _moveAction = "Player/Move";
        [SerializeField] private string _jumpAction = "Player/Jump";
        [SerializeField] private string _sprintAction = "Player/Sprint";

        [Header("References")]
        [SerializeField] private Transform _camera;

        private CharacterController _controller;
        private InputAction _move;
        private InputAction _jump;
        private InputAction _sprint;
        private Vector3 _horizontalVelocity;
        private float _verticalVelocity;
        private float _yaw;

        public bool IsGrounded => _controller != null && _controller.isGrounded;

        public float CurrentSpeed => _horizontalVelocity.magnitude;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            if (_controller == null)
            {
                Debug.LogError($"{nameof(PlayerController)} requires a CharacterController.", this);
                enabled = false;
                return;
            }

            if (_camera == null && Camera.main != null)
            {
                _camera = Camera.main.transform;
            }

            if (_camera == null)
            {
                Debug.LogWarning($"{nameof(PlayerController)} has no camera; movement falls back to world axes.", this);
            }

            InputActionAsset actions = InputSystem.actions;
            if (actions == null)
            {
                Debug.LogError($"{nameof(PlayerController)} needs the project-wide input actions.", this);
                enabled = false;
                return;
            }

            _move = actions.FindAction(_moveAction);
            _jump = actions.FindAction(_jumpAction);
            _sprint = actions.FindAction(_sprintAction);

            if (_move == null || _jump == null)
            {
                Debug.LogError(
                    $"{nameof(PlayerController)} could not find '{_moveAction}' or '{_jumpAction}' in the project-wide actions.",
                    this);
                enabled = false;
            }
        }

        private void Start()
        {
            _yaw = transform.eulerAngles.y;
        }

        private void Update()
        {
            Vector2 input = _move != null ? _move.ReadValue<Vector2>() : Vector2.zero;
            Vector3 wishDirection = ToWorldDirection(input);

            float targetSpeed = _sprint != null && _sprint.IsPressed() ? _sprintSpeed : _walkSpeed;
            Vector3 targetVelocity = wishDirection * targetSpeed;
            _horizontalVelocity = Vector3.MoveTowards(
                _horizontalVelocity, targetVelocity, _acceleration * Time.deltaTime);

            if (_controller.isGrounded)
            {
                if (_verticalVelocity < 0f)
                {
                    _verticalVelocity = -_groundStick;
                }

                if (_jump != null && _jump.WasPressedThisFrame())
                {
                    _verticalVelocity = Mathf.Sqrt(-2f * _gravity * _jumpHeight);
                }
            }

            _verticalVelocity += _gravity * Time.deltaTime;

            Vector3 motion = _horizontalVelocity + Vector3.up * _verticalVelocity;
            CollisionFlags flags = _controller.Move(motion * Time.deltaTime);

            if ((flags & CollisionFlags.Above) != 0 && _verticalVelocity > 0f)
            {
                _verticalVelocity = 0f;
            }

            if (_faceMovementDirection && wishDirection.sqrMagnitude > 0.0001f)
            {
                float targetYaw = Mathf.Atan2(wishDirection.x, wishDirection.z) * Mathf.Rad2Deg;
                _yaw = Mathf.MoveTowardsAngle(_yaw, targetYaw, _turnSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
            }
        }

        private Vector3 ToWorldDirection(Vector2 input)
        {
            Vector3 forward = Vector3.forward;
            Vector3 right = Vector3.right;

            if (_camera != null)
            {
                forward = _camera.forward;
                forward.y = 0f;

                if (forward.sqrMagnitude < 0.0001f)
                {
                    forward = Vector3.forward;
                }
                else
                {
                    forward.Normalize();
                }

                right = Vector3.Cross(Vector3.up, forward);
            }

            Vector3 direction = forward * input.y + right * input.x;
            return Vector3.ClampMagnitude(direction, 1f);
        }

        private void OnValidate()
        {
            _gravity = Mathf.Min(_gravity, -0.01f);
        }
    }
}
