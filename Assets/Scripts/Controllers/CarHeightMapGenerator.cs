using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using System.Linq;

namespace PhysicsSimulations
{
    public class CarHeightMapGenerator : MonoBehaviour
    {
        public Texture2D heightmapTexture;
        public float heightmapScale;

        public List<CarHeightMap> carHeightMaps = new List<CarHeightMap>();

        // Start is called before the first frame update
        void Start()
        {
            Invoke(nameof(GenerateHeightmap), 1f);
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void GenerateHeightmap()
        {
            carHeightMaps = new List<CarHeightMap>();

            Debug.Log($"Width: {heightmapTexture.width} | Height: {heightmapTexture.height}");

            for(int i = 0; i < heightmapTexture.width; i++)
            {
                for(int j = 0; j < heightmapTexture.height; j++)
                {
                    CarHeightMap heightMap = new CarHeightMap();
                    heightMap.row = j;
                    heightMap.column = i;
                    heightMap.height = heightmapTexture.GetPixel(i, j).grayscale * heightmapScale;

                    carHeightMaps.Add(heightMap);
                }
            }
        }

        public float GetHeight(int row, int column)
        {
            if(carHeightMaps == null || carHeightMaps.Count == 0)
                return 0f;

            float _height = carHeightMaps.Find(c => c.row == row && c.column == column).height;
            return _height;
        }
    }

    [Serializable]
    public class CarHeightMap
    {
        public int row;
        public int column;
        public float height;
    }
}
