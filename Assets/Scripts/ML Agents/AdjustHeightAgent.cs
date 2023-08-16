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

        private Academy academy;

        private float ogHeightmapSum;
        private float updatedHeightmapSum;

        private void Start()
        {
            scc = SimConfigurationController.Instance;
            tc = TrainingController.Instance;
            scc.OnAirSpawnStopped += AirSpawnStopped;
            scc.OnVoxelsReady += OnVoxelsReady;
        }

        protected override void OnDisable()
        {
            scc.OnAirSpawnStopped -= AirSpawnStopped;
            scc.OnVoxelsReady -= OnVoxelsReady;
        }

        public override void Initialize()
        {
            Debug.Log($"<color=magenta>Initialize {GetComponent<BehaviorParameters>().BrainParameters.ActionSpec.NumContinuousActions}</color>");

            academy = Academy.Instance;
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
                if(tc.CurrentTrainConfig.enableKineticEnergyMetric) sensor.AddObservation(scc.AverageKineticEnergy);
                if (tc.CurrentTrainConfig.enableDragForceMetric) sensor.AddObservation(scc.AverageDragForce);
                if (tc.CurrentTrainConfig.enableCollisionCountMetric) sensor.AddObservation(scc.VoxelCollisionCount);
                sensor.AddObservation(scc.carHeightMapGenerator.updatedHeightmapList.ToArray());

                //Save observations to CSV file
                tc.SaveObservationsToCSV(CompletedEpisodes);
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

        private void OnVoxelsReady()
        {
            //Calculate sum of the original heightmap
            if(ogHeightmapSum == 0)
                ogHeightmapSum = scc.carHeightMapGenerator.carHeightMapList.Sum();
        }

        private void AirSpawnStopped()
        {
            //Debug.Log($"<color=maroon>AirSpawnStopped</color>");

            bool shouldEndEpisode = false;
            bool isFixedEpisodeLength = tc.CurrentTrainConfig.fixedEpisodeLength;

            tc.StepCountText = academy.StepCount;
            tc.EpisodeCount = CompletedEpisodes;
            tc.CumulativeReward = GetCumulativeReward();

            #region KINETIC ENERGY REWARD
            //Give rewards as per kinectic energy difference
            //Positive if KE increases, negative if KE decreases
            if (previousAvgKineticEnergy != 0 && tc.CurrentTrainConfig.enableKineticEnergyMetric)
            {
                string _debugColor = "yellow";
                if (!isFixedEpisodeLength && baseLineAvgKineticEnergy != 0 && Mathf.Abs(baseLineAvgKineticEnergy - scc.AverageKineticEnergy) > tc.CurrentTrainConfig.maxKineticEnergyVariance)
                {
                    AddReward(baseLineAvgKineticEnergy < scc.AverageKineticEnergy ? tc.CurrentTrainConfig.kineticEnergyPositiveScore : tc.CurrentTrainConfig.kineticEnergyNegativeScore);
                    _debugColor = baseLineAvgKineticEnergy < scc.AverageKineticEnergy ? "green" : "red";
                    Debug.Log($"<color=lime>========== End Episode : <color={_debugColor}>[AKE]  Base Var: {scc.AverageKineticEnergy - baseLineAvgKineticEnergy}</color> ============</color>");
                    shouldEndEpisode = true;
                }
                else if (previousAvgKineticEnergy < scc.AverageKineticEnergy)
                {
                    _debugColor = "green";
                    AddReward(tc.CurrentTrainConfig.kineticEnergyPositiveScore);
                }
                else if (previousAvgKineticEnergy > scc.AverageKineticEnergy)
                {
                    _debugColor = "red";
                    AddReward(tc.CurrentTrainConfig.kineticEnergyNegativeScore);
                }
                Debug.Log($"<color={_debugColor}> Baseline {baseLineAvgKineticEnergy} | AKE Variance: {scc.AverageKineticEnergy - previousAvgKineticEnergy} " +
                    $"| AKE Base Variance: {scc.AverageKineticEnergy - baseLineAvgKineticEnergy}</color>");
            }
            
            previousAvgKineticEnergy = scc.AverageKineticEnergy;
            if (baseLineAvgKineticEnergy == 0 && scc.AverageKineticEnergy != 0) baseLineAvgKineticEnergy = scc.AverageKineticEnergy;
            #endregion

            #region DRAG FORCE REWARDS
            //Give rewards as per drag force difference
            //Positive if drag force decreases, negative if drag force increases
            if (previousDragForce != 0 && tc.CurrentTrainConfig.enableDragForceMetric)
            {
                string _debugColor = "yellow";
                if (!isFixedEpisodeLength && baseLineDragForce != 0 && Mathf.Abs(baseLineDragForce - scc.AverageDragForce) > tc.CurrentTrainConfig.maxDragForceVariance)
                {
                    AddReward(baseLineDragForce < scc.AverageDragForce ? tc.CurrentTrainConfig.dragForceNegativeScore : tc.CurrentTrainConfig.dragForcePositiveScore);
                    _debugColor = baseLineDragForce < scc.AverageDragForce ? "red" : "green";
                    Debug.Log($"<color=lime>========== End Episode : <color={_debugColor}>[ADF]  Base Var {scc.AverageDragForce - baseLineDragForce}</color> ============</color>");
                    shouldEndEpisode = true;
                }
                else if (previousDragForce > scc.AverageDragForce)
                {
                    _debugColor = "green";
                    AddReward(tc.CurrentTrainConfig.dragForcePositiveScore);
                }
                else if (previousDragForce < scc.AverageDragForce)
                {
                    _debugColor = "red";
                    AddReward(tc.CurrentTrainConfig.dragForceNegativeScore);
                }
                Debug.Log($"<color={_debugColor}> Baseline {baseLineDragForce} | ADF Variance: {scc.AverageDragForce - previousDragForce} " +
                   $"| ADF Base Variance: {scc.AverageDragForce - baseLineDragForce} </color>");
            }
            previousDragForce = scc.AverageDragForce;
            if (baseLineDragForce == 0 && scc.AverageDragForce != 0) baseLineDragForce = scc.AverageDragForce;
            #endregion

            #region COLLISION COUNT REWARDS
            //Give rewards as per collision count difference
            //Positive if collision count decreases, negative if collision count increases
            if (previousCollisionCount != 0 && tc.CurrentTrainConfig.enableCollisionCountMetric)
            {
                string _debugColor = "yellow";
                if (!isFixedEpisodeLength && baseLineCollisionCount != 0 && Mathf.Abs(baseLineCollisionCount - scc.VoxelCollisionCount) > tc.CurrentTrainConfig.maxCollisionCountVariance)
                {
                    AddReward(baseLineCollisionCount < scc.VoxelCollisionCount ? tc.CurrentTrainConfig.collisionCountNegativeScore : tc.CurrentTrainConfig.collisionCountPositiveScore);
                    _debugColor = baseLineCollisionCount < scc.VoxelCollisionCount ? "red" : "green";
                    Debug.Log($"<color=lime>========== End Episode : <color={_debugColor}>[VCC]  Base Var {scc.VoxelCollisionCount - baseLineCollisionCount}</color> ============</color>");
                    shouldEndEpisode = true;
                }
                else if (previousCollisionCount > scc.VoxelCollisionCount)
                {
                    _debugColor = "green";
                    AddReward(tc.CurrentTrainConfig.collisionCountPositiveScore);
                }
                else if (previousCollisionCount < scc.VoxelCollisionCount)
                {
                    _debugColor = "red";
                    AddReward(tc.CurrentTrainConfig.collisionCountNegativeScore);
                }
                Debug.Log($"<color={_debugColor}> Baseline {baseLineCollisionCount} | VCC Variance: {scc.VoxelCollisionCount - previousCollisionCount} " +
                   $"| VCC Base Variance: {scc.VoxelCollisionCount - baseLineCollisionCount} </color>");
            }
            previousCollisionCount = scc.VoxelCollisionCount;
            if (baseLineCollisionCount == 0 && scc.VoxelCollisionCount != 0) baseLineCollisionCount = scc.VoxelCollisionCount;
            #endregion

            #region HEIGHTMAP SUM REWARDS
            //Give rewards as per the sum of heightmap values
            //Positive if sum decreases, negative if sum increases
            if(tc.CurrentTrainConfig.enableHeightmapSumMetric)
            {
                string _heightmapDebugColor;
                updatedHeightmapSum = scc.carHeightMapGenerator.updatedHeightmapList.Sum();
                if (updatedHeightmapSum < ogHeightmapSum)
                {
                    _heightmapDebugColor = "green";
                    AddReward(tc.CurrentTrainConfig.heightmapSumPositiveScore);
                }
                else
                {
                    _heightmapDebugColor = "red";
                    AddReward(tc.CurrentTrainConfig.heightmapSumNegativeScore);
                }
                Debug.Log($"<color={_heightmapDebugColor}> Updated Heightmap Sum: {updatedHeightmapSum} | Og Sum: {ogHeightmapSum}</color>");
            }
            #endregion

            airStoppedCount++;
            //Check whether to end the episode
            if (!isFixedEpisodeLength && shouldEndEpisode) 
                EndEpisode();
            else if(isFixedEpisodeLength && airStoppedCount % tc.CurrentTrainConfig.episodePeriod == 0)
                EndEpisode();

            //Check wheter to take decision or an action
            if (airStoppedCount % tc.CurrentTrainConfig.decisionPeriod == 0) RequestDecision();
            else RequestAction();
        }
    }
}
