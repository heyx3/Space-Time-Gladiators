using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class PowerupBehavior : MonoBehaviour
{
    public static CollectibleObjectiveConstants Consts = null;

    public static void RandomEffect(PowerupBehavior beh)
    {
        if (Consts == null)
        {
            Consts = WorldConstants.ConstantsOwner.GetComponent<CollectibleObjectiveConstants>();
        }

        const int numbEffects = 6;
        beh.EffectIndex = Random.Range(0, numbEffects);

        switch (beh.EffectIndex)
        {
            case 0:
                beh.Effect = new PowerupIncreasesMultiplier(Consts.PowerupGiveMultiplierIncrement);
                beh.positiveEffect = true;
                break;

            case 1:
                beh.Effect = new PowerupThrowsEnemies();
                beh.positiveEffect = false;
                break;

            case 2:
                beh.Effect = new PowerupJumblesControls();
                beh.positiveEffect = false;
                break;

            case 3:
                beh.Effect = new PowerupHidesWalls();
                beh.positiveEffect = false;
                break;

            case 4:
                beh.Effect = new PowerupTeleportsEnemies();
                beh.positiveEffect = false;
                break;

            case 5:
                if (!WorldConstants.MatchData.GeneratedLevel.GenSettings.WrapY)
                {
                    RandomEffect(beh);
                }
                else
                {
                    beh.Effect = new PowerupStopsCollision();
                    beh.positiveEffect = true;
                }
                break;

            default: throw new System.InvalidOperationException("Invalid powerup effect index!");
        }
    }

    public PowerupEffect Effect { get; private set; }
    public int EffectIndex { get; private set; }
    private bool positiveEffect;

    void Start()
    {
        GetComponent<Animator>().CurrentAnimation = Animations.Ob_Powerup;
        Consts = WorldConstants.ConstantsOwner.GetComponent<CollectibleObjectiveConstants>();
    }

    void CollideWithActor(StateMachine st)
    {
        if (Effect == null)
        {
            RandomEffect(this);
        }

        Effect.ApplyEffect(st);
        Consts.PowerupSoundInstance().StartClip((positiveEffect ? Consts.PowerupPositiveNoiseIndex : (1 - Consts.PowerupPositiveNoiseIndex)));

        Destroy(gameObject);
    }
}

/// <summary>
/// Represents a specific powerup effect.
/// </summary>
public abstract class PowerupEffect
{
    public PowerupEffect()
    {
    }

    public abstract void ApplyEffect(StateMachine actor);
}
/// <summary>
/// A powerup effect that increases the collecting player's score multiplier.
/// </summary>
public class PowerupIncreasesMultiplier : PowerupEffect
{
    public float Increment;

    public PowerupIncreasesMultiplier(float increment)
    {
        Increment = increment;
    }

