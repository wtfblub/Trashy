using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Live2D.Cubism.Framework.Raycasting;
using UnityEngine;

namespace Trashy
{
    public static class VTubeStudioModelLoaderExtensions
    {
        private static readonly FieldInfo s_vtsModelFieldInfo;
        private static readonly FieldInfo s_allowLoadingNextModelFieldInfo;

        static VTubeStudioModelLoaderExtensions()
        {
            s_vtsModelFieldInfo = typeof(VTubeStudioModelLoader)
                .GetField("vtsModel", BindingFlags.Instance | BindingFlags.NonPublic);

            s_allowLoadingNextModelFieldInfo = typeof(VTubeStudioModelLoader)
                .GetField("allowLoadingNextModel", BindingFlags.Instance | BindingFlags.NonPublic);

            if (s_vtsModelFieldInfo == null)
                Log.Error($"Unable to find field 'vtsModel' on {nameof(VTubeStudioModelLoader)}");

            if (s_vtsModelFieldInfo == null)
                Log.Error($"Unable to find field 'allowLoadingNextModel' on {nameof(VTubeStudioModelLoader)}");
        }

        public static VTubeStudioModel VTubeStudioModel(this VTubeStudioModelLoader loader)
        {
            if (s_vtsModelFieldInfo == null)
                return null;

            return (VTubeStudioModel)s_vtsModelFieldInfo.GetValue(loader);
        }

        public static bool AllowLoadingNextModel(this VTubeStudioModelLoader loader)
        {
            if (s_allowLoadingNextModelFieldInfo == null)
                return false;

            return (bool)s_allowLoadingNextModelFieldInfo.GetValue(loader);
        }
    }

    public static class VTubeStudioModelExtensions
    {
        private static readonly MethodInfo s_setRaycastableMethodInfo;

        static VTubeStudioModelExtensions()
        {
            s_setRaycastableMethodInfo = typeof(VTubeStudioModel)
                .GetMethod("SetRaycastable", BindingFlags.Instance | BindingFlags.NonPublic);

            if (s_setRaycastableMethodInfo == null)
            {
                s_setRaycastableMethodInfo = typeof(VTubeStudioModel)
                    .GetMethod("SetModelRaycastable", BindingFlags.Instance | BindingFlags.NonPublic);
            }

            if (s_setRaycastableMethodInfo == null)
                Log.Error($"Unable to find method 'SetModelRaycastable' on {nameof(VTubeStudioModel)}");
        }

        public static void SetRaycastable(this VTubeStudioModel model, bool value)
        {
            s_setRaycastableMethodInfo.Invoke(model, new object[] { value });
        }

        public static bool Raycast(this VTubeStudioModel model, Vector3 position)
        {
            var ray = new Ray(position, Vector3.forward);
            var array = new CubismRaycastHit[model.Live2DModel.Drawables.Length];

            model.SetRaycastable(true);
            var count = model.Live2DModel.GetComponent<CubismRaycaster>().Raycast(ray, array, float.PositiveInfinity);
            model.SetRaycastable(false);

            return array
                .Take(count)
                .OrderBy(x => x.Drawable.GetComponent<MeshRenderer>().sortingOrder)
                .Any(x => !x.Drawable.name.ToLower().Contains("hitarea"));
        }
    }

    public static class ConfigWindowControllerExtensions
    {
        private static readonly FieldInfo s_windowTabsFieldInfo;

        static ConfigWindowControllerExtensions()
        {
            s_windowTabsFieldInfo = typeof(ConfigWindowController)
                .GetField("windowTabs", BindingFlags.Instance | BindingFlags.NonPublic);

            if (s_windowTabsFieldInfo == null)
                Log.Error($"Unable to find field 'windowTabs' on {nameof(ConfigWindowController)}");
        }

        public static List<ConfigWindowTab> WindowTabs(this ConfigWindowController configWindowController)
        {
            return (List<ConfigWindowTab>)s_windowTabsFieldInfo.GetValue(configWindowController);
        }
    }

    public static class ListExtensions
    {
        public static T Random<T>(this IReadOnlyList<T> list)
        {
            if (list.Count == 0)
                return default;

            return list[UnityEngine.Random.Range(0, list.Count)];
        }
    }

    public static class TaskExtensions
    {
        public static ConfiguredTaskAwaitable AnyContext(this Task task)
        {
            return task.ConfigureAwait(false);
        }

        public static ConfiguredTaskAwaitable<T> AnyContext<T>(this Task<T> task)
        {
            return task.ConfigureAwait(false);
        }
    }

    public static class AsyncOperationExtensions
    {
        // Support await on unity AsyncOperations
        public static TaskAwaiter GetAwaiter(this AsyncOperation asyncOp)
        {
            var tcs = new TaskCompletionSource<object>();
            asyncOp.completed += obj => tcs.SetResult(null);
            return ((Task)tcs.Task).GetAwaiter();
        }
    }
}
