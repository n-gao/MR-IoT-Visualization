using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
#if WINDOWS_UWP
using System.Threading.Tasks;
#endif

namespace IoTVisualization.Visualization
{
    /// <summary>
    /// This component controls the LineRenderer in order to draw the line of a line chart.
    /// </summary>
    [RequireComponent(typeof(LineChartLineRenderer))]
    public class LineChartLine : MonoBehaviour
    {
        private LineChart _lineChart;
        /// <summary>
        /// List of local points to be used to render.
        /// </summary>
        public readonly List<Vector2> Points = new List<Vector2>();
        
        private readonly List<Vector2> _tmpPoints = new List<Vector2>();

        private bool EnoughItems
        {
            get { return _lineChart.Values.Count >= 2; }
        }

        private LineChartLineRenderer _myRenderer;
        private LineRenderer _unityRenderer;
        
        private readonly List<LineChart.Data> _dataCpy = new List<LineChart.Data>();


        private readonly EventWaitHandle _waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);


        private void CalculationLoop()
        {
            while (true)
            {
                CalculatePositions();
                _waitHandle.WaitOne();
            }
        }

#if WINDOWS_UWP
        private CancellationTokenSource _token;
        
        private void StartThread()
        {
            _token = new CancellationTokenSource();
            Task.Factory.StartNew(CalculationLoop, _token.Token, TaskCreationOptions.LongRunning | TaskCreationOptions.RunContinuationsAsynchronously, TaskScheduler.Default);
        }

        private void OnDestroy()
        {
            _token.Cancel();
        }
#else
        private Thread _thread;
        private void StartThread()
        {
            if (_thread == null)
            {
                _thread = new Thread(CalculationLoop);
                _thread.Start();
            }
        }

        void OnDestroy()
        {
            _thread.Abort();
        }
#endif

        // Use this for initialization
        void Start ()
        {
            _lineChart = GetComponentInParent<LineChart>();
            _myRenderer = GetComponent<LineChartLineRenderer>();
            _unityRenderer = GetComponent<LineRenderer>();
            if (_lineChart == null || _myRenderer == null)
            {
                enabled = false;
                return;
            }
            if (_myRenderer != null)
                _myRenderer.Points = Points;
            StartThread();
        }
	
        // Update is called once per frame
        void Update ()
        {
            if (!_lineChart.ShouldDraw) return;
//            CalculatePositions();
            //This code was used for creating comparison screenshots between the self developed LineRenderer and Unitys built in.
            if (_unityRenderer != null && _unityRenderer.enabled)
            {
                lock (Points)
                {
                    _unityRenderer.positionCount = Points.Count;
                    for (int i = 0; i < Points.Count; i++)
                        _unityRenderer.SetPosition(i, Points[i]);
                }
                _myRenderer.enabled = false;
            }
            else
            {
                _myRenderer.enabled = true;
            }
        }

        void LateUpdate()
        {
            if (_lineChart.ShouldDraw)
                _waitHandle.Set();
        }

        private DateTimeOffset _xMin;
        private DateTimeOffset _xMax;
        private float _min;
        private float _max;

        private void CalculatePositions()
        {
            _xMin = _lineChart.XMin;
            _xMax = _lineChart.XMax;
            _min = _lineChart.MinValue;
            _max = _lineChart.MaxValue;

            _dataCpy.Clear();
            lock (_lineChart.Values)
                _dataCpy.AddRange(_lineChart.Values);

            _tmpPoints.Clear();

            if (!EnoughItems) return;

            var data = _dataCpy;
            int offset = 0;
            while (offset < data.Count && data[offset].Time <= _xMin)
                offset++;

            if (offset == data.Count)
                return;

            LineChart.Data lastRemoved = data[offset - 1 < 0 ? 0 : offset - 1];

            //Smooth left
            _tmpPoints.Add(GetLineChartPosition(_xMin, Interpolate(lastRemoved, data[offset], _xMin)));
            //Values
            for (; offset < data.Count; offset++)
            {
                LineChart.Data current = data[offset];
                if (current.Time >= _xMax)
                    break;
                _tmpPoints.Add(GetLineChartPosition(data[offset]));
            }
            //Smooth right
            LineChart.Data secondLast = data[offset - 1];
            LineChart.Data last = data[offset == data.Count ? offset - 1 : offset];
            _tmpPoints.Add(GetLineChartPosition(_xMax, Interpolate(secondLast, last, _xMax)));

            lock (Points)
            {
                Points.Clear();
                Points.AddRange(_tmpPoints);
            }
        }

        /// <summary>
        /// Determines the 3D position of an entry.
        /// </summary>
        /// <param name="time">Timestamp of the entry</param>
        /// <param name="value">Value of the entry</param>
        /// <returns>3D LineRenderer position</returns>
        private Vector2 GetLineChartPosition(DateTimeOffset time, float value)
        {
            var timeSpan = _xMax.Ticks - _xMin.Ticks;
            var span = (float)(time.Ticks - _xMin.Ticks);
            Vector2 pos = new Vector2
            {
                x = span / timeSpan - 0.5f,
                y = (_max == _min) ? 0 : (value - _min) / (_max - _min) - 0.5f,
            };
            return pos;
        }

        /// <summary>
        /// Overloads GetLineChartPosition in order to take a single item.
        /// </summary>
        /// <param name="data">LineChart.Data</param>
        /// <returns>Position used for drawing the line</returns>
        private Vector2 GetLineChartPosition(LineChart.Data data)
        {
            return GetLineChartPosition(data.Time, data.FloatValue);
        }

        /// <summary>
        /// Used to interpolate between two given entries at a given time.
        /// </summary>
        /// <param name="first">First entry</param>
        /// <param name="second">Second entry</param>
        /// <param name="time">Time</param>
        /// <returns>Interpolated entry</returns>
        public static float Interpolate(LineChart.Data first, LineChart.Data second, DateTimeOffset time)
        {
            return first.FloatValue + (second.FloatValue - first.FloatValue) *
                                      (time.Ticks - first.Time.Ticks) /
                                      (second.Time.Ticks - first.Time.Ticks);
        }
    }
}
