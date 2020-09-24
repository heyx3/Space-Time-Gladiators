using UnityEngine;
using System.Collections;

public class JumpingState : ActorState
{
	private float originalXVelocity;
	private double elapsed;
	
	public override bool Hurtable { get { return false; } }
	
	public override void EnteringState ()
    {
        //TODO: Push-off particles.

		elapsed = 0.0;
		
		//Save the original x velocity for once the player gets in the air.
		originalXVelocity = Owner.Velocity.x;
		Owner.Velocity = Vector3.zero;
		Owner.Propulsion = Vector3.zero;
		
		Owner.Animator.CurrentAnimation = Animations.P_Jump;
	}
	public override void ExitingState ()
    {
        WorldConstants.PlayPhysNoises.PlayNoise(PlayerPhysicsNoises.Events.Jump);

		//Make the player jump and give him back his x velocity.
        Owner.Velocity = new Vector3(originalXVelocity, Owner.PlayerProps.JumpYSpeed, 0.0f);
        Owner.OwnerStats.Jumps += 1;
	}
	
	public override void Sideways (float intensity) { /* Don't move. */ }
	
	public override void AfterConstrainVelocity ()
    {
		Owner.Velocity = Vector3.zero;
	}
	
	public override void AfterFixed ()
    {
		//Add to the elapsed time.
		elapsed += Time.fixedDeltaTime;
        Owner.OwnerStats.TimeOnGround += Time.fixedDeltaTime;
		
		//See if it's time to be in air.
        if (elapsed > Owner.PlayerProps.JumpToAirTime)
        {
            Owner.ChangeState(ScriptableObject.CreateInstance<InAirState>(), true);
        }
	}
	
	public override string ToString ()
    {
		return "Jumping";
	}
}