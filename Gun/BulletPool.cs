using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    [SerializeField] private GameObject _container;
    [SerializeField] private int _capacity;
    private List<Bullet> _pool = new List<Bullet>();

    protected void Initialized(Bullet bullet)
    {
        for (int i = 0; i < _capacity; i++)
        {
            Bullet spawned = Instantiate(bullet, _container.transform);
            spawned.gameObject.SetActive(false);

            _pool.Add(spawned);
        }
    }

    protected bool TryGetObject(out Bullet result)
    {
        result = _pool.FirstOrDefault(p => p.gameObject.activeSelf == false);
        return result != null;
    }
}