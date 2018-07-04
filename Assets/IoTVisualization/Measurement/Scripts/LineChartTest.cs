using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IoTVisualization.Localization;
using IoTVisualization.Networking.Utils;
using IoTVisualization.Utils;
using IoTVisualization.Visualization;
using UnityEngine;

namespace IoTVisualization.Measurement
{
    /// <summary>
    /// This component executes the test to measure the performance of LineChart implementations.
    /// </summary>
    public class LineChartTest : ProviderMonoBehaviour
    {
        private enum State
        {
            Increasing,
            Capturing,
            WaitForPicture,
            Preparing,
            Finished,
            Idle,
            WaitToStart
        }

        private DeviceObjectManager _manager;
        private FpsMeasure _measure;

        private State _state = State.Idle;

        private const float WaitToStart = 3;
        private const float PrepareTime = 2;
        private const float TimeToTakePicture = 1;

        private float _placedTime;

        private int _placedCount;

        private LineChartController[] _controllers;

        private int _duration = 0;

        private const int StepSize = 30;


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
                case State.WaitToStart:
                    Wait();
                    break;
                case State.Increasing:
                    Increasing();
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
            if (Time.time - _placedTime >= TimeToTakePicture)
            {
                if (ScreenshotUtil.IsInitialized)
                    ScreenshotUtil.Instance.TakePicture(_duration.ToString("0000"));
                _state = State.Preparing;
            }
        }

        private float _timer;
        private void Idle()
        {
            if (Provider == null || !Provider.IsConnected) return;
            _timer += Time.deltaTime;
            if (_timer <= 15) return;
            Placing();
            Placing();
            _controllers = DeviceObjectManager.Instance.Objects.Values
                .SelectMany(v => v.GetComponentsInChildren<LineChartController>()).ToArray();
            _state = State.WaitToStart;
            _placedTime = Time.time;
        }

        private void Wait()
        {
            if (Time.time - _placedTime >= WaitToStart)
            {
                _controllers = DeviceObjectManager.Instance.Objects.Values
                    .SelectMany(v => v.GetComponentsInChildren<LineChartController>()).ToArray();
                _state = State.Increasing;
            }
        }

        private void Capturing()
        {
            if (!_measure.Capturing)
                _state = State.Increasing;
        }

        private void Preparing()
        {
            if (Time.time - _placedTime >= PrepareTime)
            {
                //            _measure.StartMeasure(_duration.ToString("0000") + ".txt");
                _state = State.Capturing;
            }
        }

        private void Increasing()
        {
            if (_duration == 1800)
            {
                print("finished");
                _state = State.Finished;
                return;
            }
            _duration = _duration + StepSize;
            foreach (var controller in _controllers)
            {
                controller.Span = _duration;
            }
            _placedTime = Time.time;
            _state = State.WaitForPicture;
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
            gameObj.transform.position = Camera.main.transform.position + Camera.main.transform.rotation * new Vector3(_placedCount % 8 * .5f - 0.5f * .5f, -_placedCount / 8 * .4f + 0 * .4f, 1.3f);
            var dir = gameObj.transform.position - Camera.main.transform.position;
            gameObj.transform.LookAt(gameObj.transform.position + dir);
            _placedCount++;
        }
    }
}
