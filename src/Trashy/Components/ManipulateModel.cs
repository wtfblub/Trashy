using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Trashy.Components
{
    public class ManipulateModel : MonoBehaviour
    {
        private static readonly Action<List<APITrackingDataParamRequest>> s_injectData;

        private bool _applied;
        private Vector3 _initialPosition;

        static ManipulateModel()
        {
            var fi = typeof(VTubeStudioAPI).GetField(
                "executors",
                BindingFlags.Static | BindingFlags.NonPublic
            );
            if (fi == null)
            {
                Log.Error<ManipulateModel>("Unable to find field 'VTubeStudioAPI.executors'");
                return;
            }

            var apiExecutors = (APIExecutors)fi.GetValue(null);
            if (apiExecutors == null)
            {
                Log.Error<ManipulateModel>("VTubeStudioAPI.executors is null");
                return;
            }

            var mi = typeof(Executor_InjectParameterDataRequest).GetMethod(
                "applyNewAPITrackingData",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            if (mi == null)
            {
                Log.Error<ManipulateModel>("Unable to find method 'Executor_InjectParameterDataRequest.applyNewAPITrackingData'");
                return;
            }

            s_injectData = (Action<List<APITrackingDataParamRequest>>)mi.CreateDelegate(
                typeof(Action<List<APITrackingDataParamRequest>>),
                apiExecutors.ExecutorInstance_InjectParameterDataRequest
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
            s_injectData(new List<APITrackingDataParamRequest>
            {
                new APITrackingDataParamRequest(id, InjectParameterDataMode.add, "Trashy", false, value, weight, time)
            });
        }
    }
}
