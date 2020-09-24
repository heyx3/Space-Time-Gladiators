using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PrefabCreator : MonoBehaviour
{
    #region References

    public CollectibleObjectiveConstants CollObjConsts
    {
        get
        {
            if (collObjConsts == null)
            {
                collObjConsts = WorldConstants.ConstantsOwner.GetComponent<CollectibleObjectiveConstants>();
            }
            return collObjConsts;
        }
    }
    private CollectibleObjectiveConstants collObjConsts = null;

    public PlayerConstants PlayConsts
    {
        get
        {
            if (playConsts == null)
            {
                playConsts = WorldConstants.ConstantsOwner.GetComponent<PlayerConstants>();
            }
            return playConsts;
        }
    }
    private PlayerConstants playConsts = null;

    public CameraConstants CameraConsts
    {
        get
        {
            if (camConsts == null)
            {
                camConsts = WorldConstants.ConstantsOwner.GetComponent<CameraConstants>();
            }
            return camConsts;
        }
    }
    private CameraConstants camConsts = null;

    public LevelManager LevelManager
    {
        get
        {
            if (levMan == null)
            {
                levMan = WorldConstants.MatchController.GetComponent<LevelManager>();
            }
            return levMan;
        }
    }
    private LevelManager levMan = null;

    private bool WrapX { get { return LevelManager.LevelGen.GenSettings.WrapX; } }
    private bool WrapY { get { return LevelManager.LevelGen.GenSettings.WrapY; } }

    #endregion

    /// <summary>
    /// Creates a GameObject with the same components/initial component data as the given GameObject.
    /// </summary>
    public GameObject CreateNewCopy(GameObject original)
    {
        return (GameObject)Instantiate(original);
    }

    public GameObject MatchControllerPrefab;
    public GameObject CreateMatchController()
    {
        return (GameObject)Instantiate(MatchControllerPrefab);
    }

    #region Wall creation

    [Serializable]
    public class ArrayHolder
    {
        public GameObject[] Array;
    }

    [SerializeField]
    public ArrayHolder[] WallStyleGroups = new ArrayHolder[0];

    [NonSerialized]
    public GameObject[] WallStyle = null;
    /// <summary>
    /// Chooses a random wall style to use for future level generation.
    /// </summary>
    public void ChooseWallStyle()
    {
        if (WallStyleGroups.Length > 0)
        {
            WallStyle = WallStyleGroups[UnityEngine.Random.Range(0, WallStyleGroups.Length)].Array;
        }
        else
        {
            WallStyle = null;
        }
    }

    private Transform WallIconContainer = null;
	private Transform WallContainer = null;

    /// <summary>
    /// Creates the given wall rectangle for a level preview.
    /// </summary>
    /// <param name="wallBounds"></param>
    /// <returns>The wall created.</returns>
    public GameObject CreateMinimapWall(RecBounds wallBounds)
    {
        if (WallIconContainer == null)
        {
            WallIconContainer = new GameObject("Wall Minimap Icons").transform;
        }

        //Make the minimap icon.
        GameObject icon = CreateMinimapIcon(wallBounds.center, Animations.MM_Wall, null, MinimapIconScaling.Wall, MinimapIconZPoses.Wall, false);
        icon.transform.localScale = new Vector3(wallBounds.size.x, wallBounds.size.y, 0.0f);
        icon.transform.parent = WallIconContainer.transform;

        return icon;
    }

    /// <summary>
    /// Creates walls for a given level, intelligently grouping them into single concave solids.
    /// </summary>
    public List<GameObject> CreateWalls(Generator level)
    {
        Location mapSize = new Location(level.Map.GetLength(0), level.Map.GetLength(1));
        bool[,] map = level.Map;
        int i, j;

        GameObject style;

        //Useful functions for dealing with the map.

        Func<Location, Location> tryWrapLoc = (loc) =>
            {
                if (level.GenSettings.WrapX)
                {
                    if (loc.X < 0)
                    {
                        loc.X += mapSize.X;
                    }
                    else if (loc.X >= mapSize.X)
                    {
                        loc.X -= mapSize.X;
                    }
                }
                if (level.GenSettings.WrapY)
                {
                    if (loc.Y < 0)
                    {
                        loc.Y += mapSize.Y;
                    }
                    else if (loc.Y >= mapSize.Y)
                    {
                        loc.Y -= mapSize.Y;
                    }
                }

                return loc;
            };

        Predicate<Location> isInsideMap = (loc) =>
            {
                return loc.X >= 0 && loc.Y >= 0 &&
                       loc.X < mapSize.X && loc.Y < mapSize.Y;
            };

        //Stores whether or not each location was traversed yet.
        bool[,] traversedYet = new bool[mapSize.X, mapSize.Y];
        for (i = 0; i < mapSize.X; ++i)
        {
            for (j = 0; j < mapSize.Y; ++j)
            {
                traversedYet[i, j] = false;
            }
        }

        //Contains the wall search space.
        Stack<Location> toSearch = new Stack<Location>((level.Map.GetLength(0) * level.Map.GetLength(1)) / 2);

        List<GameObject> walls = new List<GameObject>();

        //Keep picking new walls to traverse as long as there are still walls to traverse.
        bool foundWall;
        Location wallSeed, wallTemp;
        bool left, right, top, bottom;

        //A function used for the traversal algorithm.
        //If the given Location houses a wall inside the level and it hasn't been traversed yet,
        //    then it is added to the search space and counted as traversed.
        //Returns whether or not the location was occupied by a wall.
        Func<Location, bool> trySearchLoc = (loc) =>
            {
				if (isInsideMap(loc))
				{
					if (map[loc.X, loc.Y])
					{
		                if (isInsideMap(loc) && !traversedYet[loc.X, loc.Y])
		                {
		                    traversedYet[loc.X, loc.Y] = true;
		                    toSearch.Push(loc);
		                }
					
						return true;
					}
	
					return false;
				}
				else
				{
					return false;
				}
            };

        //Traverse every concave wall shape.
        //The code for breaking out of this loop is inside the middle of the loop itself.
        while (true)
        {
            foundWall = false;

            //Go through the map and pick out an unused wall.
            wallSeed = new Location(-1, -1);
            for (i = 0; i < mapSize.X; ++i)
            {
                for (j = 0; j < mapSize.Y; ++j)
                {
                    if (!traversedYet[i, j] && map[i, j])
                    {
                        foundWall = true;
                        wallSeed = new Location(i, j);
                        break;
                    }
                }

                //Exit if a wall has been found.
                if (foundWall)
                {
                    break;
                }
            }

            //If no more walls were found, we're finished.
            if (!foundWall)
            {
                break;
            }

            style = WallStyle[UnityEngine.Random.Range(0, WallStyle.Length)];

            //Now traverse the whole solid wall.
            toSearch.Clear();
            toSearch.Push(wallSeed);
			traversedYet[wallSeed.X, wallSeed.Y] = true;
            while (toSearch.Count > 0)
            {
                wallTemp = toSearch.Pop();

                left = trySearchLoc(tryWrapLoc(wallTemp.Left));
                right = trySearchLoc(tryWrapLoc(wallTemp.Right));
                bottom = trySearchLoc(tryWrapLoc(wallTemp.Above));
                top = trySearchLoc(tryWrapLoc(wallTemp.Below));

                walls.Add(CreateWall(wallTemp, style, top, bottom, left, right));
            }
        }

        return walls;
    }
    /// <summary>
    /// Creates a wall at the given location, using the correct tile given which sides are connected to other walls.
    /// </summary>
    /// <param name="loc"></param>
    /// <param name="isAbove">Is the top of this wall connected to another wall?</param>
    /// <param name="isBelow">Is the bottom of this wall connected to another wall?</param>
    /// <param name="isLeft">Is the left side of this wall connected to another wall?</param>
    /// <param name="isRight">Is the right side of this wall connected to another wall?</param>
    private GameObject CreateWall(Location loc, GameObject style, bool isAbove, bool isBelow, bool isLeft, bool isRight)
    {
        if (WallContainer == null)
        {
            WallContainer = new GameObject("Walls").transform;
        }


        Animations tile = Animations.P_Run;

        #region Get proper tile.

        if (!isAbove && !isBelow && !isLeft && !isRight)
            tile = Animations.W_Single;

        else if (!isAbove && !isBelow && (isLeft || isRight))
            tile = Animations.W_HorzCenter;
        else if (!isLeft && !isRight && (isAbove || isBelow))
            tile = Animations.W_VertCenter;

        else if (!isLeft && !isRight && isAbove && !isBelow)
            tile = Animations.W_TopEnd;
        else if (!isLeft && !isRight && !isAbove && isBelow)
            tile = Animations.W_BottomEnd;
        else if (isLeft && !isRight && !isAbove && !isBelow)
            tile = Animations.W_RightEnd;
        else if (!isLeft && isRight && !isAbove && !isBelow)
            tile = Animations.W_LeftEnd;

        else if (isRight && isBelow && !isLeft && !isAbove)
            tile = Animations.W_TLCorner;
        else if (isRight && isAbove && !isLeft && !isBelow)
            tile = Animations.W_BLCorner;
        else if (isLeft && isBelow && !isRight && !isAbove)
            tile = Animations.W_TRCorner;
        else if (isLeft && isAbove && !isRight && !isBelow)
            tile = Animations.W_BRCorner;

        else if (isRight && !isLeft && isAbove && isBelow)
            tile = Animations.W_LeftSide;
        else if (isLeft && !isRight && isAbove && isBelow)
            tile = Animations.W_RightSide;
        else if (isAbove && !isBelow && isLeft && isRight)
            tile = Animations.W_BottomSide;
        else if (isBelow && !isAbove && isLeft && isRight)
            tile = Animations.W_TopSide;

        else if (isLeft && isRight && isAbove && isBelow)
            tile = Animations.W_Center;

        else Debug.Log("Invalid wall tile: isAbove: " + isAbove + "; isBelow: " + isBelow + "; isLeft: " + isLeft + "; isRight: " + isRight);

        #endregion


        GameObject wall = (GameObject)Instantiate(style);
        wall.layer = LayerMask.NameToLayer("Walls");
		Transform t = wall.transform;
        wall.name = "Wall";
        wall.GetComponent<Animator>().SetWallFrame(tile, wall.GetComponent<WallSheetData>());
		t.position = new Vector3(loc.X, loc.Y, t.position.z);
        t.parent = WallContainer;

        return wall;
    }

    #endregion

    #region Player creation

    public GameObject PlayerPrefab;
    public GameObject CreatePlayer(Vector2 pos, byte id, byte input, Color team)
    {
        //TODO: Also take in the player's chosen character, input id, and set animation/stats.

        GameObject g = (GameObject)Instantiate(PlayerPrefab);
        g.transform.position = new Vector3(pos.x, pos.y, PlayerPrefab.transform.position.z);
        g.name = "Player";

        IDData dat = g.GetComponent<IDData>();
        dat.PlayerID = id;
        dat.Team = team;
        dat.SetRenderColor();

        WorldConstants.MatchController.GetComponent<InputManager>().RegisterPlayerInput(id, input);

        WorldConstants.ColTracker.AddActor(g.GetComponent<StateMachine>());

        CreateMinimapIcon(pos, Animations.MM_Player, team, MinimapIconScaling.Player, MinimapIconZPoses.Player).transform.parent = g.transform;

        return g;
    }

    #endregion

    #region Camera creation

    public GameObject PlayerCameraPrefab;
    public float SpriteHeight = 256f;
    public int scale = 4;
    public GameObject CreatePlayerCamera(GameObject playerFollowing, int totalCameras)
    {
        //Create.
        GameObject g = (GameObject)Instantiate(PlayerCameraPrefab);
		Vector3 playPos = playerFollowing.transform.position;
		
        g.transform.position = new Vector3(playPos.x, playPos.y, g.transform.position.z);

        //Get components.
        IDData dat = playerFollowing.GetComponent<IDData>();
        CameraFollowScript c = g.GetComponent<CameraFollowScript>();

        //Set component data.
        g.name = dat.PlayerID + " Camera";
        c.SetTarget(playerFollowing.GetComponent<StateMachine>());

        //Set the view area.
        int i = dat.PlayerID - 1;
        switch (totalCameras)
        {
            case 1: break;
            case 2: c.camera.rect = new Rect(i * 0.5f, 0.0f, 0.5f, 1.0f);
                break;
            case 3: c.camera.rect = (i < 2 ? new Rect(i * 0.5f, 0.0f, 0.5f, 0.5f) :
                                             new Rect(0.25f, 0.5f, 0.5f, 0.5f));
                break;
            case 4: c.camera.rect = (i < 2 ? new Rect(i * 0.5f, 0.0f, 0.5f, 0.5f) :
                                             new Rect((i - 2) * 0.5f, 0.5f, 0.5f, 0.5f));
                break;

            default: throw new ArgumentOutOfRangeException("Must be one to four players!");
        }

        //Set the view size.
        c.camera.orthographicSize = Screen.height * c.camera.rect.height * 0.5f * scale / SpriteHeight;
        ShowCullLayer(c.camera, "Background " + dat.PlayerID);

        return g;
    }

    public static void ShowCullLayer(Camera c, string layer)
    {
        int i = LayerMask.NameToLayer(layer);
        int i2 = 1 << i;
        c.cullingMask |= 1 << LayerMask.NameToLayer(layer);
    }
    public static void HideCullLayer(Camera c, string layer)
    {
        c.cullingMask &= ~(1 << LayerMask.NameToLayer(layer));
    }
    public static void ToggleCullLayer(Camera c, string layer)
    {
        c.cullingMask ^= 1 << LayerMask.NameToLayer(layer);
    }

    public GameObject MinimapCameraPrefab;
    public GameObject CreateMinimapCamera(Location MapSize, int localPlayers)
    {
        GameObject cam = (GameObject)Instantiate(MinimapCameraPrefab);

        cam.name = "Minimap Camera";
        cam.transform.position = new Vector3((MapSize.X * 0.5f) - 0.5f, (MapSize.Y * 0.5f) - 0.5f, cam.transform.position.z);

        MinimapCameraData data = cam.GetComponent<MinimapCameraData>();
        data.LocalPlayers = localPlayers;
        data.LevelSize = new Vector2(MapSize.X, MapSize.Y);

        return cam;
    }

    #endregion

    #region Objective/Collectible creation

    public GameObject PowerupPrefab;
    public GameObject CreatePowerup(Vector2 pos)
    {
        //Create the powerup.
        GameObject p = (GameObject)Instantiate(PowerupPrefab);

        //Set its properties.

        p.name = "Powerup";
        p.transform.position = new Vector3(pos.x, pos.y, 0.0f);

        WorldConstants.ColTracker.AddOther(p);

        CreateMinimapIcon(pos, Animations.MM_Powerup, null, MinimapIconScaling.Powerup, MinimapIconZPoses.Powerup).transform.parent = p.transform;

        return p;
    }

    public GameObject FlagPrefab, FlagBasePrefab;
    public GameObject[] CreateFlagAndFlagBase(Vector2 pos, Color team)
    {
        FlagBehavior flB;
        FlagBaseBehavior flBB;

        GameObject flag, flagBase;

        //Create the flag.
        flag = (GameObject)Instantiate(FlagPrefab);
        flag.name = "Flag";
        flB = flag.GetComponent<FlagBehavior>();
        WorldConstants.ColTracker.AddOther(flag);
        CreateFlagParticles(flag.transform);

        //Create the flag base.
        flagBase = (GameObject)Instantiate(FlagBasePrefab);
        flagBase.name = "Flag Base";
        flBB = flagBase.GetComponent<FlagBaseBehavior>();
        WorldConstants.ColTracker.AddOther(flagBase);

        //Set properties of the flag.
        flB.SetData(flBB, team);
        flag.transform.position = new Vector3(pos.x + CollObjConsts.FlagOffsetFromBase.x,
                                              pos.y + CollObjConsts.FlagOffsetFromBase.y, 0.0f);

        //Set properties of the flag base.
        flBB.SetData(team, flB);
        flagBase.transform.position = new Vector3(pos.x, pos.y, 0.0f);

        CreateMinimapIcon(pos, Animations.MM_Flag, team, MinimapIconScaling.Flag, MinimapIconZPoses.Flag).transform.parent = flag.transform;
        CreateMinimapIcon(pos, Animations.MM_FlagBase, team, MinimapIconScaling.FlagBase, MinimapIconZPoses.FlagBase).transform.parent = flagBase.transform;

        return new GameObject[2] { flag, flagBase };
    }

    public GameObject WaypointPrefab;
    public GameObject CreateWaypoint(Vector2 pos)
    {
        GameObject w = (GameObject)Instantiate(WaypointPrefab);

        w.transform.position = new Vector3(pos.x, pos.y, w.transform.position.z);
        w.name = "Waypoint";

        WorldConstants.ColTracker.AddOther(w);

        CreateMinimapIcon(pos, Animations.MM_Waypoint, null, MinimapIconScaling.Waypoint, MinimapIconZPoses.Waypoint).transform.parent = w.transform;

        return w;
    }

    public GameObject AuraPrefab;
    public GameObject CreateAura(Transform following)
    {
        GameObject g = (GameObject)Instantiate(AuraPrefab);
        g.name = "VIP Aura";

        if (following != null)
        {
            g.transform.parent = following;
        }

        AuraBehavior ab = g.GetComponent<AuraBehavior>();

        ab.VIPChanged();

        GameObject g23 = CreateMinimapIcon(following.position, Animations.MM_Aura, null, MinimapIconScaling.Aura, MinimapIconZPoses.Aura);
        g23.transform.parent = g.transform;

        ab.ParticleEffects = CreateAuraParticles(g.transform);

        return g;
    }

    #endregion

    #region Floating text creation

    /*TODO: Add hooks for all text events. The following events still need to be hooked:
            -Match Events
                >Lost the lead
                >Took the lead
    */

    public Color GoodPowerupColor = new Color(1.0f, 0.666f, 0.0f),
                 BadPowerupColor = new Color(1.0f, 0.333f, 0.0f),

                 GoodFlagColor = Color.green,
                 BadFlagColor = Color.red,

                 GoodVIPColor = Color.gray,
                 BadVIPColor = new Color(0.4f, 0.4f, 0.4f),

                 GoodWaypointColor = Color.yellow,
                 BadWaypointColor = new Color(0.666f, 0.666f, 0.0f),

                 GoodMatchColor = Color.white,
                 BadMatchColor = new Color(1.0f, 0.8f, 0.8f);

    /// <summary>
    /// Creates a good or bad floating text for all players.
    /// </summary>
    /// <param name="messageForPlayer">Given the player, gets the floating text to display over him.</param>
    /// <param name="isPlayerGood">A function to determine if the given player gets a good message or bad message.</param>
    /// <param name="floatingTextCreator">One of: this.CreatePowerupFloatingText, this.CreateFlagFloatingText, this.CreateVIPFloatingText, this.CreateWaypointFloatingText, this.CreateMatchFloatingText.</param>
    /// <returns>All the floating texts that were created.</returns>
    public GameObject[] CreateFloatingTextsForPlayers(Func<StateMachine, string> messageForPlayer, Func<StateMachine, bool> isPlayerGood,
                                                      Func<string, Vector2, Transform, bool, GameObject> floatingTextCreator)
    {
        StateMachine[] players = WorldConstants.ColTracker.Actors.ToArray();
        GameObject[] ret = new GameObject[players.Length];

        for (int i = 0; i < ret.Length; ++i)
        {
            if (players[i].IsPlayer)
            {
                if (isPlayerGood(players[i]))
                {
                    ret[i] = floatingTextCreator(messageForPlayer(players[i]), players[i].transform.position, players[i].transform, true);
                }
                else
                {
                    ret[i] = floatingTextCreator(messageForPlayer(players[i]), players[i].transform.position, players[i].transform, false);
                }
            }
        }

        return ret;
    }
    /// <summary>
    /// The four different categories of floating text messages for players.
    /// </summary>
    [Serializable]
    public class PlayerMessages
    {
        public string Trigger, Allies, Neutrals, Enemies;
        public PlayerMessages(string trigger, string allies, string neutrals, string enemies)
        {
            Trigger = trigger;
            Allies = allies;
            Neutrals = neutrals;
            Enemies = enemies;
        }
        public override bool Equals(object obj)
        {
            PlayerMessages oth = obj as PlayerMessages;
            return oth != null && oth.Trigger == Trigger && oth.Allies == Allies && oth.Neutrals == Neutrals && oth.Enemies == Enemies;
        }
        public override int GetHashCode()
        {
            return Trigger.GetHashCode() + Allies.GetHashCode() + Neutrals.GetHashCode() + Enemies.GetHashCode();
        }
        public override string ToString()
        {
            return "Trigger message: \"" + Trigger + "\", allies message: \"" + Allies + "\", neutrals message: \"" + Neutrals + "\", enemies message: \"" + Enemies + "\"";
        }
    }
    /// <summary>
    /// Gets a function that gets the floating text message for any given player.
    /// </summary>
    /// <param name="trigger">The message for the player that triggered the floating text.</param>
    /// <param name="allies">The message for the teammates of the player that triggered the floating text.</param>
    /// <param name="neutrals">The message for neutral (not-involved) teams.</param>
    /// <param name="enemies">The message for enemies of the player that triggered the floating text.</param>
    /// <param name="playerTrigger">The player that triggered the floating text.</param>
    /// <param name="enemyTeam">The team that was specifically targeted by the player, or "null" if all enemies were targeted.</param>
    /// <returns>A function for use in "CreateFloatingTextsForPlayers" that gets the correct message depending on the player it is sent to.</returns>
    public Func<StateMachine, string> FourMessages(PlayerMessages messages, StateMachine playerTrigger, Color? enemyTeam)
    {
        return (st) =>
            {
                if (st == playerTrigger)
                {
                    return messages.Trigger;
                }
                else if (st.ActorData.Team == playerTrigger.ActorData.Team)
                {
                    return messages.Allies;
                }
                else if (!enemyTeam.HasValue || (enemyTeam.HasValue && st.ActorData.Team == enemyTeam.Value))
                {
                    return messages.Enemies;
                }
                else
                {
                    return messages.Neutrals;
                }
            };
    }
    /// <summary>
    /// Gets a function that gets the floating text message for any given player.
    /// </summary>
    /// <param name="allies">The message for the teammates of the player that triggered the floating text.</param>
    /// <param name="neutrals">The message for neutral (not-involved) teams.</param>
    /// <param name="enemies">The message for enemies of the team that triggered the floating text.</param>
    /// <param name="playerTrigger">The team of the player that triggered the floating text.</param>
    /// <param name="enemyTeam">The team that was specifically targeted by the player, or "null" if all enemies were targeted.</param>
    /// <returns>A function for use in "CreateFloatingTextsForPlayers" that gets the correct message depending on the player it is sent to.</returns>
    public Func<StateMachine, string> ThreeMessages(PlayerMessages messages, Color playerTrigger, Color? enemyTeam)
    {
        return (st) =>
        {
            if (st.ActorData.Team == playerTrigger)
            {
                return messages.Allies;
            }
            else if (!enemyTeam.HasValue || (enemyTeam.HasValue && st.ActorData.Team == enemyTeam.Value))
            {
                return messages.Enemies;
            }
            else
            {
                return messages.Neutrals;
            }
        };
    }

    public GameObject PowerupFloatingTextPrefab;
    public GameObject CreatePowerupFloatingText(string text, Vector2 pos, Transform parentOrNull, bool good)
    {
        return CreateFloatingText(PowerupFloatingTextPrefab, text, pos, parentOrNull, (good ? GoodPowerupColor : BadPowerupColor));
    }

    public GameObject FlagFloatingTextPrefab;
    public GameObject CreateFlagFloatingText(string text, Vector2 pos, Transform parentOrNull, bool good)
    {
        return CreateFloatingText(FlagFloatingTextPrefab, text, pos, parentOrNull, (good ? GoodFlagColor : BadFlagColor));
    }

    public GameObject VIPFloatingTextPrefab;
    public GameObject CreateVIPFloatingText(string text, Vector2 pos, Transform parentOrNull, bool good)
    {
        return CreateFloatingText(VIPFloatingTextPrefab, text, pos, parentOrNull, (good ? GoodVIPColor : BadVIPColor));
    }

    public GameObject WaypointFloatingTextPrefab;
    public GameObject CreateWaypointFloatingText(string text, Vector2 pos, Transform parentOrNull, bool good)
    {
        return CreateFloatingText(WaypointFloatingTextPrefab, text, pos, parentOrNull, (good ? GoodWaypointColor : BadWaypointColor));
    }

    public GameObject MatchFloatingTextPrefab;
    public GameObject CreateMatchFloatingText(string text, Vector2 pos, Transform parentOrNull, bool good)
    {
        return CreateFloatingText(MatchFloatingTextPrefab, text, pos, parentOrNull, (good ? GoodMatchColor : BadMatchColor));
    }

    private GameObject CreateFloatingText(GameObject textPrefab, string text, Vector2 pos, Transform parentOrNull, Color c)
    {
        pos += PlayConsts.FloatingTextOffset;

        GameObject ret = (GameObject)Instantiate(textPrefab);
        ret.GetComponent<TextMesh>().text = text;
        ret.transform.position = pos;

        if (parentOrNull != null)
        {
            ret.transform.parent = parentOrNull;
            ret.layer = LayerMask.NameToLayer("Background " + parentOrNull.GetComponent<IDData>().PlayerID);
        }

        ret.GetComponent<MeshRenderer>().material.color = c;

        return ret;
    }

    #endregion

    #region Parallax creation

    //References for creating objects with parallax.
    private GameObject[] LocalPlayers;
    private GameObject[] Cameras;
    private void GetReferences()
    {
        if (StarsContainer == null)
            StarsContainer = new GameObject("Stars Container");
        if (SpecialsContainer == null)
            SpecialsContainer = new GameObject("Specials Container");

        if (LocalPlayers == null || LocalPlayers[0] == null)
            LocalPlayers = GetLocalPlayers();
        if (Cameras == null || Cameras[0] == null)
            Cameras = GameObject.FindGameObjectsWithTag("MainCamera");

    }
    /// <summary>
    /// Gets all local players, arranged in increasing order by player ID.
    /// </summary>
    public static GameObject[] GetLocalPlayers()
    {

        //Get all the players.
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        //Get the local players.
        List<GameObject> localPlayers = new List<GameObject>();
        byte s;
        foreach (GameObject g in players)
        {
            s = g.GetComponent<IDData>().PlayerID;
            if (s < 5)
                localPlayers.Add(g);
        }

        //Order them according to id number.
        GameObject[] rets = new GameObject[localPlayers.Count];
        for (int i = 0; i < localPlayers.Count; ++i)
            rets[localPlayers[i].GetComponent<IDData>().PlayerID - 1] = localPlayers[i];

        return rets;
    }

    public GameObject StarsPrefab;
    private GameObject StarsContainer;
    public void CreateStars(Vector2 pos)
    {
        GetReferences();

        Transform t;
        ParallaxBehavior b = null;

        //Create one copy for each camera.
        foreach (GameObject g in Cameras)
        {
            //Create the stars.

            GameObject s = (GameObject)Instantiate(StarsPrefab);

            s.name = "Stars";
            t = s.transform;
            t.parent = StarsContainer.transform;
            t.position = pos;
            t.Rotate(0, 0, UnityEngine.Random.Range(0, 4) * 90);

            b = s.GetComponent<ParallaxBehavior>();
            int numb = Int32.Parse(g.name.Substring(0, 1));
            b.SetLayer(numb);


            //TODO: Track each star and see if any of them are never inside the player's view. Also, change that inner "if" statement to use a RecBounds for the player's view at that simulated position, not the whole view bounds!
            continue;

            //Check to see if the created stars are too far away to ever be seen.

            //If the star isn't viewable from anywhere in the viewing area, remove it.
            Vector2 testPos, maxMovement;
            for (int i = -1; i <= 1; i += 1)
            {
                if (s == null) break;

                for (int j = -1; j <= 1; j += 1)
                {
                    testPos = WorldConstants.LevelBounds.center +
                              new Vector2(WorldConstants.LevelBounds.extents.x * i,
                                          WorldConstants.LevelBounds.extents.y * j);

                    //Imagine the player moving to the test position and see if the star is viewable from there.
                    maxMovement = b.ParallaxStrength * (testPos - (Vector2)LocalPlayers[numb - 1].transform.position);
                    if (!WorldConstants.MaxViewBounds.Touches(pos + maxMovement +
                                                              new Vector2(0, 0)))
                    {
                        GameObject.Destroy(s);
                        break;
                    }
                }
            }
        }
    }
    public GameObject SpecialPrefab;
    private GameObject SpecialsContainer;
    public void CreateSpecial(Vector2 pos)
    {
        GetReferences();

        //Create one copy for each camera.
        foreach (GameObject g in Cameras)
        {
            GameObject s = (GameObject)Instantiate(SpecialPrefab);

            s.name = "Special";
            s.transform.parent = SpecialsContainer.transform;

            s.transform.position = pos;
            s.transform.Rotate(0, 0, UnityEngine.Random.Range(0, 4) * 90);

            s.GetComponent<ParallaxBehavior>().SetLayer(Int32.Parse(g.name.Substring(0, 1)));
        }
    }

    #endregion

    #region Mirror creation

    public GameObject MirrorObjectPrefab;
    public GameObject CreateMirrorObject(string typeName, GameObject owner, Vector3 offset)
    {
        if (WorldConstants.MaxViewBounds.left == -1)
        {
            throw new Exception("MaxViewBounds hasn't been set!");
        }
        if (typeName.ToLower() == "wall" &&
            (!WorldConstants.MaxViewBounds.Intersects(new RecBounds(owner.transform.position + offset,
                                                                    owner.collider.bounds.size))))
        {
            return null;
        }

        GameObject m = (GameObject)Instantiate(MirrorObjectPrefab);

        m.name = typeName + " Mirror";
        m.GetComponent<MirrorMesh>().SetData(owner, offset);

        return m;
    }

    #endregion

    #region Minimap icon creation

    /// <summary>
    /// A collection of floating point values for each kind of minimap icon.
    /// </summary>
    [Serializable]
    public class MinimapIconFloats
    {
        public float Player, Flag, FlagBase, Aura, Waypoint, Powerup, Team, Wall;
        public MinimapIconFloats(float play, float flag, float flagBase, float aura, float waypoint, float powerup, float team, float wall)
        {
            Player = play; 
            Flag = flag; 
            FlagBase = flagBase; 
            Aura = aura; 
            Waypoint = waypoint; 
            Powerup = powerup;
            Team = team;
            Wall = wall;
        }
    }
    public MinimapIconFloats MinimapIconScaling = new MinimapIconFloats(1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f);
    public MinimapIconFloats MinimapIconZPoses = new MinimapIconFloats(-1.0f, -1.01f, -1.005f, -1.01f, -0.99f, -1.02f, -1.02f, -0.5f);

    public GameObject MinimapIconPrefab;
    public GameObject TeamColorMinimapIconPrefab;
    public GameObject CreateMinimapIcon(Vector2 pos, Animations iconAnim, Color? teamColor, float scale, float zPos, bool mirror = true)
    {
        GameObject g;
        if (teamColor.HasValue)
        {
            g = (GameObject)Instantiate(TeamColorMinimapIconPrefab);
            g.renderer.material.SetColor("_TeamCol", teamColor.Value);
        }
        else
        {
            g = (GameObject)Instantiate(MinimapIconPrefab);
        }
		
		if (!mirror)
		{

			GameObject.Destroy (g.GetComponent<CreateMirrors>());
		}
		
        g.name = "Minimap Icon";
        g.transform.position = new Vector3(pos.x, pos.y, zPos);
        g.transform.localScale = new Vector3(scale, scale, g.transform.localScale.z);

        g.GetComponent<SetAnimationStart>().Anim = iconAnim;

        return g;
    }

    #endregion

    #region Particle emitter creation

    private ParticleHandler CreateParticlesWithMirrors(Func<Vector2, GameObject> offsetToParticle, bool wrapX, bool wrapY, ParticleHandler.ParticleSystems type)
    {
        GameObject[] ps = new GameObject[9];

        GameObject center = null;

        int index;
        for (int x = -1; x <= 1; x += 1)
        {
            for (int y = -1; y <= 1; y += 1)
            {
                index = x + 1 + (3 * (y + 1));
                ps[index] = null;

                if ((x != 0 && !wrapX) ||
                    (y != 0 && !wrapY))
                {
                    continue;
                }

                ps[index] = offsetToParticle(new Vector2(WorldConstants.Size.x * x, WorldConstants.Size.y * y));
                if (x == 0 && y == 0)
                {
                    center = ps[index];
                }
            }
        }

        return new ParticleHandler(type, ps.Where(g => g != null), center);
    }

    public GameObject RunningDebrisParticlePrefab, WallslidingDebrisParticlePrefab;
    public ParticleHandler CreateRunningDebrisParticles(StateMachine player)
    {
        return CreateParticlesWithMirrors(offset =>
                                          {
                                              GameObject g = (GameObject)Instantiate(RunningDebrisParticlePrefab);

                                              g.transform.position = new Vector3(player.transform.position.x + offset.x,
                                                                                 player.transform.position.y + offset.y,
                                                                                 RunningDebrisParticlePrefab.transform.position.z);
                                              g.name = "Kicked-up dirt";
                                              g.transform.parent = player.transform;

                                              g.particleEmitter.emit = false;

                                              return g;
                                          },
                                          WrapX, WrapY, ParticleHandler.ParticleSystems.Legacy);
    }
    public ParticleHandler CreateWallslidingDebrisParticles(StateMachine player)
    {
        return CreateParticlesWithMirrors(offset =>
                                          {
                                              GameObject g = (GameObject)Instantiate(WallslidingDebrisParticlePrefab);

                                              g.transform.position = new Vector3(player.transform.position.x + offset.x,
                                                                                 player.transform.position.y + offset.y,
                                                                                 WallslidingDebrisParticlePrefab.transform.position.z);
                                              g.name = "Kicked-up wall dirt";
                                              g.transform.parent = player.transform;

                                              g.particleEmitter.emit = false;

                                              return g;
                                          },
                                          WrapX, WrapY, ParticleHandler.ParticleSystems.Legacy);
    }

    public GameObject GroundpoundParticlePrefab, GroundpoundBitsParticlePrefab;
    public ParticleHandler CreateGroundpoundParticles(StateMachine player)
    {
        Transform pl = player.transform;
        return CreateParticlesWithMirrors(offset =>
                                          {
                                              GameObject g = (GameObject)Instantiate(GroundpoundParticlePrefab);
                                              g.transform.position = new Vector3(pl.position.x + offset.x,
                                                                                 pl.position.y + offset.y,
                                                                                 GroundpoundParticlePrefab.transform.position.z);
                                              g.name = "Groundpound Particles";
                                              g.transform.parent = pl;

                                              g.particleEmitter.emit = true;

                                              GameObject g2 = (GameObject)Instantiate(GroundpoundBitsParticlePrefab);
                                              g2.transform.position = g.transform.position;
                                              g2.name = "Groundpound Particle Bits";
                                              g2.transform.parent = pl;

                                              g2.particleEmitter.emit = true;

                                              return g;
                                          },
                                          WrapX, WrapY, ParticleHandler.ParticleSystems.Legacy);
    }

    public GameObject HardLandingParticlePrefab, SoftLandingParticlePrefab;
    public ParticleHandler CreateLandingParticles(bool hardLanding, StateMachine player)
    {
        Func<Vector2, GameObject> factory = null;
        GameObject p;

        if (hardLanding)
        {
            factory = offset =>
                {
                    p = (GameObject)Instantiate(HardLandingParticlePrefab);
                    p.name = "Heavy landing dirt";
                    p.transform.position = new Vector3(player.transform.position.x + offset.x,
                                                       player.transform.position.y + offset.y,
                                                       p.transform.position.z);
                    p.transform.parent = player.transform;

                    p.particleEmitter.emit = false;

                    return p;
                };
        }
        else
        {
            factory = offset =>
            {
                p = (GameObject)Instantiate(SoftLandingParticlePrefab);
                p.name = "Soft landing dirt";
                p.transform.position = new Vector3(player.transform.position.x + offset.x,
                                                   player.transform.position.y + offset.y,
                                                   p.transform.position.z);
                p.transform.parent = player.transform;

                p.particleEmitter.emit = false;

                return p;
            };
        }

        return CreateParticlesWithMirrors(factory, WrapX, WrapY, ParticleHandler.ParticleSystems.Legacy);
    }

    public GameObject PlayerHurtParticlePrefab;
    public ParticleHandler CreatePlayerHurtParticles(StateMachine player)
    {
        Transform pl = player.transform;

        return CreateParticlesWithMirrors(offset =>
                                          {
                                              GameObject p = (GameObject)Instantiate(PlayerHurtParticlePrefab);
                                              p.name = "Player Hurt";
                                              p.transform.position = new Vector3(pl.position.x + offset.x,
                                                                                 pl.position.y + offset.y,
                                                                                 p.transform.position.z);
                                              p.transform.parent = pl;

                                              p.particleSystem.enableEmission = true;

                                              return p;
                                          },
                                          WrapX, WrapY, ParticleHandler.ParticleSystems.Shruiken);
    }
    
    public GameObject AuraParticlePrefab;
    public ParticleHandler CreateAuraParticles(Transform auraTransform)
    {
        GameObject p;

        return CreateParticlesWithMirrors(offset =>
                                          {
                                              p = (GameObject)Instantiate(AuraParticlePrefab);
                                              p.name = "Aura particles";
                                              p.transform.position = new Vector3(auraTransform.position.x + offset.x,
                                                                                 auraTransform.position.y + offset.y,
                                                                                 p.transform.position.z);
                                              p.transform.parent = auraTransform;

                                              p.particleSystem.enableEmission = true;

                                              return p;
                                          },
                                          WrapX, WrapY, ParticleHandler.ParticleSystems.Shruiken);
    }

    public GameObject FlagParticlesPrefab;
    public ParticleHandler CreateFlagParticles(Transform flagTransform)
    {
        GameObject p;

        return CreateParticlesWithMirrors(offset =>
                                          {
                                              p = (GameObject)Instantiate(FlagParticlesPrefab);

                                              p.transform.position = flagTransform.position + new Vector3(offset.x, offset.y + 0.5f, 0.0f);
                                              p.transform.parent = flagTransform;

                                              return p;
                                          },
                                          WrapX, WrapY, ParticleHandler.ParticleSystems.Shruiken);
    }

    #endregion

    #region Timers

    private List<Timer> timers = new List<Timer>();

    /// <summary>
    /// Adds the given timer to the global timer list.
    /// </summary>
    /// <param name="removeWhenTriggered">If true, this timer will be removed from the timer list as soon as it is triggered.</param>
    public void AddTimer(Timer t, bool removeWhenTriggered)
    {
        timers.Add(t);

        if (removeWhenTriggered)
        {
            t.OnTimerWentOff += (f => autoRemoveTimer(t));
        }
    }
    /// <summary>
    /// Removes the given timer from the global list.
    /// </summary>
    public void RemoveTimer(Timer t)
    {
        timers.Remove(t);
    }
    /// <summary>
    /// Removes the auto-remove delegate from the timer's event, then removes it from the global list.
    /// </summary>
    private void autoRemoveTimer(Timer t)
    {
        t.OnTimerWentOff -= (f => timers.Remove(t));

        timers.Remove(t);
    }

    #endregion

    void Update()
    {
        //Start at the end of the timer list in case timers are removed (which would result in a timer being skipped).
        for (int i = timers.Count - 1; i >= 0; --i)
        {
            timers[i].Update(Time.deltaTime);
        }
    }
}