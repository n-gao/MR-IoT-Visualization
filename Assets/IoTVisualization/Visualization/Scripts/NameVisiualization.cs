using IoTVisualization.Utils;
using UnityEngine;

namespace IoTVisualization.Visualization
{
    /// <summary>
    /// This visualization is used to display the name of a device at the top of a panel.
    /// 
    /// Therefore it does not require an attribute. 
    /// </summary>
    [RequireComponent(typeof(TextMesh))]
    class NameVisiualization : Visualization
    {
        public int MaxCharacters = 30;
        private TextMesh _mesh;

        void Awake()
        {
            if (_mesh == null)
                _mesh = GetComponent<TextMesh>();
            if (_mesh == null)
                Destroy(this);
        }

        protected override void Start()
        {
            base.Start();
            _mesh.text = Device.DisplayName.TruncateWithEllipsis(MaxCharacters);
            float normalizedFontSize = _mesh.fontSize / 120.0f;
            Size = new Vector2(_mesh.text.Length * normalizedFontSize * 0.022f, 0.08f * normalizedFontSize);
            Layout.Priority = 1000;
        }
    }
}
