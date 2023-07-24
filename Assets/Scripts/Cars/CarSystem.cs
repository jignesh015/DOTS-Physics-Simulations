using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

namespace PhysicsSimulations
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(PhysicsSystemGroup))]
    public partial struct CarSystem : ISystem
    {
        private int isInitialized;

        [BurstCompile]
        void OnCreate(ref SystemState state)
        {
            isInitialized = 0;
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            SimConfigurationController scc = SimConfigurationController.Instance;
            if (scc == null) return;

            if (isInitialized == 0)
            {
                //Debug.Log("Not init!");
                isInitialized = 1;
                scc.ChangeCar(0);
                return;
            }
        }
    }
}
