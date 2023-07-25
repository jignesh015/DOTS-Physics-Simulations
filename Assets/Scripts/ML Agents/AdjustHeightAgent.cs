using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace PhysicsSimulations
{
    public class AdjustHeightAgent : Agent
    {
        private SimConfigurationController scc;

        private void Start()
        {
            scc = SimConfigurationController.Instance;
        }

        public override void Initialize()
        {
            base.Initialize();
            ActionSpec continuousActionSpec = new(100, new int[] { });
            GetComponent<BehaviorParameters>().BrainParameters.ActionSpec = continuousActionSpec;
            Debug.Log($"<color=cyan>Initialize {GetComponent<BehaviorParameters>().BrainParameters.ActionSpec.NumContinuousActions}</color>");
        }

        public override void OnEpisodeBegin()
        {
            Debug.Log($"<color=green>OnEpisodeBegin {GetComponent<BehaviorParameters>().BrainParameters.ActionSpec.NumContinuousActions}</color>");
        }

        //public override void CollectObservations(VectorSensor sensor)
        //{
        //    if(scc != null && !scc.SpawnAirParticles && scc.AverageKineticEnergy != 0)
        //        Debug.Log($"CollectObservations: {scc.AverageKineticEnergy}");
        //}

        public override void OnActionReceived(ActionBuffers actions)
        {
            Debug.Log($"Continuous Action: {actions.ContinuousActions.Length}");
        }

        public void InitializeAgent(int actionSpecSize)
        {
            ActionSpec continuousActionSpec = new(actionSpecSize, new int[] { });
            GetComponent<BehaviorParameters>().BrainParameters.ActionSpec = continuousActionSpec;
            Debug.Log($"<color=yellow>InitializeAgent {GetComponent<BehaviorParameters>().BrainParameters.ActionSpec.NumContinuousActions}</color>");
        }

        public void CustomRequestDecision()
        {
            RequestDecision();
        }
    }
}
