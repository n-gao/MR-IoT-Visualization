using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HoloToolkit.Unity;
using IoTVisualization.Localization;
using UnityEngine;
using UnityEngine.VR.WSA.WebCam;
#if WINDOWS_UWP
//using Windows.Storage;
#endif


namespace IoTVisualization.Utils
{
    public class ScreenshotUtil : Singleton<ScreenshotUtil>
    {
        /// <summary>
        /// Indicates whether the scanner is currently scanning.
        /// </summary>
        public bool TakingPicture { get; private set; }

        private PhotoCapture _photoCaptureObject = null;

        private Resolution _resolution;
        
        private string _tmpFileName;
        private string _savedLocation;

        private string _name;

        
#if WINDOWS_UWP
//        private StorageFolder _imgFolder;
//
//        void Start()
//        {
//            GetStorageFolder();
//        }
//
//        private async void GetStorageFolder()
//        {
//            var picLib = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);
//            _imgFolder = picLib.SaveFolder;
//        }
#endif


        /// <summary>
        /// Clean up when being destroyed.
        /// </summary>
        protected override void OnDestroy()
        {
            if (_photoCaptureObject != null)
                _photoCaptureObject.Dispose();
            _photoCaptureObject = null;
        }

        /// <summary>
        /// Takes a picture and saved it to storage.
        /// </summary>
        public void TakePicture(string name = null)
        {
            if (TakingPicture) return;
            _name = name;
            TakingPicture = true;
            PhotoCapture.CreateAsync(true, InitParameters);
        }

        /// <summary>
        /// Setting capture parameters and starts the photo mode.
        /// 
        /// Is be used as callback after creating a PhotoCapture.
        /// </summary>
        /// <param name="capture">PhotoCapture</param>
        void InitParameters(PhotoCapture capture)
        {
            _photoCaptureObject = capture;
            _resolution = PhotoCapture.SupportedResolutions.First(r => r.width == 1408 && r.height == 792);
//            _resolution = PhotoCapture.SupportedResolutions
//                .OrderByDescending(res => res.width * res.height).FirstOrDefault();

            CameraParameters param = new CameraParameters
            {
                hologramOpacity = 1,
                cameraResolutionWidth = _resolution.width,
                cameraResolutionHeight = _resolution.height,
                pixelFormat = CapturePixelFormat.BGRA32
            };

            _photoCaptureObject.StartPhotoModeAsync(param, TakePhoto);
        }

        /// <summary>
        /// Takes a picture and starts the saves the image afterwards.
        /// 
        /// Is used as callback after starting photo mode.
        /// </summary>
        /// <param name="result">Capture result</param>
        private void TakePhoto(PhotoCapture.PhotoCaptureResult result)
        {
            if (result.success)
            {
                _tmpFileName = string.IsNullOrEmpty(_name) ? string.Format("img_{0}.jpg", Time.time) : string.Format("img_{0}.jpg", _name);
                _savedLocation = Path.Combine(Application.persistentDataPath, _tmpFileName);
                _photoCaptureObject.TakePhotoAsync(_savedLocation,
                    PhotoCaptureFileOutputFormat.JPG,
                    OnCapturedPhotoToDiskCallback);
            }
            else
            {
                Debug.LogError("Unable to start photo mode!");
                TakingPicture = false;
            }
        }

        private void OnCapturedPhotoToDiskCallback(PhotoCapture.PhotoCaptureResult result)
        {
            print(result.success
                ? "Successfully saved image to storage."
                : "An error occured while saving to storage.");
#if WINDOWS_UWP
//            if (result.success)
//                File.Move(_savedLocation, Path.Combine(_imgFolder.Path, _tmpFileName));
#endif
            _photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
        }
        
        private void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
        {
            _photoCaptureObject.Dispose();
            _photoCaptureObject = null;
            TakingPicture = false;
        }
    }

}