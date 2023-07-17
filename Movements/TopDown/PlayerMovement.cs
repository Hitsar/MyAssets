using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float _walkSpeed = 4;
        [SerializeField] private float _sprintSpeed = 6;

        private Vector2 _direction;
        private Rigidbody2D _rigidbody;
        private InputSystem _inputSystem;

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            
            _inputSystem = new InputSystem();
            _inputSystem.Player.Enable();
        }
        
        private void FixedUpdate()
        {
             _direction = _inputSystem.Player.Move.ReadValue<Vector2>();
             Vector2 scaledDirection = _direction;

             scaledDirection *= _inputSystem.Player.Sprint.ReadValue<float>() > 0 ? _sprintSpeed : _walkSpeed;
            
             _rigidbody.velocity = scaledDirection;
        }    
    }
}