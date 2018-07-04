using IoTVisualization.Visualization;
using UnityEngine;

namespace IoTVisualization.Localization
{
    /// <summary>
    /// This class contains a few methods related to Localization which can be used as voice commands.
    /// </summary>
    public class LocalizationCommands : MonoBehaviour {

        /// <summary>
        /// If the first GameObject in sight is a visualization it will be removed and its position delted from the IoT-network.
        /// </summary>
        public void Remove()
        {
            print("[VoiceCommand]Remove");
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                DeviceWrapper device = hit.transform.GetComponentInParent<DeviceWrapper>();

                if (device == null) return;
                
                DeviceObjectManager.Instance.Remove(device.Device);
            }
        }
    }
}
