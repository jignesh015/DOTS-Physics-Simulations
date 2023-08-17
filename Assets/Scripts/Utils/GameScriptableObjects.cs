using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsSimulations
{
    [Serializable]
    public class SimConfiguration
    {
        public float airSpeed = 50;
        public Vector3 windSpawnZoneDimension;
        public float airParticleRatio = 0.5f;
        public int airParticleBurstCount = 10;
        public bool spawnAirParticlesAutomatically = true;
        public int carId;

        public SimConfiguration Clone()
        {
            return new SimConfiguration
            {
                airSpeed = airSpeed,
                windSpawnZoneDimension = windSpawnZoneDimension,
                airParticleRatio = airParticleRatio,
                airParticleBurstCount = airParticleBurstCount,
                spawnAirParticlesAutomatically = spawnAirParticlesAutomatically,
                carId = carId
            };
        }

        // Deserialize JSON and update properties
        public void LoadFromJson(string json)
        {
            JsonUtility.FromJsonOverwrite(json, this);
        }
    }

    [Serializable]
    public class SimConfigurationSanity
    {
        public float airSpeedMin = 10;
        public float airSpeedMax = 120;

        public Vector3 windSpawnZoneDimensionMin;
        public Vector3 windSpawnZoneDimensionMax;

        public float airParticleRatioMin = 0.1f;
        public float airParticleRatioMax = 1f;

        public int airParticleBurstCountMin = 0;
        public int airParticleBurstCountMax = 80;
    }

    [Serializable]
    public class TrainingConfiguration
    {
        [Header("TRAINING SETTINGS")]
        public float maxVoxelHeightVariance = 0.075f;
        public float stepVarianceFactor = 5f;
        public int decisionPeriod = 2;
        public int episodePeriod = 5;
        public bool onlyModifyCollidedVoxels = true;
        public bool onlyDecreaseHeight = false;
        public bool fixedEpisodeLength = false;

        [Header("METRICS SETTINGS")]
        public bool enableKineticEnergyMetric = true;
        public bool enableDragForceMetric = true;
        public bool enableCollisionCountMetric = true;
        public bool enableHeightmapSumMetric = true;

        public float maxKineticEnergyVariance = 30;
        public int maxDragForceVariance = 200;
        public int maxCollisionCountVariance = 2000;

        [Header("REWARD SCORES")]
        //KINETIC ENERGY REWARD
        public float kineticEnergyPositiveScore = 2;
        public float kineticEnergyNegativeScore = -1;
        //DRAG FORCE REWARD
        public float dragForcePositiveScore = 2;
        public float dragForceNegativeScore = -1;
        //COLLISION COUNT REWARD
        public float collisionCountPositiveScore = 2;
        public float collisionCountNegativeScore = -1;
        //HEIGHTMAP SUM REWARD
        public float heightmapSumPositiveScore = 2;
        public float heightmapSumNegativeScore = -1;


        [Header("RESULT")]
        public string configName;

        [Header("HYPER PARAMETERS")]
        public int timeScale = 20;

        public TrainingConfiguration Clone()
        {
            return new TrainingConfiguration
            {
                maxVoxelHeightVariance = maxVoxelHeightVariance,
                stepVarianceFactor = stepVarianceFactor,
                decisionPeriod = decisionPeriod,
                episodePeriod = episodePeriod,
                onlyModifyCollidedVoxels = onlyModifyCollidedVoxels,
                onlyDecreaseHeight = onlyDecreaseHeight,
                fixedEpisodeLength = fixedEpisodeLength,
                enableKineticEnergyMetric = enableKineticEnergyMetric,
                enableDragForceMetric = enableDragForceMetric,
                enableCollisionCountMetric = enableCollisionCountMetric,
                enableHeightmapSumMetric = enableHeightmapSumMetric,
                maxKineticEnergyVariance = maxKineticEnergyVariance,
                maxDragForceVariance = maxDragForceVariance,
                maxCollisionCountVariance = maxCollisionCountVariance,
                kineticEnergyPositiveScore = kineticEnergyPositiveScore,
                kineticEnergyNegativeScore = kineticEnergyNegativeScore,
                dragForcePositiveScore = dragForcePositiveScore,
                dragForceNegativeScore = dragForceNegativeScore,
                collisionCountPositiveScore = collisionCountPositiveScore,
                collisionCountNegativeScore = collisionCountNegativeScore,
                heightmapSumPositiveScore = heightmapSumPositiveScore,
                heightmapSumNegativeScore = heightmapSumNegativeScore,
                configName = configName,
                timeScale = timeScale
            };
        }


        // Deserialize JSON and update properties
        public void LoadFromJson(string json)
        {
            JsonUtility.FromJsonOverwrite(json, this);
        }
    }

    [Serializable]
    public class TrainingOutput
    {
        public float baselineKineticEnergy;
        public float baselineDragForce;
        public int baselineVoxelCollisionCount;
        public string trainingTime;

        // Deserialize JSON and update properties
        public void LoadFromJson(string json)
        {
            JsonUtility.FromJsonOverwrite(json, this);
        }
    }
}
