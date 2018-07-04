using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IoTVisualization.Localization;
using IoTVisualization.Networking;
using IoTVisualization.Networking.Utils;
using UnityEngine;

namespace IoTVisualization.Desktop
{
    /// <summary>
    /// This component will keep a list of buttons for each Device as childs.
    /// </summary>
    public class UiDeviceColoumn : ProviderMonoBehaviour
    {
        /// <summary>
        /// Button prefab.
        /// </summary>
        public GameObject ButtonPrefab;
        private readonly Dictionary<IDevice, GameObject> _buttons = new Dictionary<IDevice, GameObject>();

        public static UiDeviceColoumn Instance { get; private set; }

        void Awake()
        {
            Instance = this;
        }

        // Use this for initialization
        void Start()
        {
            Provider.DataSourceAdded += AddDevice;
            Provider.DataSourceRemoved += RemoveDevice;
        }

        public void AddDevice(IDevice obj)
        {
            if (_buttons.ContainsKey(obj)) return;
            var button = Instantiate(ButtonPrefab, transform);
            button.GetComponent<DeviceMonoBehaviour>().Device = obj;
            _buttons[obj] = button;
            var order = _buttons.Keys.OrderBy(d => d.DisplayName);
            int i = 0;
            foreach (IDevice device in order)
            {
                _buttons[device].transform.SetSiblingIndex(i++);
            }
        }

        public void RemoveDevice(IDevice obj)
        {
            if (_buttons.ContainsKey(obj))
            {
                Destroy(_buttons[obj]);
                _buttons.Remove(obj);
            }
        }
    }
}
