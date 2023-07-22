using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Aspects;

namespace PhysicsSimulations
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct VoxelSystem : ISystem
    {
        bool isDone;

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>()
               .CreateCommandBuffer(state.WorldUnmanaged);

            JobHandle adjustHeight = new AdjustHeight
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                Ecb = ecb,
            }.Schedule<AdjustHeight>(state.Dependency);

            adjustHeight.Complete();            
        }

        [BurstCompile]
        public partial struct AdjustHeight : IJobEntity
        {
            public float DeltaTime;
            public EntityCommandBuffer Ecb;

            public void Execute(ref Voxel voxel, ref LocalToWorld localToWorld, ref PhysicsCollider collider, in Entity entity)
            {
                //Debug.Log($"{localToWorld.Value[0]} | {localToWorld.Value[1]} | {localToWorld.Value[2]} |{localToWorld.Value[3]}");
                if (!voxel.IsVoxelReady)
                {
                    if(voxel.Height <= voxel.MinHeight)
                    {
                        Ecb.DestroyEntity(entity);
                        return;
                    }

                    if (math.abs(localToWorld.Value[1][1] - voxel.Height) > 0.05f)
                    {
                        //Lerp the Y-scale of the voxel
                        float yScale = math.lerp(localToWorld.Value[1][1], voxel.Height, DeltaTime);
                        localToWorld.Value[1][1] = yScale;

                        //Lerp the Y-position of the voxel to keep it grounded
                        localToWorld.Value[3][1] = yScale / 2f;

                        ////Lerp the collider scale
                        //collider.Value = Unity.Physics.BoxCollider.Create(new BoxGeometry
                        //{
                        //    Center = float3.zero,
                        //    BevelRadius = 0.01f,
                        //    Orientation = quaternion.identity,
                        //    Size = new float3(voxel.VoxelSize, yScale, voxel.VoxelSize)
                        //});
                    }
                    else
                    {
                        //Set the final Y-scale and Y-position
                        localToWorld.Value[1][1] = voxel.Height;
                        localToWorld.Value[3][1] = voxel.Height / 2f;

                        //Scale the collider
                        collider.Value = BoxCollider.Create(new BoxGeometry
                        {
                            Center = float3.zero,
                            BevelRadius = voxel.VoxelSize / 2f,
                            Orientation = quaternion.identity,
                            Size = new float3(voxel.VoxelSize, voxel.Height, voxel.VoxelSize)
                        },
                        //Add collision filter
                        collider.Value.Value.GetCollisionFilter(),
                        //Add mat to raise events
                        new Material
                        {
                            CollisionResponse = CollisionResponsePolicy.CollideRaiseCollisionEvents
                        });

                        voxel.IsVoxelReady = true;

                        //SimConfigurationController scc = SimConfigurationController.Instance;
                        //if (scc != null && !scc.SpawnAirParticlesCommand && !scc.SpawnAirParticles)
                        //    scc.SpawnAirParticlesWithDelay(2000);
                    }
                }
            }
        }
    }
}
