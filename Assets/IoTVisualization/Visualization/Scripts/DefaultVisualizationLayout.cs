using System;

namespace IoTVisualization.Visualization
{
    /// <summary>
    /// Default implementation of the IVisualizationLayout interface. At default all margins are 0 and there are no linebreaks.
    /// </summary>
    public class DefaultVisualizationLayout : IVisualizationLayout
    {
        public event Action LayoutChanged;
        public float[] Margins { get { return new[] {0, 0, 0, 0.0f}; } }
        public float TopMargin { get { return 0; } }
        public float RightMargin { get { return 0; } }
        public float BottomMargin { get { return 0; } }
        public float LeftMargin { get { return 0; } }
        public bool LineBreakBefore { get { return false; } }
        public bool LineBreakAfter { get { return false; } }
        private int _orderPriority;

        public int Priority
        {
            get { return _orderPriority; }
            set
            {
                _orderPriority = value;
                if (LayoutChanged != null)
                    LayoutChanged();
            }
        }
    }
}
