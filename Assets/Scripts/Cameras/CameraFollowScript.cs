using System;
using UnityEngine;
using System.Collections.Generic;

public class CameraFollowScript : MonoBehaviour
{
    private static int CamerasInWorld = 0;
    void OnDestroy() { CamerasInWorld -= 1; }
    private int ThisCamID = CamerasInWorld;

    public StateMachine Target { get; private set; }
    public Transform TargetTransform { get; private set; }

    public void SetTarget(StateMachine target)
    {
        Target = target;
        TargetTransform = target.transform;
    }
    public void ReleaseTarget()
    {
        Target = null;
        TargetTransform = null;
    }

    [System.NonSerialized]
    public List<ParallaxBehavior> ParallaxFollowers = new List<ParallaxBehavior>();

    public Transform ThisTransform { get; private set; }

    public CameraShakeData ShakeData = CameraShakeData.DefaultData;
    public CameraPhysicsData PhysicsData = CameraPhysicsData.DefaultValues;
	public CameraBehaviorData BehaviorData = CameraBehaviorData.DefaultValues;

    public LevelManager LevelManager
    {
        get
        {
            if (levMan == null)
            {
                levMan = WorldConstants.MatchController.GetComponent<LevelManager>();
            }

            return levMan;
        }
    }
    private LevelManager levMan = null;

    public CameraConstants CameraConsts
    {
        get
        {
            if (camConsts == null)
            {
                camConsts = WorldConstants.ConstantsOwner.GetComponent<CameraConstants>();
            }

            return camConsts;
        }
    }
    private CameraConstants camConsts = null;
	
    /// <summary>
    /// The position of this camera before adding in the shaking effects.
    /// </summary>
    public Vector2 BeforeShakePos;
    /// <summary>
    /// The position of this camera after adding in the shaking effects.
    /// </summary>
    public Vector2 AfterShakePos { get { return ThisTransform.position; } private set { ThisTransform.position = new Vector3(value.x, value.y, ThisTransform.position.z); } }

    public Vector2 Velocity { get; private set; }
    public float MaxVelocity = Single.PositiveInfinity;

    public Vector2 MaxDistFromPlayer = new Vector2(4.0f, 4.0f);

    /// <summary>
    /// The scale of the distance between the target and the camera's target position.
    /// </summary>
    public float LookAheadDist = 10.0f;
	
	public Vector2 TargetPos;
	
    public float ShakeStrength { get; private set; }
    private Vector2 GetShakeDelta(float time)
    {
        if (ShakeStrength == 0)
        {
            return Vector2.zero;
        }

        return new Vector2((float)System.Math.Sin(UnityEngine.Random.Range(-ShakeData.TimeVariance,
                                                                           ShakeData.TimeVariance) +
                                                  (time * ShakeData.TimeScale)) *
                                  ShakeStrength,
                           (float)System.Math.Sin(UnityEngine.Random.Range(-ShakeData.TimeVariance,
                                                                           ShakeData.TimeVariance) +
                                                  ShakeData.TimeYOffset +
                                                  (time * ShakeData.TimeScale)) *
                                  ShakeStrength);
    }
    public void CreateShake(float strength)
    {
        ShakeStrength += strength;
    }

    public void HideWalls()
    {
        PrefabCreator.HideCullLayer(camera, "Walls");
        WorldConstants.Creator.AddTimer(new Timer(WorldConstants.CollObjConsts.PowerupHideWallsTime, false, (f => ShowWalls())), true);
    }
    public void ShowWalls()
    {
        PrefabCreator.ShowCullLayer(camera, "Walls");
    }

    void Awake()
    {
        CamerasInWorld += 1;

        ThisTransform = transform;
        ReleaseTarget();

        BeforeShakePos = (Vector2)ThisTransform.position;

        Velocity = Vector2.zero;

        ShakeStrength = 0.0f;
    }

    void LateUpdate()
    {
        if (ShakeStrength > 0)
        {
            ShakeStrength = System.Math.Max(0, ShakeStrength - (Time.deltaTime * ShakeData.FadeSpeed));
        }

        if (Target == null)
        {
            return;
        }

        UpdatePos();
    }

