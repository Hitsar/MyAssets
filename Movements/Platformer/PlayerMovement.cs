using System;
using System.Collections;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Move")]
        [SerializeField] private float _walkSpeed = 3; 
        [SerializeField] private float _sprintSpeed = 5.5f;
        [SerializeField] private float _couchSpeed = 1.5f;
        [Space]
        [SerializeField] private float _jumpForce = 5;
        [Header("Dash")]
        [SerializeField] private float _dashForce = 10;
        [SerializeField] private float _dashDuration = 0.1f;
        [SerializeField] private float _dashCooldown = 1;

        private Rigidbody2D _rigidbody;
        private InputSystem _inputSystem;
        private float _direction;

        #region Bools
        
        private bool _isDashing;
        private bool _canDashing = true;
        private bool _isCouching;
        private bool _isGround;
        private bool _isJump;
        private bool _isSprint;

        #endregion
        #region Events

        public event Action<bool> OnMove;
        public event Action<bool> OnSprint;
        public event Action OnJump;
        public event Action OnDash;
        public event Action<bool> OnCouch;
        public event Action<bool> OnFall;

        #endregion
        
        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody2D>();

            PlayerVFX vfx = new PlayerVFX(this, GetComponent<Animator>());

            _inputSystem = new InputSystem();
            _inputSystem.Player.Jump.performed += _ => Jump();
            _inputSystem.Player.Dash.performed += _ => StartCoroutine(Dash());
            _inputSystem.Player.Enable();
        }

        private void FixedUpdate()
        {
            if (_isDashing) return;
            
            _direction = _inputSystem.Player.Move.ReadValue<float>();
            _isSprint = _inputSystem.Player.Sprint.ReadValue<float>() > 0;
            _isCouching = _inputSystem.Player.Coutch.ReadValue<float>() > 0;
            
            OnMove?.Invoke(_direction == 0);
            OnSprint?.Invoke(_isSprint);
            OnCouch?.Invoke(_isCouching);
            
            float scaledDirection = _direction;

            if (_isCouching == false && _isSprint) scaledDirection *= _sprintSpeed;
            
            else if (_isCouching) scaledDirection *= _couchSpeed;
            
            else scaledDirection *= _walkSpeed;
            
            _rigidbody.velocity = new Vector2(scaledDirection, _rigidbody.velocity.y);
        }
        
        private void Jump()
        {
            if (_isCouching || _isDashing || _isGround == false) return;
            
            _rigidbody.velocity = new Vector2(_direction, 0);
            _rigidbody.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
            _isJump = true;
            
            OnJump.Invoke();
        }
        
        private IEnumerator Dash()
        {
            if (_canDashing == false) yield break;
            
            _canDashing = false;
            _isDashing = true;
            _rigidbody.velocity = new Vector2(_direction * _dashForce, 0);
            
            OnDash?.Invoke();
            yield return new WaitForSeconds(_dashDuration);
            _isDashing = false;
            yield return new WaitForSeconds(_dashCooldown);
            _canDashing = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            _isGround = true;
            _isJump = false;
            OnFall?.Invoke(false);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            _isGround = false;
            if (_isJump) return;
            OnFall?.Invoke(true);
        }
    }
}