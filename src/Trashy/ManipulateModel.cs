using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Trashy
{
    public class ManipulateModel : MonoBehaviour
    {
        private static readonly Func<List<APITrackingDataParam>, Tuple<string, string>> s_injectData;

        private bool _applied;
        private Vector3 _initialPosition;

        static ManipulateModel()
        {
            var fi = typeof(Executor_InjectParameterDataRequest).GetField(
                "apiTrackingData",
                BindingFlags.Static | BindingFlags.NonPublic
            );
            if (fi == null)
            {
                Log.Error<ManipulateModel>("Unable to find field 'apiTrackingData'");
                return;
            }

            var apiTrackingData = (APITrackingData)fi.GetValue(null);
            if (apiTrackingData == null)
            {
                Log.Error<ManipulateModel>("apiTrackingData is null");
                return;
            }

            var mi = typeof(APITrackingData).GetMethod(
                "NewTrackingDataArrived",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            if (mi == null)
            {
                Log.Error<ManipulateModel>("Unable to find method 'NewTrackingDataArrived'");
                return;
            }

            s_injectData = (Func<List<APITrackingDataParam>, Tuple<string, string>>)mi.CreateDelegate(
                typeof(Func<List<APITrackingDataParam>, Tuple<string, string>>),
                apiTrackingData
            );
        }

        private void Start()
        {
            _initialPosition = transform.position;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_applied)
                return;

            _applied = true;
            var power = ConfigManager.ManipulateModelPower.Value;
            var direction = (transform.position - _initialPosition).normalized;
            Manipulate("FaceAngleX", direction.x * power, 0.8f, 0.1f);
            Manipulate("FaceAngleY", power, 1, 0.1f);
            Manipulate("FaceAngleZ", direction.x * power, 0.8f, 0.1f);
            Manipulate("EyeOpenLeft", 0, 1, 0.2f);
            Manipulate("EyeOpenRight", 0, 1, 0.2f);
        }

        private static void Manipulate(string id, float value, float weight, float time)
        {
            s_injectData(new List<APITrackingDataParam>
            {
                new APITrackingDataParam(id, "Trashy", false, value, weight, time)
            });
        }
    }
}
