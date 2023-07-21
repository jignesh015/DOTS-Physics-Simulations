using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Events
{
    public class CollisionEventAuthoring : MonoBehaviour
    {
        class Baker : Baker<CollisionEventAuthoring>
        {
            public override void Bake(CollisionEventAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new CollisionEvent()
                {
                    CollisionCount = 0,
                });
            }
        }
    }

    public struct CollisionEvent : IComponentData
    {
        public int CollisionCount;
        public float ImpactForce;
    }
}
