using System.Collections;
using UnityEngine;

namespace Trashy
{
    public class Sticky : MonoBehaviour
    {
        private bool _appliedStick;

        private void OnCollisionEnter(Collision collision)
        {
            if (_appliedStick)
                return;

            _appliedStick = true;
            var body = GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.velocity = Vector3.zero;

            StartCoroutine(StickTimer());
        }

        private IEnumerator StickTimer()
        {
            yield return new WaitForSeconds(ConfigManager.StickyDuration.Value);

            GetComponent<Rigidbody>().isKinematic = false;
        }
    }
}
