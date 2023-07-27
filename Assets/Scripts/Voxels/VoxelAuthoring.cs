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

        public GameObject baseMatRefPrefab;
        public GameObject positiveMatRefPrefab;
        public GameObject negativeMatRefPrefab;

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
                    BaseMatRef = GetEntity(authoring.baseMatRefPrefab, TransformUsageFlags.None),
                    PositiveMatRef = GetEntity(authoring.positiveMatRefPrefab, TransformUsageFlags.None),
                    NegativeMatRef = GetEntity(authoring.negativeMatRefPrefab, TransformUsageFlags.None),
                    MatRefIndex = 0,
                    HasCollided = false,
                    HadPreviouslyCollided = false,
                }); ;
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
        public Entity BaseMatRef;
        public Entity PositiveMatRef;
        public Entity NegativeMatRef;
        public int MatRefIndex;
        public bool HasCollided;
        public bool HadPreviouslyCollided;
    }
}
