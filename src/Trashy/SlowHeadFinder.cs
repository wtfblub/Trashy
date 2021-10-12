using System.Collections;
using UnityEngine;

namespace Trashy
{
    public class SlowHeadFinder : MonoBehaviour
    {
        private float? _distanceY;

        public Vector3 GetHead()
        {
            var modelTransform = TrashyPlugin.ModelLoader.ModelTransformController.transform;
            var position = modelTransform.position;

            if (_distanceY == null)
                return position;

            return new Vector3(position.x, position.y + _distanceY.Value * modelTransform.localScale.y, position.z);
        }

        public IEnumerator FindHeadAsync()
        {
            _distanceY = null;
            var model = TrashyPlugin.Model;
            var modelTransform = TrashyPlugin.ModelLoader.ModelTransformController.transform;
            var scale = modelTransform.localScale.x;
            var position = modelTransform.position;

            while (model.Raycast(position))
            {
                position.y += 2 * scale;
                yield return new WaitForEndOfFrame();
            }

            // Get Y distance on 1.0 scale
            _distanceY = (position.y - modelTransform.position.y) / modelTransform.localScale.y;
        }
    }
}