    void OnGUI()
    {
		if (LevelManager.TeamScores == null)
		{
			return;
		}
		
        Rect scoreR = new Rect(0, 0, Screen.width * CameraConsts.ScoreWidth, MyGUI.SliderHeight);

        int i = ThisCamID;

        switch (CamerasInWorld)
        {
            case 2:

                scoreR = new Rect((i == 0) ? Screen.width * CameraConsts.ScoreOffset.x :
											 (Screen.width * (1.0f - CameraConsts.ScoreOffset.x)) - scoreR.width,
                                  (Screen.height * (0.0f + CameraConsts.ScoreOffset.y)) + MyGUI.SliderHeight,
                                  scoreR.width, scoreR.height);

                break;

            case 3:

                if (i == 2)
                {
                    scoreR = new Rect(Screen.width * (0.25f + CameraConsts.ScoreOffset.x),
                                      (Screen.height * CameraConsts.ScoreOffset.y) + MyGUI.SliderHeight,
                                      scoreR.width, scoreR.height);

                }
                else
                {
                    scoreR = new Rect(Screen.width * (i == 0 ? CameraConsts.ScoreOffset.x :
                                                               1.0f - CameraConsts.ScoreOffset.x - (scoreR.width / Screen.width)),
                                      (Screen.height * (0.5f + CameraConsts.ScoreOffset.y)) + MyGUI.SliderHeight,
                                      scoreR.width, scoreR.height);

                }

                break;

            case 4:

                Vector2 offset = new Vector2((i % 2 == 0) ? CameraConsts.ScoreOffset.x :
															1.0f - CameraConsts.ScoreOffset.x - (scoreR.width / Screen.width),
										     (i < 2) ? 0.5f + CameraConsts.ScoreOffset.y :
													   0.0f + CameraConsts.ScoreOffset.y);
                scoreR = new Rect(Screen.width * offset.x,
                                  (Screen.height * offset.y) + MyGUI.SliderHeight,
                                  scoreR.width, scoreR.height);

                break;

            default: throw new NotImplementedException();
        }

        Color old = GUI.color;
        GUI.color = Target.ActorData.Team;
		
		if (LevelManager.TeamScores.Count > 0)
		{
	        GUI.HorizontalSlider(scoreR, LevelManager.TeamScores[Target.ActorData.Team],
	                             0.0f, LevelManager.MatchRules.ScoreGoal,
	                             WorldConstants.GUIStyles.ScoreSliderBar, WorldConstants.GUIStyles.ScoreSliderButton);
		}

        GUI.color = old;
    }

    /// <summary>
    /// Moves towards an interpolation of the player and his camera position.
    /// </summary>
    private void UpdatePos()
    {
		if (BehaviorData.SnapToPlayer)
		{
			BeforeShakePos = TargetTransform.position;
			TargetPos = BeforeShakePos;
			AfterShakePos = BeforeShakePos + GetShakeDelta (Time.deltaTime);
			return;
		}
		
        //Get target position as an interpolation between the player and his reticule.
		Vector2 start = TargetTransform.position;
		Vector2 lookAhead = LookAheadDist * (Vector2)Target.Velocity;
		TargetPos = start + lookAhead;
	
        //If the camera is close enough, just snap to the player.
        if (Vector2.Distance(BeforeShakePos, TargetPos) < PhysicsData.SnapToPlayerMaxDist)
        {
            BeforeShakePos = TargetPos;
            Velocity = Vector2.zero;
        }
        //Otherwise, accelerate towards him.
        else
        {
            //Accelerate towards the target.
            Vector2 accel = (TargetPos - BeforeShakePos).normalized;
			if (BehaviorData.UseStaticAccel)
			{
				accel *= BehaviorData.StaticAccel;
			}
			else
			{
            	accel *= PhysicsData.Acceleration(this);
			}
			
            if (BehaviorData.GetMaxVelocityEveryStep)
			{
				MaxVelocity = BehaviorData.SpeedMultiplier * PhysicsData.Speed(this);
			}
            Velocity += accel * Time.deltaTime;

            Velocity = (TargetPos - BeforeShakePos).normalized * Velocity.magnitude;
			if (Velocity.magnitude > MaxVelocity)
			{
				Velocity = Velocity.normalized * MaxVelocity;
			}
			
			Velocity = new Vector2(Velocity.x, Velocity.y * PhysicsData.YSpeedDamp);
            BeforeShakePos += Velocity * Time.deltaTime;

            //Clamp to be close to the player.
            if (BehaviorData.ClampToPlayer)
			{
				Vector2 newPos = BeforeShakePos;
            	newPos.x = Mathf.Clamp(BeforeShakePos.x, TargetTransform.position.x - MaxDistFromPlayer.x, TargetTransform.position.x + MaxDistFromPlayer.x);
            	newPos.y = Mathf.Clamp(BeforeShakePos.y, TargetTransform.position.y - MaxDistFromPlayer.y, TargetTransform.position.y + MaxDistFromPlayer.y);
            	BeforeShakePos = newPos;
			}
        }

        //Shaking.
        AfterShakePos = BeforeShakePos + GetShakeDelta(Time.deltaTime);
    }    

    /// <summary>
    /// Tells this camera to instantly move the given amount.
    /// </summary>
    public void Teleport(float deltaX, float deltaY)
    {
        BeforeShakePos += new Vector2(deltaX, deltaY);
        ThisTransform.position += new Vector3(deltaX, deltaY, 0.0f);

        for (int i = 0; i < ParallaxFollowers.Count; ++i)
        {
            ParallaxFollowers[i].Wrap(deltaX, deltaY);
        }
    }
}