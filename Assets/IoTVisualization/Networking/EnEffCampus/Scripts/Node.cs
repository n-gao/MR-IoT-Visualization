using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using IoTVisualization.Networking;
using IoTVisualization.Networking.Utils;
using IoTVisualization.Utils;
using NUnit.Framework.Interfaces;
using UnityEngine;

namespace IoTVisualization.Networking.EnEffCampus
{
    /// <summary>
    /// This node represents an Energy Router. It implements IDevice and IAttribute at the same time in order to fit the Energy Router framework.
    /// But it will always only interpreted as one of these depending on its metadata. Just when it is marked as device it will be interpreteda as
    /// device. Otherwise it will be an attribute of the next higher device.
    /// </summary>
    public class Node : IDevice, IAttribute
    {
        private const string VisualizationOrderKey = "visualization_order";
        private const string PositionType = "position.info";

        public event Action<string, string> MetaDataModified;
        public event Action<IoTData> ValueModified;
        public event Action<byte[]> PositionAvailable;

        public string Name
        {
            get { return GetName(null); }
        }

        public string DisplayName
        {
            get { return Regex.Replace(SuperNode == null ? "/" : NameToParent.Remove(0, 1), @"\.\w+", ""); }
        }
        
        /// <summary>
        /// Key of the parent under which this node is saved.
        /// </summary>
        private string NameToParent
        {
            get { return SuperNode == null ? "" : SuperNode.Nodes.First(n => n.Value == this).Key; }
        }

        public string AttributeName { get { return NameToDevice; } }

        public Dictionary<string, string> MetaData { get; private set; }
        /// <summary>
        /// This regex is used to split a path into single steps.
        /// </summary>
        private const string NodeNameRegex = @"^\/[^\/]*";
        /// <summary>
        /// This regex is used to split the type of an object into its parts.
        /// </summary>
        private const string TypeRegex = @"\w+";


        public string Type
        {
            get
            {
                if (MetaData.ContainsKey("type"))
                    return MetaData["type"];
                else
                    return Regex.Replace(AttributeName, NodeNameRegex, "");
            }
        }

        public string ContentType
        {
            get
            {

                var matches = Regex.Matches(Type, TypeRegex);
                return matches.Count > 0 ? matches[0].Value : "none";
            }
        }

        public string Format
        {
            get
            {
                var matches = Regex.Matches(Type, TypeRegex);
                return matches.Count > 1 ? matches[1].Value : "none";
            }
        }

        public IEnumerable<IAttribute> Atrributes
        {
            get { return _allChilds.Where(c => c.Value.HasValue).Select(c => (IAttribute)c.Value); }
        }

        public IoTData LatestValue
        {
            get { return Store.Latest; }
        }

        public bool HasValue
        {
            get { return Store.Any(); }
        }
        
        private AttributeStore Store { get; set; }
        
        public IoTData OldestValue
        {
            get { return Store.Oldest; }
        }

        public IEnumerable<IoTData> Data { get { return Store.Values; } }
        public IoTData[] DataFromTo(DateTimeOffset start, DateTimeOffset end)
        {
            return Store.GetDataFromTo(start, end);
        }

        public IoTData[] DataFromToInclusive(DateTimeOffset start, DateTimeOffset end)
        {
            return Store.GetDataFromToInclusive(start, end);
        }

        public IoTData LatestValueBefore(DateTimeOffset time)
        {
            return Store.LatestValueBefore(time);
        }

        public IoTData EarliestValueAfter(DateTimeOffset time)
        {
            return Store.EarliestValueAfter(time);
        }

        public IEnumerable<IoTData> Values
        {
            get { return _allChilds.SelectMany(n => n.Value.Store.Values); }
        }

        public event Action<Node> OnChildAdded;
        public event Action<Node> OnChildRemoved;
        public IEnumerable<IHub> Childs { get { return Nodes.Cast<IHub>(); } }
        public IHub Parent { get { return SuperNode; } }

        /// <summary>
        /// Next higher node in hirachy.
        /// </summary>
        public Node SuperNode { get; private set; }
        /// <summary>
        /// Dictionary containing all child nodes by their names.
        /// </summary>
        public Dictionary<string, Node> Nodes { get; private set; }
        /// <summary>
        /// Dictionary of all childs and their chidls, etc. by their name.
        /// </summary>
        private readonly Dictionary<string, Node> _allChilds = new Dictionary<string, Node>();

        /// <summary>
        /// Returns the relative name to the given node.
        /// The given node needs to be an ancestor of this node.
        /// </summary>
        /// <param name="n">Ancestor</param>
        /// <returns></returns>
        private string NameTo(Node n)
        {
            string result = DisplayName;
            Node current = SuperNode;
            while (current != null && current != n)
            {
                result = current.DisplayName + result;
                current = current.SuperNode;
            }
            return result;
        }

        /// <summary>
        /// Relative name to the first device ancestor.
        /// </summary>
        public string NameToDevice
        {
            get
            {
                return IsDevice || SuperNode == null ? "" : SuperNode.NameToDevice + NameToParent;
            }
        }

