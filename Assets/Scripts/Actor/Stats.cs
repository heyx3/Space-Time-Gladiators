using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// A collection of basic Actor stats.
/// </summary>
[RequireComponent(typeof(ActorProperties))]
public class Stats : MonoBehaviour
{
    //TODO: Track/update stats when things happen. Anything that hasn't definitely been tracked yet is marked with an empty comment.

    //TODO: Once enemies are created, track all these stats for them as well.

    /// <summary>
    /// Gets all interesting stats to display for the given players.
    /// The return value is a collection of strings, where each string is a single stat.
    /// The collection is indexed by the corresponding player, and the stats for a player
    ///      are grouped by category into individual sub-lists.
    /// </summary>
    public static Dictionary<StateMachine, List<List<string>>> GetStats(Rules gameRules, IEnumerable<StateMachine> players)
    {
        List<Func<StateMachine, string>> gameTypeStats = new List<Func<StateMachine, string>>();
        List<Func<StateMachine, string>> otherStats = new List<Func<StateMachine, string>>();
        List<Func<StateMachine, string>> timingStats = new List<Func<StateMachine, string>>();

        Dictionary<StateMachine, List<List<string>>> statsToDisplay = new Dictionary<StateMachine, List<List<string>>>();

        #region Choose stats to get

        //Game-type stuff.
        gameTypeStats.Add(st => "Pain dealt to enemies: " + st.OwnerStats.PainDealtToEnemy);
        gameTypeStats.Add(st => "Pain taken from enemies: " + st.OwnerStats.PainReceivedByEnemy);
        gameTypeStats.Add(st => "Powerups collected: " + st.OwnerStats.PowerupsCollected);
        if (gameRules.CTF != null)
        {
            gameTypeStats.Add(st => "Flags captured: " + st.OwnerStats.FlagsCaptured);
            gameTypeStats.Add(st => "Flags returned: " + st.OwnerStats.FlagsReturned);
        }
        if (gameRules.Bullseye != null)
        {
            if (UnityEngine.Random.value > 0.5f)
            {
                gameTypeStats.Add(st => "Time spent as Target: " + st.OwnerStats.TotalTimeAsVIP);
            }
            else
            {
                gameTypeStats.Add(st => "Times chosen to be Target: " + st.OwnerStats.TimesAsVIP);
            }

            if (UnityEngine.Random.value > 0.5f)
            {
                gameTypeStats.Add(st => "Damage dealt to Targets: " + st.OwnerStats.PainDealtToVIP);
                gameTypeStats.Add(st => "Damage taken by Targets: " + st.OwnerStats.PainReceivedByVIP);
            }
            else
            {
                gameTypeStats.Add(st => "Damage dealt as Target: " + st.OwnerStats.PainDealtAsVIP);
                gameTypeStats.Add(st => "Damage taken as Target: " + st.OwnerStats.PainReceivedAsVIP);
            }
        }
        if (gameRules.WaypointFight != null)
        {
            gameTypeStats.Add(st => "Waypoints captured: " + st.OwnerStats.WaypointsCaptured);
        }

        //Other stuff.
        otherStats.Add(st => "Jumps: " + st.OwnerStats.Jumps);
        otherStats.Add(st => "Damage dealt to allies: " + st.OwnerStats.PainDealtToAllies);
        otherStats.Add(st => "Damage taken from allies: " + st.OwnerStats.PainReceivedFromAllies);

        //Time spent doing various things.
        timingStats.Add(st => "Time spent on the ground: " + st.OwnerStats.TimeOnGround);
        if (UnityEngine.Random.value > 0.5f)
        {
            timingStats.Add(st => "Time spent in air: " + st.OwnerStats.TimeInAir);
            timingStats.Add(st => "Time spent hurt: " + st.OwnerStats.TimeHurt);
        }
        else
        {
            timingStats.Add(st => "Time spent on a wall: " + st.OwnerStats.TimeOnWall);
            timingStats.Add(st => "Time spent on a ceiling: " + st.OwnerStats.TimeOnCeiling);
        }

        #endregion

        #region Get the stats

        foreach (StateMachine st in players)
        {
            statsToDisplay.Add(st, new List<List<string>>() { new List<string>(), new List<string>(), new List<string>() });

            foreach (Func<StateMachine, string> stats in gameTypeStats)
            {
                statsToDisplay[st][0].Add(stats(st));
            }
            foreach (Func<StateMachine, string> stats in otherStats)
            {
                statsToDisplay[st][1].Add(stats(st));
            }
            foreach (Func<StateMachine, string> stats in timingStats)
            {
                statsToDisplay[st][2].Add(stats(st));
            }
        }

        #endregion

        return statsToDisplay;
    }

    //Physics properties.

    PlayerProperties PlayProps;
    ActorProperties ActorProps;
    PlayerConstants PlayConsts;
    ActorConstants ActorConsts;

    public float Spec
    {
        get { return spec; }
        set { spec = SpecInterval.Clamp(value); }
    }
    [SerializeField]
    private float spec;
    private static Interval SpecInterval = new Interval(-1.0f, 1.0f, true, 2);

    public float Speed { get; private set; }
    public float Strength { get; private set; }

    //General properties.

    public int Jumps = 0;

    public float TimeOnGround = 0.0f;
    public float TimeOnCeiling = 0.0f;
    public float TimeOnWall = 0.0f;
    public float TimeInAir = 0.0f;
    public float TimeHurt = 0.0f;

    public int PowerupsCollected = 0;
    public float PowerupMultiplier = 1.0f;

    //TODO: Make a system for bursts and reflect them in the Player/ActorProperties.
    public int BurstsTriggered = 0;//

    public float PainDealt = 0.0f;
    public float PainReceived = 0.0f;

    public float PainDealtToEnemy = 0.0f;
    public float PainReceivedByEnemy = 0.0f;

    public float PainDealtToVIP = 0.0f;
    public float PainReceivedByVIP = 0.0f;

    public float PainDealtToAllies { get { return PainDealt - PainDealtToEnemy; } }
    public float PainReceivedFromAllies { get { return PainReceived - PainReceivedByEnemy; } }

    //Game-type-specific properties.

    public bool IsVIP = false;
    public double TotalTimeAsVIP = 0.0;
    public int TimesAsVIP = 0;

    public float PainDealtAsVIP = 0.0f;
    public float PainReceivedAsVIP = 0.0f;

    public int FlagsCaptured = 0;
    public int FlagsReturned = 0;
    public bool CarryingFlag = false;

    public int WaypointsCaptured = 0;

    void Start()
    {
        PlayProps = GetComponent<PlayerProperties>();
        ActorProps = GetComponent<ActorProperties>();

        PlayConsts = WorldConstants.ConstantsOwner.GetComponent<PlayerConstants>();
        ActorConsts = WorldConstants.ConstantsOwner.GetComponent<ActorConstants>();
    }

    void Update()
    {
        Speed = SpecInterval.Map(ActorConsts.SpeedInterval, Spec);
        Strength = SpecInterval.Map(ActorConsts.StrengthInterval, SpecInterval.ReflectAroundCenter(Spec));
    }
}
