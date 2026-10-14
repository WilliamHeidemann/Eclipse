using System;
using System.IO;
using UnityEngine;

namespace Game.Presentation
{
    public class ScreenCaptureTool : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log(Directory.Exists(Application.streamingAssetsPath));
            Debug.Log(Application.streamingAssetsPath);
        }

        private void Update()
        {
            // if (Input.GetKeyDown(KeyCode.S))
            // {
            //     ScreenCapture.CaptureScreenshot(Application.streamingAssetsPath + "/" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".png");
            // }
        }
    }
}