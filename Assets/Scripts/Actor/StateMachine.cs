using UnityEngine;
using System.Collections;

/// <summary>
/// A Finite State Machine used for handling an Actor's behavior and physics.
/// </summary>
[RequireComponent(typeof(SetMeshFor2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Stats))]
[RequireComponent(typeof(IDData))]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(CollisionManager))]
[RequireComponent(typeof(ActorProperties))]
public class StateMachine : MonoBehaviour
{
    public static void DrawBounds(RecBounds b, Color col)
    {
        Vector3 topLeft = new Vector3(b.center.x - b.extents.x, b.center.y + b.extents.y, -0.001f);
        Vector3 topRight = new Vector3(b.center.x + b.extents.x, b.center.y + b.extents.y, -0.001f);
        Vector3 botLeft = new Vector3(b.center.x - b.extents.x, b.center.y - b.extents.y, -0.001f);
        Vector3 botRight = new Vector3(b.center.x + b.extents.x, b.center.y - b.extents.y, -0.001f);

        Debug.DrawLine(topLeft, topRight, col);
        Debug.DrawLine(topLeft, botLeft, col);
        Debug.DrawLine(botRight, botLeft, col);
        Debug.DrawLine(botRight, topRight, col);
    }

    public static int Sign(float f, float error)
    {
        if (WithinError(f, 0.0f, error)) return 0;
        if (f < 0.0f) return -1;
        else return 1;
    }
    public static bool WithinError(float one, float two, float error) { return Mathf.Abs(one - two) < error; }

    /// <summary>
    /// The resulting winner and loser velocities after a collision between two players.
    /// </summary>
    public struct CollisionVelocityResults
    {
        public Vector2 Winner, Loser;
    }
    /// <summary>
    /// Gets the winner's velocity and the loser's velocity after they collide, assuming both actors are players.
    /// </summary>
    public static CollisionVelocityResults NewVs(StateMachine winner, StateMachine loser)
    {
        //Get the momentum of both actors together.
        Vector2 totalMomentum = (winner.Velocity * winner.ActorProps.Mass) +
                                (loser.Velocity * loser.ActorProps.Mass);

        totalMomentum *= loser.Manager.MatchRules.MomentumDamp;

        //Get the winner's momentum.

        Vector2 winnerV = winner.Velocity,
                loserV = loser.Velocity;
        //TODO: Extrapolate to PlayerConstants.
        float WinCollisionMovingUpwardsScale = 1.4f,
              WinCollisionMovingDownwardsScale = 0.85f,
              WinCollisionMovingUpDiagonalRotLerp = 0.65f,
              WinCollisionMovingDownDiagonalIncrement = 0.0f,
              WinCollisionMovingSidewaysIncrement = 9.0f;

        //Different cases for when the winner is moving...
        if (StateMachine.WithinError(winnerV.x, 0.0f, 0.01f))
        {
            if (winnerV.y > 0.0f)
            {
                //...straight up.

                winnerV.y *= WinCollisionMovingUpwardsScale;
            }
            else
            {
                //...straight down.

                winnerV.y = WinCollisionMovingDownwardsScale * -winnerV.y;
            }
        }
        else if (StateMachine.WithinError(winnerV.y, 0.0f, 0.01f))
        {
            //...straight to the side.

            winnerV.y += WinCollisionMovingSidewaysIncrement;
        }
        else if (winnerV.y > 0.0f)
        {
            //...upwards to the side.
			
			Vector3 rotAxis = new Vector3(0, 1, 0);
			float toUpwards = -Vector3.Angle (winnerV, rotAxis);
			if (winnerV.x < 0.0f)
			{
				rotAxis *= -1.0f;
			}
			float rotAmount = Mathf.Lerp (0.0f, toUpwards, WinCollisionMovingUpDiagonalRotLerp);
			Quaternion rot = Quaternion.AngleAxis(rotAmount, rotAxis);
			
			winnerV = rot * winnerV;
        }
        else
        {
            //...downwards to the side.

            winnerV.y = -winnerV.y + WinCollisionMovingDownDiagonalIncrement;
        }

        //Use conservation of momentum to get the loser's momentum.
        loserV = (totalMomentum - (winnerV * winner.ActorProps.Mass)) / loser.ActorProps.Mass;

        return new CollisionVelocityResults() { Winner = winnerV, Loser = loserV };
    }

