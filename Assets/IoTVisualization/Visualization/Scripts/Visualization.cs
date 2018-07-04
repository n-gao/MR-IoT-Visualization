using System.Text.RegularExpressions;
using IoTVisualization.Networking;
using IoTVisualization.Networking.Utils;
using UnityEngine;

namespace IoTVisualization.Visualization
{
    /// <summary>
    /// Default component implementation of IVisualization.
    /// </summary>
    public abstract class Visualization : MonoBehaviour, IVisualization
    {
        /// <summary>
        /// Colors used to display visualization boxes in editor.
        /// </summary>
        private static readonly Color[] Colors = {
            Color.blue,
            Color.green,
            Color.red,
            Color.yellow,
            Color.cyan,
            Color.black,
            Color.magenta,
            Color.white,
        };
        /// <summary>
        /// Index used to select debug color.
        /// </summary>
        private static int _currentIndex = 0;
        /// <summary>
        /// Selected index
        /// </summary>
        private int _index = -1;

        /// <summary>
        /// The currently used provider.
        /// </summary>
        public IProvider Provider { get; set; }
        /// <summary>
        /// Device this visualization belongs to.
        /// </summary>
        public IDevice Device { get; set; }
        /// <summary>
        /// Visualized attribute.
        /// </summary>
        public IAttribute Attribute { get; set; }
        [SerializeField]
        private Vector2 _size;
        /// <summary>
        /// If set to true localScale will be equal to Size.
        /// </summary>
        public bool UseAsTransformSize = true;
        /// <summary>
        /// Device wrapper which this visualization belongs to.
        /// </summary>
        public DeviceWrapper DeviceWrapper { get; set; }
        /// <summary>
        /// Indicates whether this visualization should be drawn.
        /// </summary>
        public bool ShouldDraw
        {
            get { return DeviceWrapper == null || DeviceWrapper.IsVisible; }
        }

        public IVisualizationLayout Layout
        {
            get
            {
                var result = GetComponent<IVisualizationLayout>();
                return result ?? new DefaultVisualizationLayout();
            }
        }

        public Vector2 Size
        {
            get { return _size; }
            set {
                _size = value;
                if (UseAsTransformSize)
                    AdjustSize();
            }
        }

        public int OrderPriority
        {
            get { return Layout.Priority; }
            set { Layout.Priority = value; }
        }

        public Vector2 Bounds
        {
            get { return new Vector2(_size.x + Layout.LeftMargin + Layout.RightMargin, _size.y + Layout.BottomMargin + Layout.TopMargin);}
        }

        protected virtual void Start()
        {
            if (Device == null)
                Device = GetComponentInParent<DeviceWrapper>().Device;
            if (Provider == null)
                Provider = ProviderMonoBehaviour.Provider;
            if (Attribute != null)
                Layout.Priority = Attribute.VisualizationOrder ?? Layout.Priority;
            if (Attribute != null)
                gameObject.name = Attribute.DisplayName;

            VisualizationLayoutManager vsManager = GetComponentInParent<VisualizationLayoutManager>();
            if (vsManager != null)
                vsManager.AddVisualization(this);
        }

        /// <summary>
        /// Called whenever the size of the visualization changes.
        /// 
        /// Sets the localScale to Size.
        /// </summary>
        protected virtual void AdjustSize()
        {
            transform.localScale = new Vector3(_size.x, _size.y, transform.localScale.z);
        }

        protected virtual void OnValidate()
        {
            Size = _size;
        }

        //Used to draw a the size as box in editor.
        void OnDrawGizmos()
        {
            if (_index == -1)
                _index = _currentIndex++;
            Gizmos.color = Colors[_index % Colors.Length];
            Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.25f);
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.DrawCube(Vector3.zero, new Vector3(Size.x, Size.y, 0.01f));
        }
    }
}
