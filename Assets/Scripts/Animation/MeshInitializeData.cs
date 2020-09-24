using UnityEngine;
using System.Collections;

/// <summary>
/// Used for initializing the drawing quad for an object.
/// </summary>
public class MeshInitializeData : MonoBehaviour
{
	public bool SetMeshSizeOnStartup = false;
	public Vector2 MeshDimensions = new Vector2(1.0f, 1.0f);
	
	public void Awake() {
		if (SetMeshSizeOnStartup)
			MeshDimensions = new Vector2(transform.localScale.x, transform.localScale.y);
	}
}

