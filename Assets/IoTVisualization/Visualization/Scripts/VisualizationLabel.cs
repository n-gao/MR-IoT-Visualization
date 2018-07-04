using System.Reflection.Emit;
using HoloToolkit.Unity;
using IoTVisualization.Utils;
using IoTVisualization.Visualization;
using UnityEngine;

namespace IoTVisualization.Visualization
{
    /// <summary>
    /// This component manages the label of a visualization.
    /// </summary>
    public class VisualizationLabel : MonoBehaviour
    {
        [SerializeField] private TextMesh _textMesh;
        public int MaxCharacters = 30;
        private IVisualization _visualization;
        /// <summary>
        /// Displayed name.
        /// </summary>
        public string Text
        {
            get { return _textMesh.text; }
            set
            {
                _textMesh.text = value;
            }
        }

        // Use this for initialization
        void Start ()
        {
            if (_textMesh == null)
                _textMesh = GetComponent<TextMesh>();
            if (_visualization == null)
                _visualization = GetComponentInParent<IVisualization>();
            Text = _visualization.Attribute.DisplayName.TruncateWithEllipsis(MaxCharacters);
        }
    }
}
