using System.Collections;
using System.IO;
using BepInEx;
using Trashy.Twitch;
using UnityEngine;

namespace Trashy
{
    [BepInPlugin("TrashyPlugin", "Throw trash at the VTuber", Version)]
    public class TrashyPlugin : BaseUnityPlugin
    {
        public const string Version = "0.1.2";

        public static AssetBundle Bundle;
        public static VTubeStudioModelLoader ModelLoader;
        public static VTubeStudioModel Model;

        public TrashyPlugin()
        {
            gameObject.AddComponent<SpriteManager>();
            gameObject.AddComponent<TwitchRedeems>();
            gameObject.AddComponent<UIManager>();
            gameObject.AddComponent<ItemSpawner>();

            Config.SaveOnConfigSet = true;
            ConfigManager.ObjectCount = Config.Bind(
                "General",
                "ObjectCount",
                5,
                "The number of objects to throw for each request"
            );
            ConfigManager.SpriteSize = Config.Bind(
                "General",
                "SpriteSize",
                1280,
                "The size of sprites"
            );
            ConfigManager.RewardName = Config.Bind(
                "General",
                "RewardName",
                "",
                "The Channel Points Redeem name"
            );
            ConfigManager.TwitchToken = Config.Bind(
                "General",
                "TwitchToken",
                "",
                "The Twitch Api Token"
            );
            Physics.gravity = new Vector3(0, -100, 0);

            var bundlePath = Path.Combine(Paths.PluginPath, "Trashy", "trashyskin");
            if (!File.Exists(bundlePath))
                Log.Warn("Bundle not found");
            else
                Bundle = AssetBundle.LoadFromMemory(File.ReadAllBytes(Path.Combine(Paths.PluginPath, "Trashy", "trashyskin")));
        }

        private void Start()
        {
            StartCoroutine(WaitForModelLoader());
        }

        private void OnDestroy()
        {
            if (Bundle != null)
                Bundle.Unload(true);

            if (Model != null)
            {
                foreach (var drawable in Model.Live2DModel.Drawables)
                    Destroy(drawable.GetComponent<MeshCollider>());
            }
        }

        private IEnumerator WaitForModelLoader()
        {
            GameObject modelGameObject = null;
            Log.Info("Waiting for model loader gameobject");

            while ((modelGameObject = GameObject.Find("Live2DModel")) == null)
                yield return new WaitForSeconds(1);

            ModelLoader = modelGameObject.GetComponent<VTubeStudioModelLoader>();
            ModelLoader.modelLoadingFinished.AddListener(_ => { StartCoroutine(WaitForModel()); });
            if (ModelLoader.AllowLoadingNextModel())
                StartCoroutine(WaitForModel());
        }

        private IEnumerator WaitForModel()
        {
            Log.Info("Waiting for model to be loaded");
            Model = null;
            while (ModelLoader.AllowLoadingNextModel() != true)
                yield return new WaitForSeconds(1);

            Model = ModelLoader.VTubeStudioModel();

            Log.Info("Adding mesh colliders to model");
            foreach (var drawable in Model.Live2DModel.Drawables)
            {
                var meshFilter = drawable.GetComponent<MeshFilter>();
                var collider = meshFilter.gameObject.AddComponent<MeshCollider>();
                collider.convex = true;
                collider.sharedMesh = meshFilter.sharedMesh;
            }
        }
    }
}
