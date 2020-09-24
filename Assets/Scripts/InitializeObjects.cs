using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Initializes basic objects and references. Belongs in the "Match Wrapper" GameObject.
/// </summary>
[RequireComponent(typeof(PrefabCreator))]
public class InitializeObjects : MonoBehaviour
{
    void Awake()
    {
        WorldConstants.ConstantsOwner = GameObject.Find("Constants Owner");
        WorldConstants.Creator = GetComponent<PrefabCreator>();

        WorldConstants.MatchWrapper = gameObject;

        WorldConstants.ActorConsts = WorldConstants.ConstantsOwner.GetComponent<ActorConstants>();
        WorldConstants.PlayerConsts = WorldConstants.ConstantsOwner.GetComponent<PlayerConstants>();
        WorldConstants.FrontEndConsts = WorldConstants.ConstantsOwner.GetComponent<FrontEndConstants>();
        WorldConstants.CameraConsts = WorldConstants.ConstantsOwner.GetComponent<CameraConstants>();
        WorldConstants.CollObjConsts = WorldConstants.ConstantsOwner.GetComponent<CollectibleObjectiveConstants>();
    }
}