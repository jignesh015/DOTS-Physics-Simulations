using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using System.IO;

namespace PhysicsSimulations
{
    public class SimConfigUIController : MonoBehaviour
    {
        [Header("SETTINGS")]
        [SerializeField] private Slider airSpeedInput;
        [SerializeField] private Slider airParticleCountInput;
        [SerializeField] private TMP_InputField airParticleBurstCountInput;

        [Header("INDICATORS")]
        [SerializeField] private TextMeshProUGUI airSpeedText;
        [SerializeField] private TextMeshProUGUI avgKineticEnergyText;
        [SerializeField] private TextMeshProUGUI avgDragForceText;
        [SerializeField] private TextMeshProUGUI collisionCountText;
        [SerializeField] private TextMeshProUGUI airBurstCountText;

        [Header("RESULT INDICATORS")]
        [SerializeField] private GameObject resultOutputPanel;
        [SerializeField] private TextMeshProUGUI originalAvgKineticEnergyText;
        [SerializeField] private TextMeshProUGUI originalAvgDragForceText;
        [SerializeField] private TextMeshProUGUI originalCollisionCountText;

        [Header("TRAINING PARAM INDICATORS")]
        [SerializeField] private GameObject trainingParamPanel;
        [SerializeField] private TextMeshProUGUI trainingTime;
        [SerializeField] private TextMeshProUGUI episodeCountText;
        [SerializeField] private TextMeshProUGUI cumulativeRewardText;

        [Header("TEST SIMULATION UI")]
        [SerializeField] private GameObject testSimPanel;
        [SerializeField] private Button exportMetricButton;

        [Header("CHECK RESULT UI")]
        [SerializeField] private GameObject checkResultPanel;
        [SerializeField] private TextMeshProUGUI resultNameText;
        [SerializeField] private Button exportResultButton;

        [Header("UI")]
        [SerializeField] private Image spawnAirButtonIcon;
        [SerializeField] private Sprite spawnAirPlaySprite;
        [SerializeField] private Sprite spawnAirStopSprite;

        [Header("BUTTONS")]
        [SerializeField] private Button spawnAirButton;
        [SerializeField] private Button screenshotButton;
        [SerializeField] private Button resetDefaultButton;

        private SimConfiguration config;
        private TrainingConfiguration trainConfig;
        private SimConfigurationSanity configSanity;

        private SimConfigurationController scc;
        private TrainingController tc;

        private bool configUISet;

        private bool showKE = true;
        private bool showDF = true;
        private bool showVCC = true;

        // Start is called before the first frame update
        void Start()
        {
            scc = SimConfigurationController.Instance;
            scc.OnSimConfigLoaded += SetConfigUI;
            scc.OnTrainConfigLoaded += GetTrainingConfig;

            trainingParamPanel.SetActive(false);
            checkResultPanel.SetActive(false);
        }

        private void OnDisable()
        {
            if (scc == null) return;
            scc.OnSimConfigLoaded -= SetConfigUI;
            scc.OnTrainConfigLoaded -= GetTrainingConfig;
        }

        // Update is called once per frame
        void Update()
        {
            if (config != null)
            {
                //Display air speed
                airSpeedText.text = $"Speed: {config.airSpeed:0} mph";

                //Display avg kinetic energy
                ToggleKineticEnergyIndicator(scc.AverageKineticEnergy, scc.InitialKineticEnergy);

                //Display Average Drag force
                ToggleDragForceIndicator(scc.AverageDragForce, scc.InitialDragForce);

                //Display Collision Count
                ToggleCollisionCountIndicator(scc.VoxelCollisionCount, scc.InitialVoxelCollisionCount, scc.vccAverageFactor);

                //Disaply Training Indicators
                ToggleTrainingParameterIndicators();

                //Display burst count
                airBurstCountText.gameObject.SetActive(scc.SpawnAirParticles);
                if (airBurstCountText.gameObject.activeSelf)
                    airBurstCountText.text = $"Burst: {scc.AirParticlesBurstCount}";

                if (spawnAirButtonIcon != null)
                {
                    spawnAirButtonIcon.sprite = scc.SpawnAirParticles ? spawnAirStopSprite : spawnAirPlaySprite;
                }
            }
        }

