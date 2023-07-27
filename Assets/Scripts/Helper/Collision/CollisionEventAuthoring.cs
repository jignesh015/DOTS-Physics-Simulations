using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Events
{
    public class CollisionEventAuthoring : MonoBehaviour
    {
        public GameObject lowImpactMatRefPrefab;
        public GameObject midImpactMatRefPrefab;
        public GameObject highImpactMatRefPrefab;

        class Baker : Baker<CollisionEventAuthoring>
        {
            public override void Bake(CollisionEventAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new CustomCollisionEvent()
                {
                    CollisionCount = 0,
                    LowImpactMatRef = GetEntity(authoring.lowImpactMatRefPrefab, TransformUsageFlags.None),
                    MidImpactMatRef = GetEntity(authoring.midImpactMatRefPrefab, TransformUsageFlags.None),
                    HighImpactMatRef = GetEntity(authoring.highImpactMatRefPrefab, TransformUsageFlags.None),
                });
            }
        }
    }

    public struct CustomCollisionEvent : IComponentData
    {
        public int CollisionCount;
        public float ImpactForce;
        public Entity LowImpactMatRef;
        public Entity MidImpactMatRef;
        public Entity HighImpactMatRef;
    }
}
