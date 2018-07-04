using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HoloToolkit.Examples.InteractiveElements;
using IoTVisualization.Localization;
using IoTVisualization.Networking;
using IoTVisualization.Networking.Utils;
using IoTVisualization.Utils;
using UnityEngine;

namespace IoTVisualization.UserInterface
{
    /// <summary>
    /// This component contains methods for showing and hiding the device menu coloumn. In addition to that 
    /// it manages all of its items.
    /// </summary>
    public class ShowDeviceButton : ProviderMonoBehaviour
    {
        /// <summary>
        /// Prefab used for buttons. Should have AnimatedItem, LabelTheme and DeviceButton attached to it.
        /// </summary>
        public GameObject DeviceButtonPrefab;
        /// <summary>
        /// The coloumn component in which all buttons should be placed.
        /// </summary>
        public MenuColumn DeviceColumn;

        private HashSet<IDevice> _devices;
        private readonly Dictionary<IDevice, GameObject> _buttons = new Dictionary<IDevice, GameObject>();

        private InteractiveToggle _toggle;
        private MenuColumn _column;

        private static IEnumerable<IDevice> AllDevices
        {
            get { return Provider.Devices; }
        }

        private bool Open
        {
            get { return _toggle.HasSelection; }
        }

        private bool _rearrange = false;
        
        void Start()
        {
            _column = GetComponentInParent<MenuColumn>();
            _toggle = GetComponent<InteractiveToggle>();
            _column.Closed += Hide;
            _devices = new HashSet<IDevice>(Provider.Devices);
            Provider.DataSourceAdded += DataSourceAdded;
            Provider.DataSourceRemoved += DataSourceRemoved;
            DeviceObjectManager.Instance.ObjectRemoved += (device, gameObj) => DataSourceAdded(device);
            DeviceObjectManager.Instance.ObjectPlaced += (device, gameObj) => DataSourceRemoved(device);
            DeviceObjectManager.Instance.PositionImportStarted += DataSourceDeactivate;
            DeviceObjectManager.Instance.PositionImportFailed += DataSourceActivate;
        }

        void LateUpdate()
        {
            if (_rearrange && Open)
            {
                _rearrange = false;
                Show();
            }
        }

        private void DataSourceActivate(IDevice obj)
        {
            _buttons[obj].GetComponent<Interactive>().IsEnabled = true;
        }

        private void DataSourceDeactivate(IDevice obj)
        {
            _buttons[obj].GetComponent<Interactive>().IsEnabled = false;
        }

        private void DataSourceRemoved(IDevice device)
        {
            _devices.Remove(device);
            if (_buttons.ContainsKey(device))
            {
                Destroy(_buttons[device]);
                _buttons.Remove(device);
            }
            _rearrange = true;
        }

        private void DataSourceAdded(IDevice device)
        {
            if (!AllDevices.Contains(device) || DeviceObjectManager.Instance.IsVisualized(device)) return;
            _devices.Add(device);
            CreateButton(device);
            if (Open)
                DeviceColumn.Open = true;
            _rearrange = true;
        }

        /// <summary>
        /// Shows the device coloumn and fills it with content.
        /// </summary>
        public void Show()
        {
            foreach (var source in _devices)
            {
                CreateButton(source);
            }
            DeviceColumn.Open = true;
            _toggle.HasSelection = true;
        }

        /// <summary>
        /// Hides the device coloumn
        /// </summary>
        public void Hide()
        {
            DeviceColumn.Open = false;
            _toggle.HasSelection = false;
        }

        /// <summary>
        /// Creates a button for the given device if there is none
        /// </summary>
        /// <param name="device">Device</param>
        /// <returns>Button GameObject</returns>
        private GameObject CreateButton(IDevice device)
        {
            if (_buttons.ContainsKey(device)) return _buttons[device];
            GameObject button = Instantiate(DeviceButtonPrefab, DeviceColumn.transform);
            button.GetComponent<AnimatedItem>().Hide(true);
            button.name = device.Name + "|button";
            button.GetComponent<LabelTheme>().Default = device.DisplayName.TruncateWithEllipsis(15);
            button.GetComponent<DeviceButton>().Device = device;
            return _buttons[device] = button;
        }
    }
}
