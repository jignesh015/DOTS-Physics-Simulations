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
        private SimConfigurationController scc;
        private TrainingController tc;

        //Kinectic energy
        private float previousAvgKineticEnergy;
        private float baseLineAvgKineticEnergy;

        //Collision Count
        private int previousCollisionCount;
        private int baseLineCollisionCount;

        //Drag Force
        private float previousDragForce;
        private float baseLineDragForce;

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
            baseLineCollisionCount = 0;
            baseLineDragForce = 0;
            Debug.Log($"<color=green>OnEpisodeBegin</color>");
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            if(scc.AverageKineticEnergy != 0)
            {
                Debug.Log($"<color=orange>CollectObservations {scc.AverageKineticEnergy} | {scc.AverageDragForce} | {scc.VoxelCollisionCount}</color>");
                sensor.AddObservation(scc.AverageKineticEnergy);
                sensor.AddObservation(scc.AverageDragForce);
                sensor.AddObservation(scc.carHeightMapGenerator.updatedHeightmapList.ToArray());
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
        }

        private void AirSpawnStopped()
        {
            //Debug.Log($"<color=maroon>AirSpawnStopped</color>");

            bool shouldEndEpisode = false;

            #region KINETIC ENERGY REWARD
            //Give rewards as per kinectic energy difference
            //Positive if KE increases, negative if KE decreases
            if (previousAvgKineticEnergy != 0)
            {
                string _debugColor = "yellow";
                if (baseLineAvgKineticEnergy != 0 && Mathf.Abs(baseLineAvgKineticEnergy - scc.AverageKineticEnergy) > tc.maxKineticEnergyVariance)
                {
                    AddReward(baseLineAvgKineticEnergy < scc.AverageKineticEnergy ? tc.kineticEnergyPositiveScore : tc.kineticEnergyNegativeScore);
                    _debugColor = baseLineAvgKineticEnergy < scc.AverageKineticEnergy ? "green" : "red";
                    Debug.Log($"<color=lime>========== End Episode : <color={_debugColor}>[AKE]  Base Var: {scc.AverageKineticEnergy - baseLineAvgKineticEnergy}</color> ============</color>");
                    shouldEndEpisode = true;
                }
                else if (previousAvgKineticEnergy < scc.AverageKineticEnergy)
                {
                    _debugColor = "green";
                    AddReward(tc.kineticEnergyPositiveScore);
                }
                else if (previousAvgKineticEnergy > scc.AverageKineticEnergy)
                {
                    _debugColor = "red";
                    AddReward(tc.kineticEnergyNegativeScore);
                }
                Debug.Log($"<color={_debugColor}> Baseline {baseLineAvgKineticEnergy} | AKE Variance: {scc.AverageKineticEnergy - previousAvgKineticEnergy} " +
                    $"| AKE Base Variance: {scc.AverageKineticEnergy - baseLineAvgKineticEnergy}</color>");
            }
            
            previousAvgKineticEnergy = scc.AverageKineticEnergy;
            if (baseLineAvgKineticEnergy == 0 && scc.AverageKineticEnergy != 0) baseLineAvgKineticEnergy = scc.AverageKineticEnergy;
            #endregion

            #region COLLISION COUNT REWARDS
            //Give rewards as per collision count difference
            //Positive if collision count decreases, negative if collision count increases
            if (previousCollisionCount != 0)
            {
                string _debugColor = "yellow";
                if (baseLineCollisionCount != 0 && Mathf.Abs(baseLineCollisionCount - scc.VoxelCollisionCount) > tc.maxCollisionCountVariance)
                {
                    AddReward(baseLineCollisionCount < scc.VoxelCollisionCount ? tc.collisionCountNegativeScore : tc.collisionCountPositiveScore);
                    _debugColor = baseLineCollisionCount < scc.VoxelCollisionCount ? "red" : "green";
                    Debug.Log($"<color=lime>========== End Episode : <color={_debugColor}>[VCC]  Base Var {scc.VoxelCollisionCount - baseLineCollisionCount}</color> ============</color>");
                    shouldEndEpisode = true;
                }
                else if (previousCollisionCount > scc.VoxelCollisionCount)
                {
                    _debugColor = "green";
                    AddReward(tc.collisionCountPositiveScore);
                }
                else if (previousCollisionCount < scc.VoxelCollisionCount)
                {
                    _debugColor = "red";
                    AddReward(tc.collisionCountNegativeScore);
                }
                Debug.Log($"<color={_debugColor}> Baseline {baseLineCollisionCount} | VCC Variance: {scc.VoxelCollisionCount - previousCollisionCount} " +
                   $"| VCC Base Variance: {scc.VoxelCollisionCount - baseLineCollisionCount} </color>");
            }
            previousCollisionCount = scc.VoxelCollisionCount;
            if (baseLineCollisionCount == 0 && scc.VoxelCollisionCount != 0) baseLineCollisionCount = scc.VoxelCollisionCount;
            #endregion

            #region DRAG FORCE REWARDS
            //Give rewards as per drag force difference
            //Positive if drag force decreases, negative if drag force increases
            if (previousDragForce != 0)
            {
                string _debugColor = "yellow";
                if(baseLineDragForce != 0 && Mathf.Abs(baseLineDragForce - scc.AverageDragForce) > tc.maxDragForceVariance)
                {
                    AddReward(baseLineDragForce < scc.AverageDragForce ? tc.dragForceNegativeScore : tc.dragForcePositiveScore);
                    _debugColor = baseLineDragForce < scc.AverageDragForce ? "red" : "green";
                    Debug.Log($"<color=lime>========== End Episode : <color={_debugColor}>[ADF]  Base Var {scc.AverageDragForce - baseLineDragForce}</color> ============</color>");
                    shouldEndEpisode = true;
                }
                else if(previousDragForce > scc.AverageDragForce)
                {
                    _debugColor = "green";
                    AddReward(tc.dragForcePositiveScore);
                }
                else if(previousDragForce < scc.AverageDragForce)
                {
                    _debugColor = "red";
                    AddReward(tc.dragForceNegativeScore);
                }
                Debug.Log($"<color={_debugColor}> Baseline {baseLineDragForce} | ADF Variance: {scc.AverageDragForce - previousDragForce} " +
                   $"| ADF Base Variance: {scc.AverageDragForce - baseLineDragForce} </color>");
            }
            previousDragForce = scc.AverageDragForce;
            if (baseLineDragForce == 0 && scc.AverageDragForce != 0) baseLineDragForce = scc.AverageDragForce;
            #endregion

            //Check whether to end the episode
            if (shouldEndEpisode) EndEpisode();
            //if (airStoppedCount % tc.episodePeriod == 0) EndEpisode();

            //Check wheter to take decision or an action
            airStoppedCount++;
            if (airStoppedCount % tc.decisionPeriod == 0) RequestDecision();
            else RequestAction();
        }
    }
}
