using UnityEngine;
using System.Collections;

using Consts = WorldConstants;

[RequireComponent(typeof(IDData))]
public class Wrap : MonoBehaviour
{
	Transform Cam, Self;
	CreateLevel GenData;
	
	void Start ()
	{
		//Get the camera if this is a player.
		if (gameObject.name.Contains ("Player"))
		{
			IDData id = GetComponent<IDData>();
			string s = id.PlayerID + " Camera";
			GameObject o = GameObject.Find(s);
			Cam = o.transform;
		}
		
		Self = transform;
		GenData = WorldConstants.MatchWrapper.GetComponent<CreateLevel>();
	}
	
	void FixedUpdate ()
	{
		if (GenData.LevelGen.GenSettings.WrapX && Self.position.x < 0)
        {
			Self.Translate(Consts.Size.x, 0, 0);

            if (Cam != null)
            {
                Cam.GetComponent<CameraFollowScript>().Teleport(Consts.Size.x, 0);
            }
		}

        if (GenData.LevelGen.GenSettings.WrapY && Self.position.y < 0)
        {
			Self.Translate (0, Consts.Size.y, 0);

            if (Cam != null)
            {
                Cam.GetComponent<CameraFollowScript>().Teleport(0, Consts.Size.y);
            }
		}

        if (GenData.LevelGen.GenSettings.WrapX && Self.position.x > Consts.Size.x)
        {
            Self.Translate(-Consts.Size.x, 0, 0);

            if (Cam != null)
            {
                Cam.GetComponent<CameraFollowScript>().Teleport(-Consts.Size.x, 0);
            }
        }

        if (GenData.LevelGen.GenSettings.WrapY && Self.position.y > Consts.Size.y)
        {
			Self.Translate (0, -Consts.Size.y, 0);

            if (Cam != null)
            {
                Cam.GetComponent<CameraFollowScript>().Teleport(0, -Consts.Size.y);
            }
		}
	}
}
