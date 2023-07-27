using System.Collections;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float _walkSpeed = 4;
        [SerializeField] private float _sprintSpeed = 6;
        [SerializeField] private float _dashForce = 6;
        [SerializeField] private float _dashDuration = 0.1f;
        [SerializeField] private float _dashCooldown = 1;

        private Rigidbody2D _rigidbody;
        private InputSystem _inputSystem;

        private bool _canDashing = true;
        private bool _isDashing;

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            
            _inputSystem = new InputSystem();
            _inputSystem.Player.Dash.performed += _ => StartCoroutine(Dash());
            _inputSystem.Player.Enable();
        }
        
        private void FixedUpdate()
        {
            if (_isDashing) return;
            
             Vector2 direction = _inputSystem.Player.Move.ReadValue<Vector2>();
             direction *= _inputSystem.Player.Sprint.IsPressed() ? _sprintSpeed : _walkSpeed;
             _rigidbody.velocity = direction;
        }

        private IEnumerator Dash()
        {
            if (_canDashing == false) yield break;
            
            _canDashing = false;
            _isDashing = true;
            _rigidbody.velocity *= _dashForce;
            
            yield return new WaitForSeconds(_dashDuration);
            _isDashing = false;
            yield return new WaitForSeconds(_dashCooldown);
            _canDashing = true;
        }
    }
}