using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace IoTVisualization.Networking.EnEffCampus.DataContracts
{
    /// <summary>
    /// This contract defines the communcation using the energy router RPC protocol. It describes each package that is send using the protocol.
    /// For further documentation take a look at the energy router repository.
    /// </summary>
    public class WebRpcContract
    {
        /// <summary>
        /// Target scope of this message. All possible values are members of the RpcScopes class.
        /// </summary>
        public string Scope = "";
        /// <summary>
        /// Defines the targeted node. For example: / or /Room112/lamp
        /// </summary>
        public string Node = "";
        /// <summary>
        /// Defines the type of the message. All possible values are members of the RpcTypes class.
        /// </summary>
        public string Type = "";
        /// <summary>
        /// Reference id which can be added to the message and should be included in the return as well in order to identify responses.
        /// </summary>
        public string Ref = "";
        /// <summary>
        /// List of arguments which can be everything which is serializable.
        /// </summary>
        public List<object> Args = new List<object>();

        public override string ToString()
        {
            JObject result = new JObject();
            result["scope"] = Scope;
            result["node"] = Node;
            result["type"] = Type;
            result["ref"] = Ref;
            JArray args = new JArray();
            for (int i = 0; i < Args.Count; i++)
            {
                var arg = Args[i];
                try
                {
                    try
                    {
                        args.Add(arg);
                    }
                    catch (Exception)
                    {
                        args.Add(JObject.FromObject(arg));
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Could not add arg\n" + e.Message + "\n" + e.StackTrace);
                }
            }
            result["args"] = args;
            return result.ToString();
        }
    }
}
