using System;
using UnityEngine;

namespace IoTVisualization.Visualization
{
    /// <summary>
    /// Component implementation of IVisualizationLayout. Provides an easy way to change layout parameters
    /// for a visualization.
    /// </summary>
    [RequireComponent(typeof(Visualization))]
    public class VisualizationLayout : MonoBehaviour, IVisualizationLayout {
        /// <summary>
        /// Called everytime a parameter has been changed.
        /// </summary>
        public event Action LayoutChanged;
        /// <summary>
        /// Number of margins.
        /// </summary>
        private const int MarginCount = 4;
        [SerializeField]
        private float[] _margins = {0f, -1.0f, -1.0f, -1.0f};

        public float[] Margins
        {
            get { return _margins; }
        }

        public float TopMargin
        {
            get
            {
                return _margins[0];
            }
            set
            {
                _margins[0] = value;
                CallLayoutChanged();
            }
        }

        public float RightMargin
        {
            get
            {
                return _margins[1] < 0 ? TopMargin : _margins[1];
            }
            set
            {
                _margins[1] = value;
                CallLayoutChanged();
            }
        }

        public float BottomMargin
        {
            get
            {
                return _margins[2] < 0 ? TopMargin : _margins[2];
            }
            set
            {
                _margins[2] = value;
                CallLayoutChanged();
            }
        }

        public float LeftMargin
        {
            get
            {
                return _margins[3] < 0 ? RightMargin : _margins[3];
            }
            set
            {
                _margins[3] = value;
                CallLayoutChanged();
            }
        }

        public bool LineBreakBefore
        {
            get
            {
                return _lineBreakBefore;
            }
            set
            {
                _lineBreakBefore = value;
                CallLayoutChanged();
            }
        }

        public bool LineBreakAfter
        {
            get
            {
                return _lineBreakAfter;
            }
            set
            {
                _lineBreakAfter = value;
                CallLayoutChanged();
            }
        }

        [SerializeField]
        private bool _lineBreakBefore = false;
        [SerializeField]
        private bool _lineBreakAfter = false;

        public int Priority
        {
            get { return _priority; }
            set { _priority = value; CallLayoutChanged(); }
        }

        [SerializeField] private int _priority;

        private void CallLayoutChanged()
        {
            if (LayoutChanged != null)
                LayoutChanged();
        }

        void OnValidate()
        {
            if (Margins.Length != MarginCount)
                Array.Resize(ref _margins, MarginCount);
            if (TopMargin < 0)
                _margins[0] = 0;
        }
    }
}
