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

        [Header("INDICATORS")]
        [SerializeField] private TextMeshProUGUI airSpeedText; 

        private SimConfiguration config;
        private SimConfigurationSanity configSanity;

        private SimConfigurationController scc;
        private bool initialValueCheck;

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
                airSpeedText.text = $"{config.airSpeed:0} mph";
            }
        }

        public void SetConfigUI()
        {
            config = scc.GetDefaultConfig();
            configSanity = scc.configSanityCheck;

            airSpeedInput.value = config.airSpeed;
            airParticleCountInput.value = config.airParticleCount;

            //Set min-max value
            airSpeedInput.minValue = configSanity.airSpeedMin;
            airSpeedInput.maxValue = configSanity.airSpeedMax;

            airParticleCountInput.minValue = configSanity.airParticleCountMin;
            airParticleCountInput.maxValue = configSanity.airParticleCountMax;
        }

        public void UpdateConfigSettings()
        {
            if (scc == null)
                return;

            if(!initialValueCheck)
            {
                initialValueCheck = true;
                return;
            }

            
            config.airSpeed = airSpeedInput.value;
            config.airParticleCount = (int)airParticleCountInput.value;

            scc.SetCurrentConfig(config);
        }

        public void OnViewDropdownChange(int _value)
        {
            if(scc == null) return;

            scc.ChangeView((ViewAngle)_value);
        }

        public void OnCarDropdownChange(int _value)
        {
            if (scc == null) return;

            scc.ChangeCar(_value);
        }

        public async void OnResetConfigButtonClicked()
        {
            initialValueCheck = false;
            scc.ResetToDefault();

            await Task.Delay(100);

            SetConfigUI();
            //initialValueCheck = true;
        }
    }
}
