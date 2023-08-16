using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PhysicsSimulations
{
    public class TrainingController : MonoBehaviour
    {
        [Header("CONFIG SETTINGS")]
        [SerializeField] private TrainingConfiguration defaultTrainConfig;

        [Header("AGENTS")]
        public AdjustHeightAgent adjustHeightAgent;

        [Header("TRAINING PATHS")]
        [SerializeField] private string resultOutputName = "[result folder name]";

        [Header("TRAINING SETTINGS")]
        public bool compareWithOgHeight;

        private SimConfigurationController scc;

        [Header("READ ONLY")]
        public bool SetNewVoxelHeight;
        public float VoxelHeightFactor;
        public List<float> VoxelHeightFactorList;

        private int heightMapTextureLength;

        public TrainingConfiguration CurrentTrainConfig { get; set; }

        //AGENT PARAMETERS
        public int StepCountText { get; set; }
        public int EpisodeCount { get; set; }
        public float CumulativeReward { get; set; }
        public DateTime TrainingStartTime { get; set; }

        private static TrainingController _instance;
        public static TrainingController Instance { get { return _instance; } }


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
            scc = SimConfigurationController.Instance;

            //Load config 
            LoadTrainConfig();

            //Assign listeners
            scc.OnAirSpawnStarted += EnableAdjustHeightAgent;

            heightMapTextureLength = scc.carHeightMapGenerator.TextureHeight;

            TrainingStartTime = DateTime.Now;
        }

        private void OnEnable()
        {
            if (scc != null)
            {
                scc.OnAirSpawnStarted += EnableAdjustHeightAgent;
            }
        }

        private void OnDisable()
        {
            scc.OnAirSpawnStarted -= EnableAdjustHeightAgent;
        }

        private async void LoadTrainConfig()
        {
            //Path to current training config file
            string pathToTrainConfigFile = Path.Combine(Data.CurrentConfigRootPathLander, Data.CurrentTrainingConfigFileName);
            if (!Directory.Exists(Data.ConfigRootPathLander)
               || !Directory.Exists(Data.CurrentConfigRootPathLander)
               || !File.Exists(pathToTrainConfigFile))
            {
                Debug.Log("<color=red>Training Config file not found. Using default training config</color>");
                CurrentTrainConfig = defaultTrainConfig.Clone();
            }
            else
            {
                // Read the JSON file content
                string jsonContent = File.ReadAllText(pathToTrainConfigFile);

                //Convert and save as current train config
                CurrentTrainConfig = new TrainingConfiguration();
                CurrentTrainConfig.LoadFromJson(jsonContent);
            }

            await Task.Delay(100);

            scc.OnTrainConfigLoaded?.Invoke(CurrentTrainConfig.Clone());
        }

        public void EnableAdjustHeightAgent()
        {
            adjustHeightAgent.gameObject.SetActive(true);
        }

        public float GetHeightFactor(int _rowIndex)
        {
            if(VoxelHeightFactorList != null && VoxelHeightFactorList.Count > 0)
            {
                return VoxelHeightFactorList[Mathf.FloorToInt(_rowIndex/(heightMapTextureLength/VoxelHeightFactorList.Count))];
            }
            return 0;
        }

        public float GetHeightFactor(int _rowIndex, int _columnIndex)
        {
            if (VoxelHeightFactorList != null && VoxelHeightFactorList.Count > 0)
            {
                if (VoxelHeightFactorList.Count == 1)
                    return VoxelHeightFactor;

                return VoxelHeightFactorList[_rowIndex + _columnIndex * heightMapTextureLength];
            }

            return 0;
        }

        public void StartTraining()
        {
            string venvPath = Data.VirtualEnvironmentPath();

            // Construct the full command
            string activateCommand = $"\"{venvPath}\\Scripts\\activate\"";
            string trainCommand = $"mlagents-learn config\\AdjustHeight.yaml --run-id={resultOutputName}";

#if !UNITY_EDITOR
            //Add executable path for standalone builds
            string executablePath = Directory.GetParent(Application.dataPath).FullName;
            trainCommand = $"mlagents-learn config\\AdjustHeight.yaml  --env={executablePath} --run-id={resultOutputName}";
#endif

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/K {activateCommand} && {trainCommand}", // Use /K to keep the Command Prompt window open
                UseShellExecute = true,
                CreateNoWindow = false
            };

            Process.Start(startInfo);
        }

        public void SaveObservationsToCSV(int _completedEpisodes)
        {
            string _resultDir = Path.Combine(Data.ResultsPathLander, CurrentTrainConfig.configName);
            if (!Directory.Exists(_resultDir)) return;

            string _csvFilePath = Path.Combine(_resultDir, $"{CurrentTrainConfig.configName}_{Data.MetricObservationFileName}");

            if(!File.Exists(_csvFilePath))
            {
                using StreamWriter writer = new(_csvFilePath);
                string header = "Episode,Kinetic Energy,Drag Force,Voxel Collision Count";
                writer.WriteLine(header);
            }

            // Append the new values to the CSV file
            using StreamWriter writerAppend = new(_csvFilePath, true);
            string line = $"{_completedEpisodes},{scc.AverageKineticEnergy},{scc.AverageDragForce},{scc.VoxelCollisionCount}";
            writerAppend.WriteLine(line);
        }

        public void OnApplicationQuit()
        {
            string _resultDir = Path.Combine(Data.ResultsPathLander, CurrentTrainConfig.configName);
            Debug.Log($"OnApplicationQuit: {_resultDir}");

            //Save current heightmap to json
            if (Directory.Exists(_resultDir))
            {
                // Convert the list to JSON format using JsonUtility.
                string jsonData = JsonUtility.ToJson(new SerializableList<float>(scc.carHeightMapGenerator.updatedHeightmapList));

                // Write the JSON data to the file.
                File.WriteAllText(Data.GetResultHeightmapPath(_resultDir, CurrentTrainConfig.configName), jsonData);

                Debug.Log($"Heightmap saved at: {_resultDir}");
            }

            //Copy current sim config to result directory
            string pathToSimConfigFile = Path.Combine(Data.CurrentConfigRootPathLander, Data.CurrentSimConfigFileName);
            if (Directory.Exists(Data.ConfigRootPathLander)
               && Directory.Exists(Data.CurrentConfigRootPathLander)
               && File.Exists(pathToSimConfigFile))
            {
                File.Copy(pathToSimConfigFile, Path.Combine(_resultDir, Path.GetFileName(pathToSimConfigFile)));
            }

            //Copy current training config file to result directory
            string pathToTrainConfigFile = Path.Combine(Data.CurrentConfigRootPathLander, Data.CurrentTrainingConfigFileName);
            if (Directory.Exists(Data.ConfigRootPathLander)
               && Directory.Exists(Data.CurrentConfigRootPathLander)
               && File.Exists(pathToTrainConfigFile))
            {
                File.Copy(pathToTrainConfigFile, Path.Combine(_resultDir, Path.GetFileName(pathToTrainConfigFile)));
            }

            //Save Training output to result directory
            if (Directory.Exists(_resultDir))
            {
                TrainingOutput trainingOutput = new()
                {
                    baselineKineticEnergy = scc.InitialKineticEnergy,
                    baselineDragForce = scc.InitialDragForce,
                    baselineVoxelCollisionCount = scc.InitialVoxelCollisionCount,
                    trainingTime = Data.FormattedTimer(TrainingStartTime)
                };

                //Convert to json
                string jsonData = JsonUtility.ToJson(trainingOutput);

                // Write the JSON data to the file.
                File.WriteAllText(Path.Combine(_resultDir, $"{CurrentTrainConfig.configName}_{Data.TrainingOutputFileName}"), jsonData);

                Debug.Log($"Training Output saved at: {_resultDir}");
            }
        }
    }
}
