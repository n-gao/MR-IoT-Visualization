using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using HoloToolkit.Unity;
using IoTVisualization.Measurement;
using IoTVisualization.Networking.EnEffCampus.DataContracts;
using IoTVisualization.Networking;
using IoTVisualization.Networking.Utils;
using IoTVisualization.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;
#if WINDOWS_UWP
using System.Threading.Tasks;
#endif

namespace IoTVisualization.Networking.EnEffCampus
{
    /// <summary>
    /// This singleton implements the provider interface. It implements a simple version of the energy router framework in order to provide simple access to their data.
    /// </summary>
    public class EnergyRouterProvider : Singleton<EnergyRouterProvider>, IProvider
    {
        private const string HistoryRef = "history";
        private const char RefDelimiter = '|';
        public const string Protocol = "ws://";
        /// <summary>
        /// Default port for the energy router ws protocol.
        /// </summary>
        public const int DefaultPort = 8081;
        public event Action Connected;
        public event Action Disconnected;
        public event Action<IDevice> DataSourceAdded;
        public event Action<IDevice> DataSourceRemoved;

        public List<IDevice> Devices
        {
            get { return Root.ToDataSources(); }
        }

        public void RequestData(IDevice device, IAttribute attribute, DateTimeOffset @from, DateTimeOffset to)
        {
            Node node = attribute as Node;
            if (node == null)
            {
                LogError("The given DataSource is no energy router.");
                return;
            }
            History(node, from, to);
        }

        public void RequestPosition(IDevice device)
        {
            Node node = device as Node;
            if (node == null)
            {
                LogError("The given DataSource is no energy router.");
                return;
            }
            if (node.PositionNode == null)
            {
                AllMessagesHandled += () => { RequestPositionInternal(node); };
            }
            else
            {
                RequestPositionInternal(node);
            }
        }

        private void RequestPositionInternal(Node node)
        {
            if (node.PositionNode == null)
            {
                LogError("This energy router does not have a position node.\n" + node);
                return;
            }
            Subscribe(node.PositionNode);
        }

        public void SavePosition(IDevice device, byte[] positionData)
        {
            Node node = device as Node;
            if (node == null)
            {
                LogError("The given DataSource is no energy router.");
                return;
            }
            if (node.PositionNode == null)
            {
                LogError("This energy router does not have a position node." + node);
                return;
            }

#if WINDOWS_UWP
            Task.Factory.StartNew(() =>{
                SendPosition(node.PositionNode, Convert.ToBase64String(positionData));
            });
#else
            SendPosition(node.PositionNode, Convert.ToBase64String(positionData));
#endif
        }

        public void RemovePosition(IDevice device)
        {
            Node node = device as Node;
            if (node == null)
            {
                LogError("The given DataSource is no energy router.");
                return;
            }
            if (node.PositionNode == null)
            {
                LogError("This energy router does not have a position node.");
                print(node);
                return;
            }
            SendPosition(node.PositionNode, "");
        }

        public void Subscribe(IDevice device)
        {
            Node node = device as Node;
            if (node == null) return;
            foreach (var pair in node.Nodes)
                Subscribe(pair.Value as IDevice);
            Subscribe(node);
        }

        public void Unsubscribe(IDevice device)
        {
            Node node = device as Node;
            if (node == null) return;
            foreach (var pair in node.Nodes)
                Unsubscribe(pair.Value as IDevice);
            Unsubscribe(node);
        }
        
        /// <summary>
        /// The root node "/" of the energy router tree.
        /// </summary>
        public Node Root { get; set; }
        /// <summary>
        /// This set to true causes the game to automatically connect to the set address when launching the application.
        /// </summary>
        public bool AutoConnect = true;

