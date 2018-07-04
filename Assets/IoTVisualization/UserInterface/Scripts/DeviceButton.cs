using IoTVisualization.Localization;
using IoTVisualization.Networking;
using IoTVisualization.Networking.Utils;

namespace IoTVisualization.UserInterface
{
    /// <summary>
    /// This component represents a button to instantiate a device.
    /// </summary>
    public class DeviceButton : ProviderMonoBehaviour
    {
        /// <summary>
        /// The device which this button belongs to.
        /// This must be set after creating the buttonm 
        /// otherwise the button will destroy itself.
        /// </summary>
        public IDevice Device;

        // Use this for initialization
        void Start () {
            if (Device == null)
                Destroy(gameObject);
        }
	    
        /// <summary>
        /// Instantiates the set device.
        /// </summary>
        public void Instantiate()
        {
            MyTapToPlace.Selected = DeviceObjectManager.Instance
                .CreateGameObject(Device)
                .GetComponent<MyTapToPlace>();
        }
    }
}
