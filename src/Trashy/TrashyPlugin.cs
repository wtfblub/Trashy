using System;
using System.Collections;
using System.IO;
using System.Linq;
using BepInEx;
using Newtonsoft.Json;
using Trashy.Twitch;
using UnityEngine;
using UnityEngine.Networking;

namespace Trashy
{
    [BepInPlugin("TrashyPlugin", "Throw trash at the VTuber", Version)]
    public class TrashyPlugin : BaseUnityPlugin
    {
        public const string Version = "0.1.5";

        public static AssetBundle Bundle;
        public static VTubeStudioModelLoader ModelLoader;
        public static VTubeStudioModel Model;

        private readonly SlowHeadFinder _headFinder;
        private GitHubRelease _newVersion;

        public TrashyPlugin()
        {
            gameObject.AddComponent<SpriteManager>();
            gameObject.AddComponent<TwitchRedeems>();
            gameObject.AddComponent<UIManager>();
            _headFinder = gameObject.AddComponent<SlowHeadFinder>();
            gameObject.AddComponent<ItemSpawner>();

            Config.SaveOnConfigSet = true;
            ConfigManager.ObjectCount = Config.Bind(
                "General",
                "ObjectCount",
                10,
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
            ConfigManager.StickyChance = Config.Bind(
                "General",
                "StickyChance",
                25,
                "The chance a item becomes sticky"
            );
            ConfigManager.StickyDuration = Config.Bind(
                "General",
                "StickyDuration",
                0.5f,
                "The duration in seconds how long a sticky item sticks"
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

        private void OnGUI()
        {
            if (_newVersion == null)
                return;

            if (UIManager.Skin != null)
                GUI.skin = UIManager.Skin;

            const int windowWidth = 400;
            const int windowHeight = 125;
            GUI.Window(
                2,
                new Rect(Screen.width / 2 - windowWidth / 2, Screen.height / 2 - windowHeight / 2, windowWidth, windowHeight),
                DrawNewUpdatePopup,
                "Trashy - New version available"
            );
        }

        private void DrawNewUpdatePopup(int windowId)
        {
            GUILayout.Space(20);
            GUILayout.BeginVertical();
            {
                GUILayout.Label(
                    "Trashy has a new version available!"
                );

                GUILayout.Space(20);
                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Open in browser", GUILayout.Width(125)))
                    Application.OpenURL(_newVersion.Url);

                if (GUILayout.Button("OK", GUILayout.Width(125)))
                    _newVersion = null;

                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
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
            yield return _headFinder.FindHeadAsync();

            Log.Info("Found head");
        }

        private IEnumerator CheckForUpdates()
        {
            using (var request = UnityWebRequest.Get("https://api.github.com/repos/wtfblub/trashy/releases"))
            {
                var a = request.GetRequestHeader("ser-agent");
                Log.Info(a);
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
                            _newVersion = latestRelease;
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
