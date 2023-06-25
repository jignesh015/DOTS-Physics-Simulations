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
        private double startTime;
        public EntityCommandBuffer ecb;

        [BurstCompile]
        void OnCreate(ref SystemState state) 
        {
            startTime = SystemAPI.Time.ElapsedTime;
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            //new ThurstJob
            //{
            //    DeltaTime = SystemAPI.Time.DeltaTime,
            //}.Schedule();

            ecb = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>()
               .CreateCommandBuffer(state.WorldUnmanaged);

            new ThurstJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                TimeSinceAlive = SystemAPI.Time.ElapsedTime - startTime,
                Ecb = ecb,
            }.Schedule();
        }

        [BurstCompile]
        public partial struct ThurstJob : IJobEntity
        {
            public float DeltaTime;
            public double TimeSinceAlive;
            public EntityCommandBuffer Ecb;

            public void Execute(ref AirParticle airParticle, RigidBodyAspect rigidBodyAspect)
            {
                float3 impulse = -airParticle.Direction * airParticle.Magnitude;
                impulse *= DeltaTime;

                rigidBodyAspect.ApplyImpulseAtPointLocalSpace(impulse, airParticle.Offset);

                airParticle.Lifespan -= DeltaTime;
                if (airParticle.Lifespan <= 0)
                {
                    Ecb.DestroyEntity(rigidBodyAspect.Entity);
                }
            }
        }
    }
}