    //TODO: When the actor wraps around, force the collision manager to check for new collisions (especially walls).
	
    public ActorState CurrentState
    {
        get;
        private set;
    }
    public ActorState LastState
    {
        get;
        private set;
    }
    public void ChangeState(ActorState newState, bool lockControls)
    {
        //Lock the controls.
        if (lockControls) LockVerticalControls = true;

        //Change state.
        LastState = CurrentState;
        CurrentState = newState;
        CurrentState.Owner = this;

        //Notify the states.
        if (LastState != null)
            LastState.ExitingState();
        CurrentState.EnteringState();
    }

    public ActorProperties ActorProps;
    public PlayerProperties PlayerProps;
    public LevelManager Manager;
    public InputManager Input;
	
	public CameraFollowScript CameraScript { get; private set; }
	public CameraConstants CameraConsts { get; private set; }
	
    public Stats OwnerStats;
    public IDData ActorData;
    public Animator Animator;
    public BoxCollider Coll;
    public SetMeshFor2D MeshController;
    public CollisionManager ColManager;

    public ParticleHandler RunningParts, WallslidingParts;
    public ParticleHandler SoftLanding, HardLanding;

    public bool IsPlayer { get { return name == "Player"; } }

    public string StringState;

    public Vector3 Velocity;
    public Vector3 Propulsion;

    public Vector3 MostRecentPropulsion;

    public Vector3 CurrentGravity
    {
        get { return CurrentState.Gravity; }
    }

    /// <summary>
    /// The player's left/right and up/down input, between (-1, -1) and (1, 1).
    /// </summary>
    public Vector2 MovementKeyAxes;
    /// <summary>
    /// If true, the up/down movement keys shouldn't trigger the Up() or Down() functions in the actor state.
    /// </summary>
    public bool LockVerticalControls;

    void Awake()
	{
        //Cache references to other actor components.

        OwnerStats = GetComponent<Stats>();
        ActorData = GetComponent<IDData>();
        Animator = GetComponent<Animator>();
        MeshController = GetComponent<SetMeshFor2D>();
        Coll = GetComponent<BoxCollider>();
        ColManager = GetComponent<CollisionManager>();

        ActorProps = GetComponent<ActorProperties>();
        PlayerProps = GetComponent<PlayerProperties>();
		CameraConsts = WorldConstants.ConstantsOwner.GetComponent<CameraConstants>();
    }

