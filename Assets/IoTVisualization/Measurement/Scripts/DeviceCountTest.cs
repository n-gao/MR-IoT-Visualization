using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IoTVisualization.Localization;
using IoTVisualization.Networking.Utils;
using IoTVisualization.Utils;
using UnityEngine;

namespace IoTVisualization.Measurement
{
    /// <summary>
    /// This component executes a test which determins the overall performance of this application by continously
    /// placing more devices.
    /// </summary>
    public class DeviceCountTest : ProviderMonoBehaviour
    {
        private enum State
        {
            Placing,
            Capturing,
            WaitForPicture,
            Preparing,
            Finished,
            Idle,
            WaitingToStart
        }

        /// <summary>
        /// If set to false the devices will be placed behind the useres field of view instead of in front of it.
        /// </summary>
        public bool Visible = true;
        /// <summary>
        /// If set to true a screenshot will be taken before every testrun.
        /// </summary>
        public bool TakeScreenshots = false;

        private DeviceObjectManager _manager;
        private FpsMeasure _measure;

        private State _state = State.Idle;

        public float WaitToStart = 15;
        public float PrepareTime = 6;
        public float TimeToTakePicture = 3;

        private float _placedTime;

        private int _placedCount;

        // Use this for initialization
        void Start()
        {
            _measure = FpsMeasure.Instance;
            _manager = DeviceObjectManager.Instance;
        }

        // Update is called once per frame
        void Update()
        {
            switch (_state)
            {
                case State.Idle:
                    Idle();
                    break;
                case State.WaitingToStart:
                    WaitingToStart();
                    break;
                case State.Placing:
                    Placing();
                    break;
                case State.Capturing:
                    Capturing();
                    break;
                case State.Preparing:
                    Preparing();
                    break;
                case State.WaitForPicture:
                    WaitForPicture();
                    break;
                case State.Finished:
                    break;
            }
        }

        private void WaitForPicture()
        {
            if (!TakeScreenshots)
                _state = State.Preparing;
            if (Time.time - _placedTime >= TimeToTakePicture)
            {
                if (ScreenshotUtil.IsInitialized)
                    ScreenshotUtil.Instance.TakePicture();
                _state = State.Preparing;
            }
        }

        private void Idle()
        {
            if (Provider == null || !Provider.IsConnected) return;
            _placedTime = Time.time;
            _state = State.WaitingToStart;
        }

        private void WaitingToStart()
        {
            if (Time.time - _placedTime >= WaitToStart)
                _state = State.Preparing;
        }

        private void Capturing()
        {
            if (!_measure.Capturing)
                _state = State.Placing;
        }

        private void Preparing()
        {
            if (Time.time - _placedTime >= PrepareTime)
            {
                _measure.StartMeasure(_manager.Objects.Count.ToString("00") + ".txt");
                _state = State.Capturing;
            }
        }

        private void Placing()
        {
            var devices = _manager.UnmatchedDevices;
            if (devices.Length == 0)
            {
                print("finished");
                _state = State.Finished;
                return;
            }
            var toPlace = devices[0];
            var gameObj = _manager.CreateGameObject(toPlace);
            gameObj.transform.position = Camera.main.transform.position + (Visible ? 1 : -1) * (Camera.main.transform.rotation * new Vector3(_placedCount % 8 * .5f - 3.5f * .5f, -_placedCount / 8 * .4f + 2 * .4f, 10));
            var dir = gameObj.transform.position - Camera.main.transform.position;
            gameObj.transform.LookAt(gameObj.transform.position + dir);
            _placedCount++;
            _placedTime = Time.time;
            _state = State.WaitForPicture;
        }
    }
}
