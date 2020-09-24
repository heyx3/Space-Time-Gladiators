using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CreateMirrors))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SetMeshFor2D))]
public class FlagBehavior : MonoBehaviour
{
	//Cached references.
	CreateMirrors mirrorScript;
	Animator animate;
	SetMeshFor2D meshController;
	LevelManager manager;
	Rules gameRules { get { return manager.MatchRules; } }
    public CTFRules ctfRules { get { return gameRules.CTF; } }
	
	//Flag base data.
	public FlagBaseBehavior FlagBase;
	public double TimeSinceFlagDropped;
	public bool AtHome;
	
	//The actor carrying the flag.
	public StateMachine Carrying;
	public Color Team;
	private float damageTakenBeforeFlagTaken;
	
    //The flag resetting after being captured.
    private float timeLeftTillReset;

	//TODO: "Don't be a dick" griefing timer for if the flag carrier isn't really moving ("hurt/throw" the player -- with lots of pain taken -- and make him drop the flag).
	
	public void SetData(FlagBaseBehavior flagBase, Color team)
    {
		Team = team;
		FlagBase = flagBase;
		renderer.material.SetColor ("_TeamCol", Team);
	}
	
	void Start ()
	{
		InitReferences();
		InitFlagBaseData();
		InitFlagCarrierData();
        InitFlagCappedReset();
	}
	private void InitReferences()
	{
		mirrorScript = GetComponent<CreateMirrors>();
		animate = GetComponent<Animator>();
		meshController = GetComponent<SetMeshFor2D>();
		manager = WorldConstants.MatchController.GetComponent<LevelManager>();
	}
	private void InitFlagBaseData() {
		
		TimeSinceFlagDropped = 0.0;
		AtHome = true;
	}
	private void InitFlagCarrierData() {
		Carrying = null;
	}
    private void InitFlagCappedReset()
    {
        timeLeftTillReset = -100.0f;
		TimeSinceFlagDropped = ctfRules.FlagTimeToReset + 10.0f;
    }

	void Update ()
	{		
		UpdateReferences ();
		UpdateFlagBaseData();
		UpdateFlagCarrierData();
        UpdateFlagCappedReset();
	}
	private void UpdateReferences()
    { 
	}
	private void UpdateFlagBaseData()
    {
		//If the flag isn't being carried, keep checking its timer for being reset.
		if (Carrying == null && TimeSinceFlagDropped <= ctfRules.FlagTimeToReset)
        {
            TimeSinceFlagDropped += Time.deltaTime;

            if (TimeSinceFlagDropped > ctfRules.FlagTimeToReset)
            {
                ResetFlag(manager.MatchRules.CTF.FlagTimeToReset > 0.0f);
            }
		}
	}
	private void UpdateFlagCarrierData()
    {
		//React to a player taking this flag since the last update step.
		if (reactToTaken)
        {
			Carrying = takenBy.GetComponent<StateMachine>();
			damageTakenBeforeFlagTaken = Carrying.OwnerStats.PainReceived;
			
			AtHome = false;
			
			reactToTaken = false;
		}
		
		//Nothing to do here if nobody is carrying this flag.
        if (Carrying == null)
        {
            return;
        }
		
		//Keep following the carrier.
		Vector3 newPos = Carrying.transform.position;
		newPos.x += WorldConstants.CollObjConsts.FlagOffsetFromCarrier.x;
        newPos.y += WorldConstants.CollObjConsts.FlagOffsetFromCarrier.y;
		transform.position = newPos;
		
		//Flip the flag animation based on the carrier's x velocity.
        if (Carrying.Velocity.x > 0)
        {
            meshController.SetDir(SetMeshFor2D.ReflectDir.Right);
        }
        else if (Carrying.Velocity.x < 0)
        {
            meshController.SetDir(SetMeshFor2D.ReflectDir.Left);
        }
		
		//If the carrier took enough damage, make him drop the flag.
		if (Carrying.OwnerStats.PainReceived > damageTakenBeforeFlagTaken + ctfRules.FlagPainReceivedDrop)
        {
			Carrying.OwnerStats.CarryingFlag = false;
			Carrying = null;
			TimeSinceFlagDropped = 0.0;
		}
	}
    private void UpdateFlagCappedReset()
    {
        if (timeLeftTillReset <= 0.0f) return;

        timeLeftTillReset -= Time.deltaTime;
        if (timeLeftTillReset <= 0.0f)
        {
            ResetFlag(false);
        }
    }
	
