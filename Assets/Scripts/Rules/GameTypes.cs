using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[Serializable]
public class PainRules : GameTypeRules
{
	//TODO: Multiplier for how much pain taken removes from score.

    public override float GetScore(Stats actorStats)
    {
        return actorStats.PainDealtToEnemy;
    }
}

[Serializable]
public class CTFRules : GameTypeRules
{
	//TODO: Option for flag gravity.

    public bool FlagAtHomeToScore;

    public float FlagCarrySpeedScale;
    public float FlagCarryStrengthScale;

    public float FlagPainReceivedDrop;
    public double FlagTimeToReset;

    public float FlagRespawnDelay;

    public bool TouchFlagToReturn;
	
    public CTFRules(bool flagAtHomeToScore,
                    float flagCarrySpeedScale,
                    float flagCarryStrengthScale,
					float flagPainReceivedDrop,
					double flagTimeToReset,
                    float flagRespawnDelay,
					bool touchFlagToReturn)
    {
        FlagAtHomeToScore = flagAtHomeToScore;
        FlagCarrySpeedScale = flagCarrySpeedScale;
        FlagCarryStrengthScale = flagCarryStrengthScale;
		
		FlagPainReceivedDrop = flagPainReceivedDrop;
		FlagTimeToReset = flagTimeToReset;
		
		TouchFlagToReturn = touchFlagToReturn;

        FlagRespawnDelay = flagRespawnDelay;
    }

    public override float GetScore(Stats actorStats)
    {
        return actorStats.FlagsCaptured;
    }
}

[Serializable]
public class BullseyeRules : GameTypeRules
{
    //TODO: Test switching players.
    //TODO: Fix this game-type: Players on the VIP's team can't be hurt for points, and add customizability by giving VIP's strength and speed modifiers.

    /// <summary>
    /// The different ways the next player target can be chosen.
    /// </summary>
    public enum NextTargetSelection
    {
        HighestScore,
        LowestScore,
        FewestTimesAsVIP,
        Random,
    }
    public NextTargetSelection ChooseNewTarget;

    //The limiting values that determine when it is time to change the VIP.
    public double TimeAsTarget;
    public float DealtPainThresholdAsTarget;
    public float TakenPainThresholdAsTarget;

    [NonSerialized]
    public StateMachine CurrentVIP;
    [NonSerialized]
    public AuraBehavior Aura;
    private ParticleSystem auraParticles;

    //The counters for when the current VIP should change.
	[SerializeField]
    private double timeTillChange;
	[SerializeField]
    private float totalPainDealtThreshold;
	[SerializeField]
    private float totalPainReceivedThreshold;

    public BullseyeRules(NextTargetSelection chooseNewTarget,
                         double timeAsTarget,
                         float thresholdForTargetTakingPain,
                         float thresholdForTargetDealingPain)
    {
        ChooseNewTarget = chooseNewTarget;

        TimeAsTarget = timeAsTarget;
        DealtPainThresholdAsTarget = thresholdForTargetDealingPain;
        TakenPainThresholdAsTarget = thresholdForTargetTakingPain;

        CurrentVIP = null;
    }

    public override float GetScore(Stats actorStats)
    {
        return actorStats.PainDealtAsVIP;
    }
	
	private bool createdAura = false;
    public override void Update()
    {
        if (Aura != null)
        {
            auraParticles.enableEmission = true;
        }

		if (!createdAura || CurrentVIP == null ||
            timeTillChange <= 0.0 ||
            CurrentVIP.OwnerStats.PainDealtAsVIP > totalPainDealtThreshold ||
            CurrentVIP.OwnerStats.PainReceivedAsVIP > totalPainReceivedThreshold)
		{
            ChangeTarget();
            if (CurrentVIP == null)
            {
                return;
            }

            WorldConstants.Creator.CreateFloatingTextsForPlayers(WorldConstants.Creator.FourMessages(WorldConstants.ActorConsts.VIPChangedMessages, CurrentVIP, null),
                                                                 st => st.OwnerStats.IsVIP,
                                                                 WorldConstants.Creator.CreateVIPFloatingText);
            WorldConstants.CrowdCheering.CreateCrowdCheer(CrowdCheers.Events.NewVIP);
			createdAura = true;
		}
        if (CurrentVIP == null)
		{
			return;
		}

        timeTillChange -= Time.deltaTime;
        CurrentVIP.OwnerStats.TotalTimeAsVIP += Time.deltaTime;
    }

    /// <summary>
    /// Thrown when no new Targets can be chosen.
    /// </summary>
    private class NoTargetsException : System.Exception
    {
        public NoTargetsException() : base("No potential Bullseye Targets!") { }
    }

