using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace PhysicsSimulations
{
    public class CarAuthoring : MonoBehaviour
    {
        public int carIndex;
        class Baker : Baker<CarAuthoring>
        {
            public override void Bake(CarAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new CarComponent
                {
                    Index = authoring.carIndex
                });
            }
        }
    }

    public struct CarComponent : IComponentData
    {
        public int Index;
    }
}
