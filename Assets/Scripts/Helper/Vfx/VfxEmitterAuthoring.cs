using Unity.Entities;
using UnityEngine;

namespace PhysicsSimulations
{
    public class VfxEmitterAuthoring: MonoBehaviour
    {
        public GameObject VfxPrefab;

        class Baker : Baker<VfxEmitterAuthoring>
        {
            public override void Bake(VfxEmitterAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new VfxEmitter
                {
                    AirParticle = entity,
                    VfxPrefab = GetEntity(authoring.VfxPrefab, TransformUsageFlags.Dynamic),
                });
            }
        }
    }
    

    public struct VfxEmitter : IComponentData 
    {
        public Entity AirParticle;
        public Entity VfxPrefab;
    }
}
