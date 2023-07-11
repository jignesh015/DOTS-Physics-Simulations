using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Aspects;
using Unity.Transforms;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.UIElements.UxmlAttributeDescription;
using UnityEngine.UIElements;
using static PhysicsSimulations.AirParticleSystem;
using Unity.Jobs;

namespace PhysicsSimulations
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct VoxelSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            //EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>()
            //   .CreateCommandBuffer(state.WorldUnmanaged);

            JobHandle adjustHeight = new AdjustHeight
            {
                DeltaTime = SystemAPI.Time.DeltaTime
            }.Schedule<AdjustHeight>(state.Dependency);

            adjustHeight.Complete();
        }

        [BurstCompile]
        public partial struct AdjustHeight : IJobEntity
        {
            public float DeltaTime;

            public void Execute(ref Voxel voxel, ref LocalToWorld localToWorld)
            {
                //Debug.Log($"{localToWorld.Value[0]} | {localToWorld.Value[1]} | {localToWorld.Value[2]} |{localToWorld.Value[3]}");
                if (!voxel.IsVoxelReady)
                {
                    if(math.abs(localToWorld.Value[1][1] - voxel.Height) > 0.05f)
                    {
                        float yScale = math.lerp(localToWorld.Value[1][1], voxel.Height, DeltaTime);
                        localToWorld.Value[1][1] = yScale;
                        localToWorld.Value[3][1] = yScale/2f;
                    }
                    else
                    {
                        voxel.IsVoxelReady = true;
                    }
                    
                }
            }
        }
    }
}
