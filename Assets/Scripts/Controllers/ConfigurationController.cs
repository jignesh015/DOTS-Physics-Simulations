using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsSimulations
{
    public class ConfigurationController : MonoBehaviour
    {



        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }

    [Serializable]
    public class Configuration
    {
        public float windSpeed;
        public Vector3 windSpawnZoneDimension;

        public float airParticleSize;
        public int airParticleCount;
        public float airParticleGravityFactor;
    }
}
