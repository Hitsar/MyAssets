using System;
using System.Collections;
using HealthSystem;
using UnityEngine;

namespace Gun
{
    public class Gun : MonoBehaviour
    {
        [SerializeField] private int _damage = 1;
        [SerializeField] private float _rateOfFire = 0.1f;
        [SerializeField] private float _recharge = 3;
        [SerializeField] private int _maxClip = 20;
        [SerializeField] private Transform _originRay;
        [SerializeField] private bool _isMachineGun;
        
        private InputSystem _inputSystem;
        private float _time;
        private float _currentClip;
        private bool _canShoot = true;

        public event Action OnShoot;
        public event Action OnRecharge;

        private void Start()
        {
            _inputSystem = new InputSystem();
            _inputSystem.Gun.Enable();
            _currentClip = _maxClip;
            _time = _rateOfFire;
        }

        private void Update()
        {
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
            OnShoot?.Invoke();
            
            if (Physics.Raycast(_originRay.position, _originRay.forward, out RaycastHit hit) && hit.collider.gameObject.TryGetComponent(out IHealthSystem healthSystem))
                healthSystem.ApplyDamage(_damage);
            
            _currentClip--;
            if (_currentClip == 0) StartCoroutine(Recharge());
            _time = 0;
        }

        private IEnumerator Recharge()
        {
            OnRecharge?.Invoke();
            
            WaitForSeconds recharge = new WaitForSeconds(_recharge);
            
            _canShoot = false;
            yield return recharge;
            _canShoot = true;
            _time = _rateOfFire;
        }
    }
}