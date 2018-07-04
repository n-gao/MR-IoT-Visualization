using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IoTVisualization.Networking
{
    /// <summary>
    /// This interface represents an attribute of a device.
    /// </summary>
    public interface IAttribute
    {
        /// <summary>
        /// Event called everytime metadata has changed.
        /// </summary>
        event Action<string, string> MetaDataModified;
        /// <summary>
        /// Event called everytime a value has been added.
        /// </summary>
        event Action<IoTData> ValueModified;

        /// <summary>
        /// Shorter name for display.
        /// </summary>
        string DisplayName { get; }
        /// <summary>
        /// Full name of this attribute.
        /// </summary>
        string AttributeName { get; }

        /// <summary>
        /// Metadata
        /// </summary>
        Dictionary<string, string> MetaData { get; }
        /// <summary>
        /// Type of this attribute.
        /// </summary>
        string Type { get; }
        /// <summary>
        /// Content of this attribute.
        /// </summary>
        string ContentType { get; }
        /// <summary>
        /// Format of the data.
        /// </summary>
        string Format { get; }
        
        /// <summary>
        /// Newest Value.
        /// </summary>
        IoTData LatestValue { get; }
        /// <summary>
        /// Oldest value.
        /// </summary>
        IoTData OldestValue { get; }

        /// <summary>
        /// All data.
        /// </summary>
        IEnumerable<IoTData> Data { get; }
        /// <summary>
        /// Data in a given time span.
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="end">End</param>
        /// <returns>Data</returns>
        IoTData[] DataFromTo(DateTimeOffset start, DateTimeOffset end);

        /// <summary>
        /// Data in a given time span. This method also includes LatestValueBefore(start) and EarliestValueAfter(end).
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="end">End</param>
        /// <returns>Data</returns>
        IoTData[] DataFromToInclusive(DateTimeOffset start, DateTimeOffset end);

        /// <summary>
        /// Next later value before a given time. Always returns a value when there is a value before the given time.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        IoTData LatestValueBefore(DateTimeOffset time);

        /// <summary>
        /// Next earlier value before a given time. Always returns a value when there is a value after the given time.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        IoTData EarliestValueAfter(DateTimeOffset time);

        /// <summary>
        /// Importance of this Attribute, determines the position inside the visualization panel.
        /// </summary>
        int? VisualizationOrder { get; }

        /// <summary>
        /// Resets all stored data.
        /// </summary>
        void Reset();
    }
}
