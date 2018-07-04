using System;
using HoloToolkit.Unity.InputModule;
using UnityEngine;
using UnityEngine.VR.WSA.Input;

namespace IoTVisualization.Visualization
{
    /// <summary>
    /// This component controlls the timespan which is used as the x axis of a line chart.
    /// It enables auto play, pausing and custom speeds.
    /// </summary>
    [RequireComponent(typeof(LineChart))]
    public class LineChartController : MonoBehaviour, IFocusable, IInputClickHandler, IInputHandler
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
            Custom  //Not used, but could be for something
        }

        /// <summary>
        /// Indicates whether the user is currently holding a tap on the line chart.
        /// </summary>
        private bool _hold = false;
        /// <summary>
        /// Indicates whether the user is currently looking at the line chart.
        /// </summary>
        private bool _isFocused = false;
        private PlayStatus _previouStatus = PlayStatus.Play;
        private PlayStatus _status = PlayStatus.Play;
        /// <summary>
        /// The current status of the line chart.
        /// </summary>
        public PlayStatus Status
        {
            get { return _hold? PlayStatus.Play : _status; }
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
        [HideInInspector]public float DragVelocity {get; private set;}

        /// <summary>
        /// Displayed time span in seconds.
        /// </summary>
        public float Span = 60;

        private LineChart _chart;

        private uint _sourceId;

        private Vector3 _startHandPosition;
        
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

        // Use this for initialization
        void Awake ()
        {
            _chart = GetComponent<LineChart>();
        }

        void Start()
        {
            _chart.XMax = Max;
            _chart.XMin = _chart.XMax.AddSeconds(-Span);
            InteractionManager.SourcePressed += InteractionManagerOnSourcePressed;
            InteractionManager.SourceUpdated += InteractionManagerOnSourceUpdated;
            InteractionManager.SourceReleased += InteractionManagerOnSourceReleased;
            InteractionManager.SourceLost += InteractionManagerOnSourceReleased;
            _chart.RequestData();
        }

        //Called when the user stopps holding his tap.
        private void InteractionManagerOnSourceReleased(InteractionSourceState state)
        {
            _hold = false;
            DragVelocity = 0;
        }

        //Called when every update while the user is holding his tap.
        private void InteractionManagerOnSourceUpdated(InteractionSourceState state)
        {
            if (state.source.id != _sourceId) return;
            Vector3 newPos;
            if (!state.properties.location.TryGetPosition(out newPos)) return;

            Vector3 right = Camera.main.transform.right;
            Vector3 projection = Vector3.Project(newPos - _startHandPosition, right);
            float length = projection.magnitude;
            if ((projection + right).magnitude > right.magnitude)
                DragVelocity = length * 100 * DragMultiplier;
            else
                DragVelocity = length * -100 * DragMultiplier;
        }

        //Called when the user taps
        private void InteractionManagerOnSourcePressed(InteractionSourceState state)
        {
            if (!_isFocused) return;
            print(state.source.kind);
            if (state.source.kind == InteractionSourceKind.Hand)
            {
                _sourceId = state.source.id;
                _hold = true;
                DragVelocity = 0;
                state.properties.location.TryGetPosition(out _startHandPosition);
            }
        }

        private float _lastRotation;

        // Update is called once per frame
        void Update ()
        {
            if (EnableUserInput)
            {
#if UNITY_EDITOR
                DeterminDragVelocity();
#endif
            }
            else
            {
                Status = PlayStatus.Play;
                _hold = false;
                OverrideVelocity = null;
            }
            switch (Status)
            {
                case PlayStatus.Play:
                    Play();
                    break;
                case PlayStatus.Paused:
                    break;
                case PlayStatus.Custom:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            _chart.XMin = _chart.XMax.AddSeconds(-Span);
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
        /// Determines the velocity while dragging. Only used in editor.
        /// </summary>
        private void DeterminDragVelocity()
        {
            if (_hold)
            {
                float rotation = Camera.main.transform.eulerAngles.y;
                DragVelocity += (Mathf.Abs(rotation - _lastRotation) > 180 ? 360 - rotation - _lastRotation : rotation - _lastRotation) * DragMultiplier;
                _lastRotation = rotation;
            }
        }

        /// <summary>
        /// Stops the line chart.
        /// </summary>
        public void Stop()
        {
            if (!_isFocused) return;
            Status = PlayStatus.Paused;
        }

        /// <summary>
        /// Continues to play the line chart.
        /// </summary>
        public void Continue()
        {
            if (!_isFocused) return;
            Status = PlayStatus.Play;
        }

        /// <summary>
        /// Sets minimum and maximum datetime of the line chart.
        /// </summary>
        /// <param name="start">Start time</param>
        /// <param name="stop">End time</param>
        public void SetTimeSpan(DateTime start, DateTime stop)
        {
            _chart.XMin = start;
            _chart.XMax = stop;
        }

        public void OnFocusEnter()
        {
            _isFocused = true;
        }

        public void OnFocusExit()
        {
            _isFocused = false;
        }

        /// <summary>
        /// Used to toggle between pause and play.
        /// </summary>
        public void TogglePlay()
        {
            Status = _status != PlayStatus.Paused ? PlayStatus.Paused : _previouStatus;
        }
    
        public void OnInputClicked(InputClickedEventData eventData)
        {
            TogglePlay();
        }

        public void OnInputUp(InputEventData eventData)
        {
            _hold = false;
            InputManager.Instance.OverrideFocusedObject = null;
        }

        public void OnInputDown(InputEventData eventData)
        {
            _hold = true;
            DragVelocity = 0;
            _lastRotation = Camera.main.transform.eulerAngles.y;
            InputManager.Instance.OverrideFocusedObject = gameObject;
        }
    }
}
