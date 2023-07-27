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
        [SerializeField] private TextMeshProUGUI airBurstCountText;

        [Header("UI")]
        [SerializeField] private Image spawnAirButtonIcon;
        [SerializeField] private Sprite spawnAirPlaySprite;
        [SerializeField] private Sprite spawnAirStopSprite;

        private SimConfiguration config;
        private SimConfigurationSanity configSanity;

        private SimConfigurationController scc;
        private bool configUISet;

        // Start is called before the first frame update
        void Start()
        {
            scc = SimConfigurationController.Instance;

            SetConfigUI();
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

        public void SetConfigUI()
        {
            config = scc.GetDefaultConfig();
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

        public async void OnResetConfigButtonClicked()
        {
            configUISet = false;
            scc.ResetToDefault();

            await Task.Delay(100);

            SetConfigUI();
            //initialValueCheck = true;
        }

        public void OnSpawnAirParticlesButtonClicked()
        {
            if (!scc.SpawnAirParticlesCommand && !scc.SpawnAirParticles)
                scc.SpawnAirParticlesWithDelay(0);
            else if(!scc.SpawnAirParticlesCommand && scc.SpawnAirParticles)
                scc.StopAirParticles();
        }

        public void ToggleKineticEnergyIndicator(float _value)
        {
            avgKineticEnergyText.gameObject.SetActive(_value != 0f);
            avgKineticEnergyText.text = $"Avg KE: {_value:F2}J";
        }
    }
}
