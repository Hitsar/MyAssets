using UnityEngine;

namespace Pool
{
    public class Spawner : MonoBehaviour
    {
        [SerializeField] private GameObject[] _prefabObjects;
        [SerializeField] private Transform[] _spawners;

	[SerializeField] GameObject _container;
	[SerializeField] int _quantity;
        [SerializeField] private float _minSecondForSpawn, _maxSecondForSpawn;
        
	private ObjectPool _objectPool;
        private float _secondForSpawn;
        private float _time;

        private void Start()
        {
            _objectPool = new ObjectPool(_prefabObjects, _container, _quantity);
            _secondForSpawn = Random.Range(_minSecondForSpawn, _maxSecondForSpawn);
        }

        private void FixedUpdate()
        {
            _time += Time.fixedDeltaTime;
            
            if (_time >= _secondForSpawn && _objectPool.TryGetRandomObject(out GameObject spawnedObject))
            {
                int spawnPointNumber = Random.Range(0, _spawners.Length);
                
                spawnedObject.SetActive(true);
                spawnedObject.transform.position = _spawners[spawnPointNumber].position;
                
                _secondForSpawn = Random.Range(_minSecondForSpawn, _maxSecondForSpawn);
                _time = 0;
            }
        }
    }
}