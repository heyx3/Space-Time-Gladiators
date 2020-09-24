using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Represents the level generation screen -- the final screen before actually playing.
/// </summary>
public class GenerateLevelMenuScreen : MenuScreen
{
    public override bool DrawBackground { get { return false; } }

    LevelManager manager;

    public GenerateLevelMenuScreen(MatchStartData matchStartData, FrontEndConstants consts, ScreenUI owner)
        : base(matchStartData, consts, owner)
    {
		MatchData.GenerateLevelAndSpawns();
        regenerate = true;
    }

    //This MenuScreen deals with starting/stopping levels, which involves creating/destroying GameObjects and so could take an update cycle or two to complete each time. These flags indicate what needs to be done next update call.
    private bool waitACycle = false;
    private bool regenerate = false;
    private bool startGame = false;
    private bool wasUpdated = false;
    protected override MenuScreen ProtectedUpdate(MenuScreen.ScreenLayout layout)
    {
        //Process bool flags.

        if (waitACycle)
        {
            waitACycle = false;
            return this;
        }
        if (regenerate)
        {
            manager = WorldConstants.Creator.CreateMatchController().GetComponent<LevelManager>();
            manager.StartWorld(true);

            regenerate = false;
            waitACycle = true;
            return this;
        }

        if (!wasUpdated)
        {
            wasUpdated = true;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Owner.BackgroundTexture);
        }

        if (startGame)
        {
            return new MatchScreen(MatchData, Consts, Owner);
        }

        DrawTitleString("Choose a level", layout.Title);
        return DrawButtonMenu(layout.ButtonMenu,
                              new GUIContent[]
                              {
                                  new GUIContent("Back"),
                                  new GUIContent("Re-generate"),
                                  new GUIContent("Start match"),
                              },
                              new GetData<MenuScreen>[]
                              {
                                  () => 
                                      {
                                          Owner.DestroyMatch(false);
                                          return new LobbyMenuScreen(MatchData, Consts, Owner);
                                      },
                                  () =>
                                      {
                                          Owner.DestroyMatch(false);
                                          MatchData.GenerateLevelAndSpawns();
                                          regenerate = true;
                                          return this;
                                      },
                                  () =>
                                      {
                                          Owner.DestroyMatch(false);
                                          waitACycle = true;
                                          startGame = true;
                                          return this;
                                      },
                              },
                              this);
    }
}