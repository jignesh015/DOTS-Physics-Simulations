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

        public void TakeCameraScreenshot(ViewAngle _angle, string _savePath)
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
            screenshotFileName = $"{Path.GetFileName(_savePath)}_{_angle}.png";
            File.WriteAllBytes(Path.Combine(_savePath, screenshotFileName), bytes);
            Debug.Log("Camera screenshot captured: " + screenshotFileName);
        }

        public void TakeCameraScreenshot(int _angle)
        {
            TakeCameraScreenshot((ViewAngle)_angle, Application.dataPath);
        }

        public void ExportCollisionHeatmap(Action _callback)
        {
            StartCoroutine(ExportCollisionHeatmapAsync(_callback));
        }

        private IEnumerator ExportCollisionHeatmapAsync(Action _callback)
        {
            if (!scc.SpawnAirParticlesCommand && scc.SpawnAirParticles)
                scc.StopAirParticles();

            yield return new WaitForSeconds(1f);

            //Take screenshot from all angle
            foreach (ViewAngle angle in Enum.GetValues(typeof(ViewAngle)))
            {
                TakeCameraScreenshot(angle, PlayerPrefs.GetString(Data.ResultPathPref));
                yield return null;
                yield return null;
            }
            yield return new WaitForSeconds(0.1f);
            _callback?.Invoke();
        }
    }
}
