using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using System.Linq;
using System.IO;

namespace PhysicsSimulations
{
    public class CarHeightMapGenerator : MonoBehaviour
    {
        public Texture2D heightmapTexture;
        public float heightmapScale;

        public List<CarHeightMap> carHeightMaps = new List<CarHeightMap>();

        public int VoxelCount {  get; private set; }
        public bool HeightmapReady {  get; private set; }

        // Start is called before the first frame update
        void Start()
        {
            LoadHeightmap(heightmapTexture.name);
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void GenerateHeightmap()
        {
            HeightmapReady = false;
            carHeightMaps = new List<CarHeightMap>();

            VoxelCount = heightmapTexture.width * heightmapTexture.height;

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
            HeightmapReady = true;
        }

        public void GenerateHeightmapJSON()
        {
            List<CarHeightMap> _carHeightMaps = new();
            VoxelCount = heightmapTexture.width * heightmapTexture.height;

            for (int i = 0; i < heightmapTexture.width; i++)
            {
                for (int j = 0; j < heightmapTexture.height; j++)
                {
                    CarHeightMap heightMap = new CarHeightMap();
                    heightMap.row = j;
                    heightMap.column = i;
                    heightMap.height = heightmapTexture.GetPixel(i, j).grayscale * heightmapScale;

                    _carHeightMaps.Add(heightMap);
                }
            }

            // Convert the list to JSON format using JsonUtility.
            string jsonData = JsonUtility.ToJson(new SerializableList<CarHeightMap>(_carHeightMaps));

            // Get a path to save the json file
            string fileName = $"{heightmapTexture.name}.json";
            string filePath = Path.Combine(Data.CarHeightmapRoot, fileName);

            // Write the JSON data to the file.
            File.WriteAllText(filePath, jsonData);
            Debug.Log("Saved to " + filePath);
            _carHeightMaps.Clear();
        }

        public float GetHeight(int row, int column)
        {
            if(carHeightMaps == null || carHeightMaps.Count == 0)
                return 0f;

            float _height = carHeightMaps.Find(c => c.row == row && c.column == column).height;
            return _height;
        }

        public void LoadHeightmap(string fileName)
        {
            HeightmapReady = false;
            // Combine the folder path and the file name to get the full file path.
            string filePath = Path.Combine(Data.CarHeightmapRoot, fileName);

            // Check if the file exists.
            if (File.Exists(filePath))
            {
                // Read the JSON data from the file.
                string jsonData = File.ReadAllText(filePath);

                // Deserialize the JSON data back to a list using JsonUtility.
                SerializableList<CarHeightMap> dataContainer = JsonUtility.FromJson<SerializableList<CarHeightMap>>(jsonData);

                if (dataContainer != null)
                {
                    carHeightMaps =  dataContainer.items;
                    HeightmapReady = true;
                }
            }
            else
            {
                GenerateHeightmap();
            }
        }
    }

    [Serializable]
    public class CarHeightMap
    {
        public int row;
        public int column;
        public float height;
    }

    [Serializable]
    public class SerializableList<T>
    {
        public List<T> items;

        public SerializableList(List<T> list)
        {
            items = list;
        }
    }
}
