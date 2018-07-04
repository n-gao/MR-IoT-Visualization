using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IoTVisualization.Visualization
{
    /// <summary>
    /// This component manages the layout of a visualization window. It arranges all visualizations according
    /// to their layout properties and the set properties of the the window. All items will be arranged in a
    /// row based system with a fixed maximum width. If the width is exceeded a new row starts.
    /// 
    /// It should be attached the root of a window.
    /// </summary>
    public class VisualizationLayoutManager : MonoBehaviour
    {
        [SerializeField]
        private float _width;

        [SerializeField] private bool _wrapContent;
        /// <summary>
        /// Maximum width of this window.
        /// </summary>
        public float Width
        {
            get { return _width; }
            set
            {
                _width = value;
                Rearrange();
            }
        }

        /// <summary>
        /// If set to true the window will shrink to the minimum size. Otherwise it will always be Width wide.
        /// </summary>
        public bool WrapContent
        {
            get { return _wrapContent; }
            set
            {
                _wrapContent = value;
                Rearrange();
            }
        }
        /// <summary>
        /// Number of margins.
        /// </summary>
        private const int MarginCount = 4;
        /// <summary>
        /// Array containing all margins.
        /// </summary>
        public float[] Margins = new float[MarginCount];

        public float TopMargin { get { return Margins.Length < 1 ? 0 : Margins[0]; } }
        public float RightMargin { get { return Margins.Length < 2 ? TopMargin : Margins[1]; } }
        public float BottomMargin { get { return Margins.Length < 3 ? TopMargin : Margins[2]; } }
        public float LeftMargin { get { return Margins.Length < 4 ? RightMargin : Margins[3]; } }
    
        private readonly List<Visualization> _visualizations = new List<Visualization>();

        [SerializeField]
        private Transform _panel;
        /// <summary>
        /// The background panel.
        /// </summary>
        public Transform Panel
        {
            get { return _panel ?? (_panel = transform.Find("Panel")); }
            set { _panel = value; }
        }

        private bool _willRearrange = false;
        /// <summary>
        /// If this variable is true the whole layout will be rearranged.
        /// </summary>
        public bool WillRearrange
        {
            get { return _willRearrange; }
            set { _willRearrange = value; }
        }

        /// <summary>
        /// Describes a row which is defined as a list of visualizations.
        /// </summary>
        private class Row : List<Visualization>
        {
            private readonly VisualizationLayoutManager _layoutManager;

            /// <summary>
            /// Height of the row.
            /// </summary>
            public float Height
            {
                get { return Count == 0 ? 0 : this.Max(v => v.Bounds.y); }
            }
            /// <summary>
            /// Width of the row
            /// </summary>
            public float Width
            {
                get { return Count == 0 ? 0 : this.Sum(v => v.Bounds.x) + _layoutManager.LeftMargin + _layoutManager.RightMargin; }
            }

            public Row(VisualizationLayoutManager layoutManager)
            {
                _layoutManager = layoutManager;
            }

            /// <summary>
            /// Arranges all items of this row.
            /// </summary>
            /// <param name="offset">Offset which is applied to all items.</param>
            public void ArrangeItems(Vector2 offset)
            {
                float xOffset = 0;
                float height = Height;
                for (int i = 0; i < this.Count; i++)
                {
                    var vis = this[i];
                    vis.transform.localPosition = new Vector3(
                        offset.x + xOffset + vis.Bounds.x / 2,
                        offset.y - height / 2);
                    xOffset += vis.Bounds.x;
                }
            }
        }

        /// <summary>
        /// Size of the window
        /// </summary>
        public Vector3 Size
        {
            get
            {
                return new Vector3(_rows.Max(r => r.Width) + LeftMargin + RightMargin, TopMargin + BottomMargin + _rows.Sum(r => r.Height), 0.01f);
            }
        }

        /// <summary>
        /// All rows
        /// </summary>
        private List<Row> _rows;


        void Awake()
        {
            if (Panel == null)
            {
                Panel = transform.Find("Panel");
            }
        }

        // Use this for initialization
        void Start () {
		
        }
	
        // Update is called once per frame
        void Update ()
        {
            if (_willRearrange)
            {
                Reorder();
                Rearrange();
            }
            _willRearrange = false;
        }

        public void AddVisualization(Visualization visualization)
        {
            _visualizations.Add(visualization);
            visualization.Layout.LayoutChanged += LayoutOnLayoutChanged;
            _willRearrange = true;
        }

        /// <summary>
        /// Listener which is set to all visualization layouts. Checks if they have changed.
        /// </summary>
        private void LayoutOnLayoutChanged()
        {
            WillRearrange = true;
        }

        /// <summary>
        /// Sorts visualizations based on their priority.
        /// </summary>
        private void Reorder()
        {
            var vs = _visualizations.OrderByDescending(v => v.Layout.Priority);
            int i = 0;
            foreach (var v in vs)
                v.transform.SetSiblingIndex(i++);
        }

        /// <summary>
        /// Rearranges all visualizations.
        /// </summary>
        private void Rearrange()
        {
            Row currentRow = new Row(this);
            _rows = new List<Row> {currentRow};
            var visualizations = GetComponentsInChildren<Visualization>();
            //Assigning rows
            for (int i = 0; i < visualizations.Length; i++)
            {
                var visualization = visualizations[i];
                Vector2 bounds = visualization.Bounds;
                var layout = visualization.Layout;
                if (layout.LineBreakBefore || currentRow.Width + bounds.x >= Width)
                {
                    _rows.Add(currentRow = new Row(this));
                }
                currentRow.Add(visualization);
                if (layout.LineBreakAfter)
                {
                    _rows.Add(currentRow = new Row(this));
                }
            }
            //Setting positions
            float height = _rows.Sum(r => r.Height) + TopMargin + BottomMargin;
            float width = _rows.Max(r => r.Width);
            Vector2 offset = new Vector2(-width/2 + LeftMargin, height / 2 - TopMargin);
            for (int i = 0; i < _rows.Count; i++)
            {
                var row = _rows[i];
                offset.x = (width - row.Width) / 2 - width / 2 + LeftMargin;
                row.ArrangeItems(offset);
                offset.y -= row.Height;
            }
            //Set panel size
            if (Panel != null)
            {
                Panel.localScale = new Vector3(WrapContent ? width : Width, height + BottomMargin, Panel.localScale.z);
            }
        }

        void OnValidate()
        {
            if (Margins.Length >= MarginCount)
                Array.Resize(ref Margins, MarginCount);
            if (_width <= 0)
                _width = 1;
            Rearrange();
        }
    }
}
