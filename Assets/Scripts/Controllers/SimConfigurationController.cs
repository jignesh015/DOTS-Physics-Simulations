using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace PhysicsSimulations
{
    public class SimConfigurationController : MonoBehaviour
    {
        [SerializeField] private SimConfiguration defaultConfig;

        public SimConfiguration CurrentSimConfig { get; private set; }

        public float WindMagnitude;


        private static SimConfigurationController _instance;
        public static SimConfigurationController Instance { get { return _instance; } }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                _instance = this;
            }

            CurrentSimConfig = defaultConfig;
        }

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            //EntityQuery airParticleQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(typeof(AirParticle));
            //NativeArray<Entity> airParticleArray = airParticleQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
            //foreach (Entity entity in airParticleArray)
            //{
            //    if(World.DefaultGameObjectInjectionWorld.EntityManager.Exists(entity))
            //    {
            //        var airP = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<AirParticle>(entity);
            //        airP.Magnitude = defaultConfig.windSpeed;
            //        World.DefaultGameObjectInjectionWorld.EntityManager.SetComponentData(entity, airP);
            //    }
            //}

            
        }

        private void FixedUpdate()
        {
            CurrentSimConfig = defaultConfig;

            WindMagnitude = CurrentSimConfig.windSpeed / Time.fixedDeltaTime;
            //Debug.Log($"Fixed time: {Time.fixedDeltaTime}");
        }
    }

    [Serializable]
    public class SimConfiguration
    {
        [Range(1, 100)]
        public float windSpeed;
        public Vector3 windSpawnZoneDimension;

        [Range(1,50)]
        public int airParticleCount;
        public float airParticleGravityFactor;
    }
}
