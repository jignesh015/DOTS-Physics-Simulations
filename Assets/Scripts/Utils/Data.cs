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

        //CONFIGURATION JSON ROOT PATH FOR LAUNCHER
        public static string ConfigRootPathLauncher = $"{Path.Combine(Directory.GetParent(Application.dataPath).FullName, "simconfig")}";

        //FILE NAME FOR CURRENT SIM CONFIG
        public static string CurrentSimConfigFileName = "CurrentSimConfig";

        //PHYSICS CONSTANTS AND VARIABLES
        public static float AirDensity = 1.2f;

        public static string Timestamp()
        {
            // Get the current timestamp.
            DateTime now = DateTime.Now;

            // Create a format string for the timestamp.
            string format = "dd-MM-yy HH-mm-ss";

            // Convert the timestamp to a string using the format string.
            return now.ToString(format);
        }
    }
}
