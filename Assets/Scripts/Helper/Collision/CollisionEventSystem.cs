using PhysicsSimulations;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Rendering;
using UnityEngine;

namespace Events
{
    // This system applies an impulse to any dynamic that collides with a Repulsor.
    // A Repulsor is defined by a PhysicsShapeAuthoring with the `Raise Collision Events` flag ticked and a
    // CollisionEventImpulse behaviour added.
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(PhysicsSystemGroup))]
    public partial struct CollisionEventSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CollisionEvent>();
            state.RequireForUpdate<SimulationSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new CollisionEventJob
            {
                CollisionEventData = SystemAPI.GetComponentLookup<CollisionEvent>(),
                AirParticle = SystemAPI.GetComponentLookup<AirParticle>(),
                MaterialMesh = SystemAPI.GetComponentLookup<MaterialMeshInfo>(),
            }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
        }

        [BurstCompile]
        struct CollisionEventJob : ICollisionEventsJob
        {
            public ComponentLookup<CollisionEvent> CollisionEventData;
            public ComponentLookup<AirParticle> AirParticle;
            public ComponentLookup<MaterialMeshInfo> MaterialMesh;

            public void Execute(Unity.Physics.CollisionEvent collisionEvent)
            {
                SimConfigurationController scc = SimConfigurationController.Instance;

                Entity entityA = collisionEvent.EntityA;
                Entity entityB = collisionEvent.EntityB;

                bool isBodyADynamic = AirParticle.HasComponent(entityA);
                bool isBodyBDynamic = AirParticle.HasComponent(entityB);

                bool isBodyACollider = CollisionEventData.HasComponent(entityA);
                bool isBodyBCollider = CollisionEventData.HasComponent(entityB);

                bool bodyAHasMaterial = MaterialMesh.HasComponent(entityA);
                bool bodyBHasMaterial = MaterialMesh.HasComponent(entityB);

                Entity colliderEntity = (isBodyACollider && isBodyBDynamic) ? entityA : entityB;
                Entity airParticleEntity = (isBodyBCollider && isBodyADynamic) ? entityA : entityB;

                var colliderComponent = CollisionEventData[colliderEntity];
                colliderComponent.CollisionCount++;
                scc.VoxelCollisionCount++;
                colliderComponent.ImpactForce = AirParticle[airParticleEntity].ImpactForce;
                CollisionEventData[colliderEntity] = colliderComponent;

                //Assign impact mat
                int _impactLevel = scc.GetImpactLevel(colliderComponent.CollisionCount);
                var refMaterialComponent = MaterialMesh[colliderEntity];
                switch (_impactLevel)
                {
                    case 1:
                        refMaterialComponent = MaterialMesh[colliderComponent.LowImpactMatRef];
                        break;
                    case 2:
                        refMaterialComponent = MaterialMesh[colliderComponent.MidImpactMatRef];
                        break;
                    case 3:
                        refMaterialComponent = MaterialMesh[colliderComponent.HighImpactMatRef];
                        break;
                }

                if (bodyAHasMaterial)
                {
                    var materialComponent = MaterialMesh[colliderEntity];
                    materialComponent.Material = refMaterialComponent.Material;
                    MaterialMesh[colliderEntity] = materialComponent;
                }


                //if (isBodyACollider && isBodyBDynamic)
                //{
                //    var colliderComponent = CollisionEventData[entityA];
                //    colliderComponent.CollisionCount++;
                //    scc.VoxelCollisionCount++;
                //    colliderComponent.ImpactForce = AirParticle[entityB].ImpactForce;
                //    CollisionEventData[entityA] = colliderComponent;

                //    if(bodyAHasMaterial)
                //    {
                //        var materialComponent = MaterialMesh[entityA];
                //        var refMaterialComponent = MaterialMesh[colliderComponent.NewMatRefEntity];
                //        materialComponent.Material = refMaterialComponent.Material;
                //        MaterialMesh[entityA] = materialComponent;
                //    }
                //}

                //if (isBodyBCollider && isBodyADynamic)
                //{
                //    var colliderComponent = CollisionEventData[entityB];
                //    colliderComponent.CollisionCount++;
                //    scc.VoxelCollisionCount++;
                //    colliderComponent.ImpactForce = AirParticle[entityA].ImpactForce;
                //    CollisionEventData[entityB] = colliderComponent;

                //    if (bodyBHasMaterial)
                //    {
                //        var materialComponent = MaterialMesh[entityB];
                //        var refMaterialComponent = MaterialMesh[colliderComponent.NewMatRefEntity];
                //        materialComponent.Material = refMaterialComponent.Material;
                //        MaterialMesh[entityB] = materialComponent;
                //    }
                //}
            }
        }
    }
}
