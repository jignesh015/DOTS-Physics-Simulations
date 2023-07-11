using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace PhysicsSimulations
{
    //[RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(PhysicsSystemGroup))]
    public partial struct VfxEmitterSystem : ISystem
    {
        [BurstCompile]
        void OnCreate(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var entities =  state.GetEntityQuery(new ComponentType[] { typeof(VfxEmitter) }).ToEntityArray(Allocator.TempJob);
            for (int j = 0; j < entities.Length; j++)
            {
                var entity = entities[j];
                var vfxEmitter = state.EntityManager.GetComponentData<VfxEmitter>(entity);
                var airParticle = state.EntityManager.GetComponentData<AirParticle>(entity);

                Entity vfxEntity = state.EntityManager.Instantiate(vfxEmitter.VfxPrefab);

                VfxParticle _vfx = new()
                {
                    ParentParticle = vfxEmitter.AirParticle
                };
                state.EntityManager.SetComponentData(vfxEntity, _vfx);

                //Remove the Emitter component once the VFX is spawned
                state.EntityManager.RemoveComponent<VfxEmitter>(entity);
            }
        }
    }
}
