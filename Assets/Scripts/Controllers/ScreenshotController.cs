using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PhysicsSimulations
{
    public class ScreenshotController : MonoBehaviour
    {
        [SerializeField] private List<Camera> cameraAngles;
        
        private string screenshotFileName;
        private Camera targetCamera;

        private SimConfigurationController scc;

        private void Start()
        {
            scc = SimConfigurationController.Instance;
        }

        public void TakeCameraScreenshot(ViewAngle _angle, string _savePath, string _fileName = "Default")
        {
            targetCamera = cameraAngles[(int)_angle];
            RenderTexture renderTexture = new(Screen.width, Screen.height, 24);
            targetCamera.targetTexture = renderTexture;
            targetCamera.Render();

            RenderTexture.active = renderTexture;

            Texture2D screenshot = new(Screen.width, Screen.height, TextureFormat.RGB24, false);
            screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            screenshot.Apply();

            RenderTexture.active = null;
            targetCamera.targetTexture = null;
            Destroy(renderTexture);

            byte[] bytes = screenshot.EncodeToPNG();
            screenshotFileName = $"{_fileName}_{_angle}.png";

            //Check if directory exists to save the file
            if(!Directory.Exists(_savePath))
                Directory.CreateDirectory(_savePath );

            File.WriteAllBytes(Path.Combine(_savePath, screenshotFileName), bytes);
            //Debug.Log("Camera screenshot captured: " + screenshotFileName);
        }

        public void TakeCameraScreenshot(int _angle)
        {
            TakeCameraScreenshot((ViewAngle)_angle, Application.dataPath);
        }

        public void ExportCollisionHeatmap(Action _callback, string _savePath, string _fileName)
        {
            StartCoroutine(ExportCollisionHeatmapAsync(_callback, _savePath, _fileName));
        }

        private IEnumerator ExportCollisionHeatmapAsync(Action _callback, string _savePath, string _fileName)
        {
            if (!scc.SpawnAirParticlesCommand && scc.SpawnAirParticles)
                scc.StopAirParticles();

            yield return new WaitForSeconds(1f);

            //Take screenshot from all angle
            foreach (ViewAngle angle in Enum.GetValues(typeof(ViewAngle)))
            {
                TakeCameraScreenshot(angle, _savePath, _fileName);
                yield return null;
                yield return null;
            }
            yield return new WaitForSeconds(0.1f);
            _callback?.Invoke();
        }
    }
}
