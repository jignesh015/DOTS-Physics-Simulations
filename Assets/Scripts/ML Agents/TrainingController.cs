using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Unity.MLAgents;
using UnityEngine;

namespace PhysicsSimulations
{
    public class TrainingController : MonoBehaviour
    {
        [Header("AGENTS")]
        public AdjustHeightAgent adjustHeightAgent;

        [Header("TRAINING PATHS")]
        [SerializeField] private string resultOutputName = "[result folder name]";
        [SerializeField] private string buildPath = "[path to build]";
        [SerializeField] private string venvPath = "[path to venv]";

        [Header("TRAINING SETTINGS")]
        [Range(0.001f,1f)]
        public float maxVoxelHeightVariance;
        public float adjacentRowMaxHeightVariance;
        public int decisionPeriod;
        public int episodePeriod;
        public bool onlyModifyCollidedVoxels;
        public bool compareWithOgHeight;

        [Header("REWARD SETTINGS")]
        public float maxKineticEnergyVariance;
        public int maxCollisionCountVariance;
        public int maxDragForceVariance;

        [Header("REWARD SCORES")]
        //KINETIC ENERGY REWARD
        public float kineticEnergyPositiveScore;
        public float kineticEnergyNegativeScore;
        //COLLISION COUNT REWARD
        public float collisionCountPositiveScore;
        public float collisionCountNegativeScore;
        //DRAG FORCE REWARD
        public float dragForcePositiveScore;
        public float dragForceNegativeScore;

        private SimConfigurationController scc;

        [Header("READ ONLY")]
        public bool SetNewVoxelHeight;
        public float VoxelHeightFactor;
        public List<float> VoxelHeightFactorList;

        private int heightMapTextureLength;

        private static TrainingController _instance;
        public static TrainingController Instance { get { return _instance; } }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                _instance = this;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            scc = SimConfigurationController.Instance;
            scc.OnAirSpawnStarted += EnableAdjustHeightAgent;

            heightMapTextureLength = scc.carHeightMapGenerator.heightmapTexture.height;

            venvPath = VirtualEnvironmentPath();
        }

        private void OnEnable()
        {
            if (scc != null)
            {
                scc.OnAirSpawnStarted += EnableAdjustHeightAgent;
            }
        }

        private void OnDisable()
        {
            scc.OnAirSpawnStarted -= EnableAdjustHeightAgent;
        }

        public void EnableAdjustHeightAgent()
        {
            //Debug.Log($"<color=cyan>EnableAdjustHeightAgent 1</color>");
            adjustHeightAgent.gameObject.SetActive(true);
        }

        public float GetHeightFactor(int _rowIndex)
        {
            if(VoxelHeightFactorList != null && VoxelHeightFactorList.Count > 0)
            {
                return VoxelHeightFactorList[Mathf.FloorToInt(_rowIndex/(heightMapTextureLength/VoxelHeightFactorList.Count))];
            }
            return 0;
        }

        public float GetHeightFactor(int _rowIndex, int _columnIndex)
        {
            if (VoxelHeightFactorList != null && VoxelHeightFactorList.Count > 0)
            {
                if (VoxelHeightFactorList.Count == 1)
                    return VoxelHeightFactor;

                return VoxelHeightFactorList[_rowIndex + _columnIndex * heightMapTextureLength];
            }

            return 0;
        }

        public void StartTraining()
        {
            // Construct the full command
            string activateCommand = $"\"{venvPath}\\Scripts\\activate\"";
            string trainCommand = $"mlagents-learn config\\AdjustHeight.yaml --run-id={resultOutputName}";

#if !UNITY_EDITOR
            //Add executable path for standalone builds
            string executablePath = Directory.GetParent(Application.dataPath).FullName;
            trainCommand = $"mlagents-learn config\\AdjustHeight.yaml  --env={executablePath} --run-id={resultOutputName}";
#endif

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/K {activateCommand} && {trainCommand}", // Use /K to keep the Command Prompt window open
                UseShellExecute = true,
                CreateNoWindow = false
            };

            Process.Start(startInfo);
        }


        private string VirtualEnvironmentPath()
        {
            // Get the path to the "Assets" folder in your Unity project
            string assetsFolderPath = Application.dataPath;

            // Get the parent directory (folder just outside "Assets")
            string parentFolderPath = Directory.GetParent(assetsFolderPath).FullName;

            // Combine the parent folder path with the folder name you want to access
            string targetFolderPath = Path.Combine(parentFolderPath, "venv");

            UnityEngine.Debug.Log($"Path: {targetFolderPath}");

            return targetFolderPath;
        }
    }
}
