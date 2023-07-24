using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace PhysicsSimulations
{
    public class AirParticleSpawnAuthoring : MonoBehaviour
    {
        public int width;
        public int height;
        public float gridOffset;
        public double spawnInterval;
        public GameObject airParticlePrefab;
        public bool spawnOnce;

        class Baker : Baker<AirParticleSpawnAuthoring>
        {
            public override void Bake(AirParticleSpawnAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new AirParticleSpawn
                {
                    Width = authoring.width,
                    Height = authoring.height,
                    GridOffset = authoring.gridOffset,
                    SpawnInterval = authoring.spawnInterval,
                    AirParticlePrefab = GetEntity(authoring.airParticlePrefab, TransformUsageFlags.Dynamic),
                    SpawnOnce = authoring.spawnOnce,
                    SpawnPlacesAdded = false,
                });
            }
        }
    }

    public struct AirParticleSpawn : IComponentData
    {
        public int Width;
        public int Height;
        public float GridOffset;
        public double SpawnInterval;
        public Entity AirParticlePrefab;
        public bool SpawnOnce;
        public bool SpawnPlacesAdded;
    }
}
