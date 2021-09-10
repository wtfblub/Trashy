using System.Collections.Generic;
using System.IO;
using BepInEx;
using UnityEngine;

namespace Trashy
{
    public class SpriteManager : MonoBehaviour
    {
        private readonly List<Sprite> _sprites = new List<Sprite>();

        public IReadOnlyList<Sprite> Items => _sprites;
        public Sprite Icon { get; private set; }

        public void Load()
        {
            Unload();

            Log.Info("Loading sprites");

            var files = Directory.GetFiles(Path.Combine(Paths.PluginPath, "Trashy", "Items"), "*.png");
            foreach (var file in files)
            {
                var data = File.ReadAllBytes(file);
                var texture = new Texture2D(1, 1);
                texture.LoadImage(data);
                var sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f)
                );
                _sprites.Add(sprite);
            }

            var iconTexture = new Texture2D(1, 1);
            var iconPath = Path.Combine(Paths.PluginPath, "Trashy", "Icon.png");
            if (File.Exists(iconPath))
                iconTexture.LoadImage(File.ReadAllBytes(iconPath));

            Icon = Sprite.Create(
                iconTexture,
                new Rect(0, 0, iconTexture.width, iconTexture.height),
                new Vector2(0.0f, 0.0f)
            );
        }

        public void Unload()
        {
            Log.Info("Unloading sprites");
            foreach (var sprite in _sprites)
            {
                Destroy(sprite.texture);
                Destroy(sprite);
            }

            _sprites.Clear();
            Destroy(Icon);
        }

        private void Start()
        {
            Load();
        }

        private void OnDestroy()
        {
            Unload();
        }
    }
}
