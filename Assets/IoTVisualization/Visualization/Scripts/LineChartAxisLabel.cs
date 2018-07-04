using HoloToolkit.Unity;
using UnityEngine;

namespace IoTVisualization.Visualization
{
    /// <summary>
    /// This component is used to enable better scaling for the label of a line chart.
    /// </summary>
    [RequireComponent(typeof(TextMesh))]
    public class LineChartAxisLabel : MonoBehaviour
    {
        /// <summary>
        /// Text fo the label.
        /// </summary>
        public string Label
        {
            get { return _textMesh.text; }
            set { _textMesh.text = value; }
        }

        private TextMesh _textMesh;

        // Use this for initialization
        void Start ()
        {
            if (_textMesh == null)
                _textMesh = GetComponent<TextMesh>();
        }
    }
}
