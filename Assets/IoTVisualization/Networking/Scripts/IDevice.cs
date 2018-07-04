using System;
using System.Collections.Generic;

namespace IoTVisualization.Networking
{
    /// <summary>
    /// This interface represents a device in a IoT-enviroment.
    /// </summary>
    public interface IDevice : IHub
    {
        /// <summary>
        /// Event called whenever any metadata has changed.
        /// </summary>
        event Action<string, string> MetaDataModified; 
        /// <summary>
        /// Event called whenever a value has been added.
        /// </summary>
        event Action<IoTData> ValueModified;
        /// <summary>
        /// Event called whenever the position information of this device is available.
        /// </summary>
        event Action<byte[]> PositionAvailable;
        
        /// <summary>
        /// Name of the device for display.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Metadata informations.
        /// </summary>
        Dictionary<string, string> MetaData { get; }

        /// <summary>
        /// List of all attributes of this device.
        /// </summary>
        IEnumerable<IAttribute> Atrributes { get; }

        /// <summary>
        /// Last added value.
        /// </summary>
        IoTData LatestValue { get; }
        /// <summary>
        /// First added value.
        /// </summary>
        IoTData OldestValue { get; }
        /// <summary>
        /// List of all values.
        /// </summary>
        IEnumerable<IoTData> Values { get; }
        /// <summary>
        /// Indicated whether position informations are available.
        /// </summary>
        bool HasPosition { get; }
        /// <summary>
        /// Position informations.
        /// </summary>
        byte[] SavedPosition { get; set; }
    }
}
