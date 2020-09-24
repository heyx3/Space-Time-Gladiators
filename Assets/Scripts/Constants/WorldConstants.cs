using UnityEngine;
using System.Collections;

public static class WorldConstants
{
    public static float UpGravity = -100.0f;
    public static float DownGravity = -25.0f;

    public static Vector2 Size = new Vector2(20, 20);
    public static Vector2 LevelOffset = new Vector2(0, 0);

    public static float ScoreWarningThreshold = 0.9f;

	public static float StarsChance = 0.3f;
	public static float SpecialsChance = 0.001f;

    public static System.TimeSpan MatchEndWaitTIme = System.TimeSpan.FromSeconds(6);

    //GameObject containers.
    public static GameObject MirrorContainer;
    public static GameObject WallContainer;

    //Component references.

    public static GameObject MatchController;
    public static GameObject ConstantsOwner;
    public static GameObject MatchWrapper;

    public static PrefabCreator Creator;
	
    public static ActorConstants ActorConsts;
    public static PlayerConstants PlayerConsts;
    public static FrontEndConstants FrontEndConsts;
    public static CollectibleObjectiveConstants CollObjConsts;
    public static CameraConstants CameraConsts;
	public static GUIStyles GUIStyles;

    public static CollisionTracker ColTracker;
    public static CrowdCheers CrowdCheering;
    public static PlayerPhysicsNoises PlayPhysNoises;
	
	public static MatchStartData MatchData;
    public static ScreenUI ScreenUI;

    //TODO: Move to ActorConstants.
    /// <summary>
    /// The minimum-allowable movement speed for an actor to be considered still moving.
    /// </summary>
    public static float MinMovementSpeed = 0.02f;
    /// <summary>
    /// Any damage dealt above this amount from a collision
    /// causes the crowd to cheer harder than for a weaker collision.
    /// </summary>
    public static float HeavyHitPainCutoff = 40.0f;

    public static RecBounds LevelBounds
    {
        get
        {
            RecBounds b = new RecBounds(((Vector3)Size - new Vector3(1.0f, 1.0f, 0.0f)) * 0.5f,
                                  (Vector3)Size - new Vector3(1.0f, 1.0f, 0.0f));

            return b;
        }
    }
    /// <summary>
    /// Gets or sets the bounds representing all possible areas a player can see on his camera.
    /// Used for ignoring attempts to create mirror objects that will never be seen.
    /// Default value until initialized is "new RecBounds({-1, -1}, {-1, -1})".
    /// </summary>
    [System.NonSerialized]
    public static RecBounds MaxViewBounds = new RecBounds(new Vector2(-1, -1), new Vector2(-1, -1));
}