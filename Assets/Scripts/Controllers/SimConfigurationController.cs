using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
using System.Collections;
using System.IO;

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

        [Header("VOXEL GRID SETTINGS")]
        public bool ShowCollisionHeatmap;
        public bool VoxelGridReady { get; set; }
        public bool VoxelsReady { get; set; }

        [Header("READ ONLY")]
        public float WindMagnitude;
        public int ChangeCarIndex = -1;
        public ViewAngle CurrentViewAngle;
        public List<float> KineticEnergyList;
        public List<float> DragForceList;
        
        public SimConfiguration CurrentSimConfig { get; private set; }

        //METRICS
        public float AverageKineticEnergy { get; private set; }
        public float AverageDragForce { get; private set; }
        public int VoxelCollisionCount { get; set; }

        //BASELINE METRICS
        public float InitialKineticEnergy { get; private set; }
        public float InitialDragForce { get; private set; }
        public int InitialVoxelCollisionCount { get; set; }

        //AIR PARTICLE SPAWN SETTINGS
        public bool SpawnAirParticlesCommand { get; set; }
        public bool SpawnAirParticles { get; private set; }
        public int AirParticlesBurstCount { get; set; }

        //EVENT DELEGATES
        public Action<SimConfiguration> OnSimConfigLoaded;
        public Action<TrainingConfiguration> OnTrainConfigLoaded;
        public Action OnVoxelsReady;
        public Action OnAirSpawnStarted;
        public Action OnAirSpawnStopped;

        private int spawnedAirCycleCount;

        //VCC Average Settings
        private int baseCycleCount = 10;
        [HideInInspector]public float vccAverageFactor;

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
        }

        // Start is called before the first frame update
        void Start()
        {
            LoadSimConfig();
            ChangeView(ViewAngle.FreeLook);

            //Assign listeners
            OnVoxelsReady += OnVoxelsReadyListener;
        }

        private async void LoadSimConfig()
        {
            int simIndicator = PlayerPrefs.GetInt(Data.SimIndicatorPref);
            bool _isCheckingResult = simIndicator == 2 && PlayerPrefs.HasKey(Data.ResultPathPref);

            //Path to current sim config file
            string pathToSimConfigFile = _isCheckingResult ? Path.Combine(PlayerPrefs.GetString(Data.ResultPathPref), Data.CurrentSimConfigFileName)
                : Path.Combine(Data.CurrentConfigRootPathLander, Data.CurrentSimConfigFileName);
            if (!Directory.Exists(Data.ConfigRootPathLander)
               || !Directory.Exists(Data.CurrentConfigRootPathLander)
               || !File.Exists(pathToSimConfigFile))
            {
                Debug.Log("<color=red>Config files not found. Starting simulation using default settings</color>");
                ResetToDefault();
            }
            else
            {
                // Read the JSON file content
                string jsonContent = File.ReadAllText(pathToSimConfigFile);

                //Convert to config
                SimConfiguration _simConfig = new SimConfiguration();
                _simConfig.LoadFromJson(jsonContent);
                SetCurrentConfig(_simConfig);
            }

            await Task.Delay(100);

            //Load heightmap as per current sim config/result 
            if(_isCheckingResult)
            {
                CurrentSimConfig.spawnAirParticlesAutomatically = false;

                //Set burst count to 200 and re-calculate VCC Avg Factor
                CurrentSimConfig.airParticleBurstCount = 200;
                vccAverageFactor = (float)CurrentSimConfig.airParticleBurstCount / baseCycleCount;

                //Load the result heightmap
                string _resultPath = PlayerPrefs.GetString(Data.ResultPathPref);
                string resultHeightmapPath = Data.GetResultHeightmapPath(_resultPath, Path.GetFileName(_resultPath));
                carHeightMapGenerator.LoadHeightmap(resultHeightmapPath, CurrentSimConfig.carId);

                //Load training output
                LoadTrainingOutput(_resultPath);
            }
            else
            {
                carHeightMapGenerator.LoadHeightmap(CurrentSimConfig.carId);
            }

            OnSimConfigLoaded?.Invoke(CurrentSimConfig.Clone());

            //Enable training if applicable
            if (simIndicator == 1 && FindObjectOfType<TrainingController>(true) != null)
            {
                CurrentSimConfig.spawnAirParticlesAutomatically = true;
                OnAirSpawnStopped += EnableTraining;
            }
        }

        private void LoadTrainingOutput(string _resultPath)
        {
            string _trainingOutputFilePath = Path.Combine(_resultPath, $"{Path.GetFileName(_resultPath)}_{Data.TrainingOutputFileName}");
            if (File.Exists(_trainingOutputFilePath))
            {
                // Read the JSON file content
                string jsonContent = File.ReadAllText(_trainingOutputFilePath);

                //Convert to Training output
                TrainingOutput trainingOutput = new();
                trainingOutput.LoadFromJson(jsonContent);

                InitialKineticEnergy = trainingOutput.baselineKineticEnergy;
                InitialDragForce = trainingOutput.baselineDragForce;
                InitialVoxelCollisionCount = trainingOutput.baselineVoxelCollisionCount;
            }
        }

        private void EnableTraining()
        {
            TrainingController tc = FindObjectOfType<TrainingController>(true);
            if(tc.gameObject.activeSelf)
                OnAirSpawnStopped -= EnableTraining;
            else
            {
                SpawnAirParticlesWithDelay(0);
                tc.gameObject.SetActive(true);
            }
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

            //Calculate VCC Avg Factor
            vccAverageFactor = (float)CurrentSimConfig.airParticleBurstCount / baseCycleCount;
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

        public void ToggleCollisionHeatmap(bool _value)
        {
            ShowCollisionHeatmap = _value;
        }

        public void ResetToDefault()
        {
            CurrentSimConfig = defaultConfig;
        }

        public void OnVoxelsReadyListener()
        {
            if(VoxelsReady && CurrentSimConfig.spawnAirParticlesAutomatically)
            {
                //Debug.Log($"<color=magenta>Voxels Ready</color>");
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
            spawnedAirCycleCount++;
            SpawnAirParticles = false;
            CalculateAverageKineticEnergy();
            CalculateAverageDragForce();

            //Store initial Collision Count
            if (spawnedAirCycleCount > 1 && InitialVoxelCollisionCount == 0)
                InitialVoxelCollisionCount = VoxelCollisionCount;

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

                //Store initial Avg KE
                if (spawnedAirCycleCount > 1 && InitialKineticEnergy == 0)
                    InitialKineticEnergy = AverageKineticEnergy;
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

                //Store initial Avg Drag Force
                if (spawnedAirCycleCount > 1 && InitialDragForce == 0)
                    InitialDragForce = AverageDragForce;
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
