using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Examples.InteractiveElements;
using HoloToolkit.Unity;
using UnityEngine;

public class ButtonGenerator : MonoBehaviour
{

    public string Label;
    public Vector3 Size;
    public Vector3 Margin;
    public GameObject ButtonPrefab;

    public void Generate()
    {
        GameObject result = Instantiate(ButtonPrefab);
        GameObject label = result.transform.Find("ButtonOutline").Find("Label").gameObject;
        var labelTheme = result.GetComponent<LabelTheme>();
        label.GetComponent<TextMesh>().text = labelTheme.Default = Label;

        GameObject outline = result.transform.Find("ButtonOutline").gameObject;
        var mesh = outline.GetComponent<MeshFilter>().mesh = new Mesh();
        mesh.vertices = new Vector3[]
        {
            //outer cube
            Size.Mul(new Vector3(-1, -1, 1)),
            Size.Mul(new Vector3(1, -1, 1)),
            Size.Mul(new Vector3(1, 1, 1)),
            Size.Mul(new Vector3(-1, 1, 1)),
            Size.Mul(new Vector3(-1, 1, -1)),
            Size.Mul(new Vector3(-1, -1, -1)),
            Size.Mul(new Vector3(1, -1, -1)),
            Size.Mul(new Vector3(1, 1, -1)),


        };
        mesh.triangles = new[]
        {
            //outer cube
            1, 2, 4,

        };
    }
}
