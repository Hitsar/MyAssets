using UnityEngine;

public class Gun : BulletPool
{
    [SerializeField] Bullet _bullet;
    [SerializeField] Transform _spawnPoint;
    [SerializeField] private float _recharge;

    private InputSystem _inputSystem;
    private float _time;
    private Camera _main;

    private void Start()
    {
        Initialized(_bullet);
        _main = Camera.main;

        _inputSystem = new InputSystem();
        _inputSystem.Gun.Shoot.performed += _ => Shoot();
        _inputSystem.Gun.Enable();
    }

    private void FixedUpdate()
    {
        Vector3 diference = _main.ScreenToWorldPoint(_inputSystem.Gun.ShootDirection.ReadValue<Vector2>()) - transform.position;
        float rotateZ = Mathf.Atan2(diference.y, diference.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, rotateZ);
        
        _time -= Time.fixedDeltaTime;
    }

    protected virtual void Shoot()
    {
        if (_time <= 0 && TryGetObject(out Bullet bullet))
        {
            bullet.transform.position = _spawnPoint.position;
            bullet.transform.rotation = _spawnPoint.rotation;
            bullet.gameObject.SetActive(true);
            _time = _recharge;
        }
    }
}