using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairQuad : MonoBehaviour
{

    public Mesh Mesh;

	// Use this for initialization
	void Start ()
	{
	    List<Vector3> verticies = new List<Vector3>
	    {
            new Vector3(-.5f, -.5f, 0),
            new Vector3(-.5f, .5f, 0),
            new Vector3(.5f, .5f, 0),
            new Vector3(.5f, -.5f, 0),
	    };
        List<int> triangles = new List<int>
        {
            0, 1, 2,
            2, 3, 0
        };
        List<Vector3> normals = new List<Vector3>
        {
            new Vector3(0, 0, -1),
            new Vector3(0, 0, -1),
            new Vector3(0, 0, -1),
            new Vector3(0, 0, -1)
        };
        List<Vector2> uv = new List<Vector2>
        {
            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(1, 0),
        };
	    Mesh.Clear();
	    Mesh.vertices = verticies.ToArray();
	    Mesh.triangles = triangles.ToArray();
	    Mesh.normals = normals.ToArray();
	    Mesh.uv = uv.ToArray();
    }
	
	// Update is called once per frame
	void Update ()
	{
	}
}
