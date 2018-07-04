using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Assets.IoTVisualization_Tests.Scripts;
using HoloToolkit.Unity;
using IoTVisualization.Networking;
using IoTVisualization.Networking.EnEffCampus;
using IoTVisualization.Networking.Utils;
using IoTVisualization.Utils;
using IoTVisualization.Visualization;
using UnityEngine;
using UnityEngine.VR.WSA.Persistence;

namespace IoTVisualization.Localization
{
    /// <summary>
    /// This singleton manages the mapping between the devices from the IoT-network and Unitys GameObjects.
    /// </summary>
    public class DeviceObjectManager : ProviderMonoBehaviour
    {
        /// <summary>
        /// If this is set to false visualizations wont be automatically placed.
        /// </summary>
        public bool AutoObjectPlacement = true;
        /// <summary>
        /// If this is set to false no position informations will be transmitted.
        /// </summary>
        public bool AutoUpdateRemotePosition = true;
        /// <summary>
        /// This event will be called when a GameObject for a device has been instantiated.
        /// </summary>
        public event Action<IDevice, GameObject> ObjectPlaced;
        /// <summary>
        /// This event will be called when a GameObject for a device has been removed.
        /// </summary>
        public event Action<IDevice, GameObject> ObjectRemoved;
        /// <summary>
        /// This event will be called when a position import has been started.
        /// </summary>
        public event Action<IDevice> PositionImportStarted;
        /// <summary>
        /// This event will be called when a position import failed.
        /// </summary>
        public event Action<IDevice> PositionImportFailed;
        
        /// <summary>
        /// Prefab for a device
        /// </summary>
        public GameObject DevicePrefab;
        /// <summary>
        /// Dictionary which maps all instantiated devices to their GameObjects.
        /// </summary>
        public Dictionary<IDevice, GameObject> Objects = new Dictionary<IDevice, GameObject>();

        /// <summary>
        /// Singleton instance
        /// </summary>
        public static DeviceObjectManager Instance { get; private set; }

        /// <summary>
        /// Time between position requests.
        /// </summary>
        public float SecondsBetweenRequests = 15;

        private float _lastRequest = 0;

        private readonly Dictionary<IDevice, Action<byte[]>> _positionListeners = new Dictionary<IDevice, Action<byte[]>>();
        private AnchorImporter _currentImporter;
        private readonly Queue<IDevice> _toImport = new Queue<IDevice>();
        private readonly Queue<IDevice> _toRequestPosition = new Queue<IDevice>();

        public WorldAnchorStore AnchorStore { get; private set; }
        private string[] _savedIds;

        void Awake()
        {
            Instance = this;
            Provider.DataSourceRemoved += AutoRemoveGameObject;
            Provider.DataSourceAdded += EnqueueRequestPosition;
        }

        void Start()
        {
            WorldAnchorStore.GetAsync(store =>
            {
                AnchorStore = store;
                AsyncUtil.Instance.Enqueue(() =>
                {
                    _savedIds = AnchorStore.GetAllIds();
                    var devices = Provider.Devices;
                    foreach (var device in devices)
                    {
                        CheckIfPositionIsSaved(device);
                    }
                });
            });
        }

        void Update()
        {
            if (Time.time - _lastRequest >= SecondsBetweenRequests)
            {
                if (_toRequestPosition.Count > 0)
                {
                    RequestPosition(_toRequestPosition.Dequeue());
                    _lastRequest = Time.time;
                }
            }
        }

        void OnDestroy()
        {
            Objects.Clear();
            _toImport.Clear();
            _toRequestPosition.Clear();
            _positionListeners.Clear();
            ObjectPlaced = null;
            ObjectRemoved = null;
            PositionImportFailed = null;
            PositionImportStarted = null;
        }

        private void CheckIfPositionIsSaved(IDevice device)
        {
            if (_savedIds == null) return;
            if (_savedIds.Contains(device.Name) && !_toImport.Contains(device) && !Objects.ContainsKey(device))
            {
                var gameObj = CreateGameObject(device);
                var result = AnchorStore.Load(device.Name, gameObj);
                if (result == null)
                {
                    RemoveGameObject(gameObj);
                }
            }
        }

        private void EnqueueRequestPosition(IDevice device)
        {
            CheckIfPositionIsSaved(device);
//            _toRequestPosition.Enqueue(device);
        }

        /// <summary>
        /// Requests the position of the given device from the iot-enviroment.
        /// </summary>
        /// <param name="device">Device</param>
        private void RequestPosition(IDevice device)
        {
            if (device.HasPosition)
            {
                CreateGameObject(device);
                return;
            }
            Provider.RequestPosition(device);
            Action<byte[]> autoCreateGameObject = bytes =>
            {
                if (AutoObjectPlacement)
                    TryImportPosition(device);
            };
            _positionListeners[device] = autoCreateGameObject;
            device.PositionAvailable += autoCreateGameObject;
        }

