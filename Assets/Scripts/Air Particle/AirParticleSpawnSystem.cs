using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

namespace PhysicsSimulations
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(PhysicsSystemGroup))]
    public partial struct AirParticleSpawnSystem : ISystem
    {
        private NativeArray<float3> spawnPositions;
        private bool hasSpawned;
        private double spawnTime;

        private int totalSpawnPosCount;

        public void OnUpdate(ref SystemState state)
        {
            EntityManager em = state.EntityManager;
            SimConfigurationController scc = SimConfigurationController.Instance;

            //Get all spawners 
            var spawnerEntities = state.GetEntityQuery(new ComponentType[] { typeof(AirParticleSpawn) }).ToEntityArray(Allocator.TempJob);
            foreach (var spawnerEntity in spawnerEntities)
            {
                AirParticleSpawn spawner = em.GetComponentData<AirParticleSpawn>(spawnerEntity);
                if (!spawner.SpawnPlacesAdded)
                {
                    var spawnerTransform = em.GetComponentData<LocalTransform>(spawnerEntity);
                    float3 spawnerPos = spawnerTransform.Position;
                    spawnPositions = new NativeArray<float3>(spawner.Width * spawner.Height, Allocator.Persistent);
                    totalSpawnPosCount = 0;

                    //Add particle spawn positions to a list
                    for (int i = 0; i < spawner.Width; i++)
                    {
                        for (int j = 0; j < spawner.Height; j++)
                        {
                            spawnPositions[totalSpawnPosCount] = new float3(spawnerPos.x + spawner.GridOffset * i, spawnerPos.y + spawner.GridOffset * j, spawnerPos.z);
                            totalSpawnPosCount++;
                        }
                    }
                    Debug.Log($"Total air spawner count: {totalSpawnPosCount}");
                    spawner.SpawnPlacesAdded = true;
                    em.SetComponentData(spawnerEntity, spawner);
                }
                else if (!hasSpawned && scc.SpawnAirParticles)
                {
                    var count = spawner.Width * spawner.Height;
                    if (count > 100)
                        count = ((int)((spawner.Width * spawner.Height * scc.CurrentSimConfig.airParticleRatio) / 100));
                    //Debug.Log($"<color=yellow>Spawn count: {count}</color>");
                    var instances = new NativeArray<Entity>(count, Allocator.Temp);
                    em.Instantiate(spawner.AirParticlePrefab, instances);

                    var randomSpawnPositions = new NativeArray<float3>(count, Allocator.Temp);
                    if (totalSpawnPosCount <= 100)
                        randomSpawnPositions = spawnPositions;
                    else
                        GetRandomSubset(spawnPositions, ref randomSpawnPositions, count, SystemAPI.Time.ElapsedTime);
                    for (int i = 0; i < count; i++)
                    {
                        var instance = instances[i];

                        //Set Transform and Rotation
                        var transform = em.GetComponentData<LocalTransform>(instance);
                        transform.Position = randomSpawnPositions[i];
                        em.SetComponentData(instance, transform);
                    }
                    hasSpawned = true;
                    spawnTime = SystemAPI.Time.ElapsedTime;
                    scc.AirParticlesBurstCount++;
                }
                else if (hasSpawned && SystemAPI.Time.ElapsedTime - spawnTime > spawner.SpawnInterval && !spawner.SpawnOnce)
                {
                    hasSpawned = false;
                }
            }
        }

        private static void GetRandomSubset(NativeArray<float3> originalList, ref NativeArray<float3> randomSpawnPositions, int subsetCount, double elapsedTime)
        {
            // Check if the original list is smaller than the required subset size
            if (originalList.Length <= subsetCount)
            {
                randomSpawnPositions.CopyFrom(originalList);
            }


            int[] subsetIndices = new int[subsetCount];
            var random = Unity.Mathematics.Random.CreateFromIndex((uint)elapsedTime);
            for (int i = 0; i < subsetCount; i++)
            {
                int randomIndex;
                bool indexExists;

                do
                {
                    indexExists = false;
                    randomIndex = random.NextInt(originalList.Length);

                    // Check if the index is already present in the subsetIndices array.
                    for (int j = 0; j < subsetIndices.Length; j++)
                    {
                        if (subsetIndices[j].Equals(randomIndex))
                        {
                            indexExists = true;
                            break;
                        }
                    }
                } while (indexExists);
                subsetIndices[i] = randomIndex;
            }

            for(int m = 0; m < subsetIndices.Length; m++)
            {
                randomSpawnPositions[m] = originalList[subsetIndices[m]];
            }
        }
    }
}
