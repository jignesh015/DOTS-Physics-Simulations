using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

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

        [Header("TRAINING PARAM INDICATORS")]
        [SerializeField] private GameObject trainingParamPanel;
        [SerializeField] private TextMeshProUGUI stepCountText;
        [SerializeField] private TextMeshProUGUI episodeCountText;
        [SerializeField] private TextMeshProUGUI cumulativeRewardText;

        [Header("UI")]
        [SerializeField] private Image spawnAirButtonIcon;
        [SerializeField] private Sprite spawnAirPlaySprite;
        [SerializeField] private Sprite spawnAirStopSprite;

        [Header("BUTTONS")]
        [SerializeField] private Button spawnAirButton;
        [SerializeField] private Button resetDefaultButton;

        private SimConfiguration config;
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
                ToggleKineticEnergyIndicator(scc.AverageKineticEnergy);

                //Display Average Drag force
                ToggleDragForceIndicator(scc.AverageDragForce);

                //Display Collision Count
                ToggleCollisionCountIndicator(scc.VoxelCollisionCount);

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

        private void ToggleKineticEnergyIndicator(float _value)
        {
            if(_value == 0) return;
            avgKineticEnergyText.gameObject.SetActive(showKE);
            avgKineticEnergyText.text = $"Avg KE: {_value:F2}J";
        }

        private void ToggleDragForceIndicator(float _value)
        {
            if (_value == 0) return;
            avgDragForceText.gameObject.SetActive(showDF);
            avgDragForceText.text = $"Drag: {_value:F2}N";
        }

        private void ToggleCollisionCountIndicator(int _value)
        {
            if (_value == 0) return;
            collisionCountText.gameObject.SetActive(showVCC);
            collisionCountText.text = $"Collsn. Count: {_value:0}";
        }

        private void ToggleTrainingParameterIndicators()
        {
            if(tc == null) return;

            stepCountText.text = $"Step/Total: {tc.StepCountText}";
            episodeCountText.text = $"Episode: {tc.EpisodeCount:0}";
            cumulativeRewardText.text = $"Reward: {tc.CumulativeReward:F2}";
        }

        private void GetTrainingConfig(TrainingConfiguration _trainConfig)
        {
            tc = FindObjectOfType<TrainingController>(true);
            showKE = _trainConfig.enableKineticEnergyMetric;
            showDF = _trainConfig.enableDragForceMetric;
            showVCC = _trainConfig.enableCollisionCountMetric;
            trainingParamPanel.SetActive(true);

            DisableAllUI();
        }

        private void DisableAllUI()
        {
            airSpeedInput.interactable = false;
            airParticleCountInput.interactable = false;
            airParticleBurstCountInput.interactable = false;
            spawnAirButton.interactable = false;
            resetDefaultButton.interactable = false;
        }
    }
}
