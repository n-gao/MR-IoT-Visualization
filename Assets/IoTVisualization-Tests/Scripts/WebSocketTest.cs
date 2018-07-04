using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using IoTVisualization.Networking.EnEffCampus;
using IoTVisualization.Networking.EnEffCampus.DataContracts;
using IoTVisualization.Networking.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class WebSocketTest : MonoBehaviour
{

    public string ServerUri = "ws://localhost:8081";
    readonly UnifiedWebSocket webSocket = new UnifiedWebSocket();

    public Node Root { get; set; }

	// Use this for initialization
	void Start ()
	{
	    Root = new Node();
	    webSocket.OnConnected += () =>
	    {
            webSocket.SendMessage(new WebRpcContract
            {
                Scope = RpcScopes.Node,
                Ref = "/",
                Type = RpcTypes.Subscribe,
            }.ToString());
            print("Connected");
	    };
	    webSocket.OnMessageReceived += message =>
	    {
	        print("Received message :");
	        var messages = JsonConvert.DeserializeObject<List<WebRpcContract>>(message);
	        foreach (var m in messages)
	        {
	            switch (m.Scope)
	            {
                    case RpcScopes.Node:
                        Match nodeMatch = Regex.Match(m.Node, @"^(\/\w*)*");
                        if (!nodeMatch.Success)
                            continue;
                        string nodeAddr = nodeMatch.Value;
                        Match attributeMatch = Regex.Match(m.Node, @"(\.\w*)*$");
                        if (!attributeMatch.Success)
                            continue;
                        string attributeName = attributeMatch.Value;

                        Node node = Root.GetOrAddNodeByPath(nodeAddr);

//                        NodeAttribute attribute;
//                        if (node.Attributes.ContainsKey(attributeName))
//                            attribute = node.Attributes[attributeName];
//                        else
//                            node.Attributes[attributeName] = attribute = new NodeAttribute();
//
//                        attribute.Set(m.Args[0], m.Args[1]);
//                        print(attribute.Values.Last().Value + " " + attribute.Values.Last().Key);

                        break;
	            }
	        }
            print(Root.ToString());
	    };
        //webSocket.ConnectTo(ServerUri);
	}

    public void OnApplicationQuit()
    {
        webSocket.Close();
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
