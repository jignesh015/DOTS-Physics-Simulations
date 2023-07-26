using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace PhysicsSimulations
{
    public class AdjustHeightAgent : Agent
    {

        public int continuousActionSpecCount;

        private SimConfigurationController scc;
        private TrainingController tc;

        private float previousAvgKineticEnergy;
        private float baseLineAvgKineticEnergy;

        private int airStoppedCount;

        private void Start()
        {
            scc = SimConfigurationController.Instance;
            tc = TrainingController.Instance;
            scc.OnAirSpawnStopped += AirSpawnStopped;
        }

        protected override void OnDisable()
        {
            scc.OnAirSpawnStopped -= AirSpawnStopped;
        }

        public override void Initialize()
        {
            //ActionSpec continuousActionSpec = new(continuousActionSpecCount, new int[] { });
            //GetComponent<BehaviorParameters>().BrainParameters.ActionSpec = continuousActionSpec;
            Debug.Log($"<color=magenta>Initialize {GetComponent<BehaviorParameters>().BrainParameters.ActionSpec.NumContinuousActions}</color>");
        }

        public override void OnEpisodeBegin()
        {
            airStoppedCount = 0;
            baseLineAvgKineticEnergy = 0;
            Debug.Log($"<color=green>OnEpisodeBegin</color>");
            //RequestDecision();
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            if(scc.AverageKineticEnergy != 0)
            {
                Debug.Log($"<color=yellow>CollectObservations {scc.AverageKineticEnergy} | {scc.VoxelCollisionCount}</color>");
                sensor.AddObservation(scc.AverageKineticEnergy);
                sensor.AddObservation(scc.VoxelCollisionCount);
            }
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            //Continuous Actions
            Debug.Log($"<color=cyan>Continuous Action: {actions.ContinuousActions.Length} | {actions.ContinuousActions[0]}</color>");
            float _heightFactor = actions.ContinuousActions[0];

            //Discreet Actions
            //Debug.Log($"<color=cyan>Continuous Action: {actions.DiscreteActions.Length} | {actions.DiscreteActions[0]} | {actions.DiscreteActions[1]}</color>");
            //float _heightFactor = actions.DiscreteActions[0]/100f;

            //Set height factor as per new actions
            TrainingController.Instance.SetNewVoxelHeight = true;
            TrainingController.Instance.VoxelHeightFactor = _heightFactor;
            TrainingController.Instance.VoxelHeightFactorList = actions.ContinuousActions.ToList();

            //Restart Air Particles
            //scc.SpawnAirParticlesWithDelay(2500);

            //float nonZero = actions.DiscreteActions.ToList().Find(v => v != 0 && v != 1);
            //Debug.Log($"<color=magenta>Non Zero Val: {nonZero}</color>");

        }

        private void AirSpawnStopped()
        {
            Debug.Log($"<color=maroon>AirSpawnStopped</color>");
            if (previousAvgKineticEnergy != 0)
            {
                if (Mathf.Abs(baseLineAvgKineticEnergy - scc.AverageKineticEnergy) > tc.maxKineticEnergyVariance)
                {
                    AddReward(baseLineAvgKineticEnergy < scc.AverageKineticEnergy ? 0.1f : -2f);
                    Debug.Log($"<color=red>========== End Episode ============</color>");
                    EndEpisode();
                }
                else if (previousAvgKineticEnergy < scc.AverageKineticEnergy)
                {
                    Debug.Log($"<color=green>++++++++++ Add Reward ++++++++++ </color>");
                    AddReward(0.1f);
                }
                else if (previousAvgKineticEnergy > scc.AverageKineticEnergy)
                {
                    Debug.Log($"<color=red>----------- Subtract Reward ----------- </color>");
                    AddReward(-2f);
                }
            }
            previousAvgKineticEnergy = scc.AverageKineticEnergy;
            if (baseLineAvgKineticEnergy == 0) baseLineAvgKineticEnergy = scc.AverageKineticEnergy;
            airStoppedCount++;
            if (airStoppedCount % tc.decisionPeriod == 0) RequestDecision();
            else RequestAction();
        }
    }
}
