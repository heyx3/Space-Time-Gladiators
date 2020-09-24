using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AnimatedTextureExtendedUV))]
public class Animator : MonoBehaviour
{
    //Sprite sheet frame dimensions.
    const int P_Width = 14, P_Height = 14;
    const int Ob_Width = 13, Ob_Height = 5;
    const int Stars_Width = 3, Stars_Height = 2;
    const int Specials_Width = 3, Specials_Height = 2;
    const int MM_Width = 1, MM_Height = 8;

    //Cached reference.
    AnimatedTextureExtendedUV spriteSheet;
	
	void Awake()
	{
		spriteSheet = GetComponent<AnimatedTextureExtendedUV>();	
	}
	
    public Animations CurrentAnimation
    {
        get
        {
            return currentAnim;
        }

        set
        {
            currentAnim = value;
            SetAnimation(value);
        }
    }
    public Animations currentAnim;

    bool initialized = false;
    void Update()
    {
        if (!initialized)
        {
            initialized = true;
            Init();
        }
    }
    void Init()
    {
        if (tag == "Player")
            CurrentAnimation = Animations.P_AirDown;

        else if (tag == "Powerup")
            CurrentAnimation = Animations.Ob_Powerup;
        else if (tag == "Flag")
            CurrentAnimation = Animations.Ob_Flag;
        else if (tag == "FlagBase")
            CurrentAnimation = Animations.Ob_FlagBase;
        else if (tag == "Waypoint")
            CurrentAnimation = Animations.Ob_Waypoint;

        else if (tag == "Stars")
            CurrentAnimation = Animations.Back_Stars;
        else if (tag == "Special")
            CurrentAnimation = Animations.Back_Special;
    }

    //Used to make setting wall frame animations quicker/simpler.
    private delegate void SWF(Location loc, int numb);

    public void SetWallFrame(Animations a, WallSheetData dat)
    {
		//Get the location of every kind of wall frame in the spritesheet.
        Location singleStart = new Location(0, 0),

                 leftEndStart = new Location(0, 1),
                 horzCenterStart = new Location(leftEndStart.X + dat.LeftEnds, leftEndStart.Y),
                 rightEndStart = new Location(horzCenterStart.X + dat.HorizontalCenters, leftEndStart.Y),

                 topEndStart = new Location(0, 2),
                 vertCenterStart = new Location(topEndStart.X + dat.TopEnds, topEndStart.Y),
                 bottomEndStart = new Location(vertCenterStart.X + dat.VerticalCenters, topEndStart.Y),
                 
                 tlCornerStart = new Location(0, 3),
                 trCornerStart = new Location(tlCornerStart.X + dat.TLCorners, tlCornerStart.Y),
                 blCornerStart = new Location(trCornerStart.X + dat.TRCorners, tlCornerStart.Y),
                 brCornerStart = new Location(blCornerStart.X + dat.BLCorners, tlCornerStart.Y),
                 
                 topSideStart = new Location(0, 4),
                 bottomSideStart = new Location(topSideStart.X + dat.TopSides, topSideStart.Y),
                 leftSideStart = new Location(bottomSideStart.X + dat.BottomSides, topSideStart.Y),
                 rightSideStart = new Location(leftSideStart.X + dat.LeftSides, topSideStart.Y),
                 
                 centerStart = new Location(0, 5);

        SWF call = (loc, numb) =>
            {
                short index = (short)(loc.X + Random.Range(0, numb - 1));
                Call(new Args((short)dat.Columns, (short)dat.Rows, index, (short)loc.Y, 1, 60));
            };
		
		currentAnim = a;
		
        switch (a)
        {
            case Animations.W_Single:
                call(singleStart, dat.Singles);
                break;

            case Animations.W_LeftEnd:
                call(leftEndStart, dat.LeftEnds);
                break;

            case Animations.W_HorzCenter:
                call(horzCenterStart, dat.HorizontalCenters);
                break;

            case Animations.W_RightEnd:
                call(rightEndStart, dat.RightEnds);
                break;

            case Animations.W_TopEnd:
                call(topEndStart, dat.TopEnds);
                break;

            case Animations.W_VertCenter:
                call(vertCenterStart, dat.VerticalCenters);
                break;

            case Animations.W_BottomEnd:
                call(bottomEndStart, dat.BottomEnds);
                break;

            case Animations.W_TLCorner:
                call(tlCornerStart, dat.TLCorners);
                break;

            case Animations.W_TRCorner:
                call(trCornerStart, dat.TRCorners);
                break;

            case Animations.W_BLCorner:
                call(blCornerStart, dat.BLCorners);
                break;

            case Animations.W_BRCorner:
                call(brCornerStart, dat.BRCorners);
                break;

            case Animations.W_TopSide:
                call(topSideStart, dat.TopSides);
                break;

            case Animations.W_BottomSide:
                call(bottomSideStart, dat.BottomSides);
                break;

            case Animations.W_LeftSide:
                call(leftSideStart, dat.LeftSides);
                break;

            case Animations.W_RightSide:
                call(rightSideStart, dat.RightSides);
                break;

            case Animations.W_Center:
                call(centerStart, dat.Centers);
                break;

            default: throw new System.NotImplementedException("Not a valid wall type!");
        }
    }

