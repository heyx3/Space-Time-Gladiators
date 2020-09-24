using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Holds all constants relating to the camera.
/// </summary>
[RequireComponent(typeof(FrontEndConstants))]
public class CameraConstants : MonoBehaviour
{
    public Vector2 MinimapScreenMaxSize = new Vector2(0.4f, 0.2f);
    public float MinimapScreenXShrink = 1.0f;

    public Vector2 ScoreOffset = new Vector2(0.075f, 0.075f);
    public float ScoreWidth = 0.2f;
    public float ScoreBackgroundBorder = 0.01f;
	
	public float PlayerLightestCollisionShake = 0.0f;
	public float PlayerHeaviestCollisionShake = 3.0f;
	public float PlayerWonCollisionShakeScale = 0.5f;
	public float PlayerMaxCollisionDamage = 40.0f;
	public float GetShakeAmount(float damage, bool won)
	{
		Interval shakes = new Interval(PlayerLightestCollisionShake, PlayerHeaviestCollisionShake, true, 3);
		Interval damages = new Interval(0.0f, PlayerMaxCollisionDamage, true, 2);
		
		float ret = damages.Map(shakes, damage);
		if (won)
		{
			ret *= PlayerWonCollisionShakeScale;
		}
		
		return ret;
	}
	
    private FrontEndConstants UIConsts
    {
        get
        {
            if (uiConsts == null)
            {
                uiConsts = GetComponent<FrontEndConstants>();
            }
            return uiConsts;
        }
    }
    private FrontEndConstants uiConsts = null;

    void Awake()
    {
        WorldConstants.CameraConsts = this;
    }

    void OnDestroy()
    {
        GameObject consts = GameObject.Find("Debugger");
        if (consts == null)
        {
            return;
        }

        ConstantsWriter cw = consts.GetComponent<ConstantsWriter>();
        if (cw == null)
        {
            return;
        }

        cw.WriteConstants();
    }
}