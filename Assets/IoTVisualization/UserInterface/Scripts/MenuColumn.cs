using System;
using System.Collections.Generic;
using UnityEngine;

namespace IoTVisualization.UserInterface
{
    /// <summary>
    /// This component represents a menu coloumn. It can be opened and closed and automatically
    /// arranges all of its items in a vertical list.
    /// </summary>
    public class MenuColumn : MonoBehaviour
    {
        private bool _open = false;
        /// <summary>
        /// Defines the state of the coloumn. If set to true it will show all items and if set to
        /// false it will hide them.
        /// </summary>
        public bool Open
        {
            get { return _open; }
            set
            {
                _open = value;
                if (_open)
                    ShowItems();
                else
                    HideItems();
            }
        }
        /// <summary>
        /// Event called when the coloumn has been opened.
        /// </summary>
        public event Action Opened;
        /// <summary>
        /// Event called when the coloumn has been closed.
        /// </summary>
        public event Action Closed;

        private FollowingMenu _menu;
        
        public List<AnimatedItem> MenuItems { get; private set; }

        private float Distance
        {
            get { return _menu.Distance; }
        }

        /// <summary>
        /// Degree offset.
        /// </summary>
        public float Degree = 30;
        /// <summary>
        /// Degree step between each item.
        /// </summary>
        public float StepSize = 4;

        private float ColumnAngleWidth
        {
            get { return _menu.ColumnAngleWidth; }
        }

        void Awake()
        {
            MenuItems = new List<AnimatedItem>();
        }

        // Use this for initialization
        void Start ()
        {
            _menu = GetComponentInParent<FollowingMenu>();
            if (_menu != null)
                _menu.Coloumns.Add(this);
        }

        /// <summary>
        /// Hides all items.
        /// </summary>
        private void HideItems()
        {
            RemoveDetroyedItems();
            for (int i = 0; i < MenuItems.Count; i++)
                MenuItems[i].Hide();
            if (Closed != null)
                Closed();
        }

        /// <summary>
        /// Opens the coloumn.
        /// </summary>
        private void ShowItems()
        {
            RemoveDetroyedItems();
            int count = MenuItems.Count;
            int index = transform.GetSiblingIndex();
            for (int i = 0; i < count; i++)
            {
                var menu = MenuItems[i];
                //Defineing each individual position
                int offsetMult = (int) Mathf.Ceil(index / 2.0f);
                int sign = index % 2 * 2 - 1;
                var menuIndex = menu.transform.GetSiblingIndex();
                Vector3 forward = Vector3.ProjectOnPlane(_menu.FollowedTransform.forward, Vector3.up).normalized;
                Vector3 right = Vector3.ProjectOnPlane(_menu.FollowedTransform.right, Vector3.up).normalized;
                menu.VisiblePosition = Quaternion.Euler(0, sign * offsetMult * ColumnAngleWidth, 0) // y-Rotation
                                       * Quaternion.AngleAxis(Degree - (count - menuIndex - 1) * StepSize, right) // x-Rotation
                                       * forward
                                       * Distance
                                       - transform.position // Transform from menu to camera space
                                       + _menu.FollowedTransform.position;
                menu.Show();
            }
            if (Opened != null)
                Opened();
        }

        private void RemoveDetroyedItems()
        {
            for (int i = MenuItems.Count - 1; i >= 0; i--)
                if (MenuItems[i] == null)
                    MenuItems.RemoveAt(i);
        }
    }
}
