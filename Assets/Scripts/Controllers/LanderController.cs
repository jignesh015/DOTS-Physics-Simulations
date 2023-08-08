using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PhysicsSimulations
{
    public class LanderController : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            Invoke(nameof(CheckForIndicator), 5f);
        }

        public void CheckForIndicator()
        {
            //Path to sim indicator file
            string pathToSimIndicator = Path.Combine(Data.CurrentConfigRootPathLander, Data.SimIndicatorFileName);

            if (!Directory.Exists(Data.ConfigRootPathLander) 
                || !Directory.Exists(Data.CurrentConfigRootPathLander)
                || !File.Exists(pathToSimIndicator))
            {
                Debug.Log("Config files not found. Starting simulation");
                PlayerPrefs.SetInt(Data.SimIndicatorPref, 0);
                LoadSimulation();
                return;
            }

            //Read from sim indicator file
            string indicatorText = File.ReadAllText(pathToSimIndicator);
            if(indicatorText.Equals("0")) 
            {
                PlayerPrefs.SetInt(Data.SimIndicatorPref, 0);

                //Load Test Simulation
                LoadSimulation();
            }
            else
            {
                PlayerPrefs.SetInt(Data.SimIndicatorPref, 1);

                //Load Training
                LoadTraining();
            }
        }

        public void LoadSimulation()
        {
            // Load the scene by name
            SceneManager.LoadScene(Data.TestSimulationSceneName);
        }

        public void LoadTraining()
        {
            // Load the scene by name
            SceneManager.LoadScene(Data.TrainingSceneName);
        }
    }
}
