using IoTVisualization.Networking;
using IoTVisualization.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IoTVisualization.Visualization
{
    /// <summary>
    /// This component can be attached to a line chart in order to aggregate its data into less samples.
    /// This should not be used when the timespan between each pair of data is bigger than the set time span.
    /// </summary>
    [RequireComponent(typeof(LineChart))]
    public class LineChartDataAggregator : MonoBehaviour
    {
        [SerializeField] private float _span = 1.0f;
        /// <summary>
        /// The number of seconds which will be aggregated into one sample.
        /// </summary>
        public float Span {
            get
            {
                return _span;
            }

            set {
                _span = value;
                _lineChart.ExtraSeconds = _span;
            }
        }
        
        private LineChart _lineChart;
        private readonly List<LineChart.Data> _newValues = new List<LineChart.Data>();
        private readonly List<LineChart.Data> _valTmp = new List<LineChart.Data>();
        private readonly List<IoTData> _tmp = new List<IoTData>();

        // Use this for initialization
        void Start ()
        {
            _lineChart = GetComponent<LineChart>();
            _lineChart.Values = _newValues;
            Span = Span;
        }

        void OnEnable()
        {
            if (_lineChart != null)
                _lineChart.Values = _newValues;
        }

        void OnDisable()
        {
            _lineChart.Values = null;
        }
	
        // Update is called once per frame
        void Update ()
        {
            if (!_lineChart.ShouldDraw) return;

            //Clearing old values
            _valTmp.Clear();
            if (!_lineChart.RawValues.Any() || Span <= 0) return;
            DateTimeOffset min = _lineChart.XMin.AddSeconds(-Span);
            long steps = (long)((min - DateTimeHelper.UtcMin).TotalSeconds / (double)Span);
            DateTimeOffset end = DateTimeHelper.UtcMin.AddSeconds(Span * steps);
            //Refilling values.
            for (int i = 0; i < _lineChart.RawValues.Length; i++)
            {
                var data = _lineChart.RawValues[i];
                while (data.Time >= end)
                {
                    end = end.AddSeconds(Span);
                    TempToValue();
                }
                _tmp.Add(data);
            }
            TempToValue();

            lock (_newValues)
            {
                _newValues.Clear();
                _newValues.AddRange(_valTmp);
            }
        }

        /// <summary>
        /// Converts the list of data into one sample.
        /// </summary>
        private void TempToValue()
        {
            if (!_tmp.Any()) return;

            float valueAvg = 0;
            double dateTimeAvg = 0;
            int count = _tmp.Count;
            for (int i = 0; i < _tmp.Count; i++)
            {
                var data = _tmp[i];
                valueAvg += data.FloatValue / count;
                dateTimeAvg += data.Time.UtcTicks / (double) count;
            }
            DateTimeOffset time = DateTimeOffset.MinValue.AddTicks((long)dateTimeAvg);

            _valTmp.Add(new LineChart.Data
            {
                Time = time,
                FloatValue = valueAvg
            });
            _tmp.Clear();
        }
    }
}
