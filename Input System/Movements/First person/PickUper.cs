using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PickUper : MonoBehaviour
    {
        [SerializeField] private float _followSpeed = 30;
        [SerializeField] private float _trowForce = 6;
        [SerializeField] private float _pickUpDistance = 5;
        [SerializeField] private LayerMask _canPickUpLayer;
        [Space]
        [SerializeField] private Collider _playerCollider;
        [SerializeField] private Camera _playerCamera;
        
        private InputSystem _inputSystem;
        private CancellationTokenSource _cancellationToken;
        
        private Rigidbody _currentRigidbodyObject;
        private Collider _currentColliderObject;

        private void Start()
        {
            _inputSystem = new();
            _inputSystem.Enable();
            
            _inputSystem.Player.PickUp.performed += PickUp;
            _inputSystem.Player.PickUp.canceled += Drop;
            _inputSystem.Player.Trow.performed += Throw;
        }
        
        private void OnDestroy()
        {
            _inputSystem.Player.PickUp.performed -= PickUp;
            _inputSystem.Player.PickUp.canceled -= Drop;
            _inputSystem.Player.Trow.performed -= Throw;
        }

        private void PickUp(InputAction.CallbackContext _)
        {
            if (!Physics.Raycast(_playerCamera.transform.position, _playerCamera.transform.forward, out RaycastHit hit, _pickUpDistance, _canPickUpLayer)) return;
            
            _currentColliderObject = hit.collider;
            _currentRigidbodyObject = _currentColliderObject.gameObject.GetComponent<Rigidbody>();
            
            Physics.IgnoreCollision(_playerCollider, _currentColliderObject);
            transform.position = _currentColliderObject.transform.position;
            
            _currentRigidbodyObject.useGravity = false;
            _currentRigidbodyObject.collisionDetectionMode = CollisionDetectionMode.Continuous;
            
            _cancellationToken = new();
            Follow();
        }

        private void Drop(bool isThrow = false)
        {
            if (_currentRigidbodyObject == null) return;
            
            _cancellationToken.Cancel();
            
            _currentRigidbodyObject.velocity = Vector3.zero;
            _currentRigidbodyObject.collisionDetectionMode = CollisionDetectionMode.Discrete;
            _currentRigidbodyObject.useGravity = true;
            
            if (isThrow) _currentRigidbodyObject.AddForce(_playerCamera.transform.forward * _trowForce, ForceMode.Impulse);

            Physics.IgnoreCollision(_playerCollider, _currentColliderObject, false);
            _currentRigidbodyObject = null;
        }

        private async void Follow()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                _currentRigidbodyObject.velocity = (transform.position - _currentRigidbodyObject.transform.position) * _followSpeed;
                await Task.Delay(20, _cancellationToken.Token);
            }
        }

        private void Drop(InputAction.CallbackContext _) => Drop();
        private void Throw(InputAction.CallbackContext _) => Drop(true);
    }
}