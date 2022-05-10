using System;
using System.Collections;
using System.IO;
using System.Linq;
using BepInEx;
using Newtonsoft.Json;
using Trashy.Twitch;
using Trashy.UI;
using UnityEngine;
using UnityEngine.Networking;

namespace Trashy
{
    [BepInPlugin("TrashyPlugin", "Throw trash at the VTuber", Version)]
    public class TrashyPlugin : BaseUnityPlugin
    {
        public const string Version = "0.3.3";

        public static AssetBundle Bundle;
        public static VTubeStudioModelLoader ModelLoader;
        public static VTubeStudioModel Model;

        private readonly SlowHeadFinder _headFinder;

        public TrashyPlugin()
        {
            ConfigManager.Initialize(Config);

            var bundlePath = Path.Combine(Paths.PluginPath, "Trashy", "trashyskin");
            if (!File.Exists(bundlePath))
                Log.Warn("Bundle not found");
            else
                Bundle = AssetBundle.LoadFromMemory(File.ReadAllBytes(bundlePath));

            gameObject.AddComponent<SpriteManager>();
            gameObject.AddComponent<PubSubService>();
            gameObject.AddComponent<ChatService>();
            gameObject.AddComponent<UIManager>();
            _headFinder = gameObject.AddComponent<SlowHeadFinder>();
            gameObject.AddComponent<ItemSpawner>();
            gameObject.AddComponent<SoundManager>();

            Physics.gravity = new Vector3(0, -100, 0);
        }

        private void Start()
        {
            StartCoroutine(WaitForModelLoader());
            StartCoroutine(CheckForUpdates());
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

            Log.Info("Finding head");
            yield return new WaitForSeconds(5);
            yield return _headFinder.FindHeadAsync();

            Log.Info("Found head");
        }

        private IEnumerator CheckForUpdates()
        {
            using (var request = UnityWebRequest.Get("https://api.github.com/repos/wtfblub/trashy/releases"))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        var releases = JsonConvert.DeserializeObject<GitHubRelease[]>(request.downloadHandler.text);
                        var latestRelease = releases?.OrderByDescending(x => x.Version).FirstOrDefault();
                        if (latestRelease != null &&
                            latestRelease.Version != null &&
                            latestRelease.Version > new Version(Version))
                        {
                            UIManager.GetWindow<MessageWindow>().Show(
                                "Trashy - New version available",
                                $"\"{latestRelease.Name}\" is now available!",
                                latestRelease.Url
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error<TrashyPlugin>($"Unable to check for updates: \n{ex}");
                    }
                }
            }
        }
    }

    public class GitHubRelease
    {
        [JsonProperty("html_url")]
        public string Url { get; set; }

        [JsonProperty("tag_name")]
        public string Tag { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonIgnore]
        public Version Version => GetVersion();

        private Version GetVersion()
        {
            if (string.IsNullOrWhiteSpace(Tag))
                return null;

            return Version.TryParse(Tag.Remove(0, 1), out var version) ? version : null;
        }
    }
}