    void Start()
    {
        //Cache references to manager components.
        GameObject g = WorldConstants.MatchController;
        Manager = g.GetComponent<LevelManager>();
        Input = g.GetComponent<InputManager>();

        //Bind the input manager.
        Input.RegisterPlayerInput(ActorData.PlayerID, Input.FindFirstAvailableInput());

        //Initialize data.
        MovementKeyAxes = Vector2.zero;
        LockVerticalControls = false;
        ChangeState(GetStartingState(), false);

        RunningParts = WorldConstants.Creator.CreateRunningDebrisParticles(this);
        WallslidingParts = WorldConstants.Creator.CreateWallslidingDebrisParticles(this);
        SoftLanding = WorldConstants.Creator.CreateLandingParticles(false, this);
        HardLanding = WorldConstants.Creator.CreateLandingParticles(true, this);
		
		foreach (GameObject go in GameObject.FindGameObjectsWithTag("MainCamera"))
		{
			if (go.name.Substring (0, 1) == ActorData.PlayerID.ToString())
			{
				CameraScript = go.GetComponent<CameraFollowScript>();
				break;
			}
		}
    }
    private ActorState GetStartingState()
    {
        if (tag == "Player")
            return ScriptableObject.CreateInstance<InAirState>();

        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Gets the character controller's current bounds.
    /// </summary>
    public RecBounds ActorBounds
    {
        get
        {
            RecBounds b = new RecBounds(Coll.bounds);
            return new RecBounds(b.center, new Vector3(b.size.x, b.size.y, 0.0f));
        }
    }
    /// <summary>
    /// Gets the character controller's next bounds (using its current velocity).
    /// </summary>
    public RecBounds NextActorBounds
    {
        get
        {
            RecBounds b = new RecBounds(Coll.bounds);
            return new RecBounds(b.center + (Time.fixedDeltaTime * (Vector2)Velocity), new Vector2(b.size.x, b.size.y));
        }
    }
    /// <summary>
    /// Gets the character controller's previous bounds (using its current velocity).
    /// </summary>
    public RecBounds PreviousActorBounds
    {
        get
        {
            RecBounds b = new RecBounds(Coll.bounds);
            return new RecBounds(b.center - (Time.fixedDeltaTime * (Vector2)Velocity), new Vector2(b.size.x, b.size.y));
        }
    }

    //Update physics and notify actor of input. The bool is used to keep track of whether or not there was a collision.
    private bool collided;
    void FixedUpdate()
    {
        CurrentState.BeforeFixed();

        //React to input.

        if (MovementKeyAxes.x != 0)
        {
            CurrentState = CurrentState;
        }
        CurrentState.Sideways(MovementKeyAxes.x);

        if (MovementKeyAxes.y == 0.0f)
            LockVerticalControls = false;

        if (!LockVerticalControls && MovementKeyAxes.y > 0.0f)
            CurrentState.Up();

        if (!LockVerticalControls && MovementKeyAxes.y < 0.0f)
            CurrentState.Down();


        //Accelerate.

        Velocity += (CurrentGravity + Propulsion) * Time.fixedDeltaTime / ActorProps.Mass;
        MostRecentPropulsion = Propulsion;
        Propulsion = Vector3.zero;

        CurrentState.AfterAccel();


        //Constrain the velocity.

        CurrentState.BeforeConstrainVelocity();

        Velocity.z = 0;
        if (Mathf.Abs(Velocity.x) > PlayerProps.MaxSpeeds.x)
        {
            Velocity.x = Sign(Velocity.x, WorldConstants.MinMovementSpeed) * PlayerProps.MaxSpeeds.x;
        }
        if (Mathf.Abs(Velocity.y) > PlayerProps.MaxSpeeds.y)
        {
            Velocity.y = Sign(Velocity.y, WorldConstants.MinMovementSpeed) * PlayerProps.MaxSpeeds.y;
        }

        CurrentState.AfterConstrainVelocity();


        //Move.

        CurrentState.BeforeMove();

        Velocity = CurrentState.ApplyFriction(Velocity);

        Vector3 old = transform.position;

        //Set velocity to zero if it's too small.
        if (Velocity.magnitude < WorldConstants.MinMovementSpeed)
        {
            Velocity = Vector3.zero;
        }

        //Move.
        if (Velocity != Vector3.zero)
        {
            collided = false;
            transform.position += Velocity * Time.fixedDeltaTime;
        }

        //Set the velocity to the displacement.
        if ((transform.position - old).magnitude / Time.fixedDeltaTime < WorldConstants.MinMovementSpeed)
        {
            Velocity = Vector3.zero;
        }
        if (!collided)
        {
            Velocity = (transform.position - old) / Time.fixedDeltaTime;
        }

        //Check for being outside the map.
        CheckOutside();

        //Finish up.

        CurrentState.AfterMove();

        ColManager.CheckWallBounds();

        CurrentState.AfterFixed();
    }

    /// <summary>
    /// Checks to see if this actor got outside the level somehow, and puts him back in.
    /// </summary>
    private void CheckOutside()
    {
        if (!ActorBounds.Intersects(WorldConstants.LevelBounds))
        {
            Vector2 pos = transform.position;
            RecBounds levelB = WorldConstants.LevelBounds;

            Vector2 dir = new Vector2(Sign(levelB.center.x - pos.x, 0.001f),
                                      Sign(levelB.center.y - pos.y, 0.001f));

            //Move in along the X.
            if (!Manager.LevelGen.GenSettings.WrapX &&
				!(new Interval(levelB.left, levelB.right, true, 2).Inside(pos.x)))
            {
                transform.position += new Vector3(2.0f * dir.x, 0.0f, transform.position.z);
                Velocity.x = 0.0f;

                if (IsPlayer && !(CurrentState is InAirState) && !(CurrentState is HurtState))
                {
                    ChangeState(ScriptableObject.CreateInstance<InAirState>(), false);
                }
            }

            //Move in along the Y.
            if (!Manager.LevelGen.GenSettings.WrapY &&
				!(new Interval(levelB.bottom, levelB.top, true, 2).Inside(pos.y)))
            {
                transform.position += new Vector3(0.0f, 2.0f * dir.y, transform.position.z);
                Velocity.y = 0.0f;

                if (IsPlayer && !(CurrentState is InAirState) && !(CurrentState is HurtState))
                {
                    ChangeState(ScriptableObject.CreateInstance<InAirState>(), false);
                }
            }
        }
    }

    private bool tookFlag = false;
    void Update()
    {
        //Update state.
        CurrentState.Update();
        CurrentState.SetReflection();

        //Debug data.
        StringState = CurrentState.ToString();

        //React to the flag being taken.
        if (tookFlag)
        {
            OwnerStats.CarryingFlag = true;
            tookFlag = false;
        }

        //Parse input.
        MovementKeyAxes = Input.GetInput(ActorData.PlayerID);
    }

    void AnimationDone()
    {
        CurrentState.AnimationDone();
    }

    /// <summary>
    /// If this actor is inside the given collider, moves him to be on the right/left side of the surface.
    /// </summary>
    public void MoveToSideOfSurface(Line l, ColType type)
    {
        if (type == ColType.Bottom || type == ColType.Top)
        {
            throw new System.ArgumentException();
        }

        //Right.
        if (type == ColType.Right)
        {
            transform.position = new Vector3(l.ConstValue + Coll.extents.x - Coll.center.x,
                                             transform.position.y,
                                             transform.position.z);
        }
        //Left.
        else
        {
            transform.position = new Vector3(l.ConstValue - Coll.extents.x - Coll.center.x,
                                             transform.position.y,
                                             transform.position.z);
        }

        //Debug draw.
        DrawBounds(ActorBounds, Color.white);
    }
    /// <summary>
    /// If this actor is inside the given collider, moves him to be on the top/bottom of the surface.
    /// </summary>
    public void MoveAboveOrBelowSurface(Line l, ColType type)
    {
        if (type == ColType.Left || type == ColType.Right)
        {
            throw new System.ArgumentException();
        }

        //Top.
        if (type == ColType.Top)
        {
            transform.position = new Vector3(transform.position.x,
                                             l.ConstValue + Coll.extents.y - Coll.center.y,
                                             transform.position.z);
        }
        //Bottom.
        if (type == ColType.Bottom)
        {
            transform.position = new Vector3(transform.position.x,
                                             l.ConstValue - Coll.extents.y - Coll.center.y,
                                             transform.position.z);
        }

        //Debug draw.
        DrawBounds(ActorBounds, Color.white);
    }

    void WallCollision(ColIndexPair pair)
    {
        ColType c = pair.Type;
        int index = pair.Index;

        Line line = ColManager.WallSides[c][index];

        //Indicate a collision happened.
        collided = true;

        //Floor.
        if (c == ColType.Top)
        {
            MoveAboveOrBelowSurface(line, c);

            //Don't count it as a floor if the player was moving upward.
            if (Velocity.y > 0)
            {
                ColManager.WallSides[c].Remove(line);
                return;
            }

            CurrentState.HitFloor(line);
        }
        //Ceiling.
        else if (c == ColType.Bottom)
        {
            MoveAboveOrBelowSurface(line, c);

            //Don't count it as a ceiling if the player was moving downward.
            if (Velocity.y < 0)
            {
                ColManager.WallSides[c].Remove(line);
                return;
            }

            CurrentState.HitCeiling(line);
        }
        //Right side.
        else if (c == ColType.Left)
        {
            MoveToSideOfSurface(line, c);

            //Don't count it as a wall collision if the player's velocity is pointing away from it.
            if (Sign(Velocity.x, WorldConstants.MinMovementSpeed) == -Sign(line.ConstValue - transform.position.x, WorldConstants.MinMovementSpeed))
            {
                ColManager.WallSides[c].Remove(line);
                return;
            }

            CurrentState.HitSide(line, c);
        }
        //Left side.
        else if (c == ColType.Right)
        {
            MoveToSideOfSurface(line, c);

            //Don't count it as a wall collision if the player's velocity is pointing away from it.
            if (Sign(Velocity.x, WorldConstants.MinMovementSpeed) == -Sign(line.ConstValue - transform.position.x, WorldConstants.MinMovementSpeed))
            {
                ColManager.WallSides[c].Remove(line);
                return;
            }

            CurrentState.HitSide(line, c);
        }
    }
    void ActorCollision(int index)
    {
        StateMachine st = ColManager.Actors[index];

        //At least one must have a certain minimum momentum and both must be hurtable.
        if (CurrentState.Hurtable && st.CurrentState.Hurtable &&
            (Velocity.magnitude >= ActorProps.MinimumCollisionVelocity ||
             st.Velocity.magnitude >= st.ActorProps.MinimumCollisionVelocity))
        {
            float damageToThis, damageToOther;

            //Damage is primarily based on momentum.
            damageToThis = st.Velocity.magnitude * st.ActorProps.Mass;
            damageToOther = Velocity.magnitude * ActorProps.Mass;

            //The actor on top has an advantage which grows as it approaches being directly above.

            float maxColDist = ActorBounds.extents.y + st.ActorBounds.extents.y;

            if (transform.position.y > st.transform.position.y)
            {
                damageToOther *= Mathf.Max(1.0f, ActorProps.MaxAdvantageMultiplier *
                                                    Mathf.Abs(transform.position.y - st.transform.position.y) / maxColDist);
            }

            else if (transform.position.y < st.transform.position.y)
            {
                damageToThis *= Mathf.Max(1.0f, st.ActorProps.MaxAdvantageMultiplier *
                                                    Mathf.Abs(st.transform.position.y - transform.position.y) / maxColDist);
            }

            //If the two actors are from the same team, scale down their damage.
            if (ActorData.Team.Equals(st.ActorData.Team))
            {
                damageToThis *= st.ActorProps.SameTeamMultiplier;
                damageToOther *= ActorProps.SameTeamMultiplier;
            }

            //Figure out which actor won the collision and notify both actors..
			CollisionVelocityResults v = new CollisionVelocityResults();
            if (damageToThis >= damageToOther)
            {
                damageToOther *= ActorProps.LostCollisionMultiplier;
                v = NewVs(st, this);
				
				HurtByActor (st.gameObject, damageToThis, damageToOther, v);
				st.HurtActor (gameObject, damageToOther, damageToThis, v);

                WorldConstants.Creator.CreatePlayerHurtParticles(this);
            }
            else
            {
                damageToThis *= st.ActorProps.LostCollisionMultiplier;
                v = NewVs(this, st);
				
            	HurtActor(st.gameObject, damageToThis, damageToOther, v);
            	st.HurtByActor(gameObject, damageToOther, damageToThis, v);

                WorldConstants.Creator.CreatePlayerHurtParticles(st);
            }

            //Update pain calculations.
            UpdatePainStats(st, damageToThis, damageToOther);
            st.UpdatePainStats(this, damageToOther, damageToThis);
        }
    }
    void OtherCollision(int index)
    {
        GameObject other = ColManager.Others[index];

        if (other.tag == "Powerup")
            OwnerStats.PowerupsCollected += 1;

        else if (other.tag == "Flag")
        {
            FlagBehavior flB = other.GetComponent<FlagBehavior>();

            //If the flag is being carried, don't do anything.
            if (flB.Carrying != null)
                return;
            //If the flag is from the same team, it might have been returned.
            if (flB.Team == ActorData.Team)
            {
                if (!flB.AtHome && Manager.MatchRules.CTF.TouchFlagToReturn)
                    OwnerStats.FlagsReturned += 1;
                return;
            }
            //If this actor is already carrying, don't do anything.
            if (OwnerStats.CarryingFlag)
                return;
            //If this actor can't pick up flags, don't do anything.
            if (!Manager.MatchRules.EnemiesArePeopleToo && !IsPlayer)
                return;
            //If this actor is hurt, don't do anything.
            if (CurrentState is HurtState)
                return;

            tookFlag = true;
        }

        else if (other.tag == "Waypoint")
        {
            //Don't bother doing anything on the player side.
        }

        else if (other.tag == "FlagBase")
        {
            //If the player isn't carrying a flag, stop.
            if (!OwnerStats.CarryingFlag) return;

            FlagBaseBehavior flBB = other.GetComponent<FlagBaseBehavior>();

            //If it's not this actor's base, then ignore it.
            if (!flBB.Team.Equals(ActorData.Team)) return;

            //If this actor's flag isn't at home and it has to be at home to score, ignore the base.
            FlagBehavior beh = Manager.FlagsByTeam[ActorData.Team].GetComponent<FlagBehavior>();
            if (Manager.MatchRules.CTF.FlagAtHomeToScore && !beh.AtHome)
                return;

            //Update the stats.
            OwnerStats.FlagsCaptured += 1;
            OwnerStats.CarryingFlag = false;
        }
    }

    /// <summary>
    /// Updates the stats for this actor and the given one dealing with pain dealt.
    /// </summary>
    private void UpdatePainStats(StateMachine other, float painToThis, float painToOther)
    {
        OwnerStats.PainDealt += painToOther;
        OwnerStats.PainReceived += painToThis;

        if (!other.ActorData.Team.Equals(ActorData.Team))
        {
            OwnerStats.PainDealtToEnemy += painToOther;
            OwnerStats.PainReceivedByEnemy += painToThis;
        }

        if (OwnerStats.IsVIP)
        {
            OwnerStats.PainDealtAsVIP += painToOther;
            OwnerStats.PainReceivedAsVIP += painToThis;
        }
        else if (other.OwnerStats.IsVIP)
        {
            OwnerStats.PainDealtToVIP += painToOther;
            OwnerStats.PainReceivedByVIP += painToThis;
        }
    }

    public void HurtActor(GameObject other, float damageToThis, float damageToOther, CollisionVelocityResults cvr)
    {
        CurrentState.Hurt(other, damageToOther, damageToThis, cvr);
    }
    public void HurtByActor(GameObject other, float damageToThis, float damageToOther, CollisionVelocityResults cvr)
    {
        CurrentState.HurtBy(other, damageToOther, damageToThis, cvr);
    }
	
	public override int GetHashCode ()
	{
		return ActorData.PlayerID;
	}
}