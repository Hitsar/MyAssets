using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
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
        
        [SerializeField] private float _pickUpDistance = 5;
        [SerializeField] private float _trowForce = 6;
        [SerializeField] private LayerMask _canPickUpLayer;

        private InputSystem _inputSystem;
        private CharacterController _characterController;
        private Collider _playerCollider;
        private Camera _playerCamera;

        private Vector3 _velocity;
        private Vector2 _rotation;
        private NativeArray<Vector2> _outputCamera;
        private NativeArray<Vector3> _outputVelocity;

        private Joint _joint;
        private Rigidbody _currentRigidbodyObject;
        private Collider _currentColliderObject;

        private void Start()
        {
            _inputSystem = new InputSystem();
            _inputSystem.Player.Jump.performed += _ => Jump();
            _inputSystem.Player.PickUp.performed += _ => PickUp();
            _inputSystem.Player.PickUp.canceled += _ => Drop();
            _inputSystem.Player.Trow.performed += _ => Drop(true);
            _inputSystem.Player.Enable();

            _characterController = GetComponent<CharacterController>();
            _playerCollider = GetComponent<Collider>();
            _playerCamera = GetComponentInChildren<Camera>();
            _joint = GetComponentInChildren<Joint>();
            
            _outputCamera = new NativeArray<Vector2>( 2, Allocator.Persistent); 
            _outputVelocity = new NativeArray<Vector3>(2, Allocator.Persistent);
            
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            NativeArray<JobHandle> jobs = new NativeArray<JobHandle>(2, Allocator.Temp);

            _outputCamera[0] = _rotation;
            _outputVelocity[0] = _velocity;

            CameraRotateCalculation cameraRotateCalculation = new CameraRotateCalculation
            {
                Rotation = _outputCamera,
                DeltaTime = Time.deltaTime,
                MouseDelta = _inputSystem.Player.Look.ReadValue<Vector2>(),
                RotateSpeed = _rotateSpeed
            };
            VelocityCalculation velocityCalculation = new VelocityCalculation
            {
                Velocity = _outputVelocity,
                WalkSpeed = _walkSpeed,
                RunSpeed = _runSpeed,
                CameraAnglesY = _playerCamera.transform.localEulerAngles.y,
                Direction = _inputSystem.Player.Move.ReadValue<Vector2>(),
                IsSprint = _inputSystem.Player.Sprint.IsPressed()
            };

            jobs[0] = cameraRotateCalculation.Schedule();
            jobs[1] = velocityCalculation.Schedule();
            JobHandle.CompleteAll(jobs);

            _rotation = _outputCamera[1];
            _velocity = _outputVelocity[1];
            

            _playerCamera.transform.localEulerAngles = _rotation;
            _characterController.Move(_velocity * Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (_characterController.isGrounded) _velocity.y = -0.1f;
            else _velocity.y += _gravity * Time.fixedDeltaTime;
        }

        private void Jump() { if (_characterController.isGrounded) _velocity.y = _jumpForce; }

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

        private void Drop(bool isThrow = false)
        {
            if (_currentRigidbodyObject == null) return;
            
            _joint.connectedBody = null;

            _currentRigidbodyObject.drag = 0;
            _currentRigidbodyObject.velocity = _velocity;
            if (isThrow) _currentRigidbodyObject.AddForce(_playerCamera.transform.forward * _trowForce, ForceMode.Impulse);

            Physics.IgnoreCollision(_playerCollider, _currentColliderObject, false);
            
            _currentRigidbodyObject = null;
        }

        private void OnDestroy()
        {
            _inputSystem.Player.Jump.performed -= _ => Jump();
            _inputSystem.Player.PickUp.performed -= _ => PickUp();
            _inputSystem.Player.PickUp.canceled -= _ => Drop();
            _inputSystem.Player.Trow.performed -= _ => Drop(true);
            _inputSystem.Player.Disable();
            
            _outputCamera.Dispose();
            _outputVelocity.Dispose();
        }
    }
}

[BurstCompile]
public struct CameraRotateCalculation : IJob
{
    [ReadOnly] public float DeltaTime;
    [ReadOnly] public Vector2 MouseDelta;
    [ReadOnly] public float RotateSpeed;
    public NativeArray<Vector2> Rotation;

    public void Execute()
    {
        Vector2 rotation = Rotation[0];
        if (MouseDelta.sqrMagnitude < 0.1f) return;

        MouseDelta *= RotateSpeed * DeltaTime;
        rotation.y += MouseDelta.x;
        rotation.x = Mathf.Clamp(rotation.x - MouseDelta.y, -90, 90);
        Rotation[1] = rotation;
    }
}

[BurstCompile]
public struct VelocityCalculation : IJob
{
    [ReadOnly] public float WalkSpeed;
    [ReadOnly] public float RunSpeed;
    [ReadOnly] public float CameraAnglesY;
    [ReadOnly] public bool IsSprint;
    [ReadOnly] public Vector2 Direction;
    public NativeArray<Vector3> Velocity;
    
    public void Execute()
    {
        Vector3 velocity = Velocity[0];
        Direction *= IsSprint ? RunSpeed : WalkSpeed;
        Vector3 move = Quaternion.Euler(0, CameraAnglesY, 0) * new Vector3(Direction.x, 0, Direction.y);
        velocity = new Vector3(move.x, velocity.y, move.z);
        Velocity[1] = velocity;
    }
}