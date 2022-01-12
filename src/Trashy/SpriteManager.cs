using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using UnityEngine;

namespace Trashy
{
    public class SpriteManager : MonoBehaviour
    {
        private readonly List<Sprite> _sprites = new List<Sprite>();
        private readonly Dictionary<string, List<Sprite>> _spriteGroups =
            new Dictionary<string, List<Sprite>>(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyList<Sprite> Items => _sprites;
        public IReadOnlyDictionary<string, List<Sprite>> Groups => _spriteGroups;
        public Sprite Icon { get; private set; }

        public void Load()
        {
            Unload();
            Log.Info("Loading sprites");

            var itemsDirectory = Path.Combine(Paths.PluginPath, "Trashy", "Items");
            foreach (var folder in Directory.EnumerateDirectories(itemsDirectory))
            {
                var files = Directory.GetFiles(folder, "*.png");
                if (files.Length == 0)
                    continue;

                var sprites = new List<Sprite>();
                _spriteGroups[Path.GetFileName(folder)] = sprites;

                foreach (var file in files)
                    sprites.Add(LoadSprite(file));
            }

            foreach (var file in Directory.GetFiles(itemsDirectory, "*.png"))
                _sprites.Add(LoadSprite(file));

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

            foreach (var sprites in _spriteGroups.Values)
            {
                foreach (var sprite in sprites)
                {
                    Destroy(sprite.texture);
                    Destroy(sprite);
                }
            }

            _sprites.Clear();
            _spriteGroups.Clear();
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

        private Sprite LoadSprite(string fileName)
        {
            var data = File.ReadAllBytes(fileName);
            var texture = new Texture2D(1, 1);
            texture.LoadImage(data);
            return Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );
        }
    }
}
