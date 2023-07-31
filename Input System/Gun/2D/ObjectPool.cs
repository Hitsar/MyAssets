using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pool
{
    public sealed class ObjectPool
    {
        private readonly List<GameObject> _pool = new List<GameObject>();
    
        public ObjectPool(GameObject prefab, Transform container, int quantity)
        {
            for (int i = 0; i < quantity; i++) 
    	        CreateObject(prefab, container);
        }
    
        public ObjectPool(GameObject[] prefabs, Transform container, int quantity)
        {
            for (int i = 0; i < quantity; i++)
            {
                int randomPrefab = Random.Range(0, prefabs.Length);
               
    		    CreateObject(prefabs[randomPrefab], container);
            }
        }
    
    	private void CreateObject(GameObject prefab, Transform container)
    	{
    	    GameObject spawned = Object.Instantiate(prefab, container);
    
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
                if (verifiedObjects >= _pool.Count) {return result == null;}
            }
        }
    }
}