        private void TryImportPosition(IDevice device)
        {
            if (!device.HasPosition)
            {
                Debug.LogError("[Localization] The given device does not have a position.");
                return;
            }
#if WINDOWS_UWP
            StartImport(device);
            PositionImportStarted?.Invoke(device);
#else
            try
            {
                PositionData transformData = PositionData.Parse(Encoding.UTF8.GetString(device.SavedPosition));
                if (PositionImportStarted != null)
                    PositionImportStarted(device);
                GameObject gameObj = CreateGameObject(device);
                gameObj.transform.position = transformData.Position;
                gameObj.transform.rotation = transformData.Rotation;
            }
            catch (Exception)
            {
                Debug.LogWarning("[Localization] The received position data could not be interpreted.");
            }
#endif
        }

#if WINDOWS_UWP
        private void StartImport(IDevice device)
        {
            if (_toImport.Contains(device))
                return;
            if (_currentImporter != null)
            {
                _toImport.Enqueue(device);
                return;
            }
            _currentImporter = AnchorImporter.Import(device.Name,
                device.SavedPosition,
                () =>
                {
                    _currentImporter = null;
                    if (_toImport.Count > 0)
                        StartImport(_toImport.Dequeue());
                    return Objects.ContainsKey(device) ? Objects[device] : CreateGameObject(device);
                },
                () => PositionImportFailed?.Invoke(device));
        }
#endif

        /// <summary>
        /// Instantiates a GameObject by the set prefab for the given device.
        /// </summary>
        /// <param name="device">Device which should be instantiated.</param>
        /// <returns>Instantiated GameObject</returns>
        public GameObject CreateGameObject(IDevice device)
        {
            if (_positionListeners.ContainsKey(device))
            {
                device.PositionAvailable -= _positionListeners[device];
                _positionListeners.Remove(device);
            }

            GameObject gameObj = Objects[device] = Instantiate(DevicePrefab);
            gameObj.transform.position = Camera.main.transform.position + Camera.main.transform.forward;
            gameObj.transform.rotation = Quaternion.identity;
            gameObj.name = device.Name;

            gameObj.GetComponent<DeviceWrapper>().Init(device);

            print("[Visualization]Created GameObject for " + device.DisplayName);

            device.SavedPosition = null;

            if (ObjectPlaced != null)
                ObjectPlaced(device, gameObj);

            return gameObj;
        }

        /// <summary>
        /// Returns the GameObject to a given device.
        /// </summary>
        /// <param name="device">Device</param>
        /// <returns>Matching GameObject</returns>
        public GameObject GetGameObject(IDevice device)
        {
            return Objects.ContainsKey(device) ? Objects[device] : null;
        }

        /// <summary>
        /// Removes all instantiated GameObjects.
        /// </summary>
        public void RemoveAllGameObjects()
        {
            for (int i = 0; i < Provider.Devices.Count; i++)
            {
                IDevice device = Provider.Devices[i];
                RemoveGameObject(device);
            }
        }

        /// <summary>
        /// Wrapper function for RemoveGameObject with void as return value.
        /// 
        /// This method will be used as listener for certain events.
        /// </summary>
        /// <param name="device">Device which will be removed</param>
        private void AutoRemoveGameObject(IDevice device)
        {
            RemoveGameObject(device);
        }

        public bool RemoveGameObject(GameObject obj)
        {
            if (!Objects.ContainsValue(obj)) return false;
            IDevice device = null;
            foreach (var pair in Objects)
            {
                if (pair.Value == obj)
                {
                    device = pair.Key;
                    break;
                }
            }
            return RemoveGameObject(device);
        }

        /// <summary>
        /// Removes the instantiated instance for a given device.
        /// </summary>
        /// <param name="device">Device</param>
        public bool RemoveGameObject(IDevice device)
        {
            if (!Objects.ContainsKey(device))
                return false;
            GameObject obj = Objects[device];
            bool b = Objects.Remove(device);
            if (ObjectRemoved != null && b)
                ObjectRemoved(device, obj);
            Destroy(obj);
            return b;
        }

        /// <summary>
        /// Removes the instantiated GameObject and deletes the belonging position data from the iot-network.
        /// </summary>
        /// <param name="device">Device which should be removed</param>
        public void Remove(IDevice device)
        {
            if (RemoveGameObject(device))
            {
                Provider.RemovePosition(device);
                if (AnchorStore != null)
                    AnchorStore.Delete(device.Name);
            }
        }

        /// <summary>
        /// Indicates whether an instance of the prefab exists for this device.
        /// </summary>
        /// <param name="device">Device</param>
        /// <returns>Indicates whether an instance of the prefab exists for this device.</returns>
        public bool IsVisualized(IDevice device)
        {
            return Objects.ContainsKey(device);
        }

        /// <summary>
        /// Returns a list of all devices which do not have a GameObject.
        /// </summary>
        public IDevice[] UnmatchedDevices
        {
            get { return Provider.Devices.Where(s => !IsVisualized(s)).ToArray(); }
        }

        /// <summary>
        /// Calls remove for all devices.
        /// </summary>
        public void RemoveAll()
        {
            for (int i = 0; i < Provider.Devices.Count; i++)
            {
                IDevice device = Provider.Devices[i];
                Remove(device);
            }
        }
    }
}
