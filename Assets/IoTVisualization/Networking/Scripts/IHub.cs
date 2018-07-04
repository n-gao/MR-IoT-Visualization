using System.Collections.Generic;

namespace IoTVisualization.Networking
{
    /// <summary>
    /// This interface represenets a hub for information in an IoT-enviroment. It can be a real device or just some virtual organization structure.
    /// </summary>
    public interface IHub
    {
        /// <summary>
        /// Name of the hub.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Parent of the hub.
        /// </summary>
        IHub Parent { get; }
        /// <summary>
        /// All childs.
        /// </summary>
        IEnumerable<IHub> Childs { get; }
    }
}
