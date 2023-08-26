using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System;

namespace PhysicsSimulations
{
    public class ResultUIController : MonoBehaviour
    {
        [SerializeField] private Camera resultUICamera;

        [Header("COLLISION HEATMAP")]
        [SerializeField] private List<Image> beforeHeatmaps;
        [SerializeField] private List<Image> afterHeatmaps;

        [Header("METRICS COMPARISON")]
        [SerializeField] private TextMeshProUGUI avgKEBefore;
        [SerializeField] private TextMeshProUGUI avgKEAfter;
        [SerializeField] private TextMeshProUGUI percentageKE;
        [SerializeField] private TextMeshProUGUI avgDFBefore;
        [SerializeField] private TextMeshProUGUI avgDFAfter;
        [SerializeField] private TextMeshProUGUI percentageDF;
        [SerializeField] private TextMeshProUGUI avgVCCBefore;
        [SerializeField] private TextMeshProUGUI avgVCCAfter;
        [SerializeField] private TextMeshProUGUI percentageVCC;

        [Header("CONFIG DETAILS")]
        [SerializeField] private TextMeshProUGUI configNameText;
        [SerializeField] private TextMeshProUGUI rewardMetrics;

        [Header("PLACEHOLDER")]
        [SerializeField] private Sprite placeholderSprite;

        private SimConfigurationController scc;

        private void Start()
        {
            scc = SimConfigurationController.Instance;
        }

        public void ExportResult(Action _callback)
        {
            StartCoroutine(ExportResultAsync(_callback));
        }

        private IEnumerator ExportResultAsync(Action _callback)
        {
            //Load images for Collision Heatmap (Before)
            if (Directory.Exists(Data.CollisionHeatmapsRootPath) &&
                Directory.Exists(Path.Combine(Data.CollisionHeatmapsRootPath, scc.carHeightMapGenerator.TextureName)))
            {
                string _folderPath = Path.Combine(Data.CollisionHeatmapsRootPath, scc.carHeightMapGenerator.TextureName);
                int i = 0;
                foreach (ViewAngle angle in Enum.GetValues(typeof(ViewAngle)))
                {
                    string _imagePath = Path.Combine(_folderPath, $"{scc.carHeightMapGenerator.TextureName}_{angle}.png");
                    beforeHeatmaps[i].sprite = LoadSpriteFromPath(_imagePath);
                    i++;
                }
            }
            else
            {
                Debug.Log($"<color={Data.RedColor}>Collision Heatmap (Before) not found at {Path.Combine(Data.CollisionHeatmapsRootPath, scc.carHeightMapGenerator.TextureName)}</color>");
            }

            //Load images for Collision Heatmap (After)
            string _resultPath = PlayerPrefs.GetString(Data.ResultPathPref);
            if( _resultPath == null || !Directory.Exists(_resultPath) )
            {
                _callback?.Invoke();
                yield break;
            }

            if (Directory.Exists(Path.Combine(_resultPath, Data.CollisionHeatmapsFolderName)))
            {
                string _folderPath = Path.Combine(_resultPath, Data.CollisionHeatmapsFolderName);
                int i = 0;
                foreach (ViewAngle angle in Enum.GetValues(typeof(ViewAngle)))
                {
                    string _imagePath = Path.Combine(_folderPath, $"{Path.GetFileName(_resultPath)}_{angle}.png");
                    afterHeatmaps[i].sprite = LoadSpriteFromPath(_imagePath);
                    i++;
                }
            }
            else
            {
                Debug.Log($"<color={Data.RedColor}>Collision Heatmap (After) not found at {Path.Combine(_resultPath, Data.CollisionHeatmapsFolderName)}</color>");
            }

            //Populate metric comparison
            avgKEBefore.text = $"{scc.InitialKineticEnergy:F2}";
            avgDFBefore.text = $"{scc.InitialDragForce:F2}";
            avgVCCBefore.text = $"{scc.InitialVoxelCollisionCount:0}";

            SimConfigUIController simUI = FindObjectOfType<SimConfigUIController>();
            if (simUI != null)
            {
                avgKEAfter.text = GetMetricValue(0)[0];
                percentageKE.text = GetMetricValue(0)[1];
                avgDFAfter.text = GetMetricValue(1)[0];
                percentageDF.text = GetMetricValue(1)[1];
                avgVCCAfter.text = GetMetricValue(2)[0];
                percentageVCC.text = GetMetricValue(2)[1];
            }
            
            //Path to current training config file
            string pathToTrainConfigFile = Path.Combine(_resultPath, Data.CurrentTrainingConfigFileName);
            if (File.Exists(pathToTrainConfigFile))
            {
                //Convert and save as train config
                TrainingConfiguration trainConfig = new TrainingConfiguration();
                trainConfig.LoadFromJson(File.ReadAllText(pathToTrainConfigFile));

                //Populate Config Details
                configNameText.text = trainConfig.configName;
                List<string> _metricList = new List<string>();
                if (trainConfig.enableKineticEnergyMetric) _metricList.Add("Avg KE");
                if (trainConfig.enableDragForceMetric) _metricList.Add("Avg DF");
                if (trainConfig.enableCollisionCountMetric) _metricList.Add("VCC");
                rewardMetrics.text = string.Join("+", _metricList);
            }

            yield return null;
            yield return null;

            //Export the UI as a PNG file
            RenderTexture renderTexture = new(Screen.width, Screen.height, 24);
            resultUICamera.targetTexture = renderTexture;
            resultUICamera.Render();

            RenderTexture.active = renderTexture;

            Texture2D screenshot = new(Screen.width, Screen.height, TextureFormat.RGB24, false);
            screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            screenshot.Apply();

            RenderTexture.active = null;
            resultUICamera.targetTexture = null;
            Destroy(renderTexture);

            byte[] bytes = screenshot.EncodeToPNG();
            string screenshotFileName = $"{Path.GetFileName(_resultPath)}_results.png";
            File.WriteAllBytes(Path.Combine(_resultPath, screenshotFileName), bytes);

            yield return null;

            _callback?.Invoke();
        }

