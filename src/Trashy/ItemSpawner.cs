using System;
using System.Collections;
using System.Collections.Generic;
using Trashy.Components;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Trashy
{
    public class ItemSpawner : MonoBehaviour
    {
        private readonly SlowHeadFinder _headFinder;
        private readonly SpriteManager _spriteManager;

        private static VTubeStudioModelLoader ModelLoader => TrashyPlugin.ModelLoader;

        public static readonly List<Collider> CurrentItemColliders = new List<Collider>();

        public ItemSpawner()
        {
            _headFinder = gameObject.GetComponent<SlowHeadFinder>();
            _spriteManager = GetComponent<SpriteManager>();
        }

        public void SpawnTrash(TriggerConfig trigger)
        {
            IReadOnlyList<Sprite> sprites = _spriteManager.Items;

            if (!string.IsNullOrWhiteSpace(trigger.ItemGroup) &&
                _spriteManager.Groups.TryGetValue(trigger.ItemGroup, out var groupSprites))
            {
                sprites = groupSprites;
            }

            SpawnTrash(trigger, sprites);
        }

        public void SpawnTrash(TriggerConfig trigger, IReadOnlyList<Sprite> sprites)
        {
            var modelPosition = ModelLoader.ModelTransformController.transform.position;
            var head = _headFinder.GetHead();
            StartCoroutine(
                SpawnTrash(
                    new Vector3(modelPosition.x, modelPosition.y, 20),
                    head,
                    trigger,
                    sprites
                )
            );
        }

        private IEnumerator SpawnTrash(Vector3 position, Vector3 headPosition, TriggerConfig trigger,
            IReadOnlyList<Sprite> sprites)
        {
            var viewport = ModelLoader.Live2DCamera.WorldToViewportPoint(headPosition);
            var spawnAdjustmentRange = new Vector3(-100, 100);

            // Note: The virtual camera feature only renders the live2d layer
            var layer = 8; // 9=UI LAYER, 8=live2d layer

            // Adjust spawn position depending if the model is on the far left or right of the screen
            // example: Items should only spawn on the left if the model is on the far right

            if (viewport.x >= 0.8f)
                spawnAdjustmentRange = new Vector3(-100, 0);

            // if (viewport.x >= 0.9f)
            //     layer = 8;

            if (viewport.x <= 0.2f)
                spawnAdjustmentRange = new Vector2(0, 100);

            // if (viewport.x <= 0.1f)
            //     layer = 8;

            for (var i = 0; i < trigger.ItemCount; ++i)
            {
                var go = new GameObject();
                go.layer = layer;

                var sprite = sprites.Random();
                var textureSize = Math.Min(sprite.texture.width, sprite.texture.height);
                var textureScale = ConfigManager.SpriteSize.Value / textureSize;
                go.transform.localScale = new Vector3(textureScale, textureScale, 1);
                go.transform.position = position + new Vector3(
                    Random.Range(spawnAdjustmentRange.x, spawnAdjustmentRange.y),
                    Random.Range(-20, 20),
                    Random.Range(40, 70)
                );

                go.AddComponent<DestroyOutOfBounds>();

                if (ConfigManager.PlayHitSound.Value)
                    go.AddComponent<PlayAudio>();

                if (ConfigManager.ManipulateModel.Value)
                    go.AddComponent<ManipulateModel>();

                var isSticky = trigger.StickyChance > 0 && Random.Range(1, 101) <= trigger.StickyChance;
                if (isSticky)
                    go.AddComponent<Sticky>().Duration = trigger.StickyDuration;

                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.sortingOrder = 1100;

                var collider = go.AddComponent<SphereCollider>();
                collider.material = new PhysicMaterial
                {
                    bounciness = 0.0f
                };

                var rigidbody = go.AddComponent<Rigidbody>();
                rigidbody.constraints = isSticky ? RigidbodyConstraints.FreezeRotation : RigidbodyConstraints.None;

                var modelTransform = ModelLoader.ModelTransformController.transform;
                var scale = modelTransform.localScale.x;
                var target = headPosition - new Vector3(0, Random.Range(0, 20) * scale, 0);
                var direction = (target - go.transform.position).normalized;
                rigidbody.AddForce(direction.x * 300, direction.y * 300, direction.z * 300, ForceMode.VelocityChange);

                // if (!isSticky)
                    // rigidbody.AddTorque(new Vector3(100, 100, 100), ForceMode.VelocityChange);

                // Ignore collision on other items
                foreach (var colliderToIgnore in CurrentItemColliders)
                    Physics.IgnoreCollision(colliderToIgnore, collider);

                CurrentItemColliders.Add(collider);
                yield return new WaitForSeconds(0.05f);
            }
        }
    }
}
