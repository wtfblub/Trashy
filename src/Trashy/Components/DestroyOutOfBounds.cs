using UnityEngine;

namespace Trashy.Components
{
    public class DestroyOutOfBounds : MonoBehaviour
    {
        private float _spawnTime;

        private void Start()
        {
            _spawnTime = Time.realtimeSinceStartup;
        }

        private void Update()
        {
            if (Time.realtimeSinceStartup - _spawnTime >= 5)
            {
                ItemSpawner.CurrentItemColliders.Remove(GetComponent<Collider>());
                Destroy(gameObject);
            }
            else if (transform.position.y <= -200)
            {
                ItemSpawner.CurrentItemColliders.Remove(GetComponent<Collider>());
                Destroy(gameObject);
            }
        }
    }
}
