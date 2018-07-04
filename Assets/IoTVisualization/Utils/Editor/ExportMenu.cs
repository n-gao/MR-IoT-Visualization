using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ExportMenu : MonoBehaviour {

    [MenuItem("Export/Study")]
    public static void ExportStudy()
    {
        PlayerSettings.productName = "Study";
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            target = BuildTarget.WSAPlayer,
            targetGroup = BuildTargetGroup.WSA,
            scenes = new[] { "Assets/IoTVisualization/Study/Scenes/StudyScene.unity" },
            locationPathName = "/D3D/Study"
        };
        BuildPipeline.BuildPlayer(options);
    }

    [MenuItem("Export/EnEffCampus")]
    public static void ExportEnEff()
    {
        PlayerSettings.productName = "EnEffCampus";
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            target = BuildTarget.WSAPlayer,
            scenes = new[] { "Assets/IoTVisualization/Networking/EnEffCampus/Scenes/EnEffCampusVisualization.unity" },
            locationPathName = "/D3D/EnEffCampus"
        };
        BuildPipeline.BuildPlayer(options);
    }
}
