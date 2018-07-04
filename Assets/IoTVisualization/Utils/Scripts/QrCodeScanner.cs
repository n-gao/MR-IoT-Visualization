using System;
using System.Collections.Generic;
using System.Linq;
using HoloToolkit.Unity;
using UnityEngine;
using UnityEngine.VR.WSA.WebCam;
#if WINDOWS_UWP
using ZXing;
using System.Threading;
using System.Threading.Tasks;
#endif

namespace IoTVisualization.Utils
{
    /// <summary>
    /// This singleton provides easy access to scan the current field of vision for qr codes.
    /// </summary>
    public class QrCodeScanner : Singleton<QrCodeScanner>
    {
        /// <summary>
        /// Last scanned text.
        /// </summary>
        public string ScannedText { get; private set; }
        /// <summary>
        /// Indicates whether the scanner is currently scanning.
        /// </summary>
        public bool Scanning { get; private set; }

        private PhotoCapture _photoCaptureObject = null;

        private Resolution _resolution;

        private Action<string> _callback;

        private Action _failureCallback;
        
        /// <summary>
        /// Clean up when being destroyed.
        /// </summary>
        protected override void OnDestroy()
        {
            if (_photoCaptureObject != null)
                _photoCaptureObject.Dispose();
            _photoCaptureObject = null;
        }

        void Start()
        {
            //Used for taking screenshots of keyboard input.
//            this.enabled = false;
        }

        /// <summary>
        /// Starts to scan for qr codes.
        /// In this method a PhotoCapture will be requested.
        /// </summary>
        /// <param name="callback">Callback which will be called as soon the scan finished</param>
        /// <param name="failureCallback">Callback which will be called if an error occurs.</param>
        public void Scan(Action<string> callback, Action failureCallback)
        {
            if (Scanning) return;
            Scanning = true;
            _callback = callback;
            _failureCallback = failureCallback;
            PhotoCapture.CreateAsync(false, InitParameters);
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
            _resolution = PhotoCapture.SupportedResolutions.FirstOrDefault(r => r.width == 1280 && r.height == 720);

            CameraParameters param = new CameraParameters
            {
                hologramOpacity = 0,
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
                _photoCaptureObject.TakePhotoAsync(SavePhotoToMemory);
            }
            else
            {
                Debug.LogError("Unable to start photo mode!");
                if (_failureCallback != null)
                    _failureCallback();
            }
        }

        /// <summary>
        /// Saves the image to memory and starts the scanning process.
        /// 
        /// Is used as callback after taking a photo.
        /// </summary>
        /// <param name="result">Capture result</param>
        /// <param name="photoCaptureFrame">Capture frame</param>
        private void SavePhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
        {
            if (result.success)
            {
                List<byte> imageBufferList = new List<byte>();
                // Copy the raw IMFMediaBuffer data into our empty byte list.
                photoCaptureFrame.CopyRawImageDataIntoBuffer(imageBufferList);

                ScanForQrCode(imageBufferList.ToArray(), _resolution.width, _resolution.height);
            }
            _photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode); 
        }
         
        /// <summary>
        /// Scans the given image for qr codes and calls the set callback afterwards.
        /// </summary>
        /// <param name="imgBytes">Image data</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        private void ScanForQrCode(byte[] imgBytes, int width, int height)
        {
#if WINDOWS_UWP
            CancellationTokenSource tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            Task.Factory.StartNew(() => {
                IBarcodeReader reader = new BarcodeReader();
                var result = reader.Decode(imgBytes, width, height, BitmapFormat.BGRA32);
                UnityEngine.WSA.Application.InvokeOnAppThread(() =>
                {
                    Scanning = false;
                    ScannedText = result?.Text;
                    Debug.Log("Found QR tag : " + ScannedText);
                    _callback?.Invoke(ScannedText);
                }, false);
            }, tokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
#endif
        }

        private void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
        {
            _photoCaptureObject.Dispose();
            _photoCaptureObject = null;
        }
    }
}
