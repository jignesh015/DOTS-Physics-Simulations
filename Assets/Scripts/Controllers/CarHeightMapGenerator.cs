using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

namespace PhysicsSimulations
{
    public class CarHeightMapGenerator : MonoBehaviour
    {
        public Texture2D heightmapTexture;
        public float heightmapScale;

        [SerializeField] private bool loadHeightmap;

        public List<float> carHeightMapList = new List<float>();

        public int VoxelCount {  get; private set; }
        public bool HeightmapReady {  get; private set; }

        // Start is called before the first frame update
        void Start()
        {
            if(loadHeightmap)
                LoadHeightmap(heightmapTexture.name);
            else
                HeightmapReady = true;
        }

        public void GenerateHeightmap()
        {
            HeightmapReady = false;

            VoxelCount = heightmapTexture.width * heightmapTexture.height;
            float[] carHeightMapListArr = new float[VoxelCount];
            int _textureHeight = heightmapTexture.height;

            Debug.Log($"Width: {heightmapTexture.width} | Height: {heightmapTexture.height}");

            for(int i = 0; i < heightmapTexture.width; i++)
            {
                for(int j = 0; j < heightmapTexture.height; j++)
                {
                    carHeightMapListArr[j + i * _textureHeight] = heightmapTexture.GetPixel(i, j).grayscale * heightmapScale;
                }
            }
            carHeightMapList = carHeightMapListArr.ToList();
            HeightmapReady = true;
        }

        public void GenerateHeightmapJSON()
        {
            VoxelCount = heightmapTexture.width * heightmapTexture.height;
            float[] carHeightMapListArr = new float[VoxelCount];
            int _textureHeight = heightmapTexture.height;

            Debug.Log($"Width: {heightmapTexture.width} | Height: {heightmapTexture.height}");

            for (int i = 0; i < heightmapTexture.width; i++)
            {
                for (int j = 0; j < heightmapTexture.height; j++)
                {
                    carHeightMapListArr[j + i * _textureHeight] = heightmapTexture.GetPixel(i, j).grayscale * heightmapScale;
                }
            }
            List<float> _carHeightMapList = carHeightMapListArr.ToList();

            // Convert the list to JSON format using JsonUtility.
            string jsonData = JsonUtility.ToJson(new SerializableList<float>(_carHeightMapList));

            // Get a path to save the json file
            string fileName = $"{heightmapTexture.name}.json";
            string filePath = Path.Combine(Data.CarHeightmapRoot, fileName);

            // Write the JSON data to the file.
            File.WriteAllText(filePath, jsonData);
            Debug.Log("Saved to " + filePath);
            _carHeightMapList.Clear();
        }

        public float GetHeight(int row, int column)
        {
            if(carHeightMapList == null ||  carHeightMapList.Count == 0) 
                return 0f;

            float _height = carHeightMapList[row + column * heightmapTexture.height];
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
                SerializableList<float> dataContainer = JsonUtility.FromJson<SerializableList<float>>(jsonData);

                if (dataContainer != null)
                {
                    carHeightMapList =  dataContainer.items;
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
    public class SerializableList<T>
    {
        public List<T> items;

        public SerializableList(List<T> list)
        {
            items = list;
        }
    }
}
