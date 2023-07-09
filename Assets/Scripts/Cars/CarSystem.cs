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
            if (isInitialized == 0)
            {
                Debug.Log("Not init!");
                isInitialized = 1;
                SimConfigurationController.Instance.ChangeCar(0);
                return;
            }
            //SimConfigurationController.Instance.ChangeCar(0);

            

            //EntityManager em = state.EntityManager;
            //EntityQuery carQuery = em.CreateEntityQuery(typeof(CarComponent));
            //NativeArray<Entity> carArray = carQuery.ToEntityArray(Unity.Collections.Allocator.Temp);

            //Debug.Log($"carArray system: {carArray.Length}");
            //foreach (Entity entity in carArray)
            //{
            //    LocalTransform obj = em.GetComponentData<LocalTransform>(entity);
            //    obj.Scale = 0;
            //    em.SetComponentData(entity, obj);
            //}
            //isInitialized = true;
        }
    }
}