        /// <summary>
        /// Relative name to the root node.
        /// </summary>
        public string FullPath
        {
            get
            {
                if (SuperNode == null)
                    return "/";
                return SuperNode.FullPath + '/' + FullPath;
            }
        }

        /// <summary>
        /// Depth of this node. Root has 0.
        /// </summary>
        private int Depth
        {
            get
            {
                if (SuperNode == null)
                    return 0;
                return SuperNode.Depth + 1;
            }
        }

        private bool _isDevice = false;

        /// <summary>
        /// Defines whether this node is a device.
        /// Settings this variable automatically implies announcing/unannouncing this node.
        /// </summary>
        public bool IsDevice
        {
            get { return _isDevice; }
            set
            {
                bool oldValue = _isDevice;
                _isDevice = value;
                if (value)
                {
                    ValueModified -= RedirectValueModified;
                    ValueModified += AnnouncePosition;
                }
                else
                {
                    ValueModified += RedirectValueModified;
                    ValueModified -= AnnouncePosition;
                }
                if (value != oldValue)
                {
                    if (SuperNode != null)
                    {
                        if (value)
                            SuperNode.OnChildAdded(this);
                        else
                            SuperNode.OnChildRemoved(this);
                    }
                }
            }
        }

        /// <summary>
        /// Indicates whether this node is an attribute.
        /// </summary>
        public bool IsAttribute
        {
            get { return !IsDevice; }
        }

        /// <summary>
        /// Defines the priortiy of this attribute in order to specify an order for the visualizations.
        /// </summary>
        public int? VisualizationOrder
        {
            get
            {
                if (MetaData.ContainsKey(VisualizationOrderKey))
                {
                    return int.Parse(MetaData[VisualizationOrderKey]);
                }
                else
                {
                    return null;
                }
            }
        }

        public void Reset()
        {
            Store.Reset();
        }

        public bool HasPosition
        {
            get { return PositionNode != null && PositionNode.HasValue; }
        }

        /// <summary>
        /// Indicates whether this node holds information about the position of an object.
        /// </summary>
        private bool IsPositionNode
        {
            get { return Type == PositionType; }
        }
        
        public byte[] SavedPosition
        {
            get { return HasPosition ? Convert.FromBase64String(PositionNode.LatestValue.StringValue) : null; }
            set {
                if (PositionNode != null)
                {
                    PositionNode.Store.Clear();
                    if (value != null)
                        PositionNode.AddValue(new IoTData{Time = DateTimeOffset.UtcNow, StringValue = Convert.ToBase64String(value) });
                }}
        }

        /// <summary>
        /// Returns the first child which holds position informations.
        /// </summary>
        public Node PositionNode
        {
            get
            {
                if (IsPositionNode)
                    return this;
                if (Nodes.Any())
                {
                    var posNode = Nodes.FirstOrDefault(n => n.Value.PositionNode != null).Value;
                    return posNode == null ? null : posNode.PositionNode;
                }
                return null;
            }
        }
        
        /// <summary>
        /// Indicates whether this node has been subscribed to.
        /// </summary>
        public bool Subscribed
        {
            get { return SubscriptionName != null; }
        }

        /// <summary>
        /// Name at the moment this node has been subscribed.
        /// </summary>
        public string SubscriptionName;

        /// <summary>
        /// Constructor which initializes some fields.
        /// </summary>
        public Node()
        {
            Nodes = new Dictionary<string, Node>();
            MetaData = new AttributeDictionary();
            Store = new AttributeStore();
            IsDevice = false;
            OnChildAdded += node => _allChilds[node.NameTo(this)] = node;
            OnChildRemoved += node => _allChilds.Remove(node.NameTo(this));
        }

        /// <summary>
        /// Constructor used when creating childs.
        /// </summary>
        /// <param name="superNode">Parent</param>
        public Node(Node superNode)
            : this()
        {
            SuperNode = superNode;
            OnChildAdded += node =>
            {
                if (SuperNode.OnChildAdded != null)
                    SuperNode.OnChildAdded(node);
            };
            OnChildRemoved += node =>
            {
                if (SuperNode.OnChildRemoved != null)
                    SuperNode.OnChildRemoved(node);
            };
        }
        

        /// <summary>
        /// Overloads the AddValue function and parses the given object using IoTData.Parse.
        /// </summary>
        /// <param name="timeObject">Time object</param>
        /// <param name="value">Value object</param>
        public void AddValue(object timeObject, object value)
        {
            AddValue(IoTData.Parse(timeObject, value));
        }

        /// <summary>
        /// Sets this as attribute and saves the data.
        /// </summary>
        /// <param name="args">Data to be added</param>
        public void AddValue(IoTData args)
        {
            args.Attribute = this;
            SaveValue(args);
        }

        /// <summary>
        /// Saves the given data.
        /// </summary>
        /// <param name="args">Data</param>
        private void SaveValue(IoTData args)
        {
            Store.Add(args);
            if (ValueModified != null)
                ValueModified(args);
        }

