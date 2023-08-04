using Unity.Burst;
using Unity.Entities;
using Unity.Physics.Stateful;
using Unity.Physics.Systems;
using Unity.Rendering;
using UnityEngine;

namespace PhysicsSimulations
{
    [UpdateInGroup(typeof(PhysicsSystemGroup))]
    [UpdateAfter(typeof(StatefulTriggerEventSystem))]
    public partial struct TriggerVolumeDestroyParticleSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TriggerVolumeDestroyParticle>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            var nonTriggerQuery = SystemAPI.QueryBuilder().WithNone<StatefulTriggerEvent>().Build();
            var nonTriggerMask = nonTriggerQuery.GetEntityQueryMask();

            foreach (var (triggerEventBuffer, destroyParticle, entity) in
                     SystemAPI.Query<DynamicBuffer<StatefulTriggerEvent>, RefRW<TriggerVolumeDestroyParticle>>()
                         .WithEntityAccess())
            {
                for (int i = 0; i < triggerEventBuffer.Length; i++)
                {
                    var triggerEvent = triggerEventBuffer[i];
                    var otherEntity = triggerEvent.GetOtherEntity(entity);

                    // exclude other triggers and processed events
                    if (triggerEvent.State == StatefulEventState.Stay ||
                        !nonTriggerMask.MatchesIgnoreFilter(otherEntity))
                    {
                        continue;
                    }

                    if (triggerEvent.State == StatefulEventState.Enter)
                    {
                        //Check if air particle
                        var airParticle = SystemAPI.GetComponentLookup<AirParticle>();
                        if(airParticle.HasComponent(otherEntity))
                        {
                            //Debug.Log($"<color=orange>Impact on Trigger: {airParticle[otherEntity].KineticEnergy}</color>");
                            UpdateMetrics(airParticle[otherEntity].KineticEnergy, airParticle[otherEntity].Drag);
                            ecb.DestroyEntity(otherEntity);
                        }
                    }
                }
            }
        }

        private void UpdateMetrics(float _kineticEnergy, float _drag)
        {
            SimConfigurationController scc = SimConfigurationController.Instance;
            scc.UpdateKineticEnergyList(_kineticEnergy);
            scc.UpdateDragForceList(_drag);
        }
    }
}
