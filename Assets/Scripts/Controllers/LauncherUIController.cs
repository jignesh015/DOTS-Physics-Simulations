using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SimpleFileBrowser;
using System.Threading.Tasks;
using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace PhysicsSimulations
{
    public class LauncherUIController : MonoBehaviour
    {
        [Header("SIMULATION CONFIG")]
        [SerializeField] private SimConfiguration defaultSimConfig;
        [SerializeField] private SimConfigurationSanity simConfigSanityCheck;

        [Header("SIMULATION CONFIG UI")]
        [SerializeField] private TMP_InputField airSpeedInput;
        [SerializeField] private TMP_InputField airParticleRatioInput;
        [SerializeField] private TMP_InputField airParticleBurstCountInput;
        [SerializeField] private Toggle spawnAirParticlesToggle;
        [SerializeField] private TMP_Dropdown carDropdown;
        [SerializeField] private TMP_InputField configNameText;

        [Header("TRAINING CONFIG")]
        [SerializeField] private TrainingConfiguration defaultTrainingConfig;

        [Header("TRAINING CONFIG SETTINGS UI")]
        [SerializeField] private TMP_InputField maxVoxelHeightVarianceInput;
        [SerializeField] private TMP_InputField decisionPeriodInput;
        [SerializeField] private TMP_InputField episodePeriodInput;
        [SerializeField] private Toggle onlyModifyCollidedVoxelsToggle;
        [SerializeField] private Toggle onlyDecreaseHeightToggle;
        [SerializeField] private Toggle fixedEpisodeLengthToggle;

        [Header("TRAINING CONFIG METRICS UI")]
        [SerializeField] private Toggle kineticEnergyMetricToggle;
        [SerializeField] private Toggle dragForceMetricToggle;
        [SerializeField] private Toggle collisionCountMetricToggle;
        [SerializeField] private Toggle heightmapSumMetricToggle;

        [SerializeField] private TMP_InputField maxKineticEnergyVarianceInput;
        [SerializeField] private TMP_InputField maxDragForceVarianceInput;
        [SerializeField] private TMP_InputField maxCollisionCountVarianceInput;

        [Header("TRAINING CONFIG REWARDS UI")]
        [SerializeField] private TMP_InputField kineticEnergyPositiveScoreInput;
        [SerializeField] private TMP_InputField kineticEnergyNegativeScoreInput;
        [SerializeField] private TMP_InputField dragForcePositiveScoreInput;
        [SerializeField] private TMP_InputField dragForceNegativeScoreInput;
        [SerializeField] private TMP_InputField collisionCountPositiveScoreInput;
        [SerializeField] private TMP_InputField collisionCountNegativeScoreInput;
        [SerializeField] private TMP_InputField heightmapSumPositiveScoreInput;
        [SerializeField] private TMP_InputField heightmapSumNegativeScoreInput;

        [Header("TRAINING CONFIG RESULT UI")]
        [SerializeField] private TMP_InputField trainingConfigNameInput;
        [SerializeField] private GameObject trainingConfigNameError;

        [Header("TRAINING HYPER PARAMETERS UI")]
        [SerializeField] private TMP_InputField trainingTimeScaleInput;

        [Header("PROCESS INDICATOR UI")]
        [SerializeField] private GameObject testSimProcessIndicator;
        [SerializeField] private GameObject trainingProcessIndicator;
        [SerializeField] private GameObject checkResultProcessIndicator;

        public SimConfiguration CurrentSimConfig { get; private set; }
        public TrainingConfiguration CurrentTrainingConfig { get; private set; }

        private bool simConfigUISet;
        private bool trainConfigUISet;

        private Process testSimProcess;
        private Process trainingProcess;
        private Process checkResultProcess;

        // Start is called before the first frame update
        void Start()
        {
            Debug.Log($"Air Speed: {defaultSimConfig.airSpeed} | Config Name: {defaultTrainingConfig.configName}");

            ResetSimConfigToDefault();
            ResetTrainingConfigToDefault();
            ToggleProcessRunningIndicator();

            //Create the root directories if doesn't exist
            List<string> _directories = new()
            {
                Data.ConfigRootPathLauncher,
                Data.SimConfigRootPathLauncher,
                Data.TrainingConfigRootPathLauncher,
                Data.CurrentConfigRootPathLauncher
            };

            foreach (string dir in _directories)
            {
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
            }
        }

        #region SIMULATION CONFIG
        public async void ResetSimConfigToDefault()
        {
            simConfigUISet = false;
            CurrentSimConfig = defaultSimConfig;
            await Task.Delay(100);
            SetSimConfigUI(CurrentSimConfig);
        }

        private void SetSimConfigUI(SimConfiguration config, string _configName = "")
        {
            airSpeedInput.text = config.airSpeed.ToString("0");
            airParticleRatioInput.text = config.airParticleRatio.ToString("F2");
            airParticleBurstCountInput.text = config.airParticleBurstCount.ToString();
            spawnAirParticlesToggle.isOn = config.spawnAirParticlesAutomatically;
            if(!string.IsNullOrEmpty(_configName)) configNameText.text = _configName;
            
            simConfigUISet = true;
        }

        public void SaveCurrentSimConfigToJSON()
        {
            if (CurrentSimConfig == null)
            {
                Debug.LogError("Sim Config not found");
                return;
            }

            // Convert the config to JSON format using JsonUtility.
            string jsonData = JsonUtility.ToJson(CurrentSimConfig);

            // Get a path to save the json file
            string _configName = configNameText.text;
            if (string.IsNullOrEmpty(_configName))
                _configName = $"SimConfig_{Data.Timestamp()}";

            string newFilePath = Path.Combine(Data.SimConfigRootPathLauncher, $"{_configName}.json");
            Debug.Log($"File path : {newFilePath}");

            //Also save to current config file
            string currentConfigPath = $"{Path.Combine(Data.CurrentConfigRootPathLauncher, Data.CurrentSimConfigFileName)}";

            // Write the JSON data to the file.
            File.WriteAllText(newFilePath, jsonData);
            File.WriteAllText(currentConfigPath, jsonData);
            Debug.Log($"Saved Sim Config to {newFilePath} and {currentConfigPath}");
        }

        public void LoadSimConfigJSON()
        {
            FileBrowser.ShowLoadDialog((chosenFilePath) =>
            {
                string pathToFile = chosenFilePath[0];
                if(File.Exists(pathToFile))
                {
                    // Read the JSON file content
                    string jsonContent = File.ReadAllText(pathToFile);

                    //Convert to config
                    SimConfiguration _simConfig = new SimConfiguration();
                    _simConfig.LoadFromJson(jsonContent);
                    CurrentSimConfig = PerformSimConfigSanityCheck(_simConfig);
                    SetSimConfigUI(CurrentSimConfig, Path.GetFileNameWithoutExtension(pathToFile));
                }
            }, () =>
            {
                Debug.Log("Load file cancelled");
            }, FileBrowser.PickMode.Files, false, Data.SimConfigRootPathLauncher);
        }

        public void UpdateSimConfigFromUI()
        {
            if (!simConfigUISet) return;

            //Create new instance of scriptable object
            SimConfiguration _config = new()
            {
                airSpeed = float.Parse(airSpeedInput.text),
                airParticleRatio = float.Parse(airParticleRatioInput.text),
                airParticleBurstCount = int.Parse(airParticleBurstCountInput.text),
                spawnAirParticlesAutomatically = spawnAirParticlesToggle.isOn,
                carId = carDropdown.value
            };

            //Perform Sanity Check
            CurrentSimConfig = PerformSimConfigSanityCheck(_config);
            
            //Reset UI
            SetSimConfigUI(CurrentSimConfig);
            
            //Save to json file
            SaveCurrentSimConfigToJSON();
        }

        private SimConfiguration PerformSimConfigSanityCheck(SimConfiguration config)
        {
            SimConfigurationSanity csc = simConfigSanityCheck;
            config.airSpeed = Mathf.Clamp(config.airSpeed, csc.airSpeedMin, csc.airSpeedMax);
            config.windSpawnZoneDimension = new Vector3(
                Mathf.Clamp(config.windSpawnZoneDimension.x, csc.windSpawnZoneDimensionMin.x, csc.windSpawnZoneDimensionMax.x),
                Mathf.Clamp(config.windSpawnZoneDimension.y, csc.windSpawnZoneDimensionMin.y, csc.windSpawnZoneDimensionMax.y),
                Mathf.Clamp(config.windSpawnZoneDimension.z, csc.windSpawnZoneDimensionMin.z, csc.windSpawnZoneDimensionMax.z)
                );
            config.airParticleRatio = Mathf.Clamp(config.airParticleRatio, csc.airParticleRatioMin, csc.airParticleRatioMax);
            config.airParticleBurstCount = Mathf.Clamp(config.airParticleBurstCount, csc.airParticleBurstCountMin, csc.airParticleBurstCountMax);
            return config;
        }
        #endregion

        #region TRAINING CONFIG
        public async void ResetTrainingConfigToDefault()
        {
            trainConfigUISet = false;
            CurrentTrainingConfig = defaultTrainingConfig;
            await Task.Delay(100);
            SetTrainingConfigUI(CurrentTrainingConfig);
        }

        private void SetTrainingConfigUI(TrainingConfiguration _trainingConfig)
        {
            maxVoxelHeightVarianceInput.text = _trainingConfig.maxVoxelHeightVariance.ToString("F3");
            decisionPeriodInput.text = _trainingConfig.decisionPeriod.ToString("0");
            episodePeriodInput.text = _trainingConfig.episodePeriod.ToString("0");
            onlyModifyCollidedVoxelsToggle.isOn = _trainingConfig.onlyModifyCollidedVoxels;
            onlyDecreaseHeightToggle.isOn = _trainingConfig.onlyDecreaseHeight;
            fixedEpisodeLengthToggle.isOn = _trainingConfig.fixedEpisodeLength;

            kineticEnergyMetricToggle.isOn = _trainingConfig.enableKineticEnergyMetric;
            dragForceMetricToggle.isOn = _trainingConfig.enableDragForceMetric;
            collisionCountMetricToggle.isOn = _trainingConfig.enableCollisionCountMetric;
            heightmapSumMetricToggle.isOn = _trainingConfig.enableHeightmapSumMetric;

            maxKineticEnergyVarianceInput.text = _trainingConfig.maxKineticEnergyVariance.ToString("0");
            maxDragForceVarianceInput.text = _trainingConfig.maxDragForceVariance.ToString("0");
            maxCollisionCountVarianceInput.text = _trainingConfig.maxCollisionCountVariance.ToString("0");

            kineticEnergyPositiveScoreInput.text = _trainingConfig.kineticEnergyPositiveScore.ToString("F1");
            kineticEnergyNegativeScoreInput.text = _trainingConfig.kineticEnergyNegativeScore.ToString("F1");
            dragForcePositiveScoreInput.text = _trainingConfig.dragForcePositiveScore.ToString("F1");
            dragForceNegativeScoreInput.text = _trainingConfig.dragForceNegativeScore.ToString("F1");
            collisionCountPositiveScoreInput.text = _trainingConfig.collisionCountPositiveScore.ToString("F1");
            collisionCountNegativeScoreInput.text = _trainingConfig.collisionCountNegativeScore.ToString("F1");
            heightmapSumPositiveScoreInput.text = _trainingConfig.heightmapSumPositiveScore.ToString("F1");
            heightmapSumNegativeScoreInput.text = _trainingConfig.heightmapSumNegativeScore.ToString("F1");

            trainingConfigNameInput.text = _trainingConfig.configName;
            trainingTimeScaleInput.text = _trainingConfig.timeScale.ToString();
            trainConfigUISet = true;
        }

        public void SaveCurrentTrainingConfigToJSON()
        {
            if(CurrentTrainingConfig == null)
            {
                Debug.LogError("Training Config not found");
                return;
            }

            // Convert the config to JSON format using JsonUtility.
            string jsonData = JsonUtility.ToJson(CurrentTrainingConfig);

            // Get a path to save the json file
            string newfilePath = Path.Combine(Data.TrainingConfigRootPathLauncher, $"{CurrentTrainingConfig.configName}.json");

            //Also save to current config file
            string currentConfigPath = $"{Path.Combine(Data.CurrentConfigRootPathLauncher, Data.CurrentTrainingConfigFileName)}";

            // Write the JSON data to the file.
            File.WriteAllText(newfilePath, jsonData);
            File.WriteAllText(currentConfigPath, jsonData);
            Debug.Log($"Saved Training Config to {newfilePath} and {currentConfigPath}");
        }

        public void LoadTrainingConfigJSON()
        {
            FileBrowser.ShowLoadDialog((chosenFilePath) =>
            {
                string pathToFile = chosenFilePath[0];
                if (File.Exists(pathToFile))
                {
                    // Read the JSON file content
                    string jsonContent = File.ReadAllText(pathToFile);

                    Debug.Log($"{jsonContent}");

                    //Convert to config
                    CurrentTrainingConfig = new TrainingConfiguration(); 
                    CurrentTrainingConfig.LoadFromJson(jsonContent);
                    SetTrainingConfigUI(CurrentTrainingConfig);
                }
            }, () =>
            {
                Debug.Log("Load file cancelled");
            }, FileBrowser.PickMode.Files, false, Data.TrainingConfigRootPathLauncher);
        }

        public void UpdateTrainingConfigFromUI()
        {
            if (!trainConfigUISet) return;

            if(string.IsNullOrEmpty(trainingConfigNameInput.text))
            {
                trainingConfigNameError.SetActive(true);
                return;
            }

            //Create new instance of scriptable object
            CurrentTrainingConfig = new TrainingConfiguration
            {
                maxVoxelHeightVariance = (float)Math.Round(float.Parse(maxVoxelHeightVarianceInput.text), 3),
                decisionPeriod = int.Parse(decisionPeriodInput.text),
                episodePeriod = int.Parse(episodePeriodInput.text),
                onlyModifyCollidedVoxels = onlyModifyCollidedVoxelsToggle.isOn,
                onlyDecreaseHeight = onlyDecreaseHeightToggle.isOn,
                fixedEpisodeLength = fixedEpisodeLengthToggle.isOn,

                enableKineticEnergyMetric = kineticEnergyMetricToggle.isOn,
                enableDragForceMetric = dragForceMetricToggle.isOn,
                enableCollisionCountMetric = collisionCountMetricToggle.isOn,
                enableHeightmapSumMetric = heightmapSumMetricToggle.isOn,

                maxKineticEnergyVariance = float.Parse(maxKineticEnergyVarianceInput.text),
                maxDragForceVariance = int.Parse(maxDragForceVarianceInput.text),
                maxCollisionCountVariance = int.Parse(maxCollisionCountVarianceInput.text),

                kineticEnergyPositiveScore = float.Parse(kineticEnergyPositiveScoreInput.text),
                kineticEnergyNegativeScore = float.Parse(kineticEnergyNegativeScoreInput.text),
                dragForcePositiveScore = float.Parse(dragForcePositiveScoreInput.text),
                dragForceNegativeScore = float.Parse(dragForceNegativeScoreInput.text),
                collisionCountPositiveScore = float.Parse(collisionCountPositiveScoreInput.text),
                collisionCountNegativeScore = float.Parse(collisionCountNegativeScoreInput.text),
                heightmapSumPositiveScore = float.Parse(heightmapSumPositiveScoreInput.text),
                heightmapSumNegativeScore = float.Parse(heightmapSumNegativeScoreInput.text),

                configName = trainingConfigNameInput.text,
                timeScale = int.Parse(trainingTimeScaleInput.text)
            };

            //Save to json file
            SaveCurrentTrainingConfigToJSON();
        }

        public void OnTrainingConfigNameChange(string _value)
        {
            if (!trainConfigUISet) return;
            trainingConfigNameError.SetActive(string.IsNullOrEmpty(_value));
        }

        #endregion

        #region UI INTERACTION METHODS
        public async void OnTestSimButtonClicked()
        {
            if (!Directory.Exists(Data.LanderBuildPath))
            {
                Debug.LogError($"Lander not found at {Data.LanderBuildPath} ");
                return;
            }
            //Save the sim config before proceeding
            UpdateSimConfigFromUI();

            await Task.Delay(100);

            LoadLanderBuild(0);
        }

        public async void OnStartTrainingButtonClicked()
        {
            if (string.IsNullOrEmpty(trainingConfigNameInput.text) ||
                Directory.Exists(Path.Combine(Data.ResultsPathLauncher, trainingConfigNameInput.text)))
            {
                trainingConfigNameError.SetActive(true);
                return;
            }

            if (!Directory.Exists(Data.LanderBuildPath))
            {
                Debug.LogError($"Lander not found at {Data.LanderBuildPath} ");
                return;
            }
            //Save the sim and training config before proceeding
            UpdateSimConfigFromUI();
            UpdateTrainingConfigFromUI();

            await Task.Delay(100);

            LoadLanderBuild(1);
        }

        public void OnCheckResultButtonClicked()
        {
            FileBrowser.ShowLoadDialog((chosenFolderPath) =>
            {
                string pathToFolder = chosenFolderPath[0];
                string resultHeightmapPath = Data.GetResultHeightmapPath(pathToFolder, Path.GetFileName(pathToFolder));
                if (Directory.Exists(pathToFolder) 
                    && File.Exists(resultHeightmapPath))
                {
                    //Save result heightmap path to an indicator file for lander
                    string resultFilePath = Path.Combine(Data.CurrentConfigRootPathLauncher, Data.ResultFolderIndicatorFileName);
                    File.WriteAllText(resultFilePath, pathToFolder);

                    LoadLanderBuild(2);
                }
            }, () =>
            {
                Debug.Log("Load folders cancelled");
            }, FileBrowser.PickMode.Folders, false, Data.ResultsPathLauncher);
        }

        /// <summary>
        /// Loads the lander build with specified indicator
        /// 0 = Simulation only
        /// 1 = Training + Simulation
        /// </summary>
        /// <param name="_indicatorIndex"></param>
        private void LoadLanderBuild(int _indicatorIndex)
        {
            string indicatorFilePath = Path.Combine(Data.CurrentConfigRootPathLauncher, Data.SimIndicatorFileName);
            File.WriteAllText(indicatorFilePath, _indicatorIndex.ToString());

            //Reset processes
            testSimProcess?.Dispose();
            trainingProcess?.Dispose();
            checkResultProcess?.Dispose();

            switch(_indicatorIndex)
            {
                case 0:
                    StartSimulation();
                    break;
                case 1:
                    StartTraining();
                    break;
                case 2:
                    CheckResult();
                    break;
            }

            InvokeRepeating(nameof(CheckForProcessTerminaiton), 1f,1f);
        }

        private void StartSimulation()
        {
            string executablePath = Path.Combine(Data.LanderBuildPath, Data.LanderBuildName);
            testSimProcess = new Process();
            testSimProcess.StartInfo.FileName = executablePath;
            testSimProcess.EnableRaisingEvents = true;
            testSimProcess.Start();

            //Toggle indicator
            ToggleProcessRunningIndicator(0);
        }

        private void StartTraining()
        {
            string venvPath = Data.VirtualEnvironmentPath();
            string executablePath = Data.LanderBuildPath;

            Debug.Log($"Venv: {venvPath}");
            Debug.Log($"executablePath: {executablePath}");

            // Construct the full command
            string activateCommand = $"\"{venvPath}\\Scripts\\activate\"";
            string trainCommand = $"mlagents-learn config\\AdjustHeight.yaml  --env={executablePath} --run-id={CurrentTrainingConfig.configName} " +
                $" --width={Screen.currentResolution.width} --height={Screen.currentResolution.height}  --max-lifetime-restarts=0 --time-scale={CurrentTrainingConfig.timeScale}";

            trainingProcess = new Process();
            trainingProcess.StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/K {activateCommand} && {trainCommand}", // Use /K to keep the Command Prompt window open
                UseShellExecute = true,
                CreateNoWindow = false
            };
            trainingProcess.EnableRaisingEvents = true;
            trainingProcess.Start();

            //Toggle indicator
            ToggleProcessRunningIndicator(1);
        }

        private void CheckResult()
        {
            string executablePath = Path.Combine(Data.LanderBuildPath, Data.LanderBuildName);
            checkResultProcess = new Process();
            checkResultProcess.StartInfo.FileName = executablePath;
            checkResultProcess.EnableRaisingEvents = true;
            checkResultProcess.Start();

            //Toggle indicator
            ToggleProcessRunningIndicator(2);
        }

        public void OnQuitButtonClicked()
        {
            Application.Quit();
        }
        #endregion

        #region UI METHODS
        private void ToggleProcessRunningIndicator(int _index = -1)
        {
            Debug.Log($"ToggleProcessRunningIndicator {_index}");
            testSimProcessIndicator.SetActive(_index == 0);
            trainingProcessIndicator.SetActive(_index == 1);
            checkResultProcessIndicator.SetActive(_index == 2);
        }
        #endregion

        private void CheckForProcessTerminaiton()
        {
            Debug.Log($"CheckForProcessTerminaiton");
            if (testSimProcess != null && testSimProcess.HasExited)
            {
                testSimProcess = null;
                ToggleProcessRunningIndicator();
                CancelInvoke(nameof(CheckForProcessTerminaiton));
            }
            if (trainingProcess != null && trainingProcess.HasExited)
            {
                trainingProcess = null;
                ToggleProcessRunningIndicator();
                CancelInvoke(nameof(CheckForProcessTerminaiton));
            }
            if(checkResultProcess != null && checkResultProcess.HasExited)
            {
                checkResultProcess = null;
                ToggleProcessRunningIndicator();
                CancelInvoke(nameof(CheckForProcessTerminaiton));
            }
        }
    }
}
