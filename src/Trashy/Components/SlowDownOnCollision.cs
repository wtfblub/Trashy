using UnityEngine;

namespace Trashy.Components
{
    public class SlowDownOnCollision : MonoBehaviour
    {
        private bool _applied;

        private void OnCollisionEnter(Collision collision)
        {
            if (_applied)
                return;

            _applied = true;
            var body = GetComponent<Rigidbody>();
            body.velocity *= Random.Range(0.01f, 0.5f);
        }
    }
}
