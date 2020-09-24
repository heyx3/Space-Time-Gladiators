using UnityEngine;
using System.Collections;

/// <summary>
/// Represents data about what frames are contained in a specific wall sprite-sheet.
/// Also initializes the animation after it is set by the PrefabCreator.
/// </summary>
public class WallSheetData : MonoBehaviour
{
    public int Singles = 1;
    public int LeftEnds = 1, HorizontalCenters = 1, RightEnds = 1;
    public int TopEnds = 1, VerticalCenters = 1, BottomEnds = 1;
    public int TLCorners = 1, TRCorners = 1, BLCorners = 1, BRCorners = 1;
    public int TopSides = 1, BottomSides = 1, LeftSides = 1, RightSides = 1;
    public int Centers = 1;

    public int Rows = 6, Columns = 4;

    public void SetAnimation()
    {
        Animator anim = GetComponent<Animator>();

        Animations thisAnim = anim.CurrentAnimation;
        anim.CurrentAnimation = Animations.Ob_Flag;
        anim.CurrentAnimation = thisAnim;
    }
}
