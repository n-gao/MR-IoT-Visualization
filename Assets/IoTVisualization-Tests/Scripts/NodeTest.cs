using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Assets.IoTVisualization_Tests.Scripts;
using IoTVisualization.Networking;
using IoTVisualization.Networking.Utils;
using IoTVisualization.Visualization;
using UnityEngine;

public class NodeTest : ProviderMonoBehaviour
{
    public GameObject NodePrefab;
    private Dictionary<IDevice, GameObject> Test = new Dictionary<IDevice, GameObject>();

	// Use this for initialization
	void Awake () {
        Provider.DataSourceAdded += ProviderOnDataSourceAdded;
	}

    private void ProviderOnDataSourceAdded(IDevice iioTDevice)
    {
        var positionData = iioTDevice.SavedPosition;
        Vector3 position = Camera.main.transform.position + Camera.main.transform.forward;
        Quaternion rotation = Quaternion.identity;
        if (iioTDevice.HasPosition)
        {
            try
            {
                PositionData pData = PositionData.Parse(Encoding.UTF8.GetString(iioTDevice.SavedPosition));
                position = pData.Position;
                rotation = pData.Rotation;
            }
            catch (Exception)
            {
                //Ignored
            }
        }
        GameObject g = Test[iioTDevice] = Instantiate(NodePrefab,
            position,
            rotation);
        g.name = iioTDevice.Name;
        g.GetComponent<DeviceWrapper>().Init(iioTDevice);
    }
}
