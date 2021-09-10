using UnityEngine;

namespace Trashy
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
                Destroy(gameObject);
            else if (transform.position.y <= -200)
                Destroy(gameObject);
        }
    }
}
