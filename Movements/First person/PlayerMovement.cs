using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float _walkSpeed = 5;
        [SerializeField] private float _runSpeed = 8;
        [SerializeField] private float _rotateSpeed = 10;
        [SerializeField] private float _jumpForce = 5;
        [SerializeField] private float _gravity = -9.81f;
        
        private PlayerInput _inputSystem;
        private CharacterController _characterController;
        private Camera _playerCamera;

        private Vector3 _velocity;
        private Vector2 _rotation;

        private void Start()
        {
            _inputSystem = new PlayerInput();
            _inputSystem.Player.Jump.performed += _ => Jump();
            _inputSystem.Player.Enable();

            _characterController = GetComponent<CharacterController>();
            _playerCamera = GetComponentInChildren<Camera>();
            
            Cursor.lockState = CursorLockMode.Locked;
        }
        
        private void Update()
        {
            _characterController.Move(_velocity * Time.deltaTime);
            
            Vector2 rotate = _inputSystem.Player.Look.ReadValue<Vector2>();
            if (rotate.sqrMagnitude < 0.1f) return;

            rotate *= _rotateSpeed * Time.deltaTime;
            _rotation.y += rotate.x;
            _rotation.x = Mathf.Clamp(_rotation.x - rotate.y, -90, 90);
            _playerCamera.transform.localEulerAngles = _rotation;
        }

        private void FixedUpdate()
        {
            Vector2 direction = _inputSystem.Player.Move.ReadValue<Vector2>();
            
            direction *= _inputSystem.Player.Sprint.ReadValue<float>() > 0 ? _runSpeed : _walkSpeed;
            Vector3 move = Quaternion.Euler(0, _playerCamera.transform.eulerAngles.y, 0) * new Vector3(direction.x, 0, direction.y);
            _velocity = new Vector3(move.x, _velocity.y, move.z);
            
            if (_characterController.isGrounded) _velocity.y = -0.1f;
            
            else _velocity.y += _gravity * Time.fixedDeltaTime;
        }

        private void Jump() { if (_characterController.isGrounded) _velocity = Vector3.up * _jumpForce; }
    }
}