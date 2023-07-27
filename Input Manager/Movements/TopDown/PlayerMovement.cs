using System.Collections;
using UnityEngine;

namespace Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float _walkSpeed = 4;
        [SerializeField] private float _sprintSpeed = 6;
        [SerializeField] private float _dashForce = 6;
        [SerializeField] private float _dashDuration = 0.1f;
        [SerializeField] private float _dashCooldown = 1;

        private Vector2 _direction;
        private Rigidbody2D _rigidbody;

        private bool _canDashing = true;
        private bool _isDashing;

        private void Start() => _rigidbody = GetComponent<Rigidbody2D>();

        private void Update()
        {
            _direction = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            if (Input.GetKey(KeyCode.Space)) StartCoroutine(Dash());
        }

        private void FixedUpdate() { if (!_isDashing) _rigidbody.velocity = _direction *= Input.GetKey(KeyCode.LeftShift) ? _sprintSpeed : _walkSpeed; }

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