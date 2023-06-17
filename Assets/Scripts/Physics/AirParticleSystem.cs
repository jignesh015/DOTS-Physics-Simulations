using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Aspects;
using Unity.Physics.Systems;

namespace PhysicsSimulations
{
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(PhysicsSystemGroup))]
    public partial struct AirParticleSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new ThurstJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
            }.Schedule();
        }

        [BurstCompile]
        public partial struct ThurstJob : IJobEntity
        {
            public float DeltaTime;

            public void Execute(in AirParticle airParticle, RigidBodyAspect rigidBodyAspect)
            {
                float3 impulse = -airParticle.Direction * airParticle.Magnitude;
                impulse *= DeltaTime;

                rigidBodyAspect.ApplyImpulseAtPointLocalSpace(impulse, airParticle.Offset);
            }
        }
    }
}
