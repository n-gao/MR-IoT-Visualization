using System.Linq;
using Assets.Scripts;
using UnityEngine;
using UnityEngine.VR.WSA;
using UnityEngine.VR.WSA.Sharing;

namespace IoTVisualization.Localization
{
    public class WorldAnchorExporter : MonoBehaviour
    {
        private byte[] _exportedData = new byte[0];

        private void debug(string s)
        {
            Debug.LogError(s);
            DebugDisplay.Instance.SetText(s);
        }

        private void OnCompleted(SerializationCompletionReason completionReason)
        {
            if (completionReason != SerializationCompletionReason.Succeeded)
            {
                debug("An error occured during exporting world anchor.\n" + completionReason);
            }
            else
            {
                debug("Export successful.");
            }
        }

        private void OnDataAvailable(byte[] data)
        {
            debug("Some data exported");
            _exportedData = _exportedData.Concat(data).ToArray();
        }

        // Use this for initialization
        void Start () {

        }

        public void StartWorldAnchorExport()
        {
            WorldAnchor worldAnchor = GetComponent<WorldAnchor>();
            if (worldAnchor == null)
            {
                debug("There is no worldAnchor");
                return;
            }
            WorldAnchorTransferBatch transferBatch = new WorldAnchorTransferBatch();
            transferBatch.AddWorldAnchor("GameRootAnchor", worldAnchor);
            WorldAnchorTransferBatch.ExportAsync(transferBatch, OnDataAvailable, OnCompleted);
        }

        // Update is called once per frame
        void Update () {
            if (Input.GetKeyDown(KeyCode.O))
                StartWorldAnchorExport();
        }
    }
}
