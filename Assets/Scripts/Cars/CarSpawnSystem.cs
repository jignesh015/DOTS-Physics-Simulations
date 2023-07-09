using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;
using static UnityEngine.EventSystems.EventTrigger;

namespace PhysicsSimulations
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct CarSpawnSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            SimConfigurationController scc = SimConfigurationController.Instance;

            if(scc == null) return;
            
            if (scc.ChangeCarIndex != -1)
            {
                int _carIndex = scc.ChangeCarIndex;
                scc.ChangeCarIndex = -1;

                EntityManager em = state.EntityManager;

                //Delete all previously spawned cars
                var spawnedCarEntities = state.GetEntityQuery(new ComponentType[] { typeof(CarComponent) }).ToEntityArray(Allocator.TempJob);
                foreach(var spawnedCarEntity in spawnedCarEntities)
                {
                    em.DestroyEntity(spawnedCarEntity);
                }

                //Spawn new car
                var carSpawnerEntities = state.GetEntityQuery(new ComponentType[] { typeof(CarSpawn) }).ToEntityArray(Allocator.TempJob);

                foreach(Entity spawnEntity in carSpawnerEntities)
                {
                    CarSpawn carSpawn = em.GetComponentData<CarSpawn>(spawnEntity);
                    if(carSpawn.CarIndex == _carIndex)
                    {
                        //Spawn the car
                        em.Instantiate(carSpawn.CarPrefab);
                    }
                }
            }
        }
    }
}
