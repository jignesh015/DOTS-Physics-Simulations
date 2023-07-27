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

        //Kinectic energy
        private float previousAvgKineticEnergy;
        private float baseLineAvgKineticEnergy;

        //Collision Count
        private int previousCollisionCount;
        private int baseLineCollisionCount;

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

            //Set height factor as per new actions
            TrainingController.Instance.SetNewVoxelHeight = true;
            TrainingController.Instance.VoxelHeightFactor = _heightFactor;
            TrainingController.Instance.VoxelHeightFactorList = actions.ContinuousActions.ToList();

            //Restart Air Particles
            //scc.SpawnAirParticlesWithDelay(2500);
        }

        private void AirSpawnStopped()
        {
            Debug.Log($"<color=maroon>AirSpawnStopped</color>");

            //Give rewards as per kinectic energy difference
            //Positive if KE increases, negative if KE decreases
            if (previousAvgKineticEnergy != 0)
            {
                if (Mathf.Abs(baseLineAvgKineticEnergy - scc.AverageKineticEnergy) > tc.maxKineticEnergyVariance)
                {
                    AddReward(baseLineAvgKineticEnergy < scc.AverageKineticEnergy ? tc.kineticEnergyPositiveScore : tc.kineticEnergyNegativeScore);
                    Debug.Log($"<color=red>========== End Episode: AKE ============</color>");
                    EndEpisode();
                }
                else if (previousAvgKineticEnergy < scc.AverageKineticEnergy)
                {
                    Debug.Log($"<color=green>++++++++++ Add Reward: AKE ++++++++++ </color>");
                    AddReward(tc.kineticEnergyPositiveScore);
                }
                else if (previousAvgKineticEnergy > scc.AverageKineticEnergy)
                {
                    Debug.Log($"<color=red>----------- Subtract Reward: AKE ----------- </color>");
                    AddReward(tc.kineticEnergyNegativeScore);
                }
            }
            previousAvgKineticEnergy = scc.AverageKineticEnergy;
            if (baseLineAvgKineticEnergy == 0) baseLineAvgKineticEnergy = scc.AverageKineticEnergy;

            //Give rewards as per collision count difference
            //Positive if collision count decreases, negative if collision count increases
            if (previousCollisionCount != 0)
            {
                if(Mathf.Abs(baseLineCollisionCount - scc.VoxelCollisionCount) > tc.maxCollisionCountVariance)
                {
                    AddReward(baseLineCollisionCount < scc.VoxelCollisionCount ? tc.collisionCountNegativeScore : tc.collisionCountPositiveScore);
                    Debug.Log($"<color=red>========== End Episode : VCC ============</color>");
                    EndEpisode();
                }
                else if(previousCollisionCount > scc.VoxelCollisionCount)
                {
                    Debug.Log($"<color=green>++++++++++ Add Reward: VCC ++++++++++ </color>");
                    AddReward(tc.collisionCountPositiveScore);
                }
                else if(previousCollisionCount < scc.VoxelCollisionCount)
                {
                    Debug.Log($"<color=red>----------- Subtract Reward: VCC ----------- </color>");
                    AddReward(tc.collisionCountNegativeScore);
                }
            }
            previousCollisionCount = scc.VoxelCollisionCount;
            if (baseLineCollisionCount == 0) baseLineCollisionCount = scc.VoxelCollisionCount;


            //Check wheter to take decision or an action
            airStoppedCount++;
            if (airStoppedCount % tc.decisionPeriod == 0) RequestDecision();
            else RequestAction();
        }
    }
}
