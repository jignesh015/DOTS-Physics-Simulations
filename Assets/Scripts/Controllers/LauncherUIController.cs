using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SimpleFileBrowser;
using System.Threading.Tasks;

namespace PhysicsSimulations
{
    public class LauncherUIController : MonoBehaviour
    {
        [Header("SIMULATION CONFIG")]
        [SerializeField] private SimConfiguration defaultConfig;
        [SerializeField] private SimConfigurationSanity configSanityCheck;

        [Header("SIMULATION CONFIG UI")]
        [SerializeField] private Slider airSpeedInput;
        [SerializeField] private Slider airParticleCountInput;
        [SerializeField] private Toggle spawnAirParticlesToggle;
        [SerializeField] private TMP_Dropdown carDropdown;
        [SerializeField] private TMP_InputField configNameText;
        [SerializeField] private TMP_InputField airParticleBurstCountInput;

        public SimConfiguration CurrentSimConfig { get; private set; }

        private bool configUISet;

        private void Awake()
        {
        }

        // Start is called before the first frame update
        void Start()
        {
            ResetToDefault();

            Invoke(nameof(InitiateSimConfigFiles), 1f);
        }

        public void InitiateSimConfigFiles()
        {
            //Create the root directory if doesn't exist
            if(!Directory.Exists(Data.ConfigRootPathLauncher))
            {
                Directory.CreateDirectory(Data.ConfigRootPathLauncher);
            }
        }

        private void SetConfigUI(SimConfiguration config, SimConfigurationSanity configSanity, string _configName = "")
        {
            airSpeedInput.value = config.airSpeed;
            airParticleCountInput.value = config.airParticleRatio;
            airParticleBurstCountInput.text = config.airParticleBurstCount.ToString();
            spawnAirParticlesToggle.isOn = config.spawnAirParticlesAutomatically;
            configNameText.text = _configName;

            //Set min-max value
            airSpeedInput.minValue = configSanity.airSpeedMin;
            airSpeedInput.maxValue = configSanity.airSpeedMax;

            airParticleCountInput.minValue = configSanity.airParticleRatioMin;
            airParticleCountInput.maxValue = configSanity.airParticleRatioMax;

            configUISet = true;
        }

        public async void ResetToDefault()
        {
            configUISet = false;
            CurrentSimConfig = defaultConfig;

            await Task.Delay(100);

            SetConfigUI(CurrentSimConfig, configSanityCheck);
        }

        public void SaveCurrentSimConfigToJSON()
        {
            if (CurrentSimConfig == null)
            {
                Debug.LogError("Config not found");
                return;
            }

            // Convert the config to JSON format using JsonUtility.
            string jsonData = JsonUtility.ToJson(CurrentSimConfig);

            // Get a path to save the json file
            string _configName = configNameText.text;
            if (string.IsNullOrEmpty(_configName))
                _configName = $"SimConfig_{Data.Timestamp()}";

            string fileName = $"{_configName}.json";
            //while(File.Exists(fileName))
            //{
            //    _configName += $"_{Data.Timestamp()}";
            //    fileName = $"{_configName}.json";
            //}
            string newFilePath = Path.Combine(Data.ConfigRootPathLauncher, fileName);
            Debug.Log($"File path : {newFilePath}");

            //Also save to current config file
            string currentConfigPath = $"{Path.Combine(Data.ConfigRootPathLauncher, Data.CurrentSimConfigFileName)}.json";

            // Write the JSON data to the file.
            File.WriteAllText(newFilePath, jsonData);
            File.WriteAllText(currentConfigPath, jsonData);
            Debug.Log($"Saved Config to {newFilePath} and {currentConfigPath}");
        }

        public void LoadSimConfigJSON()
        {
            string pathToFile;
            FileBrowser.ShowLoadDialog((chosenFilePath) =>
            {
                pathToFile = chosenFilePath[0];
                if(File.Exists(pathToFile))
                {
                    // Read the JSON file content
                    string jsonContent = File.ReadAllText(pathToFile);

                    Debug.Log($"{jsonContent}");

                    //Convert to config
                    SimConfiguration _simConfig = ScriptableObject.CreateInstance<SimConfiguration>();
                    _simConfig.LoadFromJson(jsonContent);
                    CurrentSimConfig = PerformSanityCheck(_simConfig);
                    SetConfigUI(CurrentSimConfig, configSanityCheck, Path.GetFileNameWithoutExtension(pathToFile));
                }
            }, () =>
            {
                Debug.Log("Load file cancelled");
            }, FileBrowser.PickMode.Files, false, Data.ConfigRootPathLauncher);
        }

        public void SetCurrentConfig(SimConfiguration config)
        {
            CurrentSimConfig = PerformSanityCheck(config);
            SaveCurrentSimConfigToJSON();
        }

        public void UpdateConfigSettings()
        {
            if (!configUISet)
            {
                return;
            }

            SimConfiguration _config = new SimConfiguration
            {
                airSpeed = airSpeedInput.value,
                airParticleRatio = airParticleCountInput.value,
                spawnAirParticlesAutomatically = spawnAirParticlesToggle.isOn,
                carId = carDropdown.value
            };
            SetCurrentConfig(_config);
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
            config.airParticleRatio = Mathf.Clamp(config.airParticleRatio, csc.airParticleRatioMin, csc.airParticleRatioMax);
            config.airParticleBurstCount = Mathf.Clamp(config.airParticleBurstCount, csc.airParticleBurstCountMin, csc.airParticleBurstCountMax);
            return config;
        }
    }
}
