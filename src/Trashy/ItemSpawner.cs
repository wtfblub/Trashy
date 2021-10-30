using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Trashy
{
    public class ItemSpawner : MonoBehaviour
    {
        private readonly SlowHeadFinder _headFinder;
        private readonly SpriteManager _spriteManager;

        private static VTubeStudioModelLoader ModelLoader => TrashyPlugin.ModelLoader;

        public ItemSpawner()
        {
            _headFinder = gameObject.GetComponent<SlowHeadFinder>();
            _spriteManager = GetComponent<SpriteManager>();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(1) && Input.GetKey(KeyCode.LeftShift))
                SpawnTrash();
        }

        public void SpawnTrash(int count = -1)
        {
            if (count < 0)
                count = ConfigManager.ObjectCount.Value;

            var modelPosition = ModelLoader.ModelTransformController.transform.position;
            var head = _headFinder.GetHead();
            SpawnTrash(
                new Vector3(modelPosition.x, modelPosition.y, 20),
                head,
                count
            );
        }

        private void SpawnTrash(Vector3 position, Vector3 targetPosition, int count)
        {
            var colliders = new List<Collider>();
            for (var i = 0; i < count; ++i)
            {
                var go = new GameObject();
                go.layer = 9; // UI LAYER

                var sprite = _spriteManager.Items.Random();
                var textureSize = Math.Min(sprite.texture.width, sprite.texture.height);
                var textureScale = ConfigManager.SpriteSize.Value / textureSize;
                go.transform.localScale = new Vector3(textureScale, textureScale, 1);
                go.transform.position = position + new Vector3(Random.Range(-100, 0), Random.Range(-20, 20), 0);

                go.AddComponent<DestroyOutOfBounds>();

                var isSticky = ConfigManager.StickyChance.Value > 0 && Random.Range(1, 101) <= ConfigManager.StickyChance.Value;
                if (isSticky)
                    go.AddComponent<Sticky>();

                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.sortingOrder = 1100;

                var collider = go.AddComponent<SphereCollider>();
                collider.material = new PhysicMaterial
                {
                    bounciness = 0.0f
                };

                var rigidbody = go.AddComponent<Rigidbody>();
                rigidbody.mass = 500;
                rigidbody.constraints = isSticky ? RigidbodyConstraints.FreezeRotation : RigidbodyConstraints.None;

                var modelTransform = ModelLoader.ModelTransformController.transform;
                var scale = modelTransform.localScale.x;
                targetPosition -= new Vector3(0, Random.Range(0, 10) * scale, 0);
                var direction = (targetPosition - go.transform.position).normalized;
                rigidbody.AddForce(direction.x * 400, direction.y * 400, direction.z * 400, ForceMode.VelocityChange);

                if (!isSticky)
                    rigidbody.AddTorque(new Vector3(100, 100, 100), ForceMode.VelocityChange);

                // Ignore colliders we just spawned
                foreach (var colliderToIgnore in colliders)
                {
                    Physics.IgnoreCollision(colliderToIgnore, collider);
                    Physics.IgnoreCollision(collider, colliderToIgnore);
                }

                colliders.Add(collider);
            }
        }
    }
}
