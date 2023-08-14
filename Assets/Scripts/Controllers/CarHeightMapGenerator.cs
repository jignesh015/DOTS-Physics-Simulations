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
        [SerializeField] private List<Texture2D> allHeightmapTextures;
        [SerializeField] private Texture2D selectedHeightmapTexture;
        [SerializeField] private float heightmapScale;
        [SerializeField] private bool loadHeightmap;

        public List<float> carHeightMapList = new List<float>();
        public List<float> updatedHeightmapList = new List<float>();

        public int VoxelCount {  get; private set; }
        public bool HeightmapReady {  get; private set; }

        public int TextureHeight { get; private set; }

        // Start is called before the first frame update
        void Start()
        {
            //if(loadHeightmap)
            //    LoadHeightmap(selectedHeightmapTexture.name);
            //else
            //    HeightmapReady = true;
        }

        public void GenerateHeightmap()
        {
            HeightmapReady = false;

            VoxelCount = selectedHeightmapTexture.width * selectedHeightmapTexture.height;
            float[] carHeightMapListArr = new float[VoxelCount];
            int _textureHeight = selectedHeightmapTexture.height;

            Debug.Log($"Width: {selectedHeightmapTexture.width} | Height: {selectedHeightmapTexture.height}");

            for(int i = 0; i < selectedHeightmapTexture.width; i++)
            {
                for(int j = 0; j < selectedHeightmapTexture.height; j++)
                {
                    carHeightMapListArr[j + i * _textureHeight] = selectedHeightmapTexture.GetPixel(i, j).grayscale * heightmapScale;
                }
            }
            carHeightMapList = carHeightMapListArr.ToList();
            HeightmapReady = true;
        }

        public void GenerateHeightmapJSON()
        {
            VoxelCount = selectedHeightmapTexture.width * selectedHeightmapTexture.height;
            float[] carHeightMapListArr = new float[VoxelCount];
            int _textureHeight = selectedHeightmapTexture.height;

            Debug.Log($"Width: {selectedHeightmapTexture.width} | Height: {selectedHeightmapTexture.height}");

            for (int i = 0; i < selectedHeightmapTexture.width; i++)
            {
                for (int j = 0; j < selectedHeightmapTexture.height; j++)
                {
                    carHeightMapListArr[j + i * _textureHeight] = selectedHeightmapTexture.GetPixel(i, j).grayscale * heightmapScale;
                }
            }
            List<float> _carHeightMapList = carHeightMapListArr.ToList();

            // Convert the list to JSON format using JsonUtility.
            string jsonData = JsonUtility.ToJson(new SerializableList<float>(_carHeightMapList));

            // Get a path to save the json file
            string fileName = $"{selectedHeightmapTexture.name}.json";
            string filePath = Path.Combine(Data.CarHeightmapRootResources, fileName);

            // Write the JSON data to the file.
            File.WriteAllText(filePath, jsonData);
            Debug.Log("Saved to " + filePath);
            _carHeightMapList.Clear();
        }

        public float GetHeight(int row, int column)
        {
            if(carHeightMapList == null ||  carHeightMapList.Count == 0) 
                return 0f;

            float _height = carHeightMapList[row + column * selectedHeightmapTexture.height];
            return _height;
        }

        public void UpdateHeight(int row, int column, float value)
        {
            if (updatedHeightmapList == null || updatedHeightmapList.Count == 0)
                return;

            updatedHeightmapList[row + column * TextureHeight] = value;
        }

        public void LoadHeightmap(Texture2D _heightmap)
        {
            HeightmapReady = false;
            selectedHeightmapTexture = _heightmap;
            TextureHeight = selectedHeightmapTexture.height;
            VoxelCount = selectedHeightmapTexture.width * selectedHeightmapTexture.height;

            // Combine the folder path and the file name to get the full file path.
            string filePath = Path.Combine(Data.CarHeightmapRoot, $"{selectedHeightmapTexture.name}");
            var textFile = Resources.Load<TextAsset>(filePath);

            // Check if the file exists.
            if (textFile != null)
            {
                // Deserialize the JSON data back to a list using JsonUtility.
                SerializableList<float> dataContainer = JsonUtility.FromJson<SerializableList<float>>(textFile.text);

                if (dataContainer != null)
                {
                    carHeightMapList = dataContainer.items;
                    updatedHeightmapList =  dataContainer.items;
                    HeightmapReady = true;
                }
            }
            else
            {
                Debug.Log($"Heightmap not found at {filePath}");
                GenerateHeightmap();
            }
        }

        public void LoadHeightmap(int _heightmapIndex)
        {
            LoadHeightmap(allHeightmapTextures[_heightmapIndex]);
        }

        public void LoadHeightmap(string _heightmapJsonPath, int _heightmapIndex)
        {
            HeightmapReady = false;
            selectedHeightmapTexture = allHeightmapTextures[_heightmapIndex];
            TextureHeight = selectedHeightmapTexture.height;
            VoxelCount = selectedHeightmapTexture.width * selectedHeightmapTexture.height;

            if (File.Exists(_heightmapJsonPath))
            {
                string _json = File.ReadAllText(_heightmapJsonPath);

                // Deserialize the JSON data back to a list using JsonUtility.
                SerializableList<float> dataContainer = JsonUtility.FromJson<SerializableList<float>>(_json);
                if (dataContainer != null)
                {
                    carHeightMapList = dataContainer.items;
                    HeightmapReady = true;
                }
                else
                    LoadHeightmap(_heightmapIndex);
            }
            else
                LoadHeightmap(_heightmapIndex);
        }
    }
}