        public void SetConfigUI(SimConfiguration _config)
        {
            config = _config;
            configSanity = scc.configSanityCheck;

            airSpeedInput.value = config.airSpeed;
            airParticleCountInput.value = config.airParticleRatio;
            airParticleBurstCountInput.text = config.airParticleBurstCount.ToString();

            //Set min-max value
            airSpeedInput.minValue = configSanity.airSpeedMin;
            airSpeedInput.maxValue = configSanity.airSpeedMax;

            airParticleCountInput.minValue = configSanity.airParticleRatioMin;
            airParticleCountInput.maxValue = configSanity.airParticleRatioMax;

            screenshotButton.gameObject.SetActive(PlayerPrefs.GetInt(Data.SimIndicatorPref) != 1);
            resultOutputPanel.SetActive(PlayerPrefs.GetInt(Data.SimIndicatorPref) == 2);
            testSimPanel.SetActive(PlayerPrefs.GetInt(Data.SimIndicatorPref) == 0);
            checkResultPanel.SetActive(PlayerPrefs.GetInt(Data.SimIndicatorPref) == 2);

            //Set result output
            originalAvgKineticEnergyText.text = $"Avg KE: {scc.InitialKineticEnergy:F2}J";
            originalAvgDragForceText.text = $"Drag: {scc.InitialDragForce:F2}N";
            originalCollisionCountText.text = $"Collsn. Count: {scc.InitialVoxelCollisionCount:0}";

            //Set result name
            if(PlayerPrefs.GetInt(Data.SimIndicatorPref) == 2)
                resultNameText.text = $"{Path.GetFileName(PlayerPrefs.GetString(Data.ResultPathPref))}";

            configUISet = true;
        }

        public void UpdateConfigSettings()
        {
            if (scc == null)
                return;

            if(!configUISet)
            {
                return;
            }

            config.airSpeed = airSpeedInput.value;
            config.airParticleRatio = airParticleCountInput.value;

            scc.SetCurrentConfig(config);
        }

        public void OnBurstCountValueChange(string _value)
        {
            if (scc == null || !configUISet) return;

            config.airParticleBurstCount = int.Parse(_value);
            scc.SetCurrentConfig(config);
        }

        public void OnViewDropdownChange(int _value)
        {
            if(scc == null || !configUISet) return;

            scc.ChangeView((ViewAngle)_value);
        }

        public void OnCarDropdownChange(int _value)
        {
            if (scc == null || !configUISet) return;

            scc.ChangeCar(_value);
        }

        public void OnCollisionHeatmapToggleChange(bool _value)
        {
            if (scc == null || !configUISet) return;
            scc.ToggleCollisionHeatmap(_value);
        }

        public async void OnResetConfigButtonClicked()
        {
            configUISet = false;
            scc.ResetToDefault();

            await Task.Delay(100);

            SetConfigUI(scc.GetDefaultConfig());
        }

        public void OnSpawnAirParticlesButtonClicked()
        {
            if (!scc.SpawnAirParticlesCommand && !scc.SpawnAirParticles)
                scc.SpawnAirParticlesWithDelay(0);
            else if(!scc.SpawnAirParticlesCommand && scc.SpawnAirParticles)
                scc.StopAirParticles();
        }

        public void OnQuitButtonCLicked()
        {
            Application.Quit();
        }

        private void ToggleKineticEnergyIndicator(float _value, float _initialValue)
        {
            if(_value == 0) return;

            string _prefix = _value < _initialValue ? "-" : _value == _initialValue ? "" : "+";
            string _diffValue = $"{_prefix}{Mathf.Abs(_value - _initialValue):0}";
            string _color = _value < _initialValue ? Data.RedColor : Data.GreenColor;

            avgKineticEnergyText.gameObject.SetActive(showKE);
            avgKineticEnergyText.text = $"Avg KE: {_value:F2}J\n(<color={_color}>{_diffValue}</color>)";
        }

        private void ToggleDragForceIndicator(float _value, float _initialValue)
        {
            if (_value == 0) return;

            string _prefix = _value < _initialValue ? "-" : _value == _initialValue ? "" : "+";
            string _diffValue = $"{_prefix}{Mathf.Abs(_value - _initialValue):0}";
            string _color = _value < _initialValue ? Data.RedColor : Data.GreenColor;

            avgDragForceText.gameObject.SetActive(showDF);
            avgDragForceText.text = $"Drag: {_value:F2}N\n(<color={_color}>{_diffValue}</color>)";
        }