    private void SetAnimation(Animations a)
    {
        switch (a)
        {
            #region Player

            case Animations.P_Stand:
                Call(new Args(P_Width, P_Height, 0, 0, 1, 18));
                break;
            case Animations.P_Run:
                Call(new Args(P_Width, P_Height, 0, 1, 8, 18));
                break;
            case Animations.P_Hurt:
                Call(new Args(P_Width, P_Height, 0, 2, 4, (short)Random.Range(15, 30)));
                break;
            case Animations.P_Jump:
                Call(new Args(P_Width, P_Height, 0, 3, 5, 18));
                break;
            case Animations.P_WallJump:
                Call(new Args(P_Width, P_Height, 0, 4, 5, 18));
                break;
            case Animations.P_AirUp:
                Call(new Args(P_Width, P_Height, 0, 5, 1, 18));
                break;
            case Animations.P_AirDown:
                Call(new Args(P_Width, P_Height, 0, 6, 1, 18));
                break;
            case Animations.P_AirUpToDown:
                Call(new Args(P_Width, P_Height, 0, 7, 1, 18));
                break;
            case Animations.P_GroundPound:
                Call(new Args(P_Width, P_Height, 0, 8, 1, 18));
                break;
            case Animations.P_Land:
                Call(new Args(P_Width, P_Height, 0, 9, 2, 18));
                break;
            case Animations.P_WallSlide:
                Call(new Args(P_Width, P_Height, 0, 10, 1, 18));
                break;
            case Animations.P_CeilingSlide:
                Call(new Args(P_Width, P_Height, 0, 11, 8, 18));
                break;
            case Animations.P_Shruiken:
                Call(new Args(P_Width, P_Height, 0, 12, 3, 18));
                break;
            case Animations.P_Laser:
                Call(new Args(P_Width, P_Height, 0, 13, 3, 18));
                break;

            #endregion

            #region Objectives

            case Animations.Ob_Flag:
                Call(new Args(Ob_Width, Ob_Height, 0, 0, 1, 18));
                break;

            case Animations.Ob_FlagBase:
                Call(new Args(Ob_Width, Ob_Height, 0, 1, 12, 10));
                break;

            case Animations.Ob_Powerup:
                Call(new Args(Ob_Width, Ob_Height, 0, 2, 13, 18));
                break;

            case Animations.Ob_Waypoint:
                Call(new Args(Ob_Width, Ob_Height, 0, 3, 8, 18));
                break;

            case Animations.Ob_Aura:
                Call(new Args(Ob_Width, Ob_Height, 0, 4, 1, 18));
                break;

            #endregion

            #region Backgrounds

            case Animations.Back_Stars:
                Call(new Args(Stars_Width, Stars_Height,
                               (short)Mathf.RoundToInt(Random.value * (Stars_Width - 1)),
                               (short)Mathf.RoundToInt(Random.value * (Stars_Height - 1)),
                               1, 60));
                break;
            case Animations.Back_Special:
                Call(new Args(Specials_Width, Specials_Height,
                               (short)Mathf.RoundToInt(Random.value * (Specials_Width - 1)),
                               (short)Mathf.RoundToInt(Random.value * (Specials_Height - 1)),
                               1, 60));
                break;
			
            #endregion

            #region Minimap Icons

            case Animations.MM_Player:
                Call(new Args(MM_Width, MM_Height, 0, 0, 1, 60));
                break;

            case Animations.MM_Powerup:
                Call(new Args(MM_Width, MM_Height, 0, 1, 1, 60));
                break;

            case Animations.MM_Flag:
                Call(new Args(MM_Width, MM_Height, 0, 2, 1, 60));
                break;

            case Animations.MM_FlagBase:
                Call(new Args(MM_Width, MM_Height, 0, 3, 1, 60));
                break;

            case Animations.MM_Aura:
                Call(new Args(MM_Width, MM_Height, 0, 4, 1, 60));
                break;

            case Animations.MM_Waypoint:
                Call(new Args(MM_Width, MM_Height, 0, 5, 1, 60));
                break;

            case Animations.MM_Wall:
                Call(new Args(MM_Width, MM_Height, 0, 6, 1, 60));
                break;

            case Animations.MM_Team:
                Call(new Args(MM_Width, MM_Height, 0, 7, 1, 60));
                break;

            #endregion

            //A wall.
			default:
				SetWallFrame(a, GetComponent<WallSheetData>());
				break;
        }
    }
    private void Call(Args a)
    {
        if (spriteSheet == null) return;
        spriteSheet.SetSpriteAnimation(a.cols, a.rows, a.colN, a.rowN, a.frames, a.fps);
    }

    private struct Args
    {
        public short cols, rows, colN, rowN, frames, fps;

        public Args(short Cols, short Rows, short ColN, short RowN, short Frames, short FPS)
        {

            cols = Cols;
            rows = Rows;
            colN = ColN;
            rowN = RowN;
            frames = Frames;
            fps = FPS;
        }
    }
}

public enum Animations
{
    //Player.
    P_Stand,
    P_Run,
    P_Hurt,
    P_Jump,
    P_WallJump,
    P_AirUp,
    P_AirDown,
    P_AirUpToDown,
    P_GroundPound,
    P_Land,
    P_WallSlide,
    P_CeilingSlide,
    P_Shruiken,
    P_Laser,

    //Collectibles/objectives.
    Ob_Powerup,
    Ob_Flag,
    Ob_FlagBase,
    Ob_Waypoint,
    Ob_Aura,

    //Background.
    Back_Stars,
    Back_Special,

    //Walls.

    W_Single,

    W_LeftEnd,
    W_HorzCenter,
    W_RightEnd,

    W_TopEnd,
    W_VertCenter,
    W_BottomEnd,

    W_TLCorner,
    W_TRCorner,
    W_BLCorner,
    W_BRCorner,
    W_LeftSide,
    W_RightSide,
    W_TopSide,
    W_BottomSide,
    W_Center,

    //Minimap.
    MM_Player,
    MM_Powerup,
    MM_Flag,
    MM_FlagBase,
    MM_Waypoint,
    MM_Aura,
    MM_Team,
    MM_Wall,
}