using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class MatchStatsScreen : MenuScreen
{
    private Dictionary<StateMachine, List<List<string>>> statsToDisplay;
    private List<StateMachine> players;

    public MatchStatsScreen(MatchStartData matchData, FrontEndConstants consts, ScreenUI owner)
        : base(matchData, consts, owner)
    {
        LevelManager manager = WorldConstants.MatchController.GetComponent<LevelManager>();

        players = WorldConstants.ColTracker.Actors;
        statsToDisplay = Stats.GetStats(manager.MatchRules, players);
    }

    private static Dictionary<int, Dictionary<byte, Vector2>> guiOffsetsByPlayersAndID =
        new Dictionary<int, Dictionary<byte, Vector2>>()
        {
            { 2, new Dictionary<byte, Vector2>()
                 {
                    { 1, Vector2.zero },
                    { 2, new Vector2(0.5f, 0.0f) },
                 }},
            { 3, new Dictionary<byte, Vector2>()
                 {
                    { 1, new Vector2(0.0f, 0.5f) },
                    { 2, new Vector2(0.5f, 0.5f) },
                    { 3, new Vector2(0.25f, 0.0f) },
                 }},
            { 4, new Dictionary<byte, Vector2>()
                 {
                    { 1, new Vector2(0.0f, 0.5f) },
                    { 2, new Vector2(0.5f, 0.5f) },
                    { 3, Vector2.zero },
                    { 4, new Vector2(0.5f, 0.0f) },
                 }},
        };
    private static Dictionary<int, Vector2> guiViewSizesByPlayers =
        new Dictionary<int, Vector2>()
        {
            { 2, new Vector2(0.5f, 1.0f) },
            { 3, new Vector2(0.5f, 0.5f) },
            { 4, new Vector2(0.5f, 0.5f) },
        };

    protected override MenuScreen ProtectedUpdate(MenuScreen.ScreenLayout layout)
    {
        //Title and "back" button.
        DrawTitleString("Stats", layout.Title);
        MenuScreen scr = DrawButtonMenu(layout.ButtonMenu,
                                        new GUIContent[]
                                        {
                                            new GUIContent("Main Menu"),
                                        },
                                        new GetData<MenuScreen>[]
                                        {
                                            () =>
                                                {
                                                    Owner.DestroyMatch();
                                                    return new MainMenuScreen(MatchData, Consts, Owner);
                                                },
                                        },
                                        null);
        if (scr != null)
        {
            return scr;
        }

        //Get screen layout data.

        Rect area = layout.Body;

        Vector2 sViewportSize, sStatsOffset;
        float sCategorySpacing, sLineSpacing;
        Vector2 sButtonSize;
        float sButtonYOffset;

        sViewportSize = new Vector2((guiViewSizesByPlayers[players.Count].x - Consts.Stats_PlayerXBorder) * area.width,
                                    guiViewSizesByPlayers[players.Count].y * area.height);
        sStatsOffset = new Vector2(Consts.Stats_Offset.x * sViewportSize.x, Consts.Stats_Offset.y * sViewportSize.y);
        sCategorySpacing = sViewportSize.y * Consts.Stats_CategorySpacing;
        sLineSpacing = sViewportSize.y * Consts.Stats_LineSpacing;

        sButtonSize = new Vector2(Screen.width * Consts.Stats_ButtonSize.x, Screen.height * Consts.Stats_ButtonSize.y);
        sButtonYOffset = Screen.height * Consts.Stats_ButtonYOffset;

        //Output each players' stats.

        Vector2 lookupOffset, sPlayerOffset, tempTextPos;
        Color old;
        foreach (StateMachine st in players)
        {
            //Get the offset for the player's box.
            lookupOffset = guiOffsetsByPlayersAndID[players.Count][st.ActorData.PlayerID];
            sPlayerOffset = new Vector2(area.width * lookupOffset.x,
                                        area.height * lookupOffset.y);
            sPlayerOffset += new Vector2(area.xMin, area.yMin);
            if (lookupOffset.x == 0.0f)
            {
                sPlayerOffset.x += (Consts.Stats_PlayerXBorder * area.width);
            }

            tempTextPos = sPlayerOffset + sStatsOffset;

            GUI.Box(new Rect(sPlayerOffset.x, sPlayerOffset.y, sViewportSize.x, sViewportSize.y), "");
            old = GUI.color;
            GUI.color = st.ActorData.Team;
            GUI.DrawTexture(new Rect(sPlayerOffset.x + (sViewportSize.x * Consts.Stats_ImageOffset),
                                     sPlayerOffset.y + (sViewportSize.y * Consts.Stats_ImageOffset),
                                     Owner.PlayerTexture.width * Consts.Lobby_PlayerBox_ImageScale,
                                     Owner.PlayerTexture.height * Consts.Lobby_PlayerBox_ImageScale),
                            Owner.PlayerTexture);
            GUI.color = old;

            foreach (List<string> category in statsToDisplay[st])
            {
                foreach (string stat in category)
                {
                    float f;
                    string s;
                    if (Single.TryParse(stat, out f))
                    {
                        s = System.Math.Round(f, 2).ToString();
                    }
                    else
                    {
                        s = stat;
                    }

                    GUI.Label(new Rect(tempTextPos.x, tempTextPos.y, sViewportSize.x - sStatsOffset.x, MyGUI.LabelHeight),
                                       s, WorldConstants.GUIStyles.GameStatsText);

                    tempTextPos.y += sLineSpacing;
                }

                tempTextPos.y -= sLineSpacing;
                tempTextPos.y += sCategorySpacing;
            }
        }

        return this;
    }

    //public MenuScreen Update()
    //{

    //    //Output each players' stats.
    //    Vector2 sPlayerOffset, tempTextPos;
    //    Color old;
    //    foreach (StateMachine st in players)
    //    {
    //        sPlayerOffset = new Vector2(Screen.width * guiOffsetsByPlayersAndID[players.Count][st.ActorData.PlayerID].x,
    //                                    Screen.height * guiOffsetsByPlayersAndID[players.Count][st.ActorData.PlayerID].y);
    //        tempTextPos = sPlayerOffset + sStatsOffset;

    //        GUI.Box(new Rect(sPlayerOffset.x, sPlayerOffset.y, sViewportSize.x, sViewportSize.y), "");
    //        old = GUI.color;
    //        GUI.color = st.ActorData.Team;
    //        GUI.DrawTexture(new Rect(sPlayerOffset.x + (sViewportSize.x * Consts.Stats_ImageOffset),
    //                                 sPlayerOffset.y + (sViewportSize.y * Consts.Stats_ImageOffset),
    //                                 Owner.PlayerTexture.width * Consts.Lobby_PlayerBox_ImageScale,
    //                                 Owner.PlayerTexture.height * Consts.Lobby_PlayerBox_ImageScale),
    //                        Owner.PlayerTexture);
    //        GUI.color = old;

    //        foreach (List<string> category in statsToDisplay[st])
    //        {
    //            foreach (string stat in category)
    //            {
    //                GUI.Label(new Rect(tempTextPos.x, tempTextPos.y, sViewportSize.x - sStatsOffset.x, MyGUI.LabelHeight), stat);

    //                tempTextPos.y += sLineSpacing;
    //            }

    //            tempTextPos.y -= sLineSpacing;
    //            tempTextPos.y += sCategorySpacing;
    //        }
    //    }

    //    //Display the "back" button.
    //    if (GUI.Button(new Rect((Screen.width * 0.5f) - (sButtonSize.x * 0.5f),
    //                            Screen.height - sButtonSize.y - sButtonYOffset,
    //                            sButtonSize.x, sButtonSize.y),
    //                   "Main Menu"))
    //    {
    //        Owner.DestroyMatch();
    //        return new MainMenuScreen(MatchData, Consts, Owner);
    //    }

    //    return this;
    //}
}