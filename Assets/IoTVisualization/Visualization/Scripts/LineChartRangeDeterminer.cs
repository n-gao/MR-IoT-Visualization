using System.Linq;
using UnityEngine;

namespace IoTVisualization.Visualization
{
    /// <summary>
    /// This component manages the maximum and minimum values of a line chart based on their highest and lowest values.
    /// Instead of instantly adjusting to the corresponding values this component interpolates in order to create a
    /// smoother transition.
    /// </summary>
    [RequireComponent(typeof(LineChart))]
    public class LineChartRangeDeterminer : MonoBehaviour
    {
        /// <summary>
        /// The velocity with which the maximum and minimum values adjust.
        /// If set to a value smaller than 0, all changes will happen instantly.
        /// </summary>
        public float Velocity = 10;
        /// <summary>
        /// If this is set to true 0 will always be included between the minimum and maximum.
        /// </summary>
        public bool DisplayZero = false;
        private LineChart _lineChart;
        private float _oldMax = 1;
        private float _oldMin = 0;

        private bool _first = true;

        // Use this for initialization
        void Start()
        {
            _lineChart = GetComponent<LineChart>();
            _lineChart.UseConstantYAxis = true;
        }

        // Update is called once per frame
        void Update ()
        {
            if (!_lineChart.ShouldDraw || _lineChart.Values.Count == 0) return;

            float max = float.MinValue;
            float min = float.MaxValue;
            var values = _lineChart.Values;
            for (int i = 0; i < values.Count; i++)
            {
                float value = values[i].FloatValue;
                if (value > max)
                    max = value;
                if (value < min)
                    min = value;
            }
            if (DisplayZero)
            {
                if (max < 0)
                    max = 0;
                if (min > 0 && max > 0)
                    min = 0;
            }
            if (_first)
            {
                _oldMin = min;
                _oldMax = max;
                _first = false;
            }
            _oldMax = _lineChart.YMax = Velocity >= 0 ? _oldMax + (max - _oldMax) * Time.deltaTime * Velocity : max;
            _oldMin = _lineChart.YMin = Velocity >= 0 ? _oldMin + (min - _oldMin) * Time.deltaTime * Velocity : min;
        }
    }
}