        private readonly UnifiedWebSocket _webSocket = new UnifiedWebSocket();
        [SerializeField] private string _adress = Protocol + "localhost" + DefaultPort;
        [SerializeField] private string _filter = "";
        /// <summary>
        /// Filter implementation of IProvider. The filter string can contain multiple filters. 
        /// Each filter is seperated by ';' and every filter consists of a domain and a value which are seperated by "=".
        /// Possible domains are "path", "type" and "name".
        /// </summary>
        public string Filter
        {
            get { return _filter ?? ""; }
            set
            {
                _filter = value;
                //Rebuild the filter list.
                _filters.Clear();
                if (string.IsNullOrEmpty(_filter)) return;
                string[] filters = _filter.Split(';');
                foreach (var filter in filters)
                {
                    string[] filterParts = filter.Split('=');
                    if (filterParts.Length < 2) continue;
                    string domain = filterParts[0];
                    StringBuilder valueBuilder = new StringBuilder();
                    for (int i = 1; i < filterParts.Length; i++)
                        valueBuilder.Append(filterParts[i]);
                    string val = valueBuilder.ToString();
                    switch (domain.ToLower())
                    {
                        case PathFilterDomain:
                            _filters.Add(new FilterPart{Value = val, Filter = PathFilter});
                            break;
                        case TypeFilterDomain:
                            _filters.Add(new FilterPart { Value = val, Filter = TypeFilter });
                            break;
                        case NameFilterDomain:
                            _filters.Add(new FilterPart { Value = val, Filter = NameFilter });
                            break;
                        default:
                            LogWarning("Could not parse filter.\n" + domain + " " + val);
                            break;
                    }
                }
            }
        }

        private class FilterPart
        {
            public string Value = "";
            public Func<Node, string, bool> Filter = (node, val) => true;
        }

        private readonly List<FilterPart> _filters = new List<FilterPart>();

        private const string PathFilterDomain = "path";
        private const string NameFilterDomain = "name";
        private const string TypeFilterDomain = "type";
        private static bool PathFilter(Node node, string value) { return node.Name.StartsWith(value); }
        private static bool NameFilter(Node node, string value) { return node.DisplayName.Contains(value); }
        private static bool TypeFilter(Node node, string value) { return node.Type == value; }

        public string Address
        {
            get { return _adress; }
            set { _adress = value; }
        }

        /// <summary>
        /// This class is used to queue actions which require node manipulation to be executed on the main thread.
        /// </summary>
        private abstract class MessageAction
        {
            public string NodePath;

            public abstract void Execute(Node root);
        }
        /// <summary>
        /// Action which adds a data entry to a node.
        /// </summary>
        private class DataAction : MessageAction
        {
            public IoTData Data;

            public override void Execute(Node root)
            {
                Node node = root.GetOrAddNodeByPath(NodePath);
                node.AddValue(Data);
            }
        }
        /// <summary>
        /// Action which announces metadata to a node.
        /// </summary>
        private class AnnouncementAction : MessageAction
        {
            public string Key;
            public string Value;

            public override void Execute(Node root)
            {
                Node node = root.GetOrAddNodeByPath(NodePath);
                node.AddMetaData(Key, Value);
            }
        }

        
        /// <summary>
        /// This class defines an announcement to a node, which can be either an added announcement or removed announcement.
        /// </summary>
        private struct Announcement
        {
            public enum AnnouncementType
            {
                Added,
                Removed
            }
            
            public readonly AnnouncementType Type;
            public readonly Node Node;
            
            public Announcement(AnnouncementType type, Node node)
            {
                Type = type;
                Node = node;
            }

            public override string ToString()
            {
                return Type + " " + Node.Name;
            }
        }
        /// <summary>
        /// Maxium per frame executed actions.
        /// If this value is too low unintended behaviour could occur.
        /// Values less or equal to 0 are handled as infinity.
        /// </summary>
        public int MaxPerFrameActions = 200;
        /// <summary>
        /// Queued actions to be executed on the game thread.
        /// </summary>
        private readonly ConcurrentQueue<MessageAction> _queuedActions = new ConcurrentQueue<MessageAction>();
        /// <summary>
        /// List of announcements to be done during the next update.
        /// </summary>
        private readonly List<Announcement> _announcements = new List<Announcement>();

        private event Action AllMessagesHandled;
        

