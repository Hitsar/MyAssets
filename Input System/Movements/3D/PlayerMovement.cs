using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        public bool useJobs;
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
        private NativeArray<Vector2> _outputCamera;
        private NativeArray<Vector3> _outputVelocity;
        private CameraRotateCalculation _cameraRotateCalculation;
        private VelocityCalculation _velocityCalculation;

        private void Start()
        {
            _inputSystem = new InputSystem();
            _inputSystem.Player.Jump.performed += Jump;
            
            if (useJobs)
            {
                _inputSystem.Player.Move.performed += CalculateMoveAndLook;
                _inputSystem.Player.Move.canceled += CalculateMoveAndLook;
                _inputSystem.Player.Look.performed += CalculateMoveAndLook;
                _inputSystem.Player.Sprint.performed += CalculateMoveAndLook;
                _inputSystem.Player.Sprint.canceled += CalculateMoveAndLook;
            }
            else
            {
                _inputSystem.Player.Move.performed += Move;
                _inputSystem.Player.Move.canceled += Move;
                _inputSystem.Player.Look.performed += Look;
                _inputSystem.Player.Sprint.performed += Move;
                _inputSystem.Player.Sprint.canceled += Move;
            }

            _inputSystem.Player.Enable();

            _characterController = GetComponent<CharacterController>();
            
            _outputCamera = new NativeArray<Vector2>(2, Allocator.Persistent); 
            _outputVelocity = new NativeArray<Vector3>(2, Allocator.Persistent);
            _cameraRotateCalculation = new CameraRotateCalculation
            {
                Rotation = _outputCamera,
                RotateSpeed = _rotateSpeed
            };
            _velocityCalculation = new VelocityCalculation
            {
                Velocity = _outputVelocity,
                WalkSpeed = _walkSpeed, 
                RunSpeed = _runSpeed
            };
            
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void CalculateMoveAndLook(InputAction.CallbackContext callbackContext)
        {
            _cameraRotateCalculation.Rotation[0] = _rotation;
            _cameraRotateCalculation.DeltaTime = Time.deltaTime;
            _cameraRotateCalculation.MouseDelta = _inputSystem.Player.Look.ReadValue<Vector2>();
            
            _velocityCalculation.Velocity[0] = _velocity;
            _velocityCalculation.CameraAnglesY = _cameraRotate.localEulerAngles.y;
            _velocityCalculation.Direction = _inputSystem.Player.Move.ReadValue<Vector2>();
            _velocityCalculation.IsSprint = _inputSystem.Player.Sprint.IsPressed();
            
            JobHandle a = _cameraRotateCalculation.Schedule();
            JobHandle b = _velocityCalculation.Schedule();
            JobHandle.CompleteAll(ref a, ref b);

            _velocity = _outputVelocity[1];
            _rotation = _outputCamera[1];
            
            _cameraRotate.localEulerAngles = _rotation;
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
            if (useJobs)
            {
                _inputSystem.Player.Move.performed -= CalculateMoveAndLook;
                _inputSystem.Player.Look.performed -= CalculateMoveAndLook;
                _inputSystem.Player.Move.canceled -= CalculateMoveAndLook;
                _inputSystem.Player.Look.canceled -= CalculateMoveAndLook;
                _inputSystem.Player.Sprint.performed -= CalculateMoveAndLook;
                _inputSystem.Player.Sprint.canceled -= CalculateMoveAndLook;
            }
            else
            {
                _inputSystem.Player.Move.performed -= Move;
                _inputSystem.Player.Look.performed -= Look;
                _inputSystem.Player.Move.canceled -= Move;
                _inputSystem.Player.Sprint.performed -= Move;
                _inputSystem.Player.Sprint.canceled -= Move;
            }
            _inputSystem.Player.Disable();
            
            _outputCamera.Dispose();
            _outputVelocity.Dispose();
        }
        
        [BurstCompile]
        private struct CameraRotateCalculation : IJob
        {
            [ReadOnly] public float DeltaTime;
            [ReadOnly] public Vector2 MouseDelta;
            [ReadOnly] public float RotateSpeed;
            public NativeArray<Vector2> Rotation;

            public void Execute()
            {
                Vector2 rotation = Rotation[0];
                MouseDelta *= RotateSpeed * DeltaTime;
                rotation.y += MouseDelta.x;
                rotation.x = Mathf.Clamp(rotation.x - MouseDelta.y, -90, 90);
                Rotation[1] = rotation;
            }
        }

        [BurstCompile]
        private struct VelocityCalculation : IJob
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
    }
}