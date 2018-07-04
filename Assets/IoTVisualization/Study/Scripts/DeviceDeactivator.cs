using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using HoloToolkit.Unity;
using IoTVisualization.Desktop;
using IoTVisualization.Localization;
using IoTVisualization.Networking.Utils;
using UnityEngine;

public static class DeviceDeactivator
{
    public static void SetActiveDeviceCount(int count)
    {
        var devices = ProviderMonoBehaviour.Provider.Devices.OrderBy(d => d.DisplayName).ToList();
        var manager = DeviceObjectManager.Instance;
        count = count < 0 ? devices.Count : count;
        count = count > devices.Count ? devices.Count : count;
        for (int i = 0; i < count; i++)
        {
            if (manager.Objects.ContainsKey(devices[i]))
                manager.Objects[devices[i]].SetActive(true);
            if (UiDeviceColoumn.Instance != null)
                UiDeviceColoumn.Instance.AddDevice(devices[i]);
        }
        for (int i = count; i < devices.Count; i++)
        {
            if (manager.Objects.ContainsKey(devices[i]))
                manager.Objects[devices[i]].SetActive(false);
            if (UiDeviceColoumn.Instance != null)
                UiDeviceColoumn.Instance.RemoveDevice(devices[i]);
        }
    }
}
