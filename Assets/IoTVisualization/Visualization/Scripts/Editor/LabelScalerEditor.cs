using System.Collections;
using System.Collections.Generic;
using IoTVisualization.Visualization;
using UnityEditor;
using UnityEngine;

namespace IoTVisualization.Visualization.Editor
{
    [CustomEditor(typeof(LabelScaler))]
    public class LabelScalerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            LabelScaler myScript = target as LabelScaler;
            if (GUILayout.Button("Set"))
                myScript.AdjustScale();
        }
    }
}
