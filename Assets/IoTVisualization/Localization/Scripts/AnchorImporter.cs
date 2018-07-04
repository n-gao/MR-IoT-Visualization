using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IoTVisualization.Utils;
using UnityEngine;
using UnityEngine.VR.WSA.Sharing;

namespace IoTVisualization.Localization
{
    /// <summary>
    /// This class offers a simple way to import a WorldAnchor.
    /// 
    /// An instance should only be created by the Import function.
    /// An instance of this class allows tracking the current status of the import.
    /// </summary>
    public class AnchorImporter
    {
        /// <summary>
        /// Indicates whether the import was successful.
        /// </summary>
        public bool Imported
        {
            get { return _transferBatch != null; }
        }
        /// <summary>
        /// Indicates whether the import failed.
        /// </summary>
        public bool Failed
        {
            get { return _retry == 0; }
        }
        /// <summary>
        /// Indicates whether the imported anchor has been applied.
        /// </summary>
        public bool AppliedAnchor { get; set; }

        private int _retry = 10;
        private byte[] _data;
        private string _name;
        private WorldAnchorTransferBatch _transferBatch = null;
        private Func<GameObject> _callback;
        private Action _failureCallback;
        
        private AnchorImporter() { }
        
        private void Import()
        {
            WorldAnchorTransferBatch.ImportAsync(_data, OnImportCompleted);
        }

        private void OnImportCompleted(SerializationCompletionReason completionReason, WorldAnchorTransferBatch deserializedTransferBatch)
        {
            if (completionReason != SerializationCompletionReason.Succeeded)
            {
                Debug.Log("[AnchorImport]" + --_retry + " tries left.");
                if (_retry == 0)
                {
                    Debug.LogError("[AnchorImport] failed to import" + _name);
                    if (_failureCallback != null)
                        _failureCallback();
                    _data = null;
                }
                else
                {
                    Import();
                }
            }
            else
            {
                _transferBatch = deserializedTransferBatch;
                AsyncUtil.Instance.Enqueue(() =>
                {
                    var gameObject = _callback();
                    _transferBatch.LockObject(_name, gameObject);
                });
                _data = null;
            }
        }
        
        /// <summary>
        /// Imports the world data given by the data parameter using the given name. The first callback will be called when the import was successful and should instantiate
        /// the GameObject which the Anchor belongs to. The second callback will be called when an error occurs.
        /// </summary>
        /// <param name="name">Name of the anchor</param>
        /// <param name="data">WorldAnchor data</param>
        /// <param name="importedCallback">This callback should return the GameObject which should own the WorldAnchor.</param>
        /// <param name="failureCallback">Failure callback</param>
        /// <returns>AnchorImporter instance which enables tracking the current state of import.</returns>
        public static AnchorImporter Import(string name, byte[] data, Func<GameObject> importedCallback, Action failureCallback)
        {
            AnchorImporter importer = new AnchorImporter
            {
                _data = data,
                _name = name,
                _callback = importedCallback,
                _failureCallback = failureCallback
            };
            importer.Import();
            return importer;
        }
    }
}
