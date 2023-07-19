using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float _speed; 
    private int _damage;

    private void FixedUpdate() =>  transform.Translate(Vector2.right * _speed * Time.fixedDeltaTime);

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent(out IHealthSystem healthSystem))
            healthSystem.TakeDamage(_damage);
        gameObject.SetActive(false);
    }
}
