using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using Consts = WorldConstants;

/// <summary>
/// Handles creation/tracking of powerups and objectives, and starting the match.
/// </summary>
public class LevelManager : MonoBehaviour
{
    //TODO: Make a rect bounds component to replace the Box Collider component; Collider components automatically use Unity's physics engine, slowing the game.

    //Cached references.
    public PrefabCreator Creator { get { return WorldConstants.Creator; } }
    public GenerateObjects ObjGenerator = null;

    public MatchStartData MatchData { get { return WorldConstants.MatchData; } }
    public Generator LevelGen { get { return MatchData.GeneratedLevel; } }
    public SpawnCreator Spawns { get { return MatchData.Spawns; } }

    private ActorConstants ActorConsts
    {
        get
        {
            if (actCnsts == null)
            {
                actCnsts = WorldConstants.ConstantsOwner.GetComponent<ActorConstants>();
            }
            return actCnsts;
        }
    }
    private ActorConstants actCnsts = null;

    private bool confirmGameEnd;
    public Texture2D ConfirmGameEndOverlay = null;
    public bool GameOver { get; private set; }

    /// <summary>
    /// The rules of the match.
    /// </summary>
    public Rules MatchRules;

    //Objects being tracked.
    public Dictionary<Color, GameObject> FlagsByTeam;
    public Dictionary<Color, GameObject> FlagBasesByTeam;
    public List<GameObject> Powerups;

    //Scoring.
    public Dictionary<StateMachine, float> PlayerScores;
    public Dictionary<Color, float> TeamScores;

    //Timers.
    public System.TimeSpan CountdownToGameEnd;
    public System.TimeSpan CountdownToPowerup;
    public string TimeToPowerup;

    void Awake()
    {
        confirmGameEnd = false;
        GameOver = false;

        //Set references.
        WorldConstants.MatchController = gameObject;
        WorldConstants.ColTracker = GetComponent<CollisionTracker>();
        WorldConstants.CrowdCheering = GetComponent<CrowdCheers>();
        WorldConstants.PlayPhysNoises = GetComponent<PlayerPhysicsNoises>();

        //Data references.

        TeamScores = new Dictionary<Color, float>();
        PlayerScores = new Dictionary<StateMachine, float>();

        FlagsByTeam = new Dictionary<Color, GameObject>();
        FlagBasesByTeam = new Dictionary<Color, GameObject>();

        Powerups = new List<GameObject>();

        MatchStarted = false;
    }

    public bool MatchStarted { get; private set; }

    public void StartWorld(bool isPreview)
    {
        WorldConstants.MirrorContainer = new GameObject("Mirror Objects");
        WorldConstants.WallContainer = new GameObject("Walls");
		
        //Get some references.
        CreateLevel levG = WorldConstants.MatchWrapper.GetComponent<CreateLevel>();
        GenerateObjects objG = WorldConstants.MatchWrapper.GetComponent<GenerateObjects>();

        //Take ownership of the game rules.
        if (!isPreview)
        {
            MatchRules = MatchData.MatchSettings;
            MatchRules.Owner = this;
			MatchStarted = true;
        }
        CountdownToGameEnd = new System.TimeSpan(MatchRules.MatchLength.Ticks);

        //Create the level and objects.
        levG.SetGenerationData(MatchData.GeneratedLevel, MatchData.Spawns);
        WorldConstants.LevelOffset = Vector2.zero;
        levG.Generate(Vector2.zero, isPreview);
        objG.Generate(MatchData, isPreview);

        //Start crowd cheering.
        if (!isPreview)
        {
            WorldConstants.CrowdCheering.StartCheering();
            WorldConstants.CrowdCheering.CreateCrowdCheer(CrowdCheers.Events.GameStart);
        }
    }

    //Every so often, refresh the collection of important objects in the world.
    private float elapsed = 0.5f;
    private const float getObjectsInterval = 0.5f;

    private bool warnedPlayersYet;

    void Update()
    {
        if (!MatchStarted || GameOver) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GetComponent<InputManager>().DisableInput = true;
            confirmGameEnd = true;
        }

        #region Update player/team scores

        TeamScores.Clear();
        PlayerScores.Clear();

        //Go through and re-add each player/team.
        foreach (StateMachine st in WorldConstants.ColTracker.Actors)
        {
            PlayerScores.Add(st, MatchRules.GetScore(st.OwnerStats));

            if (!TeamScores.ContainsKey(st.ActorData.Team))
            {
                TeamScores.Add(st.ActorData.Team, 0.0f);
            }

            TeamScores[st.ActorData.Team] += PlayerScores[st];
        }

        //Get the teams in order by score.
        List<KeyValuePair<Color, float>> descendingTeams = TeamScores.ToList();
        descendingTeams = DescendingBubbleSort(descendingTeams);
        Color bestTeam = descendingTeams[0].Key;
        float largestScore = descendingTeams[0].Value;

        #endregion

        #region Check for team won/near winning

        //Warn players of the match nearly ending if applicable.
        if (!warnedPlayersYet)
        {
            //If any of the teams are near winning, say so.
            if (largestScore > (WorldConstants.ScoreWarningThreshold * MatchRules.ScoreGoal))
            {
                WorldConstants.Creator.CreateFloatingTextsForPlayers(WorldConstants.Creator.ThreeMessages(ActorConsts.AlmostWonMessages, bestTeam, null),
                                                                     st => st.ActorData.Team == bestTeam,
                                                                     WorldConstants.Creator.CreateMatchFloatingText);
                warnedPlayersYet = true;
            }
            //If time is almost out, say so.
            if (CountdownToGameEnd.TotalSeconds <= 60.0)
            {
                WorldConstants.Creator.CreateFloatingTextsForPlayers(WorldConstants.Creator.ThreeMessages(ActorConsts.OneMinuteLeftMessages, bestTeam, null),
                                                                     st => st.ActorData.Team == bestTeam,
                                                                     WorldConstants.Creator.CreateMatchFloatingText);
                warnedPlayersYet = true;
            }
        }

