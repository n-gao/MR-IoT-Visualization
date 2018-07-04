using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using IoTVisualization.Networking.EnEffCampus;
using UnityEditor;
using UnityEngine;

namespace Assets.IoTVisualization.Networking.Scripts.EnEffCampus
{
    /// <summary>
    /// This editor class adds two buttons to the EnergyRouterProvider, the first one allows the ip set to 
    /// your local ip and the second one sets the ip to localhost.
    /// </summary>
    [CustomEditor(typeof(EnergyRouterProvider))]
    public class EnergyRouterProviderEditor : Editor
    {
        public override void OnInspectorGUI() 
        {
            DrawDefaultInspector();
            EnergyRouterProvider myScript = target as EnergyRouterProvider;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Set to localhost"))
            {
                myScript.Address = EnergyRouterProvider.Protocol + "localhost:" + EnergyRouterProvider.DefaultPort;
                EditorUtility.SetDirty(myScript);
            }
            if (GUILayout.Button("Set to local IP"))
            {
                myScript.Address = EnergyRouterProvider.Protocol + Dns.GetHostEntry(Dns.GetHostName()).AddressList
                    .First(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToString() + ":" + EnergyRouterProvider.DefaultPort;
                EditorUtility.SetDirty(myScript);
                
            }
            GUILayout.EndHorizontal();
        }
    }
}