    public override void ApplyEffect(StateMachine actor)
    {
        WorldConstants.CrowdCheering.CreateCrowdCheer(CrowdCheers.Events.SmallPowerup);

        WorldConstants.Creator.CreateFloatingTextsForPlayers(WorldConstants.Creator.FourMessages(WorldConstants.ActorConsts.PowerupMultiplierMessages, actor, null),
                                                             st => st.ActorData.Team == actor.ActorData.Team,
                                                             WorldConstants.Creator.CreatePowerupFloatingText);

        actor.OwnerStats.PowerupMultiplier += Increment;
    }
}
/// <summary>
/// A powerup effect that throws all enemies of the collecting player as if they were hurt.
/// </summary>
public class PowerupThrowsEnemies : PowerupEffect
{
    public override void ApplyEffect(StateMachine actor)
    {
        WorldConstants.CrowdCheering.CreateCrowdCheer(CrowdCheers.Events.LargePowerup);

        //Get some needed references.
        CollisionTracker colTrack = WorldConstants.ColTracker;
        PlayerProperties props = actor.PlayerProps;
        Rules gameRules = WorldConstants.MatchController.GetComponent<LevelManager>().MatchRules;

        //Get all enemies to this player.
        Color team = actor.ActorData.Team;

        //Artificially set the player's velocity really high to emulate a huge hit.
        Vector3 oldV = actor.Velocity;
        actor.Velocity = PowerupBehavior.Consts.PowerupHurtOthersTempVel;
        //Randomize direction.
        if (Random.value > 0.5f)
        {
            actor.Velocity.x *= -1.0f;
        }
        if (Random.value > 0.5f)
        {
            actor.Velocity.y *= -1.0f;
        }

        //Create floating texts.
        WorldConstants.Creator.CreateFloatingTextsForPlayers(WorldConstants.Creator.FourMessages(WorldConstants.ActorConsts.PowerupThrewEnemiesMessages, actor, null),
                                                             st => st.ActorData.Team == actor.ActorData.Team,
                                                             WorldConstants.Creator.CreatePowerupFloatingText);

        //Throw each enemy as if they were hurt.
        foreach (StateMachine oth in colTrack.Actors)
        {
            //Include non-player enemies if the game rules call for it.
            if (!team.Equals(oth.ActorData.Team) && (gameRules.EnemiesArePeopleToo || oth.IsPlayer))
            {
                //Hurt the actor.
                oth.HurtByActor(actor.gameObject, 0.0f, 0.0f, StateMachine.NewVs(actor, oth));

                //Artificially extend the time that the actor spends hurt.
                if (oth.tag == "Player")
                    ((HurtState)oth.CurrentState).elapsed -= PowerupBehavior.Consts.PowerupHurtOtherExtraHurtTime;
            }
        }

        //Set the player's velocity back to normal.
        actor.Velocity = oldV;
    }
}
/// <summary>
/// A powerup effect that jumbles all enemies' controls.
/// </summary>
public class PowerupJumblesControls : PowerupEffect
{
    public override void ApplyEffect(StateMachine actor)
    {
        WorldConstants.CrowdCheering.CreateCrowdCheer(CrowdCheers.Events.SmallPowerup);

        //Get some needed references.
        CollisionTracker colTrack = WorldConstants.ColTracker;
        Rules gameRules = WorldConstants.MatchController.GetComponent<LevelManager>().MatchRules;

        //Get all enemies to this player.
        Color team = actor.ActorData.Team;

        //Let the actor know what he did.
        WorldConstants.Creator.CreateFloatingTextsForPlayers(WorldConstants.Creator.FourMessages(WorldConstants.ActorConsts.PowerupConfusedEnemiesMessages, actor, null),
                                                             st => st.ActorData.Team == actor.ActorData.Team,
                                                             WorldConstants.Creator.CreatePowerupFloatingText);

        //Jumble each enemy's controls.
        foreach (StateMachine oth in colTrack.Actors)
        {
            //Include non-player enemies if the game rules call for it.
            if (!team.Equals(oth.ActorData.Team) && (gameRules.EnemiesArePeopleToo || oth.IsPlayer))
            {
                oth.Input.JumbleInput(oth.ActorData.PlayerID,
                                      PowerupBehavior.Consts.PowerupJumbleControlsTime);
            }
        }
    }
}
/// <summary>
/// A powerup effect that hides the walls from all enemies' screens.
/// </summary>
public class PowerupHidesWalls : PowerupEffect
{
    public override void ApplyEffect(StateMachine actor)
    {
        Color team = actor.ActorData.Team;

        //Create floating text to let everybody know what happened.
        WorldConstants.Creator.CreateFloatingTextsForPlayers(WorldConstants.Creator.FourMessages(WorldConstants.ActorConsts.PowerupHidWallsMessages, actor, null),
                                                             st => st.ActorData.Team == actor.ActorData.Team,
                                                             WorldConstants.Creator.CreatePowerupFloatingText);

        //Hide walls from all enemies.
        foreach (StateMachine st in WorldConstants.ColTracker.Actors)
        {
            if (st.IsPlayer && st.ActorData.Team != team)
            {
                st.CameraScript.HideWalls();
            }
        }
    }
}
/// <summary>
/// A powerup effect that "glitches" the game, stopping vertical collision detection for a few seconds,
/// then teleporting all the players.
/// </summary>
public class PowerupStopsCollision : PowerupEffect
{
    public override void ApplyEffect(StateMachine actor)
    {
        WorldConstants.CrowdCheering.CreateCrowdCheer(CrowdCheers.Events.LargePowerup);

        WorldConstants.Creator.CreateFloatingTextsForPlayers(st => "Glitch!", st => true, WorldConstants.Creator.CreatePowerupFloatingText);

        bool b = WorldConstants.MatchController.GetComponent<LevelManager>().MatchRules.EnemiesArePeopleToo;
        foreach (StateMachine st in WorldConstants.ColTracker.Actors)
        {
            if (b || st.IsPlayer)
            {
                st.ColManager.DisableVerticalCollisions();
            }
        }

        countdown = 3;
        WorldConstants.Creator.AddTimer(new Timer(WorldConstants.CollObjConsts.PowerupDisableCollisionTime - 3, false, CountdownEvent), true);
    }

