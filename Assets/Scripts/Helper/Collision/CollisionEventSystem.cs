using PhysicsSimulations;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
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
            }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
        }

        [BurstCompile]
        struct CollisionEventJob : ICollisionEventsJob
        {
            public ComponentLookup<CollisionEvent> CollisionEventData;
            public ComponentLookup<AirParticle> AirParticle;

            public void Execute(Unity.Physics.CollisionEvent collisionEvent)
            {
                Entity entityA = collisionEvent.EntityA;
                Entity entityB = collisionEvent.EntityB;

                bool isBodyADynamic = AirParticle.HasComponent(entityA);
                bool isBodyBDynamic = AirParticle.HasComponent(entityB);

                bool isBodyACollider = CollisionEventData.HasComponent(entityA);
                bool isBodyBCollider = CollisionEventData.HasComponent(entityB);

                if (isBodyACollider && isBodyBDynamic)
                {
                    var colliderComponent = CollisionEventData[entityA];
                    colliderComponent.CollisionCount++;
                    colliderComponent.ImpactForce = AirParticle[entityB].ImpactForce;
                    CollisionEventData[entityA] = colliderComponent;
                }

                if (isBodyBCollider && isBodyADynamic)
                {
                    var colliderComponent = CollisionEventData[entityB];
                    colliderComponent.CollisionCount++;
                    colliderComponent.ImpactForce = AirParticle[entityA].ImpactForce;
                    CollisionEventData[entityB] = colliderComponent;
                }
            }
        }
    }
}
