using System;
using System.Collections.Generic;
using System.Linq;
using IoTVisualization.Networking;
using IoTVisualization.Networking.Utils;
using UnityEngine;

namespace IoTVisualization.Visualization
{
    /// <summary>
    /// This component manages a device and all of its attributes. If any new attribute has been added it will automatically
    /// be visualized and displayed as part of its window. In addition to That it links the window to its corresponding device.
    /// </summary>
    public class DeviceWrapper : DeviceMonoBehaviour
    {
        /// <summary>
        /// This class is used to link a content type to a specific visualization prefab.
        /// </summary>
        [Serializable]
        public class ContentToVisualization
        {
            /// <summary>
            /// Content type
            /// </summary>
            public string ContentType;
            /// <summary>
            /// Visualization prefab
            /// </summary>
            public GameObject Prefab;
        }
        
        /// <summary>
        /// This dictionary contains all instantiated attributes and their corresponding visualization.
        /// </summary>
        public Dictionary<IAttribute, IVisualization> Visualizations = new Dictionary<IAttribute, IVisualization>();

        /// <summary>
        /// Default prefab for numerical data.
        /// </summary>
        public GameObject FloatAttributePrefab;
        /// <summary>
        /// Default prefab for textual data.
        /// </summary>
        public GameObject StringAttributePrefab;
        /// <summary>
        /// List containing all special matchings. Use this to define custom visualizations for certain content types.
        /// </summary>
        public List<ContentToVisualization> ContentToVisualizations = new List<ContentToVisualization>();
        /// <summary>
        /// List of ignored content types, which will not be displayed.
        /// </summary>
        public List<string> IgnoredVisualizations = new List<string>
        {
            "position"
        };

        /// <summary>
        /// Indicates whether this device is currently visible.
        /// </summary>
        public bool IsVisible { get; private set; }

        private static float _lastConnect = 0;
        /// <summary>
        /// Time between two devices connecting to the IoT-enviroment. Used to prevent memory overflow.
        /// </summary>
        public float TimeBetweenConnects = 2;
        private bool _shouldConnect = false;

        private Renderer _panelRenderer;

        void Start()
        {
            var layoutManager = GetComponent<VisualizationLayoutManager>();
            if (layoutManager != null)
                if (layoutManager.Panel != null)
                    _panelRenderer = layoutManager.Panel.GetComponent<Renderer>();
        }

        void Update()
        {
            IsVisible = _panelRenderer == null || _panelRenderer.isVisible;
            if (_shouldConnect && Time.time - _lastConnect >= TimeBetweenConnects)
            {
                Connect();
                _lastConnect = Time.time;
            }
        }

        void OnDestroy()
        {
            Device.ValueModified -= ValueAdded;
            Disconnect();
        }


        /// <summary>
        /// Initializes this component with the given device.
        /// </summary>
        /// <param name="device">Device</param>
        public void Init(IDevice device)
        {
            Device = device;
            GetComponentInChildren<TextMesh>().text = Device.Name;
            foreach (IAttribute attribute in Device.Atrributes)
                InitAttribute(attribute);
            Device.ValueModified += ValueAdded;
            _shouldConnect = true;
        }

        /// <summary>
        /// Checks if the attribute of a data entry is already visualized. If not it will be.
        /// Used as listener for IDEvice.ValueModified.
        /// </summary>
        /// <param name="data">Data</param>
        private void ValueAdded(IoTData data)
        {
            if (Visualizations.ContainsKey(data.Attribute)) return;
            InitAttribute(data.Attribute);
        }

        /// <summary>
        /// Initializes a visualization for a given attribute. If the given attribute should not be visualized
        /// calling this will do nothing.
        /// </summary>
        /// <param name="attribute">Attribute to be visualized</param>
        private void InitAttribute(IAttribute attribute)
        {
            GameObject prefab = GetPrefab(attribute);
            if (prefab == null) return;
            GameObject visualizationGameObject = Instantiate(prefab, transform);
            IVisualization visualization = visualizationGameObject.GetComponent<IVisualization>();
            visualization.Provider = Provider;
            visualization.Device = Device;
            visualization.Attribute = attribute;
            Visualizations[attribute] = visualization;

            Visualization vis = visualization as Visualization;
            if (vis != null)
                vis.DeviceWrapper = this;
        }

        /// <summary>
        /// Returns the visualization prefab that matches the best to the given attribute. If none is found or the attribute
        /// is ignored null will be returnes.
        /// </summary>
        /// <param name="attribute">Attribute</param>
        /// <returns>Visualization prefab</returns>
        private GameObject GetPrefab(IAttribute attribute)
        {
            if (IgnoredVisualizations.Contains(attribute.ContentType)) return null;
            ContentToVisualization pair =
                ContentToVisualizations.FirstOrDefault(c => c.ContentType == attribute.ContentType);
            if (pair != null) return pair.Prefab;
            return attribute.LatestValue.IsString ? StringAttributePrefab : FloatAttributePrefab;
        }

        /// <summary>
        /// Subscribes to all updates to this device.
        /// </summary>
        public void Connect()
        {
            Provider.Subscribe(Device);
            _shouldConnect = false;
        }

        /// <summary>
        /// Unsubscribes to all updates of this device.
        /// </summary>
        public void Disconnect()
        {
            Provider.Unsubscribe(Device);
            _shouldConnect = false;
        }
    }
}