        protected override void Awake()
        {
            base.Awake();
            ProviderMonoBehaviour.Provider = this;
            InitRoot();
            _webSocket.OnConnected += WebSocketOnConnected;
            _webSocket.OnClosed += WebSocketOnClosed;
            _webSocket.OnErrorOccured += WebSocketOnErrorOccured;
            _webSocket.OnMessageReceived += WebSocketOnMessageReceived;
        }

        void Start()
        {
#if UNITY_EDITOR
            Filter = _filter;
            if (AutoConnect)
                Connect();
#endif
        }

        private bool _connected = false;

        void Update()
        {
            if (IsConnected != _connected)
            {
                if (IsConnected && Connected != null)
                    Connected();
                if (!IsConnected && Disconnected != null)
                    Disconnected();
                _connected = IsConnected;
            }
            int i = 0;
            int maxActions = MaxPerFrameActions <= 0 ? int.MaxValue : MaxPerFrameActions;
            //Parsing received messages
            while (_queuedActions.Count > 0 && i++ <= maxActions)
                _queuedActions.Dequeue().Execute(Root);
            if (_queuedActions.Count == 0 && AllMessagesHandled != null)
            {
                AllMessagesHandled();
                AllMessagesHandled = null;
            }
            //Calling all announcements
            for (int j = 0; j < _announcements.Count; j++)
            {
                var announcement = _announcements[j];
                if (!IsInFilter(announcement.Node)) continue;
                switch (announcement.Type)
                {
                    case Announcement.AnnouncementType.Added:
                        if (DataSourceAdded != null)
                            DataSourceAdded(announcement.Node);
                        break;
                    case Announcement.AnnouncementType.Removed:
                        if (DataSourceRemoved != null)
                            DataSourceRemoved(announcement.Node);
                        announcement.Node.Clear();
                        break;
                }
            }
            _announcements.Clear();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Disconnect();
            _announcements.Clear();
            _filters.Clear();
            _queuedActions.Clear();
            AllMessagesHandled = null;
            Connected = null;
            DataSourceAdded = null;
            DataSourceRemoved = null;
            Disconnected = null;
        }
        

