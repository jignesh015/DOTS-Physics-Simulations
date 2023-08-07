using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
using System.Collections;

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

        [Header("AIR PARTICLE SPAWN SETTINGS")]
        public bool SpawnAirParticlesAutomatically;
        public bool SpawnAirParticlesCommand { get; set; }
        public bool SpawnAirParticles { get; private set; }
        public int AirParticlesBurstCount { get; set; }

        [Header("VOXEL GRID SETTINGS")]
        public bool ShowCollisionHeatmap;
        public bool VoxelGridReady { get; set; }
        public bool VoxelsReady { get; set; }

        [Header("READ ONLY")]
        public float WindMagnitude;
        public int ChangeCarIndex = -1;
        public ViewAngle CurrentViewAngle;
        public int VoxelCollisionCount;
        public List<float> KineticEnergyList;
        public List<float> DragForceList;
        
        public SimConfiguration CurrentSimConfig { get; private set; }

        //METRICS
        public float AverageKineticEnergy { get; private set; }
        public float AverageDragForce { get; private set; }

        //EVENT DELEGATES
        public Action OnVoxelsReady;
        public Action OnAirSpawnStarted;
        public Action OnAirSpawnStopped;


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
            OnVoxelsReady += OnVoxelsReadyListener;
        }

        // Update is called once per frame
        void Update()
        {
           
        }

        private void FixedUpdate()
        {
            //WindMagnitude = CurrentSimConfig.airSpeed / Time.fixedDeltaTime;
            WindMagnitude = CurrentSimConfig.airSpeed;

            if (SpawnAirParticles && CurrentSimConfig.airParticleBurstCount > 0)
            {
                if (AirParticlesBurstCount >= CurrentSimConfig.airParticleBurstCount)
                {
                    StopAirParticles();
                }
            }
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
            config.airParticleBurstCount = Mathf.Clamp(config.airParticleBurstCount, csc.airParticleBurstCountMin, csc.airParticleBurstCountMax );
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

        public void OnVoxelsReadyListener()
        {
            if(VoxelsReady && SpawnAirParticlesAutomatically)
            {
                Debug.Log($"<color=magenta>Voxels Ready</color>");
                SpawnAirParticlesWithDelay(2000);
            }
        }

        public void SpawnAirParticlesWithDelay(int _delayInMS)
        {
            if (SpawnAirParticlesCommand) return;
            SpawnAirParticlesCommand = true;
            //Debug.Log($"<color=cyan>SpawnAirParticlesWithDelay {_delayInMS}</color>");
            StartCoroutine(SpawnAirParticlesWithDelayAsync(_delayInMS));
        }

        private IEnumerator SpawnAirParticlesWithDelayAsync(int _delayInMS)
        {
            yield return new WaitForSeconds((float)_delayInMS / 1000);
            SpawnAirParticles = true;
            SpawnAirParticlesCommand = false;

            VoxelCollisionCount = 0;
            AirParticlesBurstCount = 0;
            OnAirSpawnStarted?.Invoke();
            ResetKineticEnergyList();
            ResetDragForceList();
        }

        public void StopAirParticles() 
        { 
            SpawnAirParticles = false;
            CalculateAverageKineticEnergy();
            CalculateAverageDragForce();

            OnAirSpawnStopped?.Invoke();
        }

        public int GetImpactLevel(int collisionCount)
        {
            float averageCollisionCount =  (float)VoxelCollisionCount/carHeightMapGenerator.VoxelCount;
            int _impactLevel = 0;
            if (collisionCount > averageCollisionCount*5)
                _impactLevel = 3;
            else if(collisionCount > averageCollisionCount*2)
                _impactLevel = 2;
            else if(collisionCount > averageCollisionCount/2)
                _impactLevel = 1;
            return _impactLevel;

        }

        #region KINETIC ENERGY
        public void ResetKineticEnergyList()
        {
            AverageKineticEnergy = 0;
            KineticEnergyList = new List<float>();
        }

        public void UpdateKineticEnergyList(float _energyValue)
        {
            if(SpawnAirParticles && KineticEnergyList.Count < 10000)
            {
                KineticEnergyList.Add(_energyValue);
            }
        }

        public void CalculateAverageKineticEnergy()
        {
            if(KineticEnergyList != null && KineticEnergyList.Count > 0)
            {
                AverageKineticEnergy = KineticEnergyList.Average();
                //Debug.Log($"<color=olive>Average KE = {AverageKineticEnergy}</color>");
                KineticEnergyList.Clear();
            }
            else
            {
                AverageKineticEnergy = 0;
                Debug.Log($"<color=red>KineticEnergyList is empty</color>");
            }
        }
        #endregion

        #region DRAG FORCE
        public void ResetDragForceList()
        {
            AverageDragForce = 0;
            DragForceList = new List<float>();
        }

        public void UpdateDragForceList(float _forceValue)
        {
            if (SpawnAirParticles && DragForceList.Count < 10000)
            {
                DragForceList.Add(_forceValue);
            }
        }

        public void CalculateAverageDragForce()
        {
            if(DragForceList != null && DragForceList.Count > 0)
            {
                AverageDragForce = DragForceList.Average();
                DragForceList.Clear();
            }
            else
            { 
                AverageDragForce = 0;
                Debug.Log($"<color=red>DragForceList is empty</color>");
            }
        }
        #endregion
    }
}
