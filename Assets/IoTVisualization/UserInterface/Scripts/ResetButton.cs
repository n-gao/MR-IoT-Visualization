using IoTVisualization.Localization;
using IoTVisualization.Networking;
using IoTVisualization.Networking.Utils;

namespace IoTVisualization.UserInterface
{
    /// <summary>
    /// This component contains a method to reset all devices.
    /// </summary>
    public class ResetButton : ProviderMonoBehaviour {

        /// <summary>
        /// Resets all devices by calling DeviceObjectManager.RemoveAll();
        /// </summary>
        public void ResetAllVisualizations()
        {
            DeviceObjectManager.Instance.RemoveAll();
        }
    }
}
