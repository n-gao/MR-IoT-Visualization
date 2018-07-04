using System.Collections;
using System.Collections.Generic;
using IoTVisualization.Networking.Utils;
using IoTVisualization.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace IoTVisualization.Desktop
{
    /// <summary>
    /// This component contains a method that will set the Provider Adress to the value of the 
    /// given Field and call the Provider Connect function.
    /// </summary>
    public class UiConnectButton : ProviderMonoBehaviour
    {
        public InputField InputField;
        private Transform _parent;

        // Use this for initialization
        void Start()
        {
            _parent = transform.parent;
            Provider.Connected += () => _parent.gameObject.SetActive(false);
            Provider.Disconnected += () => _parent.gameObject.SetActive(true);
            _parent.gameObject.SetActive(!Provider.IsConnected);
        }

        /// <summary>
        /// Connects to the specified address.
        /// </summary>
        public void Connect()
        {
            if (InputField != null)
                Provider.Address = InputField.text;
            Provider.Connect();
        }
    }
}
