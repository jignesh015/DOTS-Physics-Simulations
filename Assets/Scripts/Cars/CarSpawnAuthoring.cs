using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace PhysicsSimulations
{
    public class CarSpawnAuthoring : MonoBehaviour
    {
        public int carIndex;
        public GameObject carPrefab;

        class Baker : Baker<CarSpawnAuthoring>
        {
            public override void Bake(CarSpawnAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new CarSpawn
                {
                    CarIndex = authoring.carIndex,
                    CarPrefab = GetEntity(authoring.carPrefab, TransformUsageFlags.Dynamic),
                });
            }
        }
    }

    public struct CarSpawn: IComponentData
    {
        public int CarIndex;
        public Entity CarPrefab;
    }
}
