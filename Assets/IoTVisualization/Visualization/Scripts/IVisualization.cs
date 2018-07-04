using IoTVisualization.Networking;
using UnityEngine;

namespace IoTVisualization.Visualization
{
    /// <summary>
    /// This interface defines a visualization and all of its properties.
    /// </summary>
    public interface IVisualization
    {
        /// <summary>
        /// The provider which provided the device and its attribute.
        /// </summary>
        IProvider Provider { get; set; }
        /// <summary>
        /// The device which the visualized attribute belongs to.
        /// </summary>
        IDevice Device { get; set; }
        /// <summary>
        /// The attribute which should be visualized.
        /// </summary>
        IAttribute Attribute { get; set; }
        /// <summary>
        /// Indicates whether this visualization should be drawn.
        /// </summary>
        bool ShouldDraw { get; }
        /// <summary>
        /// The size of the visualization.
        /// </summary>
        Vector2 Size { get; set; }
        /// <summary>
        /// The boundaries of the visualization used to arrange the visualizations.
        /// </summary>
        Vector2 Bounds { get; }
        /// <summary>
        /// Layout informations about the visualization.
        /// </summary>
        IVisualizationLayout Layout { get; }
    }
}
