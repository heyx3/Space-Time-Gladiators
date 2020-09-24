using UnityEngine;
using System.Collections;

public class ParallaxBehavior : MonoBehaviour
{
	public float MinParallaxStrength = 0.1f;
	public float MaxParallaxStrength = 0.8f;
    public float ParallaxStrength
    {
        get;
        private set;
    }
		
	private Transform cam;
	private CameraFollowScript camFollow;
	private Vector3 lastCamPos;

    public Transform MyTransf;

	private Vector2 totalOffset;

    void Awake()
    {
        MyTransf = transform;
    }

	void Start()
	{
		ParallaxStrength = MinParallaxStrength + (Random.value * (MaxParallaxStrength - MinParallaxStrength));
        MyTransf.position = new Vector3(MyTransf.position.x, MyTransf.position.y, ParallaxStrength);
	}
	
	private float xWrapped = 0.0f, yWrapped = 0.0f;
	void LateUpdate ()
	{
		//Don't do anything if there's no camera to follow.
		if (cam == null) return;
		
		//Get the change of the camera pos since the last update.
		Vector3 cDelta = cam.position - lastCamPos;
		
		//Take out any wrapping around that happened.
		cDelta -= new Vector3(xWrapped, yWrapped, 0.0f);
		
		//Move with parallax.
		Vector3 delta = cDelta * ParallaxStrength;
		totalOffset += (Vector2)(delta - cDelta);
        MyTransf.position += (Vector3)(Vector2)delta;
		
		//Wrap around if the player is nearing the edge of this parallax item.
		if (Mathf.Abs (totalOffset.x) >= WorldConstants.Size.x)
		{
			float xFix = StateMachine.Sign (totalOffset.x, 0.0f) * WorldConstants.Size.x;
			totalOffset.x -= xFix;

            MyTransf.position += new Vector3(-xFix, 0.0f, 0.0f);
		}
		if (Mathf.Abs (totalOffset.y) >= WorldConstants.Size.y)
		{
			float yFix = StateMachine.Sign (totalOffset.y, 0.0f) * WorldConstants.Size.y;
			totalOffset.y -= yFix;

            MyTransf.position += new Vector3(0.0f, -yFix, 0.0f);
		}
		
		//Get the new cam position.
		lastCamPos = cam.position;
		
		//Reset wrapping data.
		xWrapped = 0.0f;
		yWrapped = 0.0f;
	}
	
	public void Wrap(float xAmount, float yAmount)
	{
        MyTransf.position += new Vector3(xAmount, yAmount, 0.0f);
		
		xWrapped = xAmount;
		yWrapped = yAmount;
	}
	
	/// <summary>
	/// Sets the layer number for this parallax movement.
	/// </summary>
	public void SetLayer(int i)
	{
		gameObject.layer = LayerMask.NameToLayer("Background " + i.ToString ());
		
		cam = GameObject.Find (i.ToString () + " Camera").transform;
		camFollow = cam.GetComponent<CameraFollowScript>();
		camFollow.ParallaxFollowers.Add (this);
		
		lastCamPos = cam.transform.position;
		
		totalOffset = Vector2.zero;
	}
}

