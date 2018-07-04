using System;
using System.Collections.Generic;
using System.Text;
using Assets.IoTVisualization_Tests.Scripts;
using IoTVisualization.Localization;
using IoTVisualization.Networking;
using IoTVisualization.Networking.Utils;
using UnityEngine;
using UnityEngine.VR.WSA.Sharing;

namespace IoTVisualization.Visualization
{
    public class DeviceSpawner : ProviderMonoBehaviour
    {
        public GameObject DevicePrefab;
        private Dictionary<IDevice, GameObject> Devices { get; set; }

        void Awake()
        {
            if (DevicePrefab == null)
            {
                Debug.LogError("Device Spawner needs a device prefab!");
                Destroy(this);
                return;
            }
            Devices = new Dictionary<IDevice, GameObject>();
            Provider.DataSourceAdded += OnDataSourceAdded;
            Provider.DataSourceRemoved += OnDataSourceRemoved;
        }

        private void OnDataSourceAdded(IDevice iioTDevice)
        {
            var device = Devices[iioTDevice] = Instantiate(DevicePrefab);
            var position = Camera.main.transform.position + Camera.main.transform.forward;
            var rotation = Quaternion.identity;
            if (iioTDevice.HasPosition)
            {
                try
                { 
#if WINDOWS_UWP
                    //AnchorImporter.Init(iioTDevice.Name, device, iioTDevice.SavedPosition); 
#else
                    var transformData = PositionData.Parse(Encoding.UTF8.GetString(iioTDevice.SavedPosition));
                    position = transformData.Position;
                    rotation = transformData.Rotation;
#endif
                } catch(Exception) {
                    //Ignored 
                }
            }
            device.transform.position = position;
            device.transform.rotation = rotation;
            device.name = iioTDevice.Name;
            device.GetComponent<DeviceWrapper>().Init(iioTDevice);
            print("[Visualization]Created GameObject for " + iioTDevice.DisplayName);
        }
        
        private void OnDataSourceRemoved(IDevice iioTDevice)
        {
            Destroy(Devices[iioTDevice]);
            Devices.Remove(iioTDevice);
        }
    }
}
