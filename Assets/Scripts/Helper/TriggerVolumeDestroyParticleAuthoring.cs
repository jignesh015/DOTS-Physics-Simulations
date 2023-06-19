using Unity.Entities;
using Unity.Physics.Authoring;
using UnityEngine;

namespace PhysicsSimulations
{
    public class TriggerVolumeDestroyParticleAuthoring : MonoBehaviour
    {
        class Baker : Baker<TriggerVolumeDestroyParticleAuthoring>
        {
            public override void Bake(TriggerVolumeDestroyParticleAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new TriggerVolumeDestroyParticle
                {
                });
            }
        }
    }

    public struct TriggerVolumeDestroyParticle : IComponentData
    {
    }
}
