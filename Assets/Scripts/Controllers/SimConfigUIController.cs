using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.UI;

namespace PhysicsSimulations
{
    public class SimConfigUIController : MonoBehaviour
    {
        [Header("SETTINGS")]
        [SerializeField] private Slider airSpeedInput;
        [SerializeField] private Slider airParticleCountInput;

        private SimConfiguration config;
        private SimConfigurationSanity configSanity;

        private SimConfigurationController scc;
        private bool initialValueCheck;

        // Start is called before the first frame update
        void Start()
        {
            scc = SimConfigurationController.Instance;

            SetInitialConfigValues();
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void SetInitialConfigValues()
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
    }
}
