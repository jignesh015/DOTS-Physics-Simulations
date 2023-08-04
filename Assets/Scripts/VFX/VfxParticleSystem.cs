using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics.Systems;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;

namespace PhysicsSimulations
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(PhysicsSystemGroup))]
    public partial struct VfxParticleSystem : ISystem
    {
        public readonly void OnUpdate(ref SystemState state)
        {
            var entities = state.GetEntityQuery(new ComponentType[] { typeof(VfxParticle) }).ToEntityArray(Allocator.TempJob);
            for (int j = 0; j < entities.Length; j++)
            {
                var entity = entities[j];
                VfxParticle vfxParticle = state.EntityManager.GetComponentData<VfxParticle>(entity);
                LocalTransform vfxTransform = state.EntityManager.GetComponentData<LocalTransform>(entity);


                if (state.EntityManager.Exists(entity) && state.EntityManager.Exists(vfxParticle.ParentParticle))
                {
                    var parentTransform = state.EntityManager.GetComponentData<LocalTransform>(vfxParticle.ParentParticle);
                    vfxTransform.Position = parentTransform.Position;
                    state.EntityManager.SetComponentData(entity, vfxTransform);
                }
                else
                {
                    state.EntityManager.DestroyEntity(entity);
                }
            }
        }
    }
}