        private Sprite LoadSpriteFromPath(string path)
        {
            if(!File.Exists(path))
            {
                Debug.Log($"<color={Data.RedColor}>File not found at {path}</color>");
                return placeholderSprite;
            }

            // Load the image from the provided path
            Texture2D texture = LoadTextureFromFile(path);

            // Check if the image was loaded successfully
            if (texture != null)
            {
                // Return the loaded texture
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            }
            else
            {
                Debug.Log($"<color={Data.RedColor}>Could not load the texture from {path}</color>");
                return placeholderSprite;
            }
        }

        private Texture2D LoadTextureFromFile(string path)
        {
            byte[] imageData = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(1920, 1000);
            texture.LoadImage(imageData);
            return texture;
        }

        private List<string> GetMetricValue(int metricIndex)
        {
            float _value = 0;
            string _valueStr="";
            float _initialValue = 0;

            switch (metricIndex)
            {
                case 0:
                    _value = scc.AverageKineticEnergy;
                    _valueStr = $"{_value:F2}";
                    _initialValue = scc.InitialKineticEnergy;
                    break;
                case 1:
                    _value = scc.AverageDragForce;
                    _valueStr = $"{_value:F2}";
                    _initialValue = scc.InitialDragForce;
                    break;
                case 2:
                    _value = scc.VoxelCollisionCount / scc.vccAverageFactor;
                    _valueStr = $"{_value:0}";
                    _initialValue = scc.InitialVoxelCollisionCount;
                    break;
            }

            float _percentage = ((_value - _initialValue) / (0.5f * (_value + _initialValue))) * 100f ; 

            string _prefix = _value < _initialValue ? "-" : _value == _initialValue ? "" : "+";
            string _percentagePrefix = _percentage < 0 ? "-" : "+";
            string _diffValue = $"{_prefix}{Mathf.Abs(_value - _initialValue):0}";
            string _percentageStr = $"{_percentagePrefix}{Mathf.Abs(_percentage):F2}%"; 

            string _color = _value < _initialValue ? Data.RedColor : Data.GreenColor;
            string _percentagecolor = _percentage < 0 ? Data.RedColor : Data.GreenColor;

            return new List<string>()
            {
                $"{_valueStr} (<color={_color}>{_diffValue}</color>)",
                $"<color={_percentagecolor}>{_percentageStr}</color>",
            };
        }
    }
}
