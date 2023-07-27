using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float _walkSpeed = 5;
        [SerializeField] private float _runSpeed = 8;
        [SerializeField] private float _rotateSpeed = 75;
        [SerializeField] private float _jumpForce = 5;
        [SerializeField] private float _gravity = -9.81f;
        
        [SerializeField] private float _pickUpDistance = 5;
        [SerializeField] private float _trowForce = 6;
        [SerializeField] private LayerMask _canPickUpLayer;

        private CharacterController _characterController;
        private Collider _playerCollider;
        private Camera _playerCamera;

        private Vector3 _velocity;
        private Vector2 _rotation;
        private Vector2 _direction;

        private Joint _joint;
        private Rigidbody _currentRigidbodyObject;
        private Collider _currentColliderObject;

        private void Start()
        {
            _characterController = GetComponent<CharacterController>();
            _playerCollider = GetComponent<Collider>();
            _playerCamera = GetComponentInChildren<Camera>();
            _joint = GetComponentInChildren<Joint>();
            
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            _characterController.Move(_velocity * Time.deltaTime);
            
            _direction = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

            if (_characterController.isGrounded) _velocity.y = Input.GetKeyDown(KeyCode.Space) ? _jumpForce : -0.1f;
            else _velocity.y += _gravity * Time.deltaTime;
            
            if (Input.GetMouseButtonDown(0)) PickUp();
            if (Input.GetMouseButtonUp(0)) Drop();
            if (Input.GetMouseButtonDown(1)) Drop(true);
            
            mouseDelta *= _rotateSpeed * Time.deltaTime;
            _rotation.y += mouseDelta.x;
            _rotation.x = Mathf.Clamp(_rotation.x - mouseDelta.y, -90, 90);
            _playerCamera.transform.localEulerAngles = _rotation;
        }

        private void FixedUpdate()
        {
            _direction *= Input.GetKey(KeyCode.LeftShift) ? _runSpeed : _walkSpeed;
            Vector3 move = Quaternion.Euler(0, _playerCamera.transform.eulerAngles.y, 0) * new Vector3(_direction.x, 0, _direction.y);
            _velocity = new Vector3(move.x, _velocity.y, move.z);
        }
        
        private void PickUp()
        {
            if (!Physics.Raycast(_playerCamera.transform.position, _playerCamera.transform.forward, out RaycastHit hit, _pickUpDistance, _canPickUpLayer)) return;
            
            _currentRigidbodyObject = hit.collider.gameObject.GetComponent<Rigidbody>();
            _currentColliderObject = _currentRigidbodyObject.GetComponent<Collider>();
            
            Physics.IgnoreCollision(_playerCollider, _currentColliderObject);
            _currentRigidbodyObject.drag = 15;
            
            _joint.gameObject.transform.position = _currentRigidbodyObject.gameObject.transform.position;
            _joint.connectedBody = _currentRigidbodyObject;
        }

        private void Drop(bool isTrow = false)
        {
            if (_currentRigidbodyObject == null) return;
            
            _joint.connectedBody = null;
            
            _currentRigidbodyObject.velocity = _velocity;
            if (isTrow) _currentRigidbodyObject.AddForce(_playerCamera.transform.forward * _trowForce, ForceMode.Impulse);
            _currentRigidbodyObject.drag = 0;
            
            Physics.IgnoreCollision(_playerCollider, _currentColliderObject, false);
            
            _currentRigidbodyObject = null;
        }
    }
}