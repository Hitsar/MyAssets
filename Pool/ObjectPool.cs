using System.Collections.Generic;
using UnityEngine;

namespace Pool
{
    public abstract sealed class ObjectPool
    {
        private List<GameObject> _pool = new List<GameObject>();

        public ObjectPool(GameObject prefab, GameObject container, int quantity)
    	{
            for (int i = 0; i < quantity; i++) 
	        CreateObject(prefab, container);
    	}

        public ObjectPool(GameObject[] prefabs, GameObject container, int quantity)
        {
            for (int i = 0; i < quantity; i++)
            {
                int randomPrefab = Random.Range(0, prefabs.Length);
               
		CreateObject(prefabs[randomIndex], container);
            }
        }

	private void CreateObject(GameObject prefab, GameObject container)
	{
	    GameObject spawned = Object.Instantiate(prefab, container.transform);

            spawned.SetActive(false);

            _pool.Add(spawned);
	}

	public bool TryGetObject(out GameObject result)
        {
            result = _pool.FirstOrDefault(p => p.gameObject.activeSelf == false);
            return result != null;
        }

        public bool TryGetRandomObject(out GameObject result)
        {
            int verifiedObjects = 0;
            while (true)
            {
                result = _pool[Random.Range(0, _pool.Count)];
                if (result.activeSelf == false) return result;
                
                verifiedObjects++;
                if (verifiedObjects >= _pool.Count) return result == null;
            }
        }
    }
}