        //See if a team won.
        if (largestScore >= MatchRules.ScoreGoal)
        {
            GameOver = true;
            Creator.CreateFloatingTextsForPlayers(Creator.ThreeMessages(ActorConsts.GameOverMessages, bestTeam, null),
                                                  st => st.ActorData.Team == bestTeam,
                                                  Creator.CreateMatchFloatingText);
			WorldConstants.CrowdCheering.CreateCrowdCheer(CrowdCheers.Events.GameWin);
            return;
        }
        //See if time ran out.
        CountdownToGameEnd -= System.TimeSpan.FromSeconds(Time.deltaTime);
        if (CountdownToGameEnd.TotalSeconds <= 0.0)
        {

            GameOver = true;
            Creator.CreateFloatingTextsForPlayers(Creator.ThreeMessages(ActorConsts.GameOverMessages, bestTeam, null),
                                                  st => st.ActorData.Team == bestTeam,
                                                  Creator.CreateMatchFloatingText);
			WorldConstants.CrowdCheering.CreateCrowdCheer(CrowdCheers.Events.GameWin);
            return;
        }

        #endregion

        #region Debug drawing

        StateMachine.DrawBounds(WorldConstants.LevelBounds, Color.red);
        StateMachine.DrawBounds(WorldConstants.MaxViewBounds, Color.yellow);

        if (ObjGenerator == null)
        {
            ObjGenerator = WorldConstants.MatchWrapper.GetComponent<GenerateObjects>();
        }

        #endregion

        #region Powerup timer

        //If it's time, refresh the collection of powerups.
        if (elapsed >= getObjectsInterval)
        {
            GetObjects();
            elapsed = 0.0f;
        }
        elapsed += Time.deltaTime;

        MatchRules.Update();

        //Update the timer to getting a powerup.
        if (Powerups.Count == 0 || !MatchRules.WaitForPowerupCollection)
        {
            CountdownToPowerup -= System.TimeSpan.FromSeconds(Time.deltaTime);
            if (CountdownToPowerup <= System.TimeSpan.Zero)
            {
                //Make the powerup.
                Vector2 spawnP = ObjGenerator.PowerupSpawnLocations[Random.Range(0, ObjGenerator.PowerupSpawnLocations.Count)];
                Creator.CreatePowerup(spawnP);

                //Reset the timer.
                CountdownToPowerup = MatchRules.PowerupSpawnInterval;
            }
        }
        TimeToPowerup = CountdownToPowerup.ToString();

        #endregion
    }
    private void GetObjects()
    {
        //Powerups.
        Powerups.Clear();
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Powerup"))
            Powerups.Add(g);

        //Flags/flag bases.
        FlagBehavior fBeh;
        FlagsByTeam.Clear();
        FlagBasesByTeam.Clear();
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Flag"))
        {
            fBeh = g.GetComponent<FlagBehavior>();
            FlagsByTeam.Add(fBeh.Team, g);
            FlagBasesByTeam.Add(fBeh.Team, fBeh.FlagBase.gameObject);
        }
    }

    void OnGUI()
    {
        if (confirmGameEnd)
        {
            GameEndConfirmation();
        }
        if (GameOver)
        {
            GUI.backgroundColor = new Color(200, 200, 200, 128);
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "");
            GUI.backgroundColor = Color.white;
        }
    }
    /// <summary>
    /// Runs the "press escape to quit, or spacebar to keep playing" GUI overlay.
    /// </summary>
    private void GameEndConfirmation()
    {
        GUI.backgroundColor = new Color(128, 128, 128, 196);
        GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "");
        if (ConfirmGameEndOverlay != null)
        {
            int halfWidth = ConfirmGameEndOverlay.width / 2;
            int halfHeight = ConfirmGameEndOverlay.height / 2;
            GUI.Box(new Rect((Screen.width / 2) - halfWidth,
                             (Screen.height / 2) - halfHeight,
                             ConfirmGameEndOverlay.width,
                             ConfirmGameEndOverlay.height),
                    ConfirmGameEndOverlay);
        }
        else
        {
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "Press Enter/Return to quit.\nPress Spacebar to keep playing.");
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            GameOver = true;
            confirmGameEnd = false;
            WorldConstants.CrowdCheering.CreateCrowdCheer(CrowdCheers.Events.GameWin);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            confirmGameEnd = false;
            GetComponent<InputManager>().DisableInput = false;
        }

        GUI.backgroundColor = Color.white;
    }

    public List<KeyValuePair<Color, float>> DescendingBubbleSort(List<KeyValuePair<Color, float>> teamScores)
    {
        List<KeyValuePair<Color, float>> newList = teamScores.ToList();
        KeyValuePair<Color, float> temp;

        bool sorted = false;
        while (!sorted)
        {
            sorted = true;

            for (int i = 0; i < newList.Count - 1; ++i)
            {
                if (newList[i].Value < newList[i + 1].Value)
                {
                    temp = newList[i];
                    newList[i] = newList[i + 1];
                    newList[i + 1] = temp;

                    sorted = false;
                }
            }
        }

        return newList;
    }
}