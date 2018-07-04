using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using HoloToolkit.Unity;
using HoloToolkit.Unity.SpatialMapping;
using IoTVisualization.Desktop;
using IoTVisualization.Localization;
using IoTVisualization.Measurement;
using IoTVisualization.Networking;
using IoTVisualization.Networking.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
#if WINDOWS_UWP
using System.Text.RegularExpressions;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
#else
using WebSocketSharp;
using WebSocketSharp.Server;
#endif

namespace IoTVisualization.Utils
{
    public class ControlHttpServer : ProviderMonoBehaviour
    {
        private const int DemoLayer = 1 << 8;
        private GameObject _ui;

        void OnEnable()
        {
            _ui = GameObject.Find("Ui");
            OpenServer();
        }

        void OnDisable()
        {
            CloseServer();
        }

        void OnApplicationQuit()
        {
            CloseServer();
        }

#if WINDOWS_UWP

        private StreamSocketListener _listener = new StreamSocketListener();

        public async void OpenServer()
        {
            _listener = new StreamSocketListener();
            _listener.ConnectionReceived += async (sender, args) =>
            {
                string message = "";
                using (IInputStream input = args.Socket.InputStream)
                {
                    byte[] data = new byte[8192];
                    IBuffer buffer = data.AsBuffer();
                    uint dataRead = 8192;
                    StringBuilder requestBuilder = new StringBuilder();
                    while (dataRead == 8192)
                    {
                        await input.ReadAsync(buffer, buffer.Capacity, InputStreamOptions.Partial);
                        requestBuilder.Append(Encoding.UTF8.GetString(data, 0, data.Length));
                        dataRead = buffer.Length;
                    }
                    message = requestBuilder.ToString();
                }

                string result = "Failure";
                if (!string.IsNullOrEmpty(message))
                {
                    string request = message.Split('\n')[0];
                    string url = request.Split(' ')[1];
                    var parameters = Regex.Matches(url, @"[^&?]*?=[^&?]*");
                    foreach (Match parameter in parameters)
                    {
                        var parts = parameter.Value.Split('=');
                        var key = parts[0];
                        var value = parts[1];
                        if (key == "framerate")
                            SetFramerate(value);
                        if (key == "address")
                            ConnectTo(value);
                        if (key == "disconnect" && value == "1")
                            Disconnect();
                        if (key == "visibility")
                            SetVisibility(value);
                        if (key == "count")
                            SetDeviceCount(value);
                        if (key == "set")
                            SetSet(value);
                        if (key == "scene")
                            SetScene(value);
                    }
                    result = "Success";
                }

                using (IOutputStream output = args.Socket.OutputStream)
                {
                    using (Stream response = output.AsStreamForWrite())
                    {
                        var content = Encoding.UTF8.GetBytes(result);
                        string header = String.Format("HTTP/1.1 200 OK\r\n" +
                                                      "Content-Length: {0}\r\n" +
                                                      "Connection: close\r\n\r\n",
                            content.Length);
                        var headerBytes = Encoding.UTF8.GetBytes(header);
                        await response.WriteAsync(headerBytes, 0, headerBytes.Length);
                        await response.WriteAsync(content, 0, content.Length);
                        await response.FlushAsync();
                    }
                }
            };
            await _listener.BindServiceNameAsync("8080");
            Debug.Log("Started server on 8080");
        }

        public void CloseServer()
        {
            _listener?.Dispose();
            _listener = null;
        }

#else
        private HttpServer _server;

        public void OpenServer()
        {
            _server = new HttpServer(IPAddress.Any, 8080);
            _server.OnGet += (sender, args) =>
            {
                print("Received request : " + args.Request.Url.ToString());
                string scene = args.Request.QueryString.Get("scene");
                SetScene(scene);
                string frate = args.Request.QueryString.Get("framerate");
                SetFramerate(frate);
                string address = args.Request.QueryString.Get("address");
                ConnectTo(address);
                if (args.Request.QueryString.Get("disconnect") == "1")
                    Disconnect();
                string visibility = args.Request.QueryString.Get("visibility");
                SetVisibility(visibility);
                string deviceCount = args.Request.QueryString.Get("count");
                SetDeviceCount(deviceCount);
                string set = args.Request.QueryString.Get("set");
                SetSet(set);
                args.Response.WriteContent(Encoding.UTF8.GetBytes("Success"));
            };
            _server.Start();
            Debug.Log("Started server on 8080");
        }

        public void CloseServer()
        {
            if (_server != null)
                _server.Stop();
            _server = null;
        }
#endif
        private void SetScene(string scene)
        {
            if (string.IsNullOrEmpty(scene)) return;
            int sceneInd = 0;
            if (int.TryParse(scene, out sceneInd))
                Execute(() => SceneManager.LoadScene(sceneInd));
        }

        private void SetSet(string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            var devices = Provider.Devices.Where(d => d.MetaData.ContainsKey("set") && d.MetaData["set"] == value).ToList();
            var toHide = Provider.Devices;
            toHide.RemoveAll(d => devices.Contains(d));
            var manager = DeviceObjectManager.Instance;
            Execute(() =>
            {
                foreach (IDevice d in toHide)
                {
                    if (manager.Objects.ContainsKey(d))
                        manager.Objects[d].SetActive(false);
                    if (UiDeviceColoumn.Instance != null)
                        UiDeviceColoumn.Instance.RemoveDevice(d);
                }
                foreach (IDevice d in devices)
                {
                    if (manager.Objects.ContainsKey(d))
                        manager.Objects[d].SetActive(true);
                    if (UiDeviceColoumn.Instance != null)
                        UiDeviceColoumn.Instance.AddDevice(d);
                }
            });
        }

        private void SetVisibility(string visibility)
        {
            if (visibility == "0")
                Execute(() => {
                    Camera.main.cullingMask = 0;
                    if (_ui != null)
                        _ui.SetActive(false);
                });
            if (visibility == "1")
                Execute(() => {
                    Camera.main.cullingMask = ~DemoLayer;
                    if (_ui != null)
                        _ui.SetActive(true);
                });
            if (visibility == "2")
                Execute(() =>
                {
                    Camera.main.cullingMask = DemoLayer | SpatialMappingManager.Instance.LayerMask;
                    if (_ui != null)
                        _ui.SetActive(false);
                });
        }
        
        private void SetFramerate(string value)
        {
            int framerate;
            if (value != null && int.TryParse(value, out framerate))
                TargetFramerate.SetFrameRate(framerate);
        }

        private void Disconnect()
        {
            Execute(() => Provider.Disconnect());
        }

        private void ConnectTo(string address)
        {
            if (Provider.IsConnected || address == null) return;
            Execute(() =>
            {
                Provider.Address = address;
                Provider.Connect();
            });
        }

        private void SetDeviceCount(string value)
        {
            int count;
            if (value != null && int.TryParse(value, out count))
                Execute(() => DeviceDeactivator.SetActiveDeviceCount(count));
        }

        private void Execute(Action toDo)
        {
            AsyncUtil.Instance.Enqueue(toDo);
        }
    }
}
