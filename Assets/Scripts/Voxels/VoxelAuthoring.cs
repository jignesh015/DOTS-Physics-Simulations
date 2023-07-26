using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace PhysicsSimulations
{
    public class VoxelAuthoring : MonoBehaviour
    {
        public int row;
        public int column;
        public float minHeight;
        public int maxHeight;

        class Baker : Baker<VoxelAuthoring>
        {
            public override void Bake(VoxelAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Voxel
                {
                    Row = authoring.row,
                    Column = authoring.column,
                    Height = 0,
                    OgHeight = 0,
                    MinHeight = authoring.minHeight,
                    MaxHeight = authoring.maxHeight,
                    IsVoxelReady = true,
                });
            }
        }
    }

    public struct Voxel : IComponentData
    {
        public int Row;
        public int Column;
        public float Height;
        public float OgHeight;
        public float MinHeight;
        public float MaxHeight;
        public float VoxelSize;
        public bool IsVoxelReady;
    }
}
