using System;
using System.Collections.Generic;
using System.Linq;
using IoTVisualization.Networking;
using IoTVisualization.Utils;
using UnityEngine;

namespace IoTVisualization.Visualization
{
    /// <summary>
    /// This component is used as the main component for all other LineChart components.
    /// It queries all required data from the set attribute.
    /// </summary>
    public class LineChart : Visualization
    {
        /// <summary>
        /// Struct used for visualizing the line chart.
        /// It is used instead of IoTData because of a reduced memory allociation.
        /// </summary>
        [Serializable]
        public struct Data
        {
            /// <summary>
            /// Timestamp of the data.
            /// </summary>
            public DateTimeOffset Time;
            /// <summary>
            /// Value of the data.
            /// </summary>
            public float FloatValue;
            /// <summary>
            /// Converts an IoTData entry to a LineChart.Data entry.
            /// </summary>
            /// <param name="data">Data</param>
            /// <returns>LineChart.Data</returns>
            public static Data FromIoTData(IoTData data)
            {
                return new Data{FloatValue = data.FloatValue, Time = data.Time};
            }

            public override string ToString()
            {
                return Time + " " + FloatValue.ToString("N");
            }
        }

        /// <summary>
        /// This event will be called when the maximum displayed value has changed.
        /// </summary>
        public event Action<float> MinValueChanged;
        /// <summary>
        /// This event will be called when the minimum displayed value has changed.
        /// </summary>
        public event Action<float> MaxValueChanged;

        /// <summary>
        /// Raw values which have been returned by the query.
        /// This data can be used to overrite Values which is used for visualization.
        /// </summary>
//        public List<IoTData> RawValues = new List<IoTData>();
        public IoTData[] RawValues = new IoTData[0];
        private List<Data> _values = null;
        private readonly List<Data> _defaultValues = new List<Data>();
        /// <summary>
        /// Values which are used for visualization. Can be overrite by another list.
        /// By default it returns RawValues parsed to LineChart.Data.
        /// </summary>
        public List<Data> Values
        {
            get { return _values ?? _defaultValues; }
            set { _values = value; }
        }

        /// <summary>
        /// If set to true the y axis will used the set constant values.
        /// </summary>
        public bool UseConstantYAxis = false; 
        /// <summary>
        /// Maximum value of the y axis. Only used when UseConstantYAxis is set to true.
        /// </summary>
        public float YMin = 0;
        /// <summary>
        /// Minimum value of the y axis. Only used when UseConstantYAxis is set to true.
        /// </summary>
        public float YMax = 1;
        /// <summary>
        /// Seconds which will be substracted from the x minimum.
        /// If the value is smaller than 0 on entry before the minimum will be added.
        /// </summary>
        public float ExtraSeconds = 0.0f;
        /// <summary>
        /// Minimum value of the x axis.
        /// </summary>
        public DateTimeOffset XMin { get; set; }
        /// <summary>
        /// Maximum value of the x axis.
        /// </summary>
        public DateTimeOffset XMax { get; set; }
        /// <summary>
        /// Maximum value.
        /// If UseConstantYAxis is set to true YMax will be returned.
        /// </summary>
        /// <returns>Maximum value.</returns>
        public float MaxValue { get; private set; }
        /// <summary>
        /// Minimum value.
        /// If UseConstantYAxis is set to true YMin will be returned.
        /// </summary>
        /// <returns>Minimum value</returns>
        public float MinValue { get; private set; }
        
        private LineChartLine _line;
        private LineRenderer _axisLineRenderer;

        // Use this for initialization
        protected override void Start()
        {
            base.Start();
            _line = GetComponentInChildren<LineChartLine>();
            _axisLineRenderer = transform.Find("Axis").GetComponent<LineRenderer>();
            _axisLineRenderer.positionCount = 3;
            AdjustSize();
            MinValue = float.MinValue;
            MaxValue = float.MaxValue;
        }

        void OnDestroy()
        {
            Attribute.Reset();
        }
        

        void Update()
        {
            if (!ShouldDraw) return;
            RawValues = Attribute.DataFromToInclusive(XMin.AddSeconds(-ExtraSeconds), XMax);
            if (_values == null)
            {
                lock (_defaultValues)
                {
                    _defaultValues.Clear();
                    for (int i = 0; i < RawValues.Length; i++)
                        _defaultValues.Add(RawValues[i].ToLineChartData());
                }
            }
        }
        
        void LateUpdate()
        {
            if (!ShouldDraw) return;

            float oldMin = MinValue;
            float oldMax = MaxValue;
            MinValue = UseConstantYAxis ? YMin : Values.Min(v => v.FloatValue);
            MaxValue = UseConstantYAxis ? YMax : Values.Max(v => v.FloatValue);
            if (MinValue != oldMin && MinValueChanged != null)
                MinValueChanged(MinValue);
            if (MaxValue != oldMax && MaxValueChanged != null)
                MaxValueChanged(MaxValue);
        }

        protected override void AdjustSize()
        {
            //Setting Size
            base.AdjustSize();
            //Setting width of the lines
            if (_axisLineRenderer != null)
                _axisLineRenderer.widthMultiplier = Mathf.Max(Size.y, Size.x) / 100;
        }

        protected override void OnValidate()
        {
            if (_line == null)
                _line = GetComponentInChildren<LineChartLine>();
            _axisLineRenderer = transform.GetChild(0).GetComponent<LineRenderer>();
            base.OnValidate();
        }

        /// <summary>
        /// Requests the data during the set period.
        /// </summary>
        public void RequestData()
        {
            Provider.RequestData(Device, Attribute, XMin, XMax);
        }
    }

    /// <summary>
    /// Util class which has an extension method for converting IoTData to LineChart.Data.
    /// </summary>
    static class LineChartDataExtension
    {
        /// <summary>
        /// Extension method which converts IoTData to LineChart.Data.
        /// </summary>
        /// <param name="data">Data</param>
        /// <returns>LineChart.Data</returns>
        public static LineChart.Data ToLineChartData(this IoTData data)
        {
            return LineChart.Data.FromIoTData(data);
        }
    }
}