    private void ChangeTarget()
    {
        if (CurrentVIP != null)
            CurrentVIP.OwnerStats.IsVIP = false;

        //Choose the next VIP.
        switch (ChooseNewTarget)
        {
            case NextTargetSelection.Random:
                CurrentVIP = GetRandomPossibleVIP(CurrentVIP);
                break;

            case NextTargetSelection.HighestScore:
                CurrentVIP = GetHighestScoringPossibleVIP(CurrentVIP);
                break;

            case NextTargetSelection.LowestScore:
                CurrentVIP = GetLowestScoringPossibleVIP(CurrentVIP);
                break;

            case NextTargetSelection.FewestTimesAsVIP:
                CurrentVIP = GetPossibleFewestTimesVIP(CurrentVIP);
                break;

            default: throw new NotImplementedException();
        }

        //If there are no eligible VIP's, complain.
        if (CurrentVIP == null)
        {
            return;//throw new NoTargetsException();
        }

        //Set the counters until the next VIP.
        timeTillChange = TimeAsTarget;
        totalPainDealtThreshold = CurrentVIP.OwnerStats.PainDealtAsVIP + DealtPainThresholdAsTarget;
        totalPainReceivedThreshold = CurrentVIP.OwnerStats.PainReceivedAsVIP + TakenPainThresholdAsTarget;

        //Set stats.
		if (Aura == null)
		{
        	Aura = WorldConstants.Creator.CreateAura(CurrentVIP.transform).GetComponent<AuraBehavior>();
            auraParticles = Aura.transform.GetChild(0).GetComponent<ParticleSystem>();
		}
        auraParticles.enableEmission = false;
        Aura.transform.parent = CurrentVIP.transform;
        Aura.VIPChanged();
        CurrentVIP.OwnerStats.IsVIP = true;
        CurrentVIP.OwnerStats.TimesAsVIP += 1;
    }
    private StateMachine GetRandomPossibleVIP(StateMachine old)
    {
        StateMachine newVIP = null;
		if (Owner.Collision == null)
		{
			return null;
		}
		
        while (newVIP == null || (!Owner.EnemiesArePeopleToo && !newVIP.IsPlayer) || newVIP == old)
		{
            int index = Mathf.RoundToInt(UnityEngine.Random.Range(0, Owner.Collision.Actors.Count));
			if (index < Owner.Collision.Actors.Count)
			{
            	newVIP = Owner.Collision.Actors[index].GetComponent<StateMachine>();
			}
			else
			{
				Debug.Log("Number too big!");	
			}
		}
        return newVIP;
    }
    private StateMachine GetHighestScoringPossibleVIP(StateMachine old)
    {
        StateMachine newV = GetRandomPossibleVIP(old);
        if (newV == null) return null;

        for (int i = 0; i < Owner.Collision.Actors.Count; ++i)
        {
            if (newV.gameObject != Owner.Collision.Actors[i] && newV != old)
            {
                if (GetScore(newV.OwnerStats) >
                    GetScore(Owner.Collision.Actors[i].GetComponent<Stats>()))
                {
                    newV = Owner.Collision.Actors[i].GetComponent<StateMachine>();
                }
            }
        }

        return newV;

    }
    private StateMachine GetLowestScoringPossibleVIP(StateMachine old)
    {
        StateMachine newV = GetRandomPossibleVIP(old);
        if (newV == null) return null;

        for (int i = 0; i < Owner.Collision.Actors.Count; ++i)
        {
            if (newV.gameObject != Owner.Collision.Actors[i] && newV != old)
            {
                if (GetScore(newV.OwnerStats) <
                    GetScore(Owner.Collision.Actors[i].GetComponent<Stats>()))
                {
                    newV = Owner.Collision.Actors[i].GetComponent<StateMachine>();
                }
            }
        }

        return newV;
    }
    private StateMachine GetPossibleFewestTimesVIP(StateMachine old)
    {
        StateMachine newV = GetRandomPossibleVIP(old);
        if (newV == null) return null;

        for (int i = 0; i < Owner.Collision.Actors.Count; ++i)
        {
            if (newV.gameObject != Owner.Collision.Actors[i] && newV != old)
            {
                if (newV.OwnerStats.TimesAsVIP >
                    Owner.Collision.Actors[i].GetComponent<Stats>().TimesAsVIP)
                {
                    newV = Owner.Collision.Actors[i].GetComponent<StateMachine>();
                }
            }
        }

        return newV;
    }
}

[Serializable]
public class PowerupHuntRules : GameTypeRules
{
    public override float GetScore(Stats actorStats)
    {
        return actorStats.PowerupsCollected;
    }
}

[Serializable]
public class WaypointFightRules : GameTypeRules
{
    public int CurrentWaypointIndex;

    /// <summary>
    /// The amount of time (in seconds) between one waypoint being taken and the next one spawning.
    /// </summary>
    public float NewWaypointDelay;

    private SpawnCreator spawns { get { return WorldConstants.MatchData.Spawns; } }

    public WaypointFightRules(float waypointDelay)
	{
        CurrentWaypointIndex = 0;
		
		NewWaypointDelay = waypointDelay;
	}

    public override float GetScore(Stats actorStats)
    {
        return actorStats.WaypointsCaptured;
    }
	
	[SerializeField]
    private float elapsed = 0.0f;
	private GameObject waypoint;
    public override void Update()
    {
		//Initialize/find the waypoint object.
		if (waypoint == null)
		{
			waypoint = GameObject.FindGameObjectWithTag("Waypoint");
		}
		
        //If there is no waypoint, find a new one.
        if (waypoint == null)
        {
            elapsed += Time.deltaTime;
            if (elapsed >= NewWaypointDelay)
			{
                //Get the next spawn location.
				GenerateObjects objG = Owner.Owner.ObjGenerator;
				List<Location> waySpawns = spawns.OtherSpawnsCreated[Spawns.Waypoint];
				CurrentWaypointIndex += 1;
				CurrentWaypointIndex %= waySpawns.Count;
				
				//Spawn a waypoint.
                Location spawn = spawns.OtherSpawnsCreated[Spawns.Waypoint][CurrentWaypointIndex];
                waypoint = Owner.Owner.Creator.CreateWaypoint(GenerateObjects.ToWorldPos(spawn, WorldConstants.MatchData));
                WorldConstants.Creator.CreateFloatingTextsForPlayers(st => WorldConstants.ActorConsts.WaypointSpawnedMessages.Allies,
                                                                     st => true,
                                                                     WorldConstants.Creator.CreateWaypointFloatingText);
				WorldConstants.CrowdCheering.CreateCrowdCheer(CrowdCheers.Events.WaypointSpawned);
				
				elapsed = 0.0f;
			}
        }
		//Otherwise, reset the timer to spawn a new one.
        else
		{
			elapsed = 0.0f;
		}
    }
}