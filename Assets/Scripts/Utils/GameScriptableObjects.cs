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
        public int airParticleBurstCount = 20;
        public bool spawnAirParticlesAutomatically = true;
        public int carId;

        public SimConfiguration Clone()
        {
            SimConfiguration config = new SimConfiguration
            {
                airSpeed = this.airSpeed,
                windSpawnZoneDimension = this.windSpawnZoneDimension,
                airParticleRatio = this.airParticleRatio,
                airParticleBurstCount = this.airParticleBurstCount,
            };
            return config;
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
        public int decisionPeriod = 2;
        public int episodePeriod = 5;
        public bool onlyModifyCollidedVoxels = true;
        public bool fixedEpisodeLength = false;

        [Header("METRICS SETTINGS")]
        public bool enableKineticEnergyMetric = true;
        public bool enableDragForceMetric = true;
        public bool enableCollisionCountMetric = true;

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

        [Header("RESULT")]
        public string configName;

        // Deserialize JSON and update properties
        public void LoadFromJson(string json)
        {
            JsonUtility.FromJsonOverwrite(json, this);
        }
    }
}