        /// <summary>
        /// This method defines the applied filter which will be applied to.
        /// </summary>
        /// <param name="node">Node which will be checked.</param>
        protected virtual bool IsInFilter(Node node)
        {
            for (int i = 0; i < _filters.Count; i++)
            {
                var filter = _filters[i];
                if (!filter.Filter(node, filter.Value))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Initializes the root element.
        /// </summary>
        private void InitRoot()
        {
            Root = new Node();
            Root.OnChildAdded += node =>
            {
                if (node.IsDevice)
                    _announcements.Add(new Announcement(Announcement.AnnouncementType.Added, node));
            };
            Root.OnChildRemoved += node =>
            {
                if (node.IsDevice)
                    _announcements.Add(new Announcement(Announcement.AnnouncementType.Removed, node));
            };
        }

        /// <summary>
        /// Tries to connect to the set address.
        /// </summary>
        public void Connect()
        {
            _webSocket.ConnectTo(Address);
            Log("Connecting to " + Address + "...");
        }

        /// <summary>
        /// Disconnects if a connection is open and clears all saved data.
        /// </summary>
        public void Disconnect()
        {
            if (_webSocket.Connected)
                _webSocket.Close();
            Root.Remove();
            InitRoot();
        }
        
        public bool IsConnected { get { return _webSocket.Connected; }}

        /// <summary>
        /// Closes the connection when the application quits.
        /// </summary>
        public void OnApplicationQuit()
        {
            _webSocket.Close();
        }

        /// <summary>
        /// Called whenever a message has been received. At first the message will be deserialized according to the contract
        /// and then enqueued to the list of to be parsed messages.
        /// </summary>
        /// <param name="message">Received message</param>
        private void WebSocketOnMessageReceived(string message)
        {
            try
            {
                try
                {
                    var messages = JsonConvert.DeserializeObject<List<WebRpcContract>>(message);
                    if (messages == null) throw new Exception();
                    for (int i = 0; i < messages.Count; i++)
                    {
                        var contract = messages[i];
                        HandleMessage(contract);
                    }
                }
                catch (Exception)
                {
                    var contract = JsonConvert.DeserializeObject<WebRpcContract>(message);
                    if (contract != null)
                        HandleMessage(contract);
                    else
                        throw;
                }
            }
            catch (Exception)
            {
                LogError("Could not parse message.\n" + message);
            }
        }

        /// <summary>
        /// Handles a received message by their scope.
        /// </summary>
        /// <param name="contract">Message</param>
        private void HandleMessage(WebRpcContract contract)
        {
            switch (contract.Scope)
            {
                case RpcScopes.Node:
                    HandleNodeRespond(contract);
                    break;
                case RpcScopes.Respond:
                    HandleRespond(contract);
                    break;
                case RpcScopes.Global:
                    HandleGlobalRespond(contract);
                    break;
                default:
                    LogError("Unidentified scope for message received!\n" + contract);
                    break;
            }
        }

        private void HandleGlobalRespond(WebRpcContract contract)
        {
            switch (contract.Type)
            {
                default:
                    LogError("Unidentified scope for message received!\n" + contract);
                    break;
            }
        }

        /// <summary>
        /// Hanldes messages which belong to a node.
        /// </summary>
        /// <param name="contract">Message</param>
        private void HandleNodeRespond(WebRpcContract contract)
        {
            try
            {
                switch (contract.Type)
                {
                    case RpcTypes.Data:
                        HandleNodeDataRespond(contract);
                        break;
                    case RpcTypes.Announce:
                        HandleNodeAnnoucement(contract);
                        break;
                }
            }
            catch (Exception e)
            {
                LogError(e.Message + '\n' + e.StackTrace);
            }
        }

        /// <summary>
        /// Handles messages which contains data information.
        /// </summary>
        /// <param name="contract">Message</param>
        private void HandleNodeDataRespond(WebRpcContract contract)
        {
            string nodeUri = contract.Node;
            try
            {
                IoTData data = IoTData.Parse(contract.Args[0], contract.Args[1]);
                _queuedActions.Enqueue(new DataAction
                {
                    Data = data,
                    NodePath = nodeUri
                });
            }
            catch (Exception e)
            {
                LogWarning("Could not parse: " + contract + "\n" + e.Message + "\n" + e.StackTrace + "\n At ");
            }
        }

        /// <summary>
        /// Handles announcement messages.
        /// </summary>
        /// <param name="contract">Message</param>
        private void HandleNodeAnnoucement(WebRpcContract contract)
        {
            string nodeUri = contract.Node;

            var jArray = JArray.FromObject(contract.Args);
            for (int i = 0; i < jArray.Count; i++)
            {
                var jToken = jArray[i];
                foreach (var property in jToken.Children<JProperty>())
                {
                    _queuedActions.Enqueue(new AnnouncementAction
                    {
                        Key = property.Name,
                        Value = property.Value.ToString(),
                        NodePath = nodeUri
                    });
                }
            }
        }

        /// <summary>
        /// Handles respond messages.
        /// </summary>
        /// <param name="contract">Message</param>
        private void HandleRespond(WebRpcContract contract)
        {
            if (contract.Args[1] != null)
                LogError("Error response: " + contract.Args[1]);
            Log("Received response message: " + contract.Args[2]);
            var array = JArray.FromObject(contract.Args);
            string reference = array[0].Value<string>();
            if (reference.StartsWith(HistoryRef))
                ParseHistory(array);
        }

        /// <summary>
        /// Parses the given JArray to data and adds those to the specified node.
        /// </summary>
        /// <param name="args">Message arguments</param>
        private void ParseHistory(JArray args)
        {
            string reference = args[0].Value<string>();
            string nodePath = reference.Remove(0, HistoryRef.Length + 1);
            foreach (var item in args[2])
            {
                var data = IoTData.Parse(item["time"].Value<double>(), item["value"].Value<string>());
                _queuedActions.Enqueue(new DataAction
                {
                    NodePath = nodePath,
                    Data = data
                });
            }
        }

        /// <summary>
        /// Called when an error regarding websockets occurs.
        /// </summary>
        /// <param name="message">Error message</param>
        private void WebSocketOnErrorOccured(string message)
        {
            LogError("An Error occured:\n" + message);
        }

        /// <summary>
        /// Called when the websocket connection has been closed.
        /// </summary>
        /// <param name="code">Code</param>
        /// <param name="reason">Reason</param>
        private void WebSocketOnClosed(ushort code, string reason)
        {
            LogWarning("Connection closed:\n" + code + " : " + reason);
            Disconnect();
        }

        /// <summary>
        /// Called when the websocket connection has been established. It will automatically subscribe to the root node.
        /// </summary>
        private void WebSocketOnConnected()
        {
            Log("Connected to: " + Address);
            SubscribeAnnouncement(Root);
        }
        
        /// <summary>
        /// Sends a subscribe message for the given node.
        /// </summary>
        /// <param name="node">Node to subscribe to</param>
        private void Subscribe(Node node)
        {
            if (node.Subscribed) return;
            _webSocket.SendMessage(new WebRpcContract
            {
                Node = node.Name,
                Scope = RpcScopes.Node,
                Type = RpcTypes.Subscribe
            });
            node.SubscriptionName = node.Name;
            Log("Send subscribe message for: " + node.Name);
        }

        /// <summary>
        /// Sends an unsubscribe message for the given node.
        /// </summary>
        /// <param name="node">Node to unsubscribe to</param>
        private void Unsubscribe(Node node)
        {
            if (!node.Subscribed) return;
            _webSocket.SendMessage(new WebRpcContract
            {
                Node = node.SubscriptionName,
                Scope = RpcScopes.Node,
                Type = RpcTypes.Unsubscribe
            });
            Log("Send unsubscribe message for: " + node.SubscriptionName);
            node.SubscriptionName = null;
        }

        /// <summary>
        /// Sends a subscribe announcement message for the given node.
        /// </summary>
        /// <param name="node">Node</param>
        private void SubscribeAnnouncement(Node node)
        {
            _webSocket.SendMessage(new WebRpcContract
            {
                Node = node.Name,
                Scope = RpcScopes.Node,
                Type = RpcTypes.SubscribeAnnouncement
            });
            Log("Send subscribe_announcement message for: " + node.Name);
        }

        /// <summary>
        /// Transmits the given position to the speficied node.
        /// </summary>
        /// <param name="node">Node</param>
        /// <param name="data">Position data</param>
        private void SendPosition(Node node, string data)
        {
            _webSocket.SendMessage(new WebRpcContract
            {
                Node = node.Name,
                Scope = RpcScopes.Node,
                Type = RpcTypes.SetMrPosition,
                Args = { data }
            });
            Log("Send set_mr_position message for: " + node.Name);
        }
        
        /// <summary>
        /// Requests the history for a given node.
        /// </summary>
        /// <param name="node">Node</param>
        /// <param name="@from">Start time</param>
        /// <param name="to">End time</param>
        private void History(Node node, DateTimeOffset @from, DateTimeOffset to)
        {
            _webSocket.SendMessage(new WebRpcContract
            {
                Node = node.Name,
                Scope = RpcScopes.Node,
                Type = RpcTypes.History,
                Ref = HistoryRef + RefDelimiter + node.Name,
                Args = { new Dictionary<string, string>
                    {
                        {"fromtime", (from - DateTimeHelper.UtcMin).TotalSeconds.ToString("F")},
                        {"totime", (to - DateTimeHelper.UtcMin).TotalSeconds.ToString("F")},
                    }
                }
            });
            Log("Send history message for: " + node.Name);
        }

        private const string LogPrefix = "[EnEff]";
        public static void Log(string message)
        {
#if DEBUG
            Debug.Log(LogPrefix.PadRight(10, ' ') + message);
#endif
        }

        public static void LogWarning(string message)
        {
#if DEBUG
            Debug.LogWarning(LogPrefix.PadRight(10, ' ') + message);
#endif
        }

        public static void LogError(string message)
        {
#if DEBUG
            Debug.LogError(LogPrefix.PadRight(10, ' ') + message);
#endif
        }
    }
}
