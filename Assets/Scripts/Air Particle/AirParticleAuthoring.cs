using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Authoring;
using UnityEngine;

namespace PhysicsSimulations
{
    [RequireComponent(typeof(PhysicsBodyAuthoring))]
    public class AirParticleAuthoring : MonoBehaviour
    {
        [Min(0)] public float Magnitude = 1.0f;
        public Vector3 LocalDirection = -Vector3.forward;
        public Vector3 LocalOffset = Vector3.zero;
        [Min(0)] public float Lifespan = 10.0f;

        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        class Baker : Baker<AirParticleAuthoring>
        {
            public override void Bake(AirParticleAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new AirParticle
                {
                    Magnitude = authoring.Magnitude,
                    Direction = authoring.LocalDirection.normalized,
                    Offset = authoring.LocalOffset,
                    Lifespan = authoring.Lifespan,
                });
            }
        }
    }

    public struct AirParticle : IComponentData
    {
        public float Magnitude;
        public float3 Direction;
        public float3 Offset;
        public float Lifespan;
        public bool isForceApplied;
    }
}
