using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Represents the full set of rules for a match.
/// </summary>
[Serializable]
public class Rules
{
	//TODO: Limiting number of teams/players.
	
	public string Description { get; set; }
	
    public CollisionTracker Collision = null;

    //TODO: Enabling/disabling powerup effects.

    public float ScoreGoal;
    public TimeSpan MatchLength;

    public TimeSpan PowerupSpawnInterval;

    public bool WaitForPowerupCollection;

    public TimeSpan RegenInterval;

    public float MomentumDamp;

    public bool EnemiesArePeopleToo;

    //Cached reference that will be set by the LevelManager that owns this rule-set.
    public LevelManager Owner;

    //The different game-types.

    public IEnumerable<GameTypeRules> EnabledGameTypes
    {
        get
        {
            if (Pain != null) yield return Pain;
            if (CTF != null) yield return CTF;
            if (Bullseye != null) yield return Bullseye;
            if (WaypointFight != null) yield return WaypointFight;
            if (PowerupHunt != null) yield return PowerupHunt;
        }
    }

    public CTFRules CTF
    {
        get { if (ctfEnabled) return ctf; else return null; }

        private set
        {
            ctf = value;
            ctf.Owner = this;
            if (value != null)
                ctfEnabled = true;
            else ctfEnabled = false;
        }
    }
    [SerializeField]
    private CTFRules ctf;
    private bool ctfEnabled = false;
    public void SetCTFRules(CTFRules settings, float importance)
    {
        CTF = settings;
        CTFImportance = importance;
    }

    public PainRules Pain
    {
        get { if (painEnabled) return pain; else return null; }

        private set
        {
            pain = value;
            pain.Owner = this;
            if (value != null)
                painEnabled = true;
            else painEnabled = false;
        }
    }
    [SerializeField]
    private PainRules pain;
    private bool painEnabled = false;
    public void SetPainRules(PainRules settings, float importance)
    {
        Pain = settings;
        PainImportance = importance;
    }

    public BullseyeRules Bullseye
    {
        get { if (bullseyeEnabled) return bullseye; else return null; }

        private set
        {
            bullseye = value;
            bullseye.Owner = this;
            if (value != null)
                bullseyeEnabled = true;
            else bullseyeEnabled = false;
        }
    }
    [SerializeField]
    private BullseyeRules bullseye;
    private bool bullseyeEnabled = false;
    public void SetBullseyeRules(BullseyeRules settings, float importance)
    {
        Bullseye = settings;
        BullseyeImportance = importance;
    }

    public WaypointFightRules WaypointFight
    {
        get { if (waypointFightEnabled) return waypointFight; else return null; }

        private set
        {
            waypointFight = value;
            waypointFight.Owner = this;
            if (value != null)
                waypointFightEnabled = true;
            else waypointFightEnabled = false;
        }
    }
    [SerializeField]
    private WaypointFightRules waypointFight;
    private bool waypointFightEnabled = false;
    public void SetWaypointFightRules(WaypointFightRules settings, float importance)
    {
        WaypointFight = settings;
        WaypointFightImportance = importance;
    }

    public PowerupHuntRules PowerupHunt
    {
        get { if (powerupHuntEnabled) return powerupHunt; else return null; }

        private set
        {
            powerupHunt = value;
            powerupHunt.Owner = this;
            if (value != null)
                powerupHuntEnabled = true;
            else powerupHuntEnabled = false;
        }
    }
    [SerializeField]
    private PowerupHuntRules powerupHunt;
    private bool powerupHuntEnabled = false;
    public void SetPowerupHuntRules(PowerupHuntRules settings, float importance)
    {
        PowerupHunt = settings;
        PowerupHuntImportance = importance;
    }

    //The importance of each game-type.

    public float CTFImportance = 1.0f;
    public float PainImportance = 1.0f;
    public float BullseyeImportance = 1.0f;
    public float WaypointFightImportance = 1.0f;
    public float PowerupHuntImportance = 1.0f;

    public Rules()
    {
		Description = "";
		
        ScoreGoal = Single.MaxValue;
        MatchLength = TimeSpan.MaxValue;

        PowerupSpawnInterval = TimeSpan.FromSeconds(30);

        WaitForPowerupCollection = true;

        RegenInterval = TimeSpan.FromSeconds(30);

        MomentumDamp = 0.7f;

        EnemiesArePeopleToo = false;
    }

    /// <summary>
    /// Gets the total score for the given actor by summing the products of each enabled game-type score with its importance.
    /// </summary>
    public float GetScore(Stats actorStats)
    {
        float sum = 0.0f;

        sum += Score(Pain, actorStats, PainImportance);
        sum += Score(CTF, actorStats, CTFImportance);
        sum += Score(Bullseye, actorStats, BullseyeImportance);
        sum += Score(WaypointFight, actorStats, WaypointFightImportance);
        sum += Score(PowerupHunt, actorStats, PowerupHuntImportance);

        return actorStats.PowerupMultiplier * sum;
    }
    /// <summary>
    /// Returns the given game-type's scoring for a given actor and given importance, or 0.0f if "gtr" is null.
    /// </summary>
    private float Score(GameTypeRules gtr, Stats actorStats, float importance)
    {
        if (gtr == null) return 0.0f;

        return gtr.GetScore(actorStats) * importance;
    }

    private bool madeWarningYet = false;
    public void Update()
    {
        if (Collision == null)
        {
            Collision = WorldConstants.ColTracker;
        }

        foreach (GameTypeRules gtr in EnabledGameTypes)
        {
            gtr.Update();
        }
    }
}

/// <summary>
/// Represents a set of game-type-specific rules.
/// </summary>
[Serializable]
public abstract class GameTypeRules
{
    public Rules Owner { get; set; }
	
	public GameTypeRules() { Owner = null; }
	
    public abstract float GetScore(Stats actorStats);

    public virtual void Update() { }
}