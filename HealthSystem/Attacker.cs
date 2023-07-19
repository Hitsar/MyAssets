using UnityEngine;

namespace HealthSystem
{
    public class Attacker : MonoBehaviour
    {
        [SerializeField] private int _damage = 1;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.TryGetComponent(out IHealthSystem healthSystem))
                healthSystem.ApplyDamage(_damage);
        }
    }
}