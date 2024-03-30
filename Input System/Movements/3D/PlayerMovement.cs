using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float _walkSpeed = 5;
        [SerializeField] private float _runSpeed = 8;
        [SerializeField] private float _rotateSpeed = 15;
        [SerializeField] private float _jumpForce = 5;
        [SerializeField] private float _gravity = -9.81f;
        [SerializeField] private Transform _cameraRotate;

        private InputSystem _inputSystem;
        private CharacterController _characterController;

        private Vector3 _velocity;
        private Vector2 _rotation;

        private void Start()
        {
            _inputSystem = new InputSystem();
            _inputSystem.Player.Jump.performed += Jump;
            _inputSystem.Player.Move.performed += Move;
            _inputSystem.Player.Move.canceled += Move;
            _inputSystem.Player.Look.performed += Look;
            _inputSystem.Player.Sprint.performed += Move;
            _inputSystem.Player.Sprint.canceled += Move;
            _inputSystem.Player.Enable();

            _characterController = GetComponent<CharacterController>();
            
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Move(InputAction.CallbackContext callbackContext)
        {
            Vector2 direction = _inputSystem.Player.Move.ReadValue<Vector2>() * (_inputSystem.Player.Sprint.IsPressed() ? _runSpeed : _walkSpeed);
            Vector3 move = Quaternion.Euler(0, _cameraRotate.localEulerAngles.y, 0) * new Vector3(direction.x, 0, direction.y);
            _velocity = new Vector3(move.x, _velocity.y, move.z);
        }

        private void Look(InputAction.CallbackContext callbackContext)
        {
            Vector2 mouseDelta = callbackContext.ReadValue<Vector2>() * _rotateSpeed * Time.deltaTime;
            _rotation.y += mouseDelta.x;
            _rotation.x = Mathf.Clamp(_rotation.x - mouseDelta.y, -90, 90);
            
            _cameraRotate.localEulerAngles = _rotation;
            
            Move(callbackContext);
        }

        private void Update() => _characterController.Move(_velocity * Time.deltaTime);

        private void FixedUpdate()
        {
            if (_characterController.isGrounded && _velocity.y <= 0f) _velocity.y = -0.1f;
            else _velocity.y += _gravity * Time.fixedDeltaTime;
        }

        private void Jump(InputAction.CallbackContext _) { if (_characterController.isGrounded) _velocity.y = _jumpForce; }

        private void OnDestroy()
        {
            _inputSystem.Player.Jump.performed -= Jump;
            _inputSystem.Player.Move.performed -= Move;
            _inputSystem.Player.Look.performed -= Look;
            _inputSystem.Player.Move.canceled -= Move;
            _inputSystem.Player.Sprint.performed -= Move;
            _inputSystem.Player.Sprint.canceled -= Move;
            _inputSystem.Player.Disable();
        }
    }
}