using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Physics.Authoring;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;

namespace PhysicsSimulations
{
    public class SimConfigurationController : MonoBehaviour
    {
        [Header("CONFIG SETTINGS")]
        [SerializeField] private SimConfiguration defaultConfig;
        public SimConfigurationSanity configSanityCheck;

        [Header("CAMERA SETTINGS")]
        [SerializeField] private GameObject sideViewVC;
        [SerializeField] private GameObject topViewVC;

        public SimConfiguration CurrentSimConfig { get; private set; }

        [Header("READ ONLY")]
        public float WindMagnitude;
        public int ChangeCarIndex = -1;
        public ViewAngle CurrentViewAngle;
        public bool SpawnAirParticles;

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

            ResetToDefault();
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
            //CurrentSimConfig = defaultConfig;

            WindMagnitude = CurrentSimConfig.airSpeed / Time.fixedDeltaTime;
            //Debug.Log($"Fixed time: {Time.fixedDeltaTime}");
        }

        public void SetCurrentConfig(SimConfiguration config)
        {
            CurrentSimConfig = PerformSanityCheck(config);
        }

        public SimConfiguration PerformSanityCheck(SimConfiguration config)
        {
            SimConfigurationSanity csc = configSanityCheck;
            config.airSpeed = Mathf.Clamp(config.airSpeed, csc.airSpeedMin, csc.airSpeedMax);
            config.windSpawnZoneDimension = new Vector3(
                Mathf.Clamp(config.windSpawnZoneDimension.x, csc.windSpawnZoneDimensionMin.x, csc.windSpawnZoneDimensionMax.x),
                Mathf.Clamp(config.windSpawnZoneDimension.y, csc.windSpawnZoneDimensionMin.y, csc.windSpawnZoneDimensionMax.y),
                Mathf.Clamp(config.windSpawnZoneDimension.z, csc.windSpawnZoneDimensionMin.z, csc.windSpawnZoneDimensionMax.z)
                );
            config.airParticleCount = Mathf.Clamp( config.airParticleCount, csc.airParticleCountMin, csc.airParticleCountMax );
            config.airParticleGravityFactor = Mathf.Clamp(config.airParticleGravityFactor, csc.airParticleGravityFactorMin, csc.airParticleGravityFactorMax);
            return config;
        }

        public SimConfiguration GetDefaultConfig() { return  CurrentSimConfig.Clone(); }

        public void ChangeView(ViewAngle _angle)
        {
            sideViewVC.SetActive(false);
            topViewVC.SetActive(false);

            Debug.Log($"Change View: {_angle}");
            CurrentViewAngle = _angle;

            switch (_angle)
            {
                case ViewAngle.Side:
                    sideViewVC.SetActive(true);
                    break;
                case ViewAngle.Top:
                    topViewVC.SetActive(true);
                    break;
            }
        }

        public void ChangeCar(int _carIndex)
        {
            //return;
            //EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;
            //EntityQuery carQuery = em.CreateEntityQuery(typeof(CarComponent));
            //NativeArray<Entity> carArray = carQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
            //foreach(Entity entity in carArray)
            //{
            //    int _entityIndex = em.GetComponentData<CarComponent>(entity).Index;
            //    LocalTransform obj = em.GetComponentData<LocalTransform>(entity);
            //    obj.Scale = (_entityIndex == _carIndex) ? 1 : 0;
            //    em.SetComponentData(entity, obj);
            //}

            ChangeCarIndex = _carIndex;
        }

        public void ResetToDefault()
        {
            CurrentSimConfig = defaultConfig;
        }
    }

    [Serializable]
    public class SimConfiguration
    {
        [Range(1, 100)]
        public float airSpeed;
        public Vector3 windSpawnZoneDimension;

        [Range(1,50)]
        public int airParticleCount;

        [Range(0,2)]
        public float airParticleGravityFactor;

        public SimConfiguration Clone()
        {
            SimConfiguration config = new SimConfiguration();
            config.airSpeed = this.airSpeed;
            config.windSpawnZoneDimension = this.windSpawnZoneDimension;
            config.airParticleCount = this.airParticleCount;
            config.airParticleGravityFactor = this.airParticleGravityFactor;
            return config;
        }
    }

    [Serializable]
    public class SimConfigurationSanity
    {
        public float airSpeedMin;
        public float airSpeedMax;

        public Vector3 windSpawnZoneDimensionMin;
        public Vector3 windSpawnZoneDimensionMax;

        public int airParticleCountMin;
        public int airParticleCountMax;

        public float airParticleGravityFactorMin;
        public float airParticleGravityFactorMax;
    }
}
