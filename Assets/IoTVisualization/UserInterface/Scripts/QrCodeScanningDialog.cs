using System;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using IoTVisualization.Utils;
using UnityEngine;

namespace IoTVisualization.UserInterface
{
    /// <summary>
    /// This singleton can be used for as user dialog to scan a QR code. After tapping a photo will be taken with 
    /// the build in camera and then processed to scan for a code.
    /// </summary>
    [RequireComponent(typeof(TextMesh))]
    public class QrCodeScanningDialog : Singleton<QrCodeScanningDialog>, IInputClickHandler
    {
        private enum State
        {
            Idle,
            Scanning,
            Failed,
            Error,
        }

        public string DefaultText = "Look at the QR code\nand tap.";
        public string ScanningText = "Scanning";
        public string TryAgainText = "Please try again.";
        public string ErrorText = "Could not open camera.\nTap to close.";

        /// <summary>
        /// Indicates whether a photo is currently processed.
        /// </summary>
        public bool Scanning { get { return _state == State.Scanning; }}
        /// <summary>
        /// This event will be called when the scanning process has been finished.
        /// If the parameter is null no code has been found.
        /// </summary>
        public event Action<string> Scanned;
        /// <summary>
        /// This event will be called when an error occurs and the dialog should be closed.
        /// </summary>
        public event Action Canceled;

        private TextMesh _textMesh;

        private int _dots = 0;
        private float _lastDot = 0;
        private const float DotInterval = 0.75f;

        private State _state;

        void Start()
        {
            _textMesh = GetComponent<TextMesh>();
            InputManager.Instance.OverrideFocusedObject = gameObject;
        }

        void Update()
        {
            if (Scanning && Time.time - _lastDot >= DotInterval)
            {
                _lastDot = Time.time;
                _dots = _dots > 2 ? 1 : _dots + 1;
                _textMesh.text = ScanningText + new string('.', _dots);
            }
        }

        public void OnInputClicked(InputClickedEventData eventData)
        {
            //if not scanning
            if (Scanning) return;
            //Start scanning
            switch (_state)
            {
                case State.Idle:
                case State.Failed:
                    Scan();
                    break;
                //Close when an error occurs
                case State.Error:
                    if (Canceled != null)
                        Canceled();
                    Destroy(gameObject);
                    break;
                case State.Scanning:
                default:
                    break;
            }
        }

        private void Scan()
        {
            _state = State.Scanning;
            _dots = 0;
            _lastDot = Time.time;
            _textMesh.text = ScanningText;
            QrCodeScanner.Instance.Scan(result =>
            {
                _state = result != null ? State.Idle : State.Failed;
                _textMesh.text = result == null ? TryAgainText : DefaultText;
                if (Scanned != null)
                    Scanned(result);
            }, () =>
            {
                _state = State.Error;
                _textMesh.text = ErrorText;
            });
        }
    }
}
