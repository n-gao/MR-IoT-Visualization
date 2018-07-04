using System.Collections;
using System.Collections.Generic;
using IoTVisualization.Utils;
using UnityEngine;

namespace IoTVisualization.UserInterface
{
    public class ScreenshotCommand : MonoBehaviour
    {
        /// <summary>
        /// Takes a picture
        /// </summary>
        public void TakeScreenshot()
        {
            if (ScreenshotUtil.IsInitialized)
                ScreenshotUtil.Instance.TakePicture();
        }
    }
}
