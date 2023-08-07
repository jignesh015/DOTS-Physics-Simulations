using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SimpleFileBrowser;
using System.Threading.Tasks;
using System;

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
        [SerializeField] private Toggle fixedEpisodeLengthToggle;

        [Header("TRAINING CONFIG METRICS UI")]
        [SerializeField] private Toggle kineticEnergyMetricToggle;
        [SerializeField] private Toggle dragForceMetricToggle;
        [SerializeField] private Toggle collisionCountMetricToggle;

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

        [Header("TRAINING CONFIG RESULT UI")]
        [SerializeField] private TMP_InputField trainingConfigNameInput;
        [SerializeField] private GameObject trainingConfigNameError;

        public SimConfiguration CurrentSimConfig { get; private set; }
        public TrainingConfiguration CurrentTrainingConfig { get; private set; }

        private bool simConfigUISet;
        private bool trainConfigUISet;

        // Start is called before the first frame update
        void Start()
        {
            Debug.Log($"Air Speed: {defaultSimConfig.airSpeed} | Config Name: {defaultTrainingConfig.configName}");

            ResetSimConfigToDefault();
            ResetTrainingConfigToDefault();

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
            string currentConfigPath = $"{Path.Combine(Data.CurrentConfigRootPathLauncher, Data.CurrentSimConfigFileName)}.json";

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

                    Debug.Log($"{jsonContent}");

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
            SimConfiguration _config = new SimConfiguration();
            _config.airSpeed = float.Parse(airSpeedInput.text);
            _config.airParticleRatio = float.Parse(airParticleRatioInput.text);
            _config.spawnAirParticlesAutomatically = spawnAirParticlesToggle.isOn;
            _config.carId = carDropdown.value;
            
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
            fixedEpisodeLengthToggle.isOn = _trainingConfig.fixedEpisodeLength;

            kineticEnergyMetricToggle.isOn = _trainingConfig.enableKineticEnergyMetric;
            dragForceMetricToggle.isOn = _trainingConfig.enableDragForceMetric;
            collisionCountMetricToggle.isOn = _trainingConfig.enableCollisionCountMetric;

            maxKineticEnergyVarianceInput.text = _trainingConfig.maxKineticEnergyVariance.ToString("0");
            maxDragForceVarianceInput.text = _trainingConfig.maxDragForceVariance.ToString("0");
            maxCollisionCountVarianceInput.text = _trainingConfig.maxCollisionCountVariance.ToString("0");

            kineticEnergyPositiveScoreInput.text = _trainingConfig.kineticEnergyPositiveScore.ToString("F1");
            kineticEnergyNegativeScoreInput.text = _trainingConfig.kineticEnergyNegativeScore.ToString("F1");
            dragForcePositiveScoreInput.text = _trainingConfig.dragForcePositiveScore.ToString("F1");
            dragForceNegativeScoreInput.text = _trainingConfig.dragForceNegativeScore.ToString("F1");
            collisionCountPositiveScoreInput.text = _trainingConfig.collisionCountPositiveScore.ToString("F1");
            collisionCountNegativeScoreInput.text = _trainingConfig.collisionCountNegativeScore.ToString("F1");

            trainingConfigNameInput.text = _trainingConfig.configName;
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
            string currentConfigPath = $"{Path.Combine(Data.CurrentConfigRootPathLauncher, Data.CurrentTrainingConfigFileName)}.json";

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
                fixedEpisodeLength = fixedEpisodeLengthToggle.isOn,

                enableKineticEnergyMetric = kineticEnergyMetricToggle.isOn,
                enableDragForceMetric = dragForceMetricToggle.isOn,
                enableCollisionCountMetric = collisionCountMetricToggle.isOn,

                maxKineticEnergyVariance = float.Parse(maxKineticEnergyVarianceInput.text),
                maxDragForceVariance = int.Parse(maxDragForceVarianceInput.text),
                maxCollisionCountVariance = int.Parse(maxCollisionCountVarianceInput.text),

                kineticEnergyPositiveScore = float.Parse(kineticEnergyPositiveScoreInput.text),
                kineticEnergyNegativeScore = float.Parse(kineticEnergyNegativeScoreInput.text),
                dragForcePositiveScore = float.Parse(dragForcePositiveScoreInput.text),
                dragForceNegativeScore = float.Parse(dragForceNegativeScoreInput.text),
                collisionCountPositiveScore = float.Parse(collisionCountPositiveScoreInput.text),
                collisionCountNegativeScore = float.Parse(collisionCountNegativeScoreInput.text),

                configName = trainingConfigNameInput.text
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

        #region GENERIC PUBLIC METHODS
        public void OnStartTrainingButtonClicked()
        {

        }

        public void OnTestSimButtonClicked()
        {

        }

        public void OnQuitButtonClicked()
        {
            Application.Quit();
        }
        #endregion
    }
}
