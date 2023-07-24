using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        [SerializeField] private GameObject mainCamera;
        [SerializeField] private GameObject freeLookCamera;
        [SerializeField] private GameObject sideViewVC;
        [SerializeField] private GameObject topViewVC;

        [Header("SCRIPT REFERENCES")]
        public CarHeightMapGenerator carHeightMapGenerator;

        public SimConfiguration CurrentSimConfig { get; private set; }

        [Header("READ ONLY")]
        public float WindMagnitude;
        public int ChangeCarIndex = -1;
        public ViewAngle CurrentViewAngle;
        public int VoxelCollisionCount;

        public bool SpawnAirParticlesCommand { get; set; }
        public bool SpawnAirParticles { get; private set; }

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
            
        }

        private void FixedUpdate()
        {
            //WindMagnitude = CurrentSimConfig.airSpeed / Time.fixedDeltaTime;
            WindMagnitude = CurrentSimConfig.airSpeed;
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
            config.airParticleRatio = Mathf.Clamp( config.airParticleRatio, csc.airParticleRatioMin, csc.airParticleRatioMax );
            config.airParticleGravityFactor = Mathf.Clamp(config.airParticleGravityFactor, csc.airParticleGravityFactorMin, csc.airParticleGravityFactorMax);
            return config;
        }

        public SimConfiguration GetDefaultConfig() { return  CurrentSimConfig.Clone(); }

        public void ChangeView(ViewAngle _angle)
        {
            mainCamera.SetActive(false);
            freeLookCamera.SetActive(false);
            sideViewVC.SetActive(false);
            topViewVC.SetActive(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Debug.Log($"Change View: {_angle}");
            CurrentViewAngle = _angle;

            switch (_angle)
            {
                case ViewAngle.Side:
                    mainCamera.SetActive(true);
                    sideViewVC.SetActive(true);
                    break;
                case ViewAngle.Top:
                    mainCamera.SetActive(true);
                    topViewVC.SetActive(true);
                    break;
                case ViewAngle.FreeLook:
                    freeLookCamera.SetActive(true);
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

        public async void SpawnAirParticlesWithDelay(int _delayInMS)
        {
            SpawnAirParticlesCommand = true;
            await Task.Delay( _delayInMS );
            SpawnAirParticles = true;
            SpawnAirParticlesCommand = false;
        }

        public void StopAirParticles() { SpawnAirParticles = false; }

        public int GetImpactLevel(int collisionCount)
        {
            float averageCollisionCount =  (float)VoxelCollisionCount/carHeightMapGenerator.VoxelCount;
            int _impactLevel = 0;
            if (collisionCount > averageCollisionCount*2)
                _impactLevel = 3;
            else if(collisionCount > averageCollisionCount)
                _impactLevel = 2;
            else if(collisionCount > averageCollisionCount/2)
                _impactLevel = 1;
            return _impactLevel;

        }
    }

    [Serializable]
    public class SimConfiguration
    {
        [Range(10, 100)]
        public float airSpeed;
        public Vector3 windSpawnZoneDimension;

        [Range(0.1f,2f)]
        public float airParticleRatio;

        [Range(0,2)]
        public float airParticleGravityFactor;

        public SimConfiguration Clone()
        {
            SimConfiguration config = new SimConfiguration();
            config.airSpeed = this.airSpeed;
            config.windSpawnZoneDimension = this.windSpawnZoneDimension;
            config.airParticleRatio = this.airParticleRatio;
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

        public float airParticleRatioMin;
        public float airParticleRatioMax;

        public float airParticleGravityFactorMin;
        public float airParticleGravityFactorMax;
    }
}
