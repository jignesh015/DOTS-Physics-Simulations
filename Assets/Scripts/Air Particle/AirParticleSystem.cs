using System.Numerics;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
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
            ecb = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>()
               .CreateCommandBuffer(state.WorldUnmanaged);

            JobHandle thurstJob = new ThurstJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                TimeSinceAlive = SystemAPI.Time.ElapsedTime - startTime,
                Ecb = ecb,
            }.Schedule<ThurstJob>(state.Dependency);

            thurstJob.Complete();
        }

        [BurstCompile]
        public partial struct ThurstJob : IJobEntity
        {
            public float DeltaTime;
            public double TimeSinceAlive;
            public EntityCommandBuffer Ecb;

            public void Execute(ref AirParticle airParticle, RigidBodyAspect rigidBodyAspect)
            {
                if (!airParticle.IsForceApplied)
                {
                    float3 impulse = -airParticle.Direction * SimConfigurationController.Instance.WindMagnitude;
                    rigidBodyAspect.ApplyImpulseAtPointLocalSpace(impulse, airParticle.Offset);
                    airParticle.InitialVelocity = rigidBodyAspect.LinearVelocity;
                    airParticle.IsForceApplied = true;
                }

                //Calculate the current kinetic energy of the air particle
                airParticle.KineticEnergy = 0.5f * rigidBodyAspect.Mass * math.lengthsq(rigidBodyAspect.LinearVelocity);

                //Calculate the change in momentum
                float3 changeInMomentum = rigidBodyAspect.Mass * (rigidBodyAspect.LinearVelocity - airParticle.InitialVelocity);

                //Calculate the total drag force
                float dragForce = math.length(changeInMomentum / DeltaTime);
                airParticle.Drag = dragForce;

                //Calculate drag coefficient
                float frontalArea = 0.01f; 
                float airDensity = Data.AirDensity;
                airParticle.DragCoefficient = dragForce / (0.5f * airDensity * math.lengthsq(rigidBodyAspect.LinearVelocity) * frontalArea);

                airParticle.Lifespan -= DeltaTime;
                if (airParticle.Lifespan <= 0)
                {
                    //UnityEngine.Debug.Log($"<color=red>Impact on Death: {airParticle.KineticEnergy}</color>");
                    SimConfigurationController.Instance.UpdateKineticEnergyList(airParticle.KineticEnergy);
                    SimConfigurationController.Instance.UpdateDragForceList(airParticle.Drag);
                    Ecb.DestroyEntity(rigidBodyAspect.Entity);
                }
            }
        }
    }
}
