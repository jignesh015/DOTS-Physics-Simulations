using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace PhysicsSimulations
{
    public class CarAuthoring : MonoBehaviour
    {
        class Baker : Baker<CarAuthoring>
        {
            public override void Bake(CarAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new CarComponent());
            }
        }
    }

    public struct CarComponent : IComponentData
    {
        
    }
}
