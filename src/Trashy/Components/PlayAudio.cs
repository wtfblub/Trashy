using UnityEngine;

namespace Trashy.Components
{
    public class PlayAudio : MonoBehaviour
    {
        private bool _applied;

        private void OnCollisionEnter(Collision collision)
        {
            if (_applied)
                return;

            _applied = true;
            SoundManager.Play();
        }
    }
}
