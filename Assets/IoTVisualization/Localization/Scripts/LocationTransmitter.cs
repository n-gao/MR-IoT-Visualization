using IoTVisualization.Localization;
using IoTVisualization.Networking;
using IoTVisualization.Networking.Utils;
using IoTVisualization.Visualization;
using UnityEngine;
using UnityEngine.VR.WSA.Sharing;

namespace IoTVisualization.Localization
{
    /// <summary>
    /// This MonoBehaviour combines TapToPlace and the AnchorExporter in order to directly export and transmit the location
    /// of this GameObject if the user changes its position.
    /// </summary>
    [RequireComponent(typeof(MyTapToPlace))]
    [RequireComponent(typeof(AnchorExporter))]
    [RequireComponent(typeof(DeviceWrapper))]
    public class LocationTransmitter : MonoBehaviour
    {

        private MyTapToPlace _tapToPlace;
        private AnchorExporter _exporter;
        private DeviceWrapper _device;

        private byte[] _data = null;

        private bool Exported
        {
            get { return _data != null; }
        }

        // Use this for initialization
        void Start ()
        {
            _tapToPlace = GetComponent<MyTapToPlace>();
            _exporter = GetComponent<AnchorExporter>();
            _device = GetComponent<DeviceWrapper>();
            _tapToPlace.PickedUp += OnPickedUp;
            _tapToPlace.Dropped += OnDropped;
            _exporter.DataExported += OnDataExported;
            _exporter.DataExportFailed += OnDataExportFailed;
        }

        private void OnDataExportFailed(SerializationCompletionReason serializationCompletionReason)
        {

        }

        private void OnDataExported(byte[] bytes)
        {
            _data = bytes;
        }

        private void OnDropped()
        {
            if (DeviceObjectManager.Instance == null || DeviceObjectManager.Instance.AutoUpdateRemotePosition)
                _exporter.Export(_device.Device.Name);
        }

        private void OnPickedUp()
        {
            
        }

        // Update is called once per frame
        void Update () {
            if (Exported)
            {
                ProviderMonoBehaviour.Provider.SavePosition(_device.Device, _data);
                _data = null;
            }
        }
    }
}
