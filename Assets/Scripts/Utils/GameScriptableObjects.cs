using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsSimulations
{
    [CreateAssetMenu(fileName = "DefaultSimConfig", menuName = "Config/SimConfiguration")]
    public class SimConfiguration: ScriptableObject
    {
        [Range(10, 100)]
        public float airSpeed;
        public Vector3 windSpawnZoneDimension;

        [Range(0.1f, 1f)]
        public float airParticleRatio;

        [Range(0, 80)]
        public int airParticleBurstCount;

        public bool spawnAirParticlesAutomatically;

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

    [CreateAssetMenu(fileName = "SimConfigSanity", menuName = "Config/SimConfigSanity")]
    public class SimConfigurationSanity : ScriptableObject
    {
        public float airSpeedMin;
        public float airSpeedMax;

        public Vector3 windSpawnZoneDimensionMin;
        public Vector3 windSpawnZoneDimensionMax;

        public float airParticleRatioMin;
        public float airParticleRatioMax;

        public int airParticleBurstCountMin;
        public int airParticleBurstCountMax;
    }
}
