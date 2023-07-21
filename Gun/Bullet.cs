using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float _speed = 10;
    [SerializeField] private float _lifeTime = 3;
    private float _time;

    private void FixedUpdate()
    {
        transform.Translate(Vector2.right * (_speed * Time.fixedDeltaTime));
        
        _time += Time.fixedDeltaTime;
        if (_time < _lifeTime) return;
        gameObject.SetActive(false);
        _time = 0;
    }

    private void OnTriggerEnter2D(Collider2D collision) => gameObject.SetActive(false);
}
