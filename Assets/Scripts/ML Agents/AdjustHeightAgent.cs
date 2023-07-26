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
                Debug.Log($"<color=yellow>CollectObservations {scc.AverageKineticEnergy}</color>");
                sensor.AddObservation(scc.AverageKineticEnergy);
            }
            //sensor.AddObservation(scc.carHeightMapGenerator.GetHeightList());
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            Debug.Log($"<color=cyan>Continuous Action: {actions.ContinuousActions.Length} | {actions.ContinuousActions[0]}</color>");
            TrainingController.Instance.SetNewVoxelHeight = true;
            TrainingController.Instance.VoxelHeightFactor = actions.ContinuousActions[0];
            //scc.carHeightMapGenerator.UpdateHeightmap(actions.ContinuousActions[0]);
            //scc.carHeightMapGenerator.UpdateHeightmap(actions.ContinuousActions.ToList());
            scc.SpawnAirParticlesWithDelay(2500);

        }

        private void AirSpawnStopped()
        {
            Debug.Log($"<color=maroon>AirSpawnStopped</color>");
            if (previousAvgKineticEnergy != 0)
            {
                if (Mathf.Abs(baseLineAvgKineticEnergy - scc.AverageKineticEnergy) > tc.maxKineticEnergyVariance)
                {
                    AddReward(baseLineAvgKineticEnergy < scc.AverageKineticEnergy ? 1f : -0.5f);
                    Debug.Log($"<color=red>========== End Episode ============</color>");
                    EndEpisode();
                }
                else if (previousAvgKineticEnergy < scc.AverageKineticEnergy)
                {
                    Debug.Log($"<color=green>++++++++++ Add Reward ++++++++++ </color>");
                    AddReward(1);
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
            if (airStoppedCount % 5 == 0) RequestDecision();
            else RequestAction();
        }
    }
}
