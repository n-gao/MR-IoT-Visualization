using System.Collections;
using System.Collections.Generic;
using IoTVisualization.Localization;
using IoTVisualization.Networking;
using IoTVisualization.Networking.Utils;
using IoTVisualization.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace IoTVisualization.Desktop
{
    /// <summary>
    /// This component contains the method to select the currently displayed device. The Select method should be called
    /// from a button.
    /// </summary>
    public class UiDeviceButton : DeviceMonoBehaviour
    {
        private static GameObject _current;
        private static IDevice _displayedDevice;

        void Start()
        {
            GetComponentInChildren<Text>().text = Device.DisplayName.TruncateWithEllipsis(15);
        }

        /// <summary>
        /// Selects the set Device as the currently displayed device. The previously displayed device will be destroyed.
        /// </summary>
        public void Select()
        {
            if (_displayedDevice == Device) return;
            if (_current != null)
                DeviceObjectManager.Instance.RemoveGameObject(_current);
            var gameObj = DeviceObjectManager.Instance.CreateGameObject(Device);
            _current = gameObj;
            gameObj.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 1.4f + Camera.main.transform.right * 0.05f;
            gameObj.transform.LookAt(gameObj.transform.position + Camera.main.transform.forward);
            _displayedDevice = Device;
        }
    }
}
