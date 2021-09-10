using System;
using System.Collections;
using UnityEngine;

namespace Trashy
{
    public class SlowHeadFinder : MonoBehaviour
    {
        private float _lastSearch;
        private Vector3 _lastPosition;
        private bool _isRunning;

        public TimeSpan CacheDuration { get; set; } = TimeSpan.FromSeconds(5);

        public void FindHead(VTubeStudioModelLoader modelLoader, Action<Vector3> onFinished)
        {
            if (_isRunning || Time.realtimeSinceStartup - _lastSearch < CacheDuration.TotalSeconds)
            {
                onFinished(_lastPosition);
                return;
            }

            _isRunning = true;
            StartCoroutine(FindHeadAsync(modelLoader, onFinished));
        }

        private IEnumerator FindHeadAsync(VTubeStudioModelLoader modelLoader, Action<Vector3> onFinished)
        {
            var model = modelLoader.VTubeStudioModel();
            var modelTransform = modelLoader.ModelTransformController.transform;
            var scale = modelTransform.localScale.x;
            var position = modelTransform.position;

            while (model.Raycast(position))
            {
                position.y += 2 * scale;
                yield return new WaitForEndOfFrame();
            }

            _lastSearch = Time.realtimeSinceStartup;
            _lastPosition = position;
            _isRunning = false;
            onFinished(position);
        }
    }
}
