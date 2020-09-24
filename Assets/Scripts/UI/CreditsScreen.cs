using System;
using UnityEngine;

public class CreditsScreen : MenuScreen
{
    public CreditsScreen(ScreenUI owner, MatchStartData msd, FrontEndConstants consts) : base(msd, consts, owner) { }

    protected override MenuScreen ProtectedUpdate(ScreenLayout layout)
    {
        //Title and "back" button.

        DrawTitleString("Credits", layout.Title);

        if (DrawButtonMenu(layout.ButtonMenu, new GUIContent[] { new GUIContent("Back") }, new GetData<bool>[] { () => true }, false))
        {
            return new MainMenuScreen(MatchData, Consts, Owner);
        }

        Vector2 offset = new Vector2(layout.Body.xMin, layout.Body.yMin);

        for (int i = 0;
             i < Consts.Credits.Length &&
                i < Consts.Credits_Offsets.Length &&
                i < WorldConstants.GUIStyles.Credits.Length;
             ++i)
        {
            GUI.Label(new Rect((Consts.Credits_Offsets[i].x * layout.Body.width) + offset.x,
                               (Consts.Credits_Offsets[i].y * layout.Body.height) + offset.y,
                               layout.Body.width * 0.5f, 100),
                      Consts.Credits[i],
                      WorldConstants.GUIStyles.Credits[i]);
        }

        return this;
    }
}