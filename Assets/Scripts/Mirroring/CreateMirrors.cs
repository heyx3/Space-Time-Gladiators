using UnityEngine;
using System.Collections;

using Constants = WorldConstants;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class CreateMirrors : MonoBehaviour
{
    static CreateLevel LevelGen = null;
    static PrefabCreator creator = null;

    void Start()
    {
        //Get the cached references.
        if (LevelGen == null)
        {
            GameObject g = WorldConstants.MatchWrapper;
            LevelGen = g.GetComponent<CreateLevel>();
            creator = WorldConstants.Creator;
        }

        bool wrapX = LevelGen.LevelGen.GenSettings.WrapX,
             wrapY = LevelGen.LevelGen.GenSettings.WrapY;

        GameObject temp;

        if (wrapX)
        {
            for (int i = -1; i <= 1; ++i)
            {
                if (i != 0)
                {
                        temp = creator.CreateMirrorObject(name, gameObject, new Vector3(i * Constants.Size.x, 0, 0));

                    if (temp != null)
                    {
                        temp.layer = gameObject.layer;
                    }
                }
            }
        }
        if (wrapY)
        {
            for (int j = -1; j <= 1; ++j)
            {
                if (j != 0)
                {
                    temp = creator.CreateMirrorObject(name, gameObject, new Vector3(0, j * Constants.Size.y, 0));

                    if (temp != null)
                    {
                        temp.layer = gameObject.layer;
                    }
                }
            }
        }

        if (wrapX && wrapY)
        {
            temp = creator.CreateMirrorObject(name, gameObject, new Vector3(-Constants.Size.x, -Constants.Size.y, 0));
            if (temp != null)
            {
                temp.layer = gameObject.layer;
            }

            temp = creator.CreateMirrorObject(name, gameObject, new Vector3(Constants.Size.x, -Constants.Size.y, 0));
            if (temp != null)
            {
                temp.layer = gameObject.layer;
            }

            temp = creator.CreateMirrorObject(name, gameObject, new Vector3(-Constants.Size.x, Constants.Size.y, 0));
            if (temp != null)
            {
                temp.layer = gameObject.layer;
            }

            temp = creator.CreateMirrorObject(name, gameObject, new Vector3(Constants.Size.x, Constants.Size.y, 0));
            if (temp != null)
            {
                temp.layer = gameObject.layer;
            }
        }
    }
}

public class SetColliderArgs
{
    public GameObject Mirror;
    public Vector3 Offset;

    public SetColliderArgs(GameObject mirror, Vector3 offset)
    {
        this.Mirror = mirror;
        this.Offset = offset;
    }
}
