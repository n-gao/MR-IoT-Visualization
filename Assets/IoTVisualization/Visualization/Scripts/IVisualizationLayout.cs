using System;

namespace IoTVisualization.Visualization
{
    /// <summary>
    /// This interface defines a few extra layout informations about a visualization in order 
    /// to customize the look of a visualization.
    /// </summary>
    public interface IVisualizationLayout
    {
        /// <summary>
        /// Event called when any property has changed.
        /// </summary>
        event Action LayoutChanged;
        /// <summary>
        /// Array containing all margin values.
        /// 0 - top
        /// 1 - right
        /// 2 - bottom
        /// 3 - left
        /// </summary>
        float[] Margins { get; }
        float TopMargin { get; }
        float RightMargin { get; }
        float BottomMargin { get; }
        float LeftMargin { get; }

        /// <summary>
        /// If set tot true a new row will be started before this visualization will be added.
        /// </summary>
        bool LineBreakBefore { get; }
        /// <summary>
        /// If set to true a new row will be started after this visualization will be added.
        /// </summary>
        bool LineBreakAfter { get; }

        /// <summary>
        /// Used to create a well defined order between the visualizations.
        /// higher priorities will lead to a higher position inside the panel.
        /// </summary>
        int Priority { get; set; }
    }
}
