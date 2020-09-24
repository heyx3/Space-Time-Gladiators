using UnityEngine;
using System.Collections;

/// <summary>
/// Mirrors another object, except one world width/height over.
/// </summary>
public class MirrorMesh : MonoBehaviour
{
	public Vector3 Offset;
	public Vector3 OwnerPos;
	
	//Cached references.
	
	public GameObject Owner { get { return meshCopy.gameObject; } }
	MeshFilter meshCopy;
	MeshRenderer rendererCopy;
	Transform transformCopy;
	Collider colliderCopy;
	
	MeshFilter thisMesh;
	MeshRenderer thisRenderer;
	Transform thisTransform;
	Collider thisCollider;
	
	// Use this for initialization
	void Start ()
	{
		//Don't cache the other two references because these have to be manually set first.
		thisTransform = transform;
		thisMesh = GetComponent<MeshFilter>();
		thisRenderer = GetComponent<MeshRenderer>();
		
		transform.parent = WorldConstants.MirrorContainer.transform;
	}
	
	public void SetData(GameObject copy, Vector3 offset)
	{
		//Set the object data.
		meshCopy = copy.GetComponent<MeshFilter>();
		transformCopy = copy.transform;
		rendererCopy = copy.GetComponent<MeshRenderer>();
		
		Offset = offset;
	}
	
	void LateUpdate ()
	{
		if (meshCopy == null)
		{
			Destroy(gameObject);
			return;
		}
		
		OwnerPos = transformCopy.position;
		
		thisTransform.position = transformCopy.position + new Vector3(Offset.x, Offset.y, 0);
		thisTransform.rotation = transformCopy.rotation;
		thisTransform.localScale = transformCopy.localScale;
		
		thisMesh.mesh = meshCopy.mesh;
		thisRenderer.materials = rendererCopy.materials;
		
		gameObject.layer = meshCopy.gameObject.layer;
	}
}