	bool reactToTaken = false;
	StateMachine takenBy;
	void CollideWithActor(StateMachine st) {
		
        bool sameTeam = st.ActorData.Team == Team;
		
		//If the player is going to pick this flag up, store him as the carrier.
		//TODO: Allow for carrying multiple flags? Make it a match option, if we can figure out a way to indicate multiple bags visually.
		if (Carrying == null &&
			(gameRules.EnemiesArePeopleToo || st.IsPlayer) &&
			!st.OwnerStats.CarryingFlag && !sameTeam &&
            !(st.CurrentState is HurtState))
		{
			reactToTaken = true;
            takenBy = st;
            WorldConstants.CrowdCheering.CreateCrowdCheer(CrowdCheers.Events.FlagGrabbed);
            WorldConstants.Creator.CreateFloatingTextsForPlayers(WorldConstants.Creator.FourMessages(WorldConstants.ActorConsts.FlagGrabbedMessages, st, Team),
                                                                 st2 => st2.ActorData.Team == st.ActorData.Team,
                                                                 WorldConstants.Creator.CreateFlagFloatingText);
		}
		
		//Otherwise, if the player is from the same team, the flag might be returned.
		else if (Carrying == null && !AtHome && sameTeam && ctfRules.TouchFlagToReturn)
		{
			ResetFlag(true);
		}
	}

	void CollideWithOther(GameObject data)
	{
		//Only care about collisions with a flag base.
		if (data.tag == "FlagBase")
		{
			//If it's not being carried, wait until a player touched it to do anything.
			if (Carrying == null) return;
			
			//If it's not this flag's base, and the base is the carrying player's base,
            //   and he is able to capture at the moment, then this flag was captured.
			FlagBaseBehavior flBB = data.GetComponent<FlagBaseBehavior>();
			if (flBB.Team != Team &&
				flBB.Team == Carrying.ActorData.Team &&
				(flBB.Flag.AtHome || !ctfRules.FlagAtHomeToScore))
			{
                WorldConstants.Creator.CreateFloatingTextsForPlayers(WorldConstants.Creator.FourMessages(WorldConstants.ActorConsts.FlagCapturedMessages, takenBy, Team),
                                                                     st => st.ActorData.Team == Carrying.ActorData.Team,
                                                                     WorldConstants.Creator.CreateFlagFloatingText);
				WorldConstants.CrowdCheering.CreateCrowdCheer(CrowdCheers.Events.FlagCaptured);
				
                if (ctfRules.FlagRespawnDelay > 0.0f)
				{
                    RemoveFlag();
				}
                else
				{
					ResetFlag(false);
				}
			}
		}
	}
	
    /// <summary>
    /// Removes any actors from carrying this flag and sets it back at its base.
    /// </summary>
    /// <param name="timeOrTouchReturn">If "true", this flag was reset because an ally touched it or it stood untouched for too long.
    /// If "false", this flag was reset after being captured.</param>
	public void ResetFlag(bool timeOrTouchReturn)
    {
		Carrying = null;
		AtHome = true;
		
		Vector3 newP = FlagBase.transform.position;
		newP.x += WorldConstants.CollObjConsts.FlagOffsetFromBase.x;
		newP.y += WorldConstants.CollObjConsts.FlagOffsetFromBase.y;
		
		transform.position = newP;

        if (timeOrTouchReturn || manager.MatchRules.CTF.FlagRespawnDelay > 0.0f)
        {
            WorldConstants.Creator.CreateFloatingTextsForPlayers(WorldConstants.Creator.ThreeMessages(WorldConstants.ActorConsts.FlagReturnedMessages, Team, null),
                                                                 st => st.ActorData.Team == Team,
                                                                 WorldConstants.Creator.CreateFlagFloatingText);
            WorldConstants.CrowdCheering.CreateCrowdCheer(CrowdCheers.Events.FlagReturned);
        }
	}
    /// <summary>
    /// Removes the flag from the level for a certain amount of time (defined by the match's CTFRules).
    /// </summary>
    public void RemoveFlag()
    {
		//Set the timer.
        timeLeftTillReset = ctfRules.FlagRespawnDelay;

        //Push the flag waaaay out of the way.
        transform.position += 10.0f * new Vector3(WorldConstants.Size.x, WorldConstants.Size.y, 0.0f);
		
		//Release control of it.
		Carrying = null;
		AtHome = false;
    }
}