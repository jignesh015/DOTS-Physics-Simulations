using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace PhysicsSimulations
{
    public class VfxParticleAuthoring : MonoBehaviour
    {

        class Baker : Baker<VfxParticleAuthoring>
        {
            public override void Bake(VfxParticleAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new VfxParticle
                {
                    ParentParticle = entity,
                });
            }
        }
    }

    

    public struct VfxParticle : IComponentData
    {
        public Entity ParentParticle;
    }
}
