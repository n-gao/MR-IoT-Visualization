using System.Collections;
using System.Collections.Generic;
using IoTVisualization.Visualization;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LineChartController))]
public class LineChartControllerEditor : Editor {

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
//        LineChartController controller = target as LineChartController;
//        if (GUILayout.Button("Play/Pause"))
//        {
//            controller.IsRealtime = !controller.IsRealtime;
//        }
    }
}
