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

        private SimConfigurationController scc;

        // Start is called before the first frame update
        void Start()
        {
            scc = SimConfigurationController.Instance;
        }
    }
}
