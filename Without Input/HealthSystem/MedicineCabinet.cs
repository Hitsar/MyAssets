using UnityEngine;

namespace HealthSystem
{
    public class MedicineCabinet : MonoBehaviour
    {
        [SerializeField] private int _health = 1;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.TryGetComponent(out IHealthSystem healthSystem))
                healthSystem.Recovery(_health);
        }
    }
}