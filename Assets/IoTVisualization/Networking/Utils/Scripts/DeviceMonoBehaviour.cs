using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IoTVisualization.Networking;

namespace IoTVisualization.Networking.Utils
{
    /// <summary>
    /// Monobehaviour which has a reference to a device.
    /// </summary>
    public class DeviceMonoBehaviour : ProviderMonoBehaviour
    {
        /// <summary>
        /// Device that reference is saved here.
        /// </summary>
        public IDevice Device { get; set; }
    }
}
