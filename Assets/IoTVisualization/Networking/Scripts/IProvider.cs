using System;
using System.Collections.Generic;

namespace IoTVisualization.Networking
{
    /// <summary>
    /// This interface describes an object which provides a connection to a certain IoT-network.
    /// </summary>
    public interface IProvider
    {
        /// <summary>
        /// Public variable to apply a filter. There is no defined way on how to use this filter variable. The default implementation ignores this Variable.
        /// 
        /// The Energy Router Provider uses this variable as a path. Example: filter: "/Room112", Device "/Room112/lamp" accepted, Device "/Room113/lamp" declined
        /// </summary>
        string Filter { get; set; }
        /// <summary>
        /// Address including the port on which the provider connects to.
        /// </summary>
        string Address { get; set; }
        /// <summary>
        /// Establishes a connection to the specified address.
        /// </summary>
        void Connect();
        /// <summary>
        /// Disconnects the provider from the IoT-network.
        /// </summary>
        void Disconnect();
        /// <summary>
        /// Indicates whether the provider is currently connected.
        /// </summary>
        bool IsConnected { get; }
        /// <summary>
        /// Event called when the provider established a connection.
        /// </summary>
        event Action Connected;
        /// <summary>
        /// Event called when the provider disconnects.
        /// </summary>
        event Action Disconnected;
        /// <summary>
        /// Event which will be called when a new device has entered the Iot-network.
        /// </summary>
        event Action<IDevice> DataSourceAdded;
        /// <summary>
        /// Event which will be called when a device has been removed from the IoT-network.
        /// </summary>
        event Action<IDevice> DataSourceRemoved;

        /// <summary>
        /// A list of all devices in the current IoT-network.
        /// </summary>
        List<IDevice> Devices { get; }

        /// <summary>
        /// Used for requesting data to a specific device during a specific period of time.
        /// </summary>
        /// <param name="device">Device</param>
        /// <param name="attribute">Attribute which has been requested</param>
        /// <param name="from">Start time</param>
        /// <param name="to">End time</param>
        void RequestData(IDevice device, IAttribute attribute, DateTimeOffset from, DateTimeOffset to);
        
        /// <summary>
        /// Used for requesting the position of a device.
        /// </summary>
        /// <param name="device">Device</param>
        void RequestPosition(IDevice device);

        /// <summary>
        /// Used for saving the position of a device.
        /// </summary>
        /// <param name="device">Device</param>
        /// <param name="positionData">Data which describes the position of the object.</param>
        void SavePosition(IDevice device, byte[] positionData);

        /// <summary>
        /// Used to clear the position of a device.
        /// </summary>
        /// <param name="device">Device</param>
        void RemovePosition(IDevice device);

        /// <summary>
        /// Used to request a constant stream of data from a device.
        /// </summary>
        /// <param name="device">Device</param>
        void Subscribe(IDevice device);

        /// <summary>
        /// Used to stop the stream of data from a device.
        /// </summary>
        /// <param name="device">Device</param>
        void Unsubscribe(IDevice device);
    }
}
