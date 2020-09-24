using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Holds data that is useful to the Minimap Camera.
/// </summary>
[RequireComponent(typeof(Camera))]
public class MinimapCameraData : MonoBehaviour
{
    public int LocalPlayers = 0;
    public Vector2 LevelSize = new Vector2(0.0f, 0.0f);

    private MinimapCameraData data;
    private CameraConstants consts;
    private FrontEndConstants uiConsts;

    void Awake()
    {
        data = GetComponent<MinimapCameraData>();
        consts = WorldConstants.ConstantsOwner.GetComponent<CameraConstants>();
        uiConsts = WorldConstants.ConstantsOwner.GetComponent<FrontEndConstants>();
    }
    float leftSceneEdge, rightSceneEdge;
    float elapsedSinceUpdateRect = 100.0f;
    void Update()
    {
        elapsedSinceUpdateRect += Time.deltaTime;
        if (elapsedSinceUpdateRect < 2.5f)
        {
            return;
        }
        elapsedSinceUpdateRect = 0.0f;

        int locals = 0;
        foreach (StateMachine st in WorldConstants.ColTracker.Actors)
        {
            if (st.IsPlayer && st.ActorData.PlayerID <= 4)
            {
                locals += 1;
            }
        }
        Vector2 mapSize = data.LevelSize;

        mapSize.x -= consts.MinimapScreenXShrink;

        Camera c = camera;

        c.orthographicSize = mapSize.y * 0.5f;

        //Use the max Y size for the minimap. If the X ends up being too big, use that instead.

        Vector2 screenSize = new Vector2(0.0f, 0.0f);
        float mapXToY = (mapSize.x + 0) / (mapSize.y + 0);

        screenSize.y = consts.MinimapScreenMaxSize.y;
        screenSize.x = consts.MinimapScreenMaxSize.y * mapXToY;

        if (screenSize.x > consts.MinimapScreenMaxSize.x)
        {
            screenSize.x = consts.MinimapScreenMaxSize.x;
            screenSize.y = consts.MinimapScreenMaxSize.x / mapXToY;
        }

        SetRect(ref screenSize, locals, c, mapXToY, true);

        //Narrow the mini-map bit by bit until the map edges are barely covered.
        FindEdges();
        const float increment = 0.01f;
		float leftW = leftSceneEdge / Screen.width;
		float rightW = rightSceneEdge / Screen.height;
        while (c.rect.xMin < (leftSceneEdge / Screen.width) && c.rect.xMax > (rightSceneEdge / Screen.width))
        {
            screenSize.x -= increment;
            SetRect(ref screenSize, locals, c, mapXToY, false);
        }
        screenSize.x += increment;
        SetRect(ref screenSize, locals, c, mapXToY, false);
    }

    private bool alreadyFound = false;
    /// <summary>
    /// Find "leftViewEdge" and "rightViewEdge".
    /// </summary>
    private void FindEdges()
    {
        if (alreadyFound) return;

        alreadyFound = true;

        leftSceneEdge = System.Single.MaxValue;
        rightSceneEdge = System.Single.MinValue;
        float tempLeft, tempRight;
        Camera c = camera;
        foreach (RecBounds wall in WorldConstants.ColTracker.WallBounds)
        {
            tempLeft = c.WorldToScreenPoint(new Vector3(wall.left, wall.top, 0.0f)).x;
            tempRight = c.WorldToScreenPoint(new Vector3(wall.right, wall.top, 0.0f)).x;

            if (tempLeft < leftSceneEdge) leftSceneEdge = tempLeft;
            if (tempRight > rightSceneEdge) rightSceneEdge = tempRight;
        }
    }

	public float openMapProportion = 0.5f;
    private void SetRect(ref Vector2 screenSize, int localPlayers, Camera c, float mapXToY, bool changeScreenSizeIfGenerating)
    {
        switch (localPlayers)
        {
            case 0:

                //Use the special Y size for the minimap. If the X ends up being too big, use that instead.
                if (changeScreenSizeIfGenerating)
                {
                    screenSize.x = 0;
                    screenSize.y = 0;
                    screenSize.y = uiConsts.Generate_LevelPreviewMaxSize.y;
                    screenSize.x = uiConsts.Generate_LevelPreviewMaxSize.y * mapXToY;

                    if (screenSize.x > uiConsts.Generate_LevelPreviewMaxSize.x)
                    {
                        screenSize.x = uiConsts.Generate_LevelPreviewMaxSize.x;
                        screenSize.y = uiConsts.Generate_LevelPreviewMaxSize.x / mapXToY;
                    }
                }

                c.rect = new Rect((1.0f - screenSize.x) * 0.5f,
                                  1.0f - uiConsts.Generate_LevelPreviewYOffset - screenSize.y,
                                  screenSize.x, screenSize.y);

                break;

            case 1:
				if (MatchStartData.IsGeneratingOpen)
				{
					screenSize.x *= openMapProportion;	
				}
                c.rect = new Rect(1.0f - screenSize.x, 0.0f, screenSize.x, screenSize.y);
				if (MatchStartData.IsGeneratingOpen)
				{
	                screenSize.x /= openMapProportion;
				}
                break;

            case 2:
				if (MatchStartData.IsGeneratingOpen)
				{
					screenSize.x *= openMapProportion;	
				}
                c.rect = new Rect(0.5f - (screenSize.x * 0.5f), 1.0f - screenSize.y, screenSize.x, screenSize.y);
				if (MatchStartData.IsGeneratingOpen)
				{
	                screenSize.x /= openMapProportion;
				}
                break;

            case 3:
				if (MatchStartData.IsGeneratingOpen)
				{
					screenSize.x *= openMapProportion;	
				}
                c.rect = new Rect(0.5f - (screenSize.x * 0.5f), 0.5f - (screenSize.y * 0.5f), screenSize.x, screenSize.y);
				if (MatchStartData.IsGeneratingOpen)
				{
	                screenSize.x /= openMapProportion;
				}
                break;

            case 4:
				if (MatchStartData.IsGeneratingOpen)
				{
					screenSize.x *= openMapProportion;	
				}
                c.rect = new Rect(0.5f - (screenSize.x * 0.5f), 0.5f - (screenSize.y * 0.5f), screenSize.x, screenSize.y);
				if (MatchStartData.IsGeneratingOpen)
				{
	                screenSize.x /= openMapProportion;
				}
                break;

            default: throw new System.ArgumentOutOfRangeException("Must be between one and four players!");
        }
    }
}