    private void CountdownEvent(float elapsedSinceTimerWentOff)
    {
        if (countdown > 0)
        {
            WorldConstants.Creator.CreateFloatingTextsForPlayers(st => (countdown.ToString() + "..."), st => true, WorldConstants.Creator.CreatePowerupFloatingText);

            countdown -= 1;

            WorldConstants.Creator.AddTimer(new Timer(1.0f, false, CountdownEvent), true);
        }
        else
        {
            new PowerupTeleportsEnemies().ApplyEffect(null);
        }
    }

    private int countdown = 3;
}
public class PowerupTeleportsEnemies : PowerupEffect
{
    Generator level;

    public PowerupTeleportsEnemies() { level = WorldConstants.MatchData.GeneratedLevel; }

    public override void ApplyEffect(StateMachine actor)
    {
        bool b = WorldConstants.MatchController.GetComponent<LevelManager>().MatchRules.EnemiesArePeopleToo;
        
        if (actor == null)
        {
            foreach (StateMachine st in WorldConstants.ColTracker.Actors)
            {
                //Ignore teammates or (sometimes) enemies.
                if (b || st.IsPlayer)
                {
                    Teleport(st);
                }
            }
        }
		else
		{
			foreach (StateMachine st in WorldConstants.ColTracker.Actors)
			{
				if ((b || st.IsPlayer) && st.ActorData.Team != actor.ActorData.Team)
				{
					Teleport (st);
				}
			}
		}

        WorldConstants.Creator.CreateFloatingTextsForPlayers(st => "Teleported!", st => true, WorldConstants.Creator.CreatePowerupFloatingText);
    }

    private void Teleport(StateMachine st)
    {
        //Get a random starting spot.
        Location jumpPos = new Location(Random.Range(0, level.Map.GetLength(0)),
                                        Random.Range(0, level.Map.GetLength(1)));

        //Find the nearest free spot. Use depth-first level traversal.

        System.Collections.Generic.Stack<Location> searchSpace = new System.Collections.Generic.Stack<Location>();
        searchSpace.Push(jumpPos);
        System.Collections.Generic.Dictionary<Location, bool> searchedYet = new System.Collections.Generic.Dictionary<Location,bool>();
        searchedYet.Add(jumpPos, true);

        while (level.Map[jumpPos.X, jumpPos.Y])
        {
            //Use depth-first traversal.

            jumpPos = searchSpace.Pop();

            if (!level.Map[jumpPos.X, jumpPos.Y])
            {
                break;
            }
            else
            {
                //Add the four adjacent sides in random order.

                int start = Random.Range(0, 4);
                Location loc = new Location();

                for (int i = 0; i < 4; ++i)
                {
                    int index = (start + i) % 4;

                    switch (index)
                    {
                        case 0:
                            loc.X = jumpPos.X + 1;
                            loc.Y = jumpPos.Y;
                            break;

                        case 1:
                            loc.X = jumpPos.X - 1;
                            loc.Y = jumpPos.Y;
                            break;

                        case 2:
                            loc.X = jumpPos.X;
                            loc.Y = jumpPos.Y + 1;
                            break;

                        case 3:
                            loc.X = jumpPos.X;
                            loc.Y = jumpPos.Y - 1;
                            break;

                        default: throw new System.NotImplementedException();
                    }

                    if (loc.X >= 0 && loc.X < level.Map.GetLength(0) &&
                        loc.Y >= 0 && loc.Y < level.Map.GetLength(1) &&
                        !searchedYet.ContainsKey(loc))
                    {
                        searchSpace.Push(loc);
                        searchedYet[loc] = true;
                    }
                }
            }

            continue;
        }

        st.transform.position = new Vector3(jumpPos.X, jumpPos.Y, st.transform.position.z);
    }
}