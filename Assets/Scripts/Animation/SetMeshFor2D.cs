using UnityEngine;
using System.Collections;

/// <summary>
/// Sets this game object's mesh to be a 2D plane 1 unit on each side.
/// </summary>
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshInitializeData))]
public class SetMeshFor2D : MonoBehaviour {

    void Start()
    {
		MeshInitializeData dat = GetComponent<MeshInitializeData>();
        MeshFilter filter = GetComponent<MeshFilter>();

        Mesh mesh = filter.mesh;

		if (name == "Player")
			transform.localScale = transform.localScale;
		
        //Create the quad.
        mesh.Clear();
        //Set the mesh vertices to a 1x1 quad.
        mesh.vertices = new Vector3[4]
		{
			new Vector3(dat.MeshDimensions.x * 0.5f, dat.MeshDimensions.y * 0.5f, 0),
			new Vector3(dat.MeshDimensions.x * 0.5f, -dat.MeshDimensions.y * 0.5f, 0),
			new Vector3(-dat.MeshDimensions.x * 0.5f, dat.MeshDimensions.y * 0.5f, 0),
			new Vector3(-dat.MeshDimensions.x * 0.5f, -dat.MeshDimensions.y * 0.5f, 0),
		};
		transform.localScale = transform.localScale;
		//transform.localScale = new Vector3(dat.MeshDimensions.x, dat.MeshDimensions.y, 0.0f);

        //Set the two triangle indices.
        mesh.triangles = new int[6]
		{
			0, 1, 2,
			2, 1, 3,
		};

        //Set the uv coordinates.
        mesh.uv = new Vector2[4]
		{
			new Vector2(1.0f, 1.0f),
			new Vector2(1.0f, 0.0f),
			new Vector2(0.0f, 1.0f),
			new Vector2(0.0f, 0.0f),
		};
    }

    public enum ReflectDir
    {
        Left,
        Right,
    }

    public void SetDir(ReflectDir dir)
    {
        GetComponent<MeshFilter>().mesh.uv = new Vector2[4]
            {
                new Vector2((dir == ReflectDir.Right) ? 1.0f : 0.0f, 1.0f),
                new Vector2((dir == ReflectDir.Right) ? 1.0f : 0.0f, 0.0f),
                new Vector2((dir == ReflectDir.Right) ? 0.0f : 1.0f, 1.0f),
                new Vector2((dir == ReflectDir.Right) ? 0.0f : 1.0f, 0.0f),
            };
    }
}
