using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using IoTVisualization.Networking.EnEffCampus;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ButtonGenerator))]
public class ButtonGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        ButtonGenerator myScript = target as ButtonGenerator;
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate"))
        {
            myScript.Generate();
        }
        GUILayout.EndHorizontal();
    }
}
