using System;
using System.Linq;
using System.Text;
using Assets.IoTVisualization_Tests.Scripts;
using HoloToolkit.Unity;
using IoTVisualization.Networking.Utils;
using UnityEngine;
using UnityEngine.VR.WSA;
using UnityEngine.VR.WSA.Sharing;

namespace IoTVisualization.Localization
{
    /// <summary>
    /// This component simplifies the export of a WorldAnchor using the Export function. Furthermore it simulates the position data when running in editor.
    /// </summary>
    public class AnchorExporter : MonoBehaviour
    {
        /// <summary>
        /// This event will be called when the position has been exported successfully.
        /// </summary>
        public event Action<byte[]> DataExported;
        /// <summary>
        /// This event will be called when the excport failed.
        /// </summary>
        public event Action<SerializationCompletionReason> DataExportFailed;

        private byte[] _data;
        private SerializationCompletionReason _completionReason;
        private bool _completed = false;
        private int _attemps = 10;
        private string _name;

        void Update()
        {
            if (_completed)
            {
                _completed = false;
                if (_completionReason != SerializationCompletionReason.Succeeded)
                {
#if UNITY_EDITOR
                    PositionData ps = new PositionData(transform);
                    _data = Encoding.UTF8.GetBytes(ps.ToString());
                    _completed = true;
                    _completionReason = SerializationCompletionReason.Succeeded;
#else
                    _attemps = _attemps - 1;
                    if (_attemps == 0)
                    {
                        Debug.LogError("Export failed\n" + _completionReason);
                        if (DataExportFailed != null)
                            DataExportFailed(_completionReason);
                        _data = null;
                    }
                    else
                    {
                        Export(_name, _attemps);
                    }
#endif
                }
                else
                {
                    Debug.Log("Export successful.");
                    if (DataExported != null)
                        DataExported(_data);
                    _data = null;
                }
            }
        }

        /// <summary>
        /// Starts the exportation of the currently attached WorldAnchor
        /// </summary>
        /// <param name="name">Name of this Anchor.</param>
        /// <param name="attemps">Number of attempts to export.</param>
        public void Export(string name, int attemps = 5)
        {
            _name = name;
            _attemps = attemps;
            if (attemps == 0) return;
            WorldAnchor anchor = GetComponent<WorldAnchor>();
            if (anchor == null)
            {
                Debug.LogError("There is not anchor to export!");
                return;
            }
            if (DeviceObjectManager.Instance.AnchorStore != null)
            {
                DeviceObjectManager.Instance.AnchorStore.Delete(_name);
                DeviceObjectManager.Instance.AnchorStore.Save(_name, anchor);
            }
            _data = new byte[0];
            WorldAnchorTransferBatch transferBatch = new WorldAnchorTransferBatch();
            transferBatch.AddWorldAnchor(_name, anchor);
            WorldAnchorTransferBatch.ExportAsync(transferBatch, OnDataAvailable, OnCompleted);
        }

        private void OnCompleted(SerializationCompletionReason completionReason)
        {
            _completionReason = completionReason;
            _completed = true;
        }

        private void OnDataAvailable(byte[] data)
        {
            _data = _data.Concat(data).ToArray();
        }
    }
}
