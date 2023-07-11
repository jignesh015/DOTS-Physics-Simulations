using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace PhysicsSimulations
{
    public class VoxelGridAuthoring : MonoBehaviour
    {
        public int length;
        public int width;
        public float gridOffset;
        public GameObject voxelPrefab;

        class Baker : Baker<VoxelGridAuthoring>
        {
            public override void Bake(VoxelGridAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new VoxelGrid
                {
                    Length = authoring.length,
                    Width = authoring.width,
                    GridOffset = authoring.gridOffset,
                    VoxelPrefab = GetEntity(authoring.voxelPrefab, TransformUsageFlags.Dynamic),
                    IsGridComplete = false,
                });
            }
        }
    }

    public struct VoxelGrid : IComponentData
    {
        public int Length;
        public int Width;
        public float GridOffset;
        public Entity VoxelPrefab;
        public bool IsGridComplete;
    }
}
