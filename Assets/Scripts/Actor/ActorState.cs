using UnityEngine;
using System.Collections;

/// <summary>
/// Represents an Actor's current state. Handles events.
/// </summary>
[System.SerializableAttribute]
public abstract class ActorState : ScriptableObject
{
	public ActorState() { }

	public StateMachine Owner;

	public virtual bool Hurtable { get { return true; } }

	public virtual Vector3 Gravity { get { return Vector3.zero; } }

	public virtual void EnteringState() { }
	public virtual void ExitingState() { }

    public virtual Vector3 ApplyFriction(Vector3 original) { return original; }

	public virtual void Sideways(float intensity)
	{
		//If the player is trying to go in a certain direction:
		if (intensity != 0.0f)
		{
			//If it's the opposite direction from his velocity, boost his acceleration a bit.
			if (Mathf.Sign(intensity) == Mathf.Sign (Owner.Velocity.x))
				Owner.Propulsion += new Vector3(Owner.ActorProps.Acceleration * intensity, 0, 0);
            else Owner.Propulsion += new Vector3(Owner.ActorProps.Acceleration * intensity * Owner.PlayerProps.ChangeDirAccelScale, 0, 0);
		}
	}
	public virtual void Up() { }
	public virtual void Down() { }

	public virtual void HurtBy(GameObject attacker, float painDealt, float painTaken, StateMachine.CollisionVelocityResults cvr)
	{
        Owner.Velocity = cvr.Loser;
		Owner.CameraScript.CreateShake(Owner.CameraConsts.GetShakeAmount (painTaken, false));

        //TODO: Create PlayerState; move this to that state. Enemies shouldn't become hurt players.
		Owner.ChangeState (ScriptableObject.CreateInstance<HurtState>(), false);
	}
	public virtual void Hurt(GameObject victim, float painDealt, float painTaken, StateMachine.CollisionVelocityResults cvr)
	{
        Owner.Velocity = cvr.Winner;
		Owner.CameraScript.CreateShake (Owner.CameraConsts.GetShakeAmount (painTaken, true));

        //TODO: Create PlayerState; move all the code underneath to that state. Enemies shouldn't become hurt players or play "hurt player" noises.

        //Physical noise.
		WorldConstants.PlayPhysNoises.PlayNoise(PlayerPhysicsNoises.Events.Hurt);
        
        //Crowd cheering.
        if (Owner.ActorData.Team == victim.GetComponent<IDData>().Team)
        {
            WorldConstants.CrowdCheering.CreateCrowdCheer(CrowdCheers.Events.PlayerTeamHurt);
        }
        else if (painDealt > WorldConstants.HeavyHitPainCutoff)
        {
            WorldConstants.CrowdCheering.CreateCrowdCheer(CrowdCheers.Events.PlayerBadlyHurt);
        }
        else
        {
            WorldConstants.CrowdCheering.CreateCrowdCheer(CrowdCheers.Events.PlayerLightlyHurt);
        }

        //Change state.
        if (!(this is InAirState))
        {
            Owner.ChangeState(ScriptableObject.CreateInstance<InAirState>(), true);
        }
	}
    
	public virtual void HitSide(Line line, ColType side)
    {
        if (line.Dir == Line.Orientation.Horizontal)
            throw new System.ArgumentException();
    }
	public virtual void HitFloor(Line line) 
    {
        if (line.Dir == Line.Orientation.Vertical)
            throw new System.ArgumentException();
    }
	public virtual void HitCeiling(Line line)
    {
        if (line.Dir == Line.Orientation.Vertical)
            throw new System.ArgumentException();
    }

	public virtual void Update() { }

    public virtual void SetReflection()
    {
        if (Owner.MovementKeyAxes.x > 0.0f)
            Owner.MeshController.SetDir(SetMeshFor2D.ReflectDir.Right);
        else if (Owner.MovementKeyAxes.x < 0.0f)
            Owner.MeshController.SetDir(SetMeshFor2D.ReflectDir.Left);
    }
	
	public virtual void AnimationDone() { }
	
	public virtual void BeforeFixed() { }
	public virtual void AfterAccel() { }
	public virtual void BeforeConstrainVelocity() { }
	public virtual void AfterConstrainVelocity() { }
	public virtual void BeforeMove() { }
	public virtual void AfterMove() { }
	public virtual void AfterFixed() { }
}