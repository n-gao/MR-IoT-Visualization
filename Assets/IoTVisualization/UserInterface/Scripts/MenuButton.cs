using HoloToolkit.Unity;
using UnityEngine;

namespace IoTVisualization.UserInterface
{
    /// <summary>
    /// This component has methods for the menu button which opens or closes the menu.
    /// </summary>
    public class MenuButton : MonoBehaviour
    {
        private MenuColumn _menu;

        void Start()
        {
            _menu = GetComponentInParent<MenuColumn>();
            if (_menu == null)
            {
                Destroy(this);
                return;
            }
            Close();
        }
        /// <summary>
        /// Opens the menu.
        /// </summary>
        public void Open()
        {
            _menu.Open = true;
        }
        /// <summary>
        /// Closes the menu
        /// </summary>
        public void Close()
        {
            _menu.Open = false;
        }
    }
}
