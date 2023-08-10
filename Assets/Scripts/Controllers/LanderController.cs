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
            int.TryParse(indicatorText, out int indicatorIndex);
            PlayerPrefs.SetInt(Data.SimIndicatorPref, indicatorIndex);

            switch (indicatorIndex)
            {
                case 0:
                    //Load Test Simulation
                    LoadSimulation();
                    break;
                case 1:
                    //Load Training
                    LoadTraining();
                    break;
                case 2:
                    //Load Simulation with result heightmap
                    LoadSimulationForResult();
                    break;
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

        public void LoadSimulationForResult()
        {
            //Path to result indicator file
            string pathToResultIndicator = Path.Combine(Data.CurrentConfigRootPathLander, Data.ResultFolderIndicatorFileName);
            if (File.Exists(pathToResultIndicator))
            {
                string resultPath = File.ReadAllText(pathToResultIndicator);
                PlayerPrefs.SetString(Data.ResultPathPref, resultPath);
            }
            LoadSimulation();
        }
    }
}
