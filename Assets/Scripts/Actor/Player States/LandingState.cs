using UnityEngine;

public class LandingState : ActorState
{
    private float originalXVelocity;

    private double elapsed;

    public override void EnteringState()
    {
        elapsed = 0.0;

        //Save the original x velocity and stop the player.
        originalXVelocity = Owner.Velocity.x;
        Owner.Velocity = Vector3.zero;

        Owner.Animator.CurrentAnimation = Animations.P_Land;
    }
    public override void ExitingState()
    {
        //Give the player back his x velocity.
        Owner.Velocity = new Vector3(originalXVelocity, 0, 0);
    }

    public override void Sideways(float intensity) { /* Don't move. */ }

    public override void AfterConstrainVelocity()
    {
        Owner.Velocity = Vector3.zero;
    }
    public override void AfterFixed()
    {
        //Add to the elapsed time.
        elapsed += Time.fixedDeltaTime;
        Owner.OwnerStats.TimeOnGround += Time.fixedDeltaTime;

        //See if it's time to stop recoiling.
        if (elapsed > Owner.PlayerProps.AirToGroundTime)
        {
            Owner.ChangeState(ScriptableObject.CreateInstance<WalkingState>(), false);
        }
    }

    public override string ToString() { return "Landing"; }
}
