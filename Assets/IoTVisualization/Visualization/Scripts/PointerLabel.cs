using System.Collections;
using System.Collections.Generic;
using IoTVisualization.Networking.Utils;
using UnityEngine;

namespace IoTVisualization.Visualization
{
    /// <summary>
    /// This component is used to display the device name to the label of a pointer.
    /// To do so it checks for a DeviceMonoBehaviour attached at any parent in order to determine its name.
    /// </summary>
    [RequireComponent(typeof(TextMesh))]
    public class PointerLabel : MonoBehaviour
    {
        private TextMesh _textMesh;
        private DeviceMonoBehaviour _device;

        private string Label
        {
            get { return _textMesh.text; }
            set { _textMesh.text = value; }
        }
        
        // Use this for initialization
        void Start()
        {
            _textMesh = GetComponent<TextMesh>();
            _device = GetComponentInParent<DeviceMonoBehaviour>();
            if (_device != null)
                Label = _device.Device.DisplayName;
        }
    }
}