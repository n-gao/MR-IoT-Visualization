using System;
using System.Text.RegularExpressions;
using HoloToolkit.Examples.InteractiveElements;
using IoTVisualization.Measurement;
using IoTVisualization.Networking.EnEffCampus;
using IoTVisualization.Networking;
using IoTVisualization.Networking.Utils;
using IoTVisualization.Utils;
using UnityEngine;

namespace IoTVisualization.UserInterface
{
    /// <summary>
    /// This component contains a method to connect the set provider to its network.
    /// When running in editor it will instantly connect, otherwise there will be a prompt
    /// to scan a QR code or to type in the address.
    /// </summary>
    public class ConnectButton : ProviderMonoBehaviour
    {
        /// <summary>
        /// This prefab should has the QrCodeScanningDialog component in order to work.
        /// It represents the dialog the user has to interact with in order to scan a
        /// QR code.
        /// </summary>
        public GameObject ScanningDialogPrefab;
        private TouchScreenKeyboard _keyboard;
        private string _address;

        private InteractiveToggle _toggle;

        private GameObject _scanningDialog;

        void Start()
        {
            _address = EnergyRouterProvider.Instance.Address;
            _toggle = GetComponent<InteractiveToggle>();
        }

        void Update()
        {
            if (TouchScreenKeyboard.visible == false && _keyboard != null)
            {
                try
                {
                    _address = _keyboard.text;
                    _keyboard = null;
                    Provider.Address = _address;
                    Provider.Connect();
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message + "\n" + e.StackTrace);
                }
            }
            _toggle.HasSelection = Provider.IsConnected;
        }

        /// <summary>
        /// If running in editor calling this method will just call Provider.Connect(), otherwise
        /// it will show a qr code scanning dialog or a keyboard in order to determin the target 
        /// address and will then connect.
        /// </summary>
        public void Connect()
        {
            //WINDOWS UWP Code to scan a qr code
#if WINDOWS_UWP
            if (QrCodeScanner.IsInitialized && QrCodeScanner.Instance.enabled) {
                if (ScanningDialogPrefab == null)
                {
                    Debug.LogError("Scanning dialog prefab is not set.");
                    return;
                }
                _scanningDialog = Instantiate(ScanningDialogPrefab, Camera.main.transform);
                _scanningDialog.transform.localPosition = Vector3.forward;
                _scanningDialog.transform.rotation = Camera.main.transform.rotation; 
                _scanningDialog.GetComponent<QrCodeScanningDialog>().Scanned += result =>
                {
                    if (result == null) return;
                    Destroy(_scanningDialog);
                    ParseQrCode(result);
                };
            } else {
                _keyboard = TouchScreenKeyboard.Open(_address, TouchScreenKeyboardType.Default, false, false, false);
            }
#else
            Provider.Connect();
#endif
        }

        public static void ParseQrCode(string code)
        {
            var snippets = code.Split('|');
            Provider.Address = snippets[0];
            if (snippets.Length > 1)
                Provider.Filter = snippets[1];
            int frameRate = -1;
            if (snippets.Length > 2 && TargetFramerate.IsInitialized && int.TryParse(snippets[2], out frameRate))
                TargetFramerate.Instance.TargetFrameRate = frameRate;
            print("Limited framerate to " + frameRate);
            Provider.Connect();
        }

        /// <summary>
        /// Disconnects the provider.
        /// </summary>
        public void Disconnect()
        {
            Provider.Disconnect();
            if (TargetFramerate.IsInitialized)
                TargetFramerate.Instance.TargetFrameRate = -1;
        }
    }
}