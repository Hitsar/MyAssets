using System;
using System.Collections;
using System.Collections.Generic;
using Pool;
using UnityEngine;

namespace Gun
{
    public class Gun : MonoBehaviour
    {
        [SerializeField] private Bullet _bullet;
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private float _rateOfFire = 0.5f;
        [SerializeField] private float _recharge = 3;
        [SerializeField] private int _maxClip = 20;
        [SerializeField] private bool _isMachineGun;
        [Header("Pool")]
        [SerializeField] private Transform _container;
        [SerializeField] private int _quantity = 1;
    
        private readonly List<Bullet> _pool = new List<Bullet>();
    
        private SpriteRenderer _spriteRenderer;
        private InputSystem _inputSystem;
        private ObjectPool _objectPool;
    
        private Camera _main;
        private float _time;
        private int _currentClip;
        private bool _canShoot = true;
        
        public event Action<int> OnShoot;
        public event Action OnRecharge;

        private void Start()
        {
            _main = Camera.main;
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _objectPool = new ObjectPool(_bullet.gameObject, _container, _quantity);
            _currentClip = _maxClip;

            _inputSystem = new InputSystem();
            _inputSystem.Gun.Recharge.performed += _ => StartCoroutine(Recharge());
            _inputSystem.Gun.Enable();
        }

        private void FixedUpdate()
        {
            Vector3 difference = _main.ScreenToWorldPoint(_inputSystem.Gun.ShootDirection.ReadValue<Vector2>()) - transform.position;
            float rotateZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, rotateZ);

            _spriteRenderer.flipY = rotateZ >= 90 || rotateZ <= -90;
            transform.localScale = transform.parent.localScale.x == -1 ? Vector2.left + Vector2.up: Vector2.one;
            
            if (!_canShoot) return;
            
            _time += Time.deltaTime;
            if (_time < _rateOfFire) return;
            
            if (_isMachineGun)
            {
                if (_inputSystem.Gun.Shoot.IsPressed()) Shoot();
            }
            else
            {
                if (_inputSystem.Gun.Shoot.WasPerformedThisFrame()) Shoot();
            }
        }

        protected virtual void Shoot()
        {
            if (_objectPool.TryGetObject(out GameObject bullet))
            {
                bullet.gameObject.transform.position = _spawnPoint.position;
                bullet.gameObject.transform.rotation = _spawnPoint.rotation;
                bullet.gameObject.transform.gameObject.SetActive(true);
                _time = _recharge;
            }
            _currentClip--;
            if (_currentClip == 0) StartCoroutine(Recharge());
            _time = 0;
            
            OnShoot?.Invoke(_currentClip);
        }
        
        private IEnumerator Recharge()
        {
            OnRecharge?.Invoke();
            
            WaitForSeconds recharge = new WaitForSeconds(_recharge);
            
            _canShoot = false;
            yield return recharge;
            _canShoot = true;
            _currentClip = _maxClip;
            _time = _rateOfFire;
        }
    }
}