using UnityEngine;

public class LockZPos : MonoBehaviour
{
	public float zValue = 0.0f;
	
	void Update ()
	{
		Vector3 pos = transform.position;
		transform.position = new Vector3(pos.x, pos.y, zValue);
	}
}