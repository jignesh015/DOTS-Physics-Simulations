using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

namespace PhysicsSimulations
{
    public class TrainingController : MonoBehaviour
    {
        [Header("AGENTS")]
        public AdjustHeightAgent adjustHeightAgent;

        [Header("TRAINING SETTINGS")]
        [Range(0.001f,0.1f)]
        public float maxVoxelVariance;
        public float maxKineticEnergyVariance;

        private SimConfigurationController scc;

        public bool SetNewVoxelHeight;
        public float VoxelHeightFactor;

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
            adjustHeightAgent.continuousActionSpecCount = scc.carHeightMapGenerator.carHeightMaps.Count;
            adjustHeightAgent.gameObject.SetActive(true);
        }

    }
}
