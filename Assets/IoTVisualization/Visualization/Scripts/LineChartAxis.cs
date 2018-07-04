using System;
using IoTVisualization.Visualization;
using UnityEngine;

namespace IoTVisualization.Visualization
{
    /// <summary>
    /// This component manages 4 labels which should be child objects with the names Lower, Upper, Left and Right
    /// or set before Start has been called. All label objects should also have the LineChartAxisLabel component.
    /// 
    /// The label Upper will be set to the highest displayed value, the lower label will be set to the lowest displayed
    /// value, the left label will be set to the lowest displayed date and the right label will be set to the highest
    /// displayed date.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class LineChartAxis : MonoBehaviour
    {
        [SerializeField] private LineChart _lineChart;
        [SerializeField] private LineChartAxisLabel _lower;
        [SerializeField] private LineChartAxisLabel _upper;
        [SerializeField] private LineChartAxisLabel _left;
        [SerializeField] private LineChartAxisLabel _right;
        [SerializeField] private Color _color;

        public Color Color
        {
            get { return _color; }
            set
            {
                _color = value;
                _lineRenderer.material.color = _color;
            }
        }

        private LineRenderer _lineRenderer;

        // Use this for initialization
        void Start ()
        {
            if (_lower != null)
                _lower = transform.Find("Lower").GetComponent<LineChartAxisLabel>();
            if (_upper != null)
                _upper = transform.Find("Upper").GetComponent<LineChartAxisLabel>();
            if (_left != null)
                _left = transform.Find("Left").GetComponent<LineChartAxisLabel>();
            if (_right != null)
                _right = transform.Find("Right").GetComponent<LineChartAxisLabel>();
            if (_lineChart != null)
                _lineChart = GetComponentInParent<LineChart>();
            _lineRenderer = GetComponent<LineRenderer>();
            _lineChart.MinValueChanged += f => _lower.Label = f.ToString("F");
            _lineChart.MaxValueChanged += f => _upper.Label = f.ToString("F");
            Color = _color;
        }

        private string _leftText;
        private string _rightText;
        void Update()
        {
            if (!_lineChart.ShouldDraw) return;
            string newLeft = FormatDateTime(_lineChart.XMin.LocalDateTime);
            string newRight = FormatDateTime(_lineChart.XMax.LocalDateTime);
            if (newLeft != _leftText)
                _leftText = _left.Label = newLeft;
            if (newRight != _rightText)
                _rightText = _right.Label = newRight;
        }

        private static readonly char[] CharBuffer = new char[8];

        private static string FormatDateTime(DateTime dt)
        {
            Write2Chars(CharBuffer, 0, dt.Hour);
            CharBuffer[2] = ':';
            Write2Chars(CharBuffer, 3, dt.Minute);
            CharBuffer[5] = ':';
            Write2Chars(CharBuffer, 6, dt.Second);
            return new string(CharBuffer);
        }

        private static void Write2Chars(char[] chars, int offset, int value)
        {
            chars[offset] = Digit(value / 10);
            chars[offset + 1] = Digit(value % 10);
        }

        private static char Digit(int value)
        {
            return (char)(value + '0');
        }
    }
}
