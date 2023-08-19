using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PhysicsSimulations
{
    public static class Data
    {
        //HEIGHTMAPS ROOT PATH
        public static string CarHeightmapRoot = "Heightmaps";
        public static string CarHeightmapRootResources = $"{Path.Combine(Application.dataPath, "Resources/Heightmaps")}";

        //LANDER BUILD PATH
        public static string LanderBuildPath = $"{Path.Combine(Directory.GetParent(Application.dataPath).FullName, "lander")}";
        public static string LanderBuildName = "Lander.exe";

        //CONFIGURATION JSON ROOT PATH FOR LAUNCHER
        public static string ConfigRootPathLauncher = $"{Path.Combine(Directory.GetParent(Application.dataPath).FullName, "custom_config")}";
        public static string SimConfigRootPathLauncher = $"{Path.Combine(ConfigRootPathLauncher, "sim_config")}";
        public static string TrainingConfigRootPathLauncher = $"{Path.Combine(ConfigRootPathLauncher, "train_config")}";
        public static string CurrentConfigRootPathLauncher = $"{Path.Combine(ConfigRootPathLauncher, "current_config")}";
        public static string ResultsPathLauncher = $"{Path.Combine(Directory.GetParent(Application.dataPath).FullName, "results")}";

        //CONFIGURATION JSON ROOT PATH FOR LANDER
        public static string ConfigRootPathLander = $"{Path.Combine(Directory.GetParent(Directory.GetParent(Application.dataPath).FullName).FullName, "custom_config")}";
        public static string SimConfigRootPathLander = $"{Path.Combine(ConfigRootPathLander, "sim_config")}";
        public static string TrainingConfigRootPathLander = $"{Path.Combine(ConfigRootPathLander, "train_config")}";
        public static string CurrentConfigRootPathLander = $"{Path.Combine(ConfigRootPathLander, "current_config")}";
        public static string ResultsPathLander = $"{Path.Combine(Directory.GetParent(Directory.GetParent(Application.dataPath).FullName).FullName, "results")}";

        //FILE NAME FOR CURRENT SIM CONFIG
        public static string CurrentSimConfigFileName = "CurrentSimConfig.json";

        //FILE NAME FOR CURRENT TRAINING CONFIG
        public static string CurrentTrainingConfigFileName = "CurrentTrainingConfig.json";

        //SIMULATION INDICATOR FILE PATH
        public static string SimIndicatorFileName = "SimIndicator";

        //RESULT FILE PATH
        public static string ResultFolderIndicatorFileName = "ResultFolderIndicator";

        //TRAINING OUTPUT FILE NAME
        public static string TrainingOutputFileName = "TrainingOutput.json";

        //METRICS OBSERVATION CSV FILE NAME
        public static string MetricObservationFileName = "MetricObservations.csv";

        //COLLISION HEATMAP FOLDER NAME
        public static string CollisionHeatmapsFolderName = "collision_heatmaps";

        //COLLISION HEATMAP ROOT PATH FOR LANDER
        public static string CollisionHeatmapsRootPath = $"{Path.Combine(Directory.GetParent(Directory.GetParent(Application.dataPath).FullName).FullName, CollisionHeatmapsFolderName)}";

        //PHYSICS CONSTANTS AND VARIABLES
        public static float AirDensity = 1.2f;

        //SCENE NAMES
        public static string TestSimulationSceneName = "TrainingScene";
        public static string TrainingSceneName = "TrainingScene";

        //PLAYER PREFS
        public static string SimIndicatorPref = "SimIndicatorPref";
        public static string ResultPathPref = "ResultPathPref";

        //TEXT COLOR HEX CODE
        public static string RedColor = "#e60d1a";
        public static string GreenColor = "#789258";

        public static string Timestamp()
        {
            // Get the current timestamp.
            DateTime now = DateTime.Now;

            // Create a format string for the timestamp.
            string format = "ddMMyy_HHmmss";

            // Convert the timestamp to a string using the format string.
            return now.ToString(format);
        }

        public static string FormattedTimer(DateTime startTime)
        {
            TimeSpan timeDifference = DateTime.Now - startTime;

            return string.Format("{0:D2}:{1:D2}:{2:D2}", timeDifference.Hours, 
                timeDifference.Minutes, timeDifference.Seconds);
        }

        public static string VirtualEnvironmentPath()
        {
            return Path.Combine(Directory.GetParent(Application.dataPath).FullName, "venv");
        }

        public static string GetResultHeightmapPath(string _rootResultPath, string _resultFolderName)
        {
            return Path.Combine(_rootResultPath, $"{_resultFolderName}_heightmap.json");
        }
    }
}