        private void ToggleCollisionCountIndicator(int _value, int _initialValue, float _avgFactor)
        {
            if (_value == 0) return;

            float _avgValue = _value / _avgFactor;

            string _prefix = _avgValue < _initialValue ? "-" : _avgValue == _initialValue ? "" : "+";
            string _diffValue = $"{_prefix}{Mathf.Abs(_avgValue - _initialValue):0}";
            string _color = _avgValue < _initialValue ? Data.RedColor : Data.GreenColor;

            collisionCountText.gameObject.SetActive(showVCC);
            collisionCountText.text = $"Collsn. Count: {_avgValue:0}\n(<color={_color}>{_diffValue}</color>)";
        }

        private void ToggleTrainingParameterIndicators()
        {
            if(tc == null) return;

            trainingTime.text = $"Time: {Data.FormattedTimer(tc.TrainingStartTime)}";
            episodeCountText.text = $"Episode: {tc.EpisodeCount:0}";
            cumulativeRewardText.text = $"Reward: {tc.CumulativeReward:F2}";
        }

        private void GetTrainingConfig(TrainingConfiguration _trainConfig)
        {
            trainConfig = _trainConfig;
            tc = FindObjectOfType<TrainingController>(true);
            showKE = _trainConfig.enableKineticEnergyMetric;
            showDF = _trainConfig.enableDragForceMetric;
            showVCC = _trainConfig.enableCollisionCountMetric;
            trainingParamPanel.SetActive(true);

            originalAvgKineticEnergyText.gameObject.SetActive(showKE);
            originalAvgDragForceText.gameObject.SetActive(showDF);
            originalCollisionCountText.gameObject.SetActive(showVCC);

            ToggleUIInteraction(false);
        }

        private void ToggleUIInteraction(bool _state)
        {
            airSpeedInput.interactable = _state;
            airParticleCountInput.interactable = _state;
            airParticleBurstCountInput.interactable = _state;
            spawnAirButton.interactable = _state;
            screenshotButton.interactable = _state;
            resetDefaultButton.interactable = _state;
            exportMetricButton.interactable = _state;
            exportResultButton.interactable = _state;
        }

        public void OnExportMetricButtonClicked()
        {
            if(!Directory.Exists(Data.OriginalSimRootPath))
                Directory.CreateDirectory(Data.OriginalSimRootPath);

            string _outputPath = Path.Combine(Data.OriginalSimRootPath, scc.carHeightMapGenerator.TextureName);
            if (!Directory.Exists(_outputPath))
                Directory.CreateDirectory(_outputPath);

            TrainingOutput trainingOutput = new()
            {
                baselineKineticEnergy = scc.AverageKineticEnergy,
                baselineDragForce = scc.AverageDragForce,
                baselineVoxelCollisionCount = Mathf.RoundToInt(scc.VoxelCollisionCount / scc.vccAverageFactor),
                trainingTime = ""
            };

            //Convert to json
            string jsonData = JsonUtility.ToJson(trainingOutput);

            // Write the JSON data to the file.
            File.WriteAllText(Path.Combine(_outputPath, Data.OriginalSimOutputFileName), jsonData);

            Debug.Log($"Original Sim Output saved at: {Path.Combine(_outputPath, Data.OriginalSimOutputFileName)}");

        }

        public void OnExportResultButtonClicked()
        {
            ResultUIController rc = FindObjectOfType<ResultUIController>();
            if (rc == null) return;
            ToggleUIInteraction(false);

            //Export result in the result folder
            rc.ExportResult(ExportHeatmapCallback);
        }

        public void OnScreenshotButtonClicked()
        {
            ScreenshotController sc = FindObjectOfType<ScreenshotController>();
            if (sc == null) return;
            ToggleUIInteraction(false);

            if(PlayerPrefs.GetInt(Data.SimIndicatorPref) == 2)
            {
                //Export heatmap in the result folder
                sc.ExportCollisionHeatmap(ExportHeatmapCallback,
                  Path.Combine(PlayerPrefs.GetString(Data.ResultPathPref), Data.CollisionHeatmapsFolderName),
                  Path.GetFileName(PlayerPrefs.GetString(Data.ResultPathPref)));
            }
            else
            {
                //Export heatmap in the root folder
                sc.ExportCollisionHeatmap(ExportHeatmapCallback,
                  Path.Combine(Data.OriginalSimRootPath, scc.carHeightMapGenerator.TextureName),
                  scc.carHeightMapGenerator.TextureName);
            }
        }

        private void ExportHeatmapCallback()
        {
            ToggleUIInteraction(true);
        }
    }
}
