using System;
using System.Collections;
using System.Collections.Generic;
using IoTVisualization.Visualization;
using UnityEngine;
using UnityEngine.VR.WSA.Input;

namespace IoTVisualization.Desktop
{
    /// <summary>
    /// This component is basically a copy of the MR LineChartController with a few adjustments to work with mouse and touch.
    /// </summary>
    [RequireComponent(typeof(LineChart))]
    public class MouseLineChartController : MonoBehaviour
    {
        /// <summary>
        /// If this is set to false, the user is not able to manipulate the LineChart.
        /// </summary>
        public bool EnableUserInput = true;
        /// <summary>
        /// Enum of possible play states.
        /// </summary>
        public enum PlayStatus
        {
            Play,   //Used for normal playing and fast forwards
            Paused, //Used to pause
        }
        /// <summary>
        /// Indicates whether the user is currently holding a tap on the line chart.
        /// </summary>
        private bool _hold = false;
        private PlayStatus _previouStatus = PlayStatus.Play;
        private PlayStatus _status = PlayStatus.Play;
        /// <summary>
        /// The current status of the line chart.
        /// </summary>
        public PlayStatus Status
        {
            get { return _hold ? PlayStatus.Play : _status; }
            set
            {
                _previouStatus = _status;
                _status = value;
            }
        }
        /// <summary>
        /// Any value set to this will override the default velocity of the line chart.
        /// if set to null, 1 will be used.
        /// </summary>
        public float? OverrideVelocity = null;

        /// <summary>
        /// Multiplier for the velocity while dragging.
        /// </summary>
        public float DragMultiplier = 2;
        /// <summary>
        /// Velocity while dragging.
        /// </summary>
        [HideInInspector] public float DragVelocity { get; private set; }

        /// <summary>
        /// Displayed time span in seconds.
        /// </summary>
        public float Span = 60;
        private Vector3 _startMousePos;
        private float _mouseStartTime;

        private float TimeMultiplier
        {
            get { return _hold ? DragVelocity : OverrideVelocity ?? 1; }
        }

        private DateTimeOffset LowestDate
        {
            get { return _chart.Attribute.OldestValue.Time; }
        }

        /// <summary>
        /// Highest possible value.
        /// </summary>
        public DateTimeOffset Max
        {
            get { return _chart.Attribute.LatestValue.Time; }
        }

        private LineChart _chart;

        // Use this for initialization
        void Awake()
        {
            _chart = GetComponent<LineChart>();
        }
        // Use this for initialization
        void Start()
        {
            _chart.XMax = Max;
            _chart.XMin = _chart.XMax.AddSeconds(-Span);
            _chart.RequestData();
        }

        // Update is called once per frame
        void Update()
        {
            if (_hold && EnableUserInput)
                DeterminDragVelocity();
            if (!EnableUserInput)
            {
                _hold = false;
                OverrideVelocity = null;
                Status = PlayStatus.Play;
            }
            switch (Status)
            {
                case PlayStatus.Play:
                    Play();
                    break;
                case PlayStatus.Paused:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            _chart.XMin = _chart.XMax.AddSeconds(-Span);
        }

        private void DeterminDragVelocity()
        {
            DragVelocity = DragMultiplier * (Input.mousePosition.x - _startMousePos.x);
        }

        /// <summary>
        /// Called every update while the status is set to play.
        /// </summary>
        private void Play()
        {
            var newMax = _chart.XMax.AddSeconds(Time.deltaTime * TimeMultiplier);
            var max = Max;
            if (max < newMax)
                newMax = max;
            var minMax = LowestDate.AddSeconds(Span);
            if (newMax < minMax)
                newMax = minMax;
            _chart.XMax = newMax;
        }

        /// <summary>
        /// Used to toggle between pause and play.
        /// </summary>
        public void TogglePlay()
        {
            Status = _status != PlayStatus.Paused ? PlayStatus.Paused : _previouStatus;
        }

        void OnMouseDown()
        {
            _hold = true;
            _startMousePos = Input.mousePosition;
            _mouseStartTime = Time.time;
        }

        void OnMouseUp()
        {
            _hold = false;
            if (Time.time - _mouseStartTime <= 0.2f)
                TogglePlay();
        }
    }

}