        /// <summary>
        /// Listener to ValueModified which checks if the new data contains position informations.
        /// </summary>
        /// <param name="data">New data</param>
        private void AnnouncePosition(IoTData data)
        {
            if (data.Attribute.Type == PositionType)
            {
                if (string.IsNullOrEmpty(data.StringValue))
                    return;
                if (PositionAvailable != null)
                    PositionAvailable(SavedPosition);
            }
        }

        /// <summary>
        /// Listener to ValueModified which calls ValueModified at the parent node if present.
        /// </summary>
        /// <param name="args"></param>
        private void RedirectValueModified(IoTData args)
        {
            if (SuperNode == null) return;
            if (SuperNode.ValueModified != null) SuperNode.ValueModified(args);
        }

        /// <summary>
        /// Used to add meta data to this node.
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        public void AddMetaData(string key, string value)
        {
            MetaData[key] = value;
            if (MetaDataModified != null)
                MetaDataModified(key, value);
            if (key == "type" && value == "device.object")
                IsDevice = true;
        }
        
        /// <summary>
        /// Used to create a new child with the given name.
        /// </summary>
        /// <param name="name">Name of the child</param>
        /// <returns>Child</returns>
        private Node AddChild(string name)
        {
            Node child = Nodes[name] = new Node(this);
            Debug.Log("[EnEff]New node: " + child.Name);
            if (OnChildAdded != null) OnChildAdded(child);
            return child;
        }

        /// <summary>
        /// Creates a tree by the given path.
        /// </summary>
        /// <param name="path">Path</param>
        /// <returns>Root</returns>
        public static Node CreateByPath(string path)
        {
            Node root = new Node();
            Node current = root;
            string next = GetNextNode(path, out path);
            while (next != null)
            {
                current = current.AddChild(next);
                next = GetNextNode(path, out path);
            }
            return root;
        }

        /// <summary>
        /// Returns the node by its relative path.
        /// </summary>
        /// <param name="path">Relative path from this node.</param>
        /// <returns>Child</returns>
        public Node GetNodeByPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return this;
            string child = GetNextNode(path, out path);
            if (child == null)
                return this;
            return !Nodes.ContainsKey(child)
                ? null
                : Nodes[child].GetNodeByPath(path);
        }

        /// <summary>
        /// Returns the node by its relative path. This method creates non existing missing nodes.
        /// </summary>
        /// <param name="path">Relative path from this node.</param>
        /// <returns>Child</returns>
        public Node GetOrAddNodeByPath(string path)
        {
            string next = GetNextNode(path, out path);
            if (next == null)
                return this;
            return Nodes.ContainsKey(next)
                ? Nodes[next].GetOrAddNodeByPath(path)
                : AddChild(next).GetOrAddNodeByPath(path);
        }

        /// <summary>
        /// Returns the full name of a given child. If child == null this method returns the name of this node.
        /// </summary>
        /// <param name="child">Child</param>
        /// <returns>Name of the child</returns>
        private string GetName(Node child)
        {
            if (child == null)
                return SuperNode == null ? "/" : SuperNode.GetName(this);
            string result = Nodes.First(n => n.Value == child).Key;
            return SuperNode == null ? result : SuperNode.GetName(this) + result;
        }

        public override string ToString()
        {
            string result = NameToParent + "\n";
            foreach (var node in Nodes)
            {
                for (int i = 0; i < Depth * 4; i++)
                    result += ' ';
                result += node.Key + "\n";
                result += node.Value.ToString();
            }
            return result;
        }

        /// <summary>
        /// Help function in order to split a path in its parts.
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="remaining">Remaining path</param>
        /// <returns>Name of the first node.</returns>
        private static string GetNextNode(string path, out string remaining)
        {
            Match match = Regex.Match(path, NodeNameRegex);
            if (match.Success)
            {
                remaining = path.Remove(0, match.Length);
                return match.Value;
            }
            remaining = "";
            return null;
        }

        public List<IDevice> ToDataSources()
        {
            List<IDevice> result = new List<IDevice>();
            if (IsDevice)
                result.Add(this);
            foreach (var node in Nodes)
                result.AddRange(node.Value.ToDataSources());
            return result;
        }
        
        /// <summary>
        /// Use this to remove this node and all child nodes.
        /// </summary>
        public void Remove()
        {
            foreach (var node in Nodes)
                node.Value.Remove();
            if (SuperNode != null && IsDevice && SuperNode.OnChildRemoved != null)
                SuperNode.OnChildRemoved(this);
        }

        /// <summary>
        /// Use this to clear all saved data.
        /// Should be called after remove
        /// </summary>
        public void Clear()
        {
            foreach (var node in Nodes)
                node.Value.Clear();
            MetaData.Clear();
            Store.Clear();
            Nodes.Clear();
            ValueModified = null;
            OnChildAdded = null;
            OnChildRemoved = null;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}