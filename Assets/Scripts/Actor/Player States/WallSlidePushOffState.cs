using UnityEngine;
using System.Collections;

public class WallSlidePushOffState : ActorState {
	
	public sbyte JumpDir;
	
	private double elapsed;
	
	public override bool Hurtable { get { return false; } }
	
	public override Vector3 Gravity { get { return Vector3.zero; } }
    public override Vector3 ApplyFriction(Vector3 original) { return original; }
	
	public override void EnteringState ()
	{
		elapsed = 0.0;
		
		Owner.Velocity = Vector3.zero;
		
		Owner.Animator.CurrentAnimation = Animations.P_WallJump;
	}
    public override void ExitingState()
    {
        //TODO: Push-off particle effect.

        WorldConstants.PlayPhysNoises.PlayNoise(PlayerPhysicsNoises.Events.SlidePushOff);
    }
	
	public override void Sideways (float intensity) { /* Don't move. */ }
	
	public override void BeforeFixed () {
		
		//Add to the elapsed time.
		elapsed += Time.fixedDeltaTime;
		
		//See if it's time to be in air.
        if (elapsed > Owner.PlayerProps.WallSlidePushOffTime)
        {
			//Wall-jump.
            Owner.transform.position += new Vector3(JumpDir * Owner.PlayerProps.WallPushOffOffset, 0.0f, 0.0f);
            Owner.Velocity = new Vector3(Owner.PlayerProps.WallSlidePushOff.x * JumpDir,
                                         Owner.PlayerProps.WallSlidePushOff.y,
										 0);
			
			//Tell the new in-air state to dim the acceleration for a bit
			//  so the player can't just hop up the side of the wall.
			InAirState i = ScriptableObject.CreateInstance<InAirState>();
			Owner.ChangeState(i, true);
			i.DimAcceleration();
		}
	}
	public override void AfterConstrainVelocity () {
		Owner.Velocity = Vector3.zero;
	}
    public override void AfterFixed()
    {
        Owner.OwnerStats.TimeOnWall += Time.fixedDeltaTime;
    }
	
	public override string ToString ()
	{
		return "Pushing off the wall";
	}
}