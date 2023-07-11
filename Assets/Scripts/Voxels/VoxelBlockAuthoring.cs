using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace PhysicsSimulations
{
    public class VoxelBlockAuthoring : MonoBehaviour
    {
        public int row;
        public int column;
        public float height;
        public float minHeight;
        public float maxHeight;
        public GameObject voxelPrefab;

        class Baker : Baker<VoxelBlockAuthoring>
        {
            public override void Bake(VoxelBlockAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new VoxelBlock
                {
                    Row = authoring.row,
                    Column = authoring.column,
                    Height = authoring.height,
                    MinHeight = authoring.minHeight,
                    MaxHeight = authoring.maxHeight,
                    HeightOffset = 0,
                    VoxelPrefab = GetEntity(authoring.voxelPrefab, TransformUsageFlags.Dynamic),
                    IsBlockComplete = true,
                });
            }
        }
    }

    public struct VoxelBlock : IComponentData
    {
        public int Row;
        public int Column;
        public float Height;
        public float MinHeight;
        public float MaxHeight;
        public float HeightOffset;
        public Entity VoxelPrefab;
        public bool IsBlockComplete;
    }
}
