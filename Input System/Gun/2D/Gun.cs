using System.Collections.Generic;
using Pool;
using UnityEngine;

namespace Gun
{
    public class Gun : MonoBehaviour
    {
        [SerializeField] private Bullet _bullet;
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private float _recharge = 0.5f;
        [Header("Pool")]
        [SerializeField] private GameObject _container;
        [SerializeField] private int _quantity = 1;
    
        private readonly List<Bullet> _pool = new List<Bullet>();
    
        private SpriteRenderer _spriteRenderer;
        private InputSystem _inputSystem;
        private ObjectPool _objectPool;
    
        private Camera _main;
        private float _time;

        private void Start()
        {
            _main = Camera.main;
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _objectPool = new ObjectPool(_bullet.gameObject, _container, _quantity);

            _inputSystem = new InputSystem();
            _inputSystem.Gun.Shoot.performed += _ => Shoot();
            _inputSystem.Gun.Enable();
        }

        private void FixedUpdate()
        {
            Vector3 difference = _main.ScreenToWorldPoint(_inputSystem.Gun.ShootDirection.ReadValue<Vector2>()) - transform.position;
            float rotateZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, rotateZ);

            _time -= Time.fixedDeltaTime;
            _spriteRenderer.flipY = rotateZ >= 90 || rotateZ <= -90;
            transform.localScale = transform.parent.localScale.x == -1 ? Vector2.left + Vector2.up: Vector2.one;
        }

        protected virtual void Shoot()
        {
            if (_time <= 0 && _objectPool.TryGetObject(out GameObject bullet))
            {
                bullet.gameObject.transform.position = _spawnPoint.position;
                bullet.gameObject.transform.rotation = _spawnPoint.rotation;
                bullet.gameObject.transform.gameObject.SetActive(true);
                _time = _recharge;
            }
        }
    }
}