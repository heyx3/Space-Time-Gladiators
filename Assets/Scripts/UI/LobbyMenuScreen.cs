using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the lobby, where players can join the game server.
/// </summary>
public class LobbyMenuScreen : MenuScreen
{
    /// <summary>
    /// Represents the data for a player in the lobby.
    /// </summary>
    private class PlayerData
    {
        /// <summary>
        /// The player's chosen input keys/buttons.
        /// </summary>
        public byte ControlScheme { get; set; }

        /// <summary>
        /// The player that this player will ally with, or this player himself if he made his own team.
        /// </summary>
        public PlayerData Ally { get; private set; }
        /// <summary>
        /// The player who made the team this player is on.
        /// </summary>
        public PlayerData TeamOwner { get { if (MadeTeam) return this; else return Ally.TeamOwner; } }
        /// <summary>
        /// If true, this player chose to make his own team rather than join another's.
        /// </summary>
        public bool MadeTeam { get { return Ally == this; } }

        /// <summary>
        /// The player's team color. Either his own chosen team color, or the color of his chosen ally.
        /// </summary>
        public Color TeamCol { get { if (MadeTeam) return madeTeamCol; else return Ally.TeamCol; } }
        /// <summary>
        /// The most recent Color this player chose to be for his own team.
        /// </summary>
        private Color madeTeamCol;

        /// <summary>
        /// Creates a new team starting with this player, using the given team color.
        /// </summary>
        public void MakeOwnTeam()
        {
            Ally = this;
        }
        /// <summary>
        /// Changes this player's team color after he has chosen to make his own team.
        /// Throws an InvalidOperationException if this player doesn't own a team.
        /// </summary>
        public void ChangeOwnTeamColor(Color newTeamCol)
        {
            if (!MadeTeam)
            {
                throw new InvalidOperationException("Can't change the team color when this player didn't make the team!");
            }

            madeTeamCol = newTeamCol;
        }

        /// <summary>
        /// Sets this player to be allied with the given player.
        /// Throws an InvalidOperationException if the given PlayerData is this PlayerData.
        /// </summary>
        public void JoinOtherPlayer(PlayerData toBeCopied)
        {
            if (toBeCopied == this)
            {
                throw new InvalidOperationException("Players can't be allied to themselves!");
            }

            Ally = toBeCopied;
        }

        /// <summary>
        /// Makes a new PlayerData with his own random team and the given control scheme.
        /// </summary>
        public PlayerData(byte controlScheme)
        {
            ControlScheme = controlScheme;

            MakeOwnTeam();
            ChangeOwnTeamColor(new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value));
        }
    }

    private GUIContent[] controlContents = new GUIContent[7];
    private PlayerData[] playerDatas = new PlayerData[4];
    private int playersInLobby { get { return (playerDatas[0] == null ? 0 : 1) + (playerDatas[1] == null ? 0 : 1) + (playerDatas[2] == null ? 0 : 1) + (playerDatas[3] == null ? 0 : 1); } }
    private byte GetUnusedControlScheme()
    {
        bool isUnused;

        for (byte b = 0; b < 8; ++b)
        {
            isUnused = true;

            for (int i = 0; i < playerDatas.Length; ++i)
            {
                if (playerDatas[i] != null && playerDatas[i].ControlScheme == b)
                {
                    isUnused = false;
                    break;
                }
            }

            if (isUnused)
            {
                return b;
            }
        }

        throw new InvalidOperationException("No invalid controls!");
    }

    public LobbyMenuScreen(MatchStartData matchStartData, FrontEndConstants consts, ScreenUI owner)
        : base(matchStartData, consts, owner)
    {
        for (int i = 0; i < Owner.ControlsTextures.Length; ++i)
        {
            controlContents[i] = new GUIContent(Owner.ControlsTextures[i]);
        }

        playerDatas[0] = new PlayerData(0);
        playerDatas[1] = new PlayerData(1);
        playerDatas[2] = null;
        playerDatas[3] = null;
    }

    private bool confirmMergeTeams = false;
    private GenerateLevelMenuScreen nextScreen = null;
    private bool advance = false;
    protected override MenuScreen ProtectedUpdate(MenuScreen.ScreenLayout layout)
    {
        if (advance)
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Owner.BackgroundTexture);
            return new GenerateLevelMenuScreen(MatchData, Consts, Owner);
        }

        //If the players just confirmed merging teams, exit.
        if (nextScreen != null)
        {
            return nextScreen;
        }
        
        //If the players don't need to confirm merging teams, do the normal display.
        if (!confirmMergeTeams)
        {
            DrawTitleString("Lobby", layout.Title);

            //Buttons.
            MenuScreen vale;
            vale = DrawButtonMenu(layout.ButtonMenu,
                                  new GUIContent[]
                                  {
                                      new GUIContent("Back"),
                                      new GUIContent("Continue"),
                                  },
                                  new GetData<MenuScreen>[]
                                  {
                                      () =>
                                          {
                                              return new CreateMatchMenuScreen(MatchData, Consts, Owner);
                                          },
                                      () =>
                                          {
                                              //Try to build the match data.
                                              if (TryBuildMatchData(true))
                                              {
                                                  advance = true;
                                                  return this;
                                              }
                                              else
                                              {
                                                  confirmMergeTeams = true;
                                                  nextScreen = null;
                                                  return this;
                                              }
                                          },
                                  },
                                  null);
            if (vale != null)
            {
                return vale;
            }

            //Players.
            for (int i = 0; i < 4; ++i)
            {
                playerDatas[i] = PlayerGUI(i, playerDatas[i]);
            }
        }
        //Otherwise, show the confirmation window.
        else
        {
            Vector2 sWndSize = new Vector2(Screen.width * Consts.Lobby_MergeTeamWarning_WindowSize.x, Screen.height * Consts.Lobby_MergeTeamWarning_WindowSize.y);
            Vector2 sWndPos = (new Vector2(Screen.width, Screen.height) - sWndSize) * 0.5f;

            Rect windBounds = new Rect(sWndPos.x, sWndPos.y, sWndSize.x, sWndSize.y);
            GUI.Window(1, windBounds, MergeTeamsWarningWindow, "Team merge warning");
        }

        return this;
    }
    //public override MenuScreen Update()
    //{
    //    //If the players don't need to confirm merging teams, do the normal display.
    //    if (!confirmMergeTeams)
    //    {
    //        //Button layout.

    //        Rect button = new Rect(Screen.width * Consts.Lobby_ButtonsXBorder, Screen.height * (1.0f - Consts.Lobby_ButtonsYBorder),
    //                               Screen.width * 0.5f * (1.0f - Consts.Lobby_ButtonsXBorder - Consts.Lobby_ButtonsXBorder - Consts.Lobby_ButtonsSpacing),
    //                               Screen.height * (1.0f - Consts.Lobby_PlayersHeight - Consts.Lobby_ButtonsYBorder - Consts.Lobby_ButtonsYBorder));

    //        //"Back" and "Next" buttons.
    //        if (GUI.Button(button, "Back to Level/Match Selection"))
    //        {
    //            return new CreateMatchMenuScreen(MatchData, Consts, Owner);
    //        }
    //        button.x += button.width + (Screen.width * Consts.Lobby_ButtonsSpacing);
    //        if (GUI.Button(button, "Generate the Level"))
    //        {
    //            //Try to build the match data.
    //            if (TryBuildMatchData(true))
    //            {
    //                advance = true;
    //                return this;
    //            }
    //            else
    //            {
    //                confirmMergeTeams = true;
    //                nextScreen = null;
    //                return this;
    //            }
    //        }

    //        //Players.
    //        for (int i = 0; i < 4; ++i)
    //        {
    //            playerDatas[i] = PlayerGUI(i, playerDatas[i]);
    //        }
    //    }
    //    //Otherwise, show the confirmation window.
    //    else
    //    {
    //        Vector2 sWndSize = new Vector2(Screen.width * Consts.Lobby_MergeTeamWarning_WindowSize.x, Screen.height * Consts.Lobby_MergeTeamWarning_WindowSize.y);
    //        Vector2 sWndPos = (new Vector2(Screen.width, Screen.height) - sWndSize) * 0.5f;

    //        Rect windBounds = new Rect(sWndPos.x, sWndPos.y, sWndSize.x, sWndSize.y);
    //        GUI.Window(1, windBounds, MergeTeamsWarningWindow, "Team merge warning");
    //    }
    //    return this;
    //}

    /// <summary>
    /// Given a player and his spot on the lobby screen (0-3), displays the GUI layout for his information.
    /// </summary>
    /// <param name="data">The data for the player to be drawn, or "null" if the player hasn't joined.</param>
    /// <returns>The given "data" (possibly with some of its memebers changed), or "null" if the player is no longer in the lobby (or never was).</returns>
    private PlayerData PlayerGUI(int screenIndex, PlayerData data)
    {
        #region Laying out the spaces

        float height = Consts.Lobby_PlayersHeight;
        height -= Consts.Lobby_PlayersYBorder;
        height -= 3.0f * Consts.Lobby_PlayersSpacing;
        height *= 0.25f;

        Rect box = new Rect(Screen.width * Consts.Lobby_PlayersXBorder,
                            (Screen.height * (Consts.Lobby_PlayersYBorder + (screenIndex * (height + Consts.Lobby_PlayersSpacing)))),
                            Screen.width * (1.0f - Consts.Lobby_PlayersXBorder - Consts.Lobby_PlayersXBorder),
                            height * Screen.height);

        Rect characterImage = new Rect(box.xMin + (Screen.width * Consts.Lobby_PlayerBox_ImageXBorder),
                                       box.center.y - (0.5f * Owner.PlayerTexture.height * Consts.Lobby_PlayerBox_ImageScale),
                                       Owner.PlayerTexture.width * Consts.Lobby_PlayerBox_ImageScale,
                                       Owner.PlayerTexture.height * Consts.Lobby_PlayerBox_ImageScale);

        MyGUI.SelectorLayout controlsLayout = new MyGUI.SelectorLayout(new Vector2(Screen.width * Consts.Lobby_PlayerBox_ControlsSelectionXOffsetFromCenter,
                                                                                   box.center.y),
                                                                       Consts.Lobby_PlayerBox_ControlsSelectionSpacing * Screen.width,
                                                                       Owner.LeftArrowTexture.width, Owner.LeftArrowTexture.height,
                                                                       Owner.ControlsTextures[0].width, Owner.ControlsTextures[0].height);

        MyGUI.RadioButtonsLayout teamOptions = new MyGUI.RadioButtonsLayout(new Vector2(box.center.x + (Screen.width * Consts.Lobby_PlayerBox_TeamChoiceXOffsetFromCenter),
                                                                                        box.yMin + (Screen.height * Consts.Lobby_PlayerBox_TeamChoiceYOffset)),
                                                                            new Vector2(Screen.width * Consts.Lobby_PlayerBox_TeamChoiceButtonWidth,
                                                                                        Screen.height * Consts.Lobby_PlayerBox_TeamChoiceButtonHeight),
                                                                            new Vector2(0.0f, Screen.height * Consts.Lobby_PlayerBox_TeamChoiceSpacing), 1);

        MyGUI.RGBLayout makeTeamSliders = Consts.Lobby_PlayerBox_TeamChoiceRGBLayout;
        makeTeamSliders.ScreenPos = new Vector2((teamOptions.TopLeftChoicePos.x / Screen.width) + Consts.Lobby_PlayerBox_TeamChoiceRGBOffset.x,
                                                (teamOptions.TopLeftChoicePos.y / Screen.height) + Consts.Lobby_PlayerBox_TeamChoiceRGBOffset.y);

        Vector2 allySelectorPos = teamOptions.GetOptionPos(1);
        allySelectorPos.y += 0.5f * teamOptions.ButtonSize.y;
        allySelectorPos.x += (Screen.width * Consts.Lobby_PlayerBox_TeamChoiceAllySelectionOffset.x);
        allySelectorPos.y += (Screen.height * Consts.Lobby_PlayerBox_TeamChoiceAllySelectionOffset.y);
        MyGUI.SelectorLayout chooseAllySelector = new MyGUI.SelectorLayout(allySelectorPos,
                                                                           Screen.width * Consts.Lobby_PlayerBox_TeamChoiceAllySelectionSpacing,
                                                                           Owner.LeftArrowTexture.width, Owner.LeftArrowTexture.height,
                                                                           Consts.Lobby_PlayerBox_TeamChoiceAllySelectionChoiceSize.x,
                                                                           Consts.Lobby_PlayerBox_TeamChoiceAllySelectionChoiceSize.y);
        
        Rect playerLeaveButton = new Rect(box.xMax - (Screen.width * Consts.Lobby_PlayerBox_PlayerLeaveLobbyOffset.x) - Owner.XTexture.width,
                                          box.yMin + (Screen.height * Consts.Lobby_PlayerBox_PlayerLeaveLobbyOffset.y),
                                          Owner.XTexture.width, Owner.XTexture.height);

        Rect clickToJoinButton = new Rect(box.xMin + (Screen.width * Consts.Lobby_PlayerBox_ClickToJoinOffset.x),
                                          box.yMin + (Screen.height * Consts.Lobby_PlayerBox_ClickToJoinOffset.y),
                                          Screen.width * Consts.Lobby_PlayerBox_ClickToJoinSize.x,
                                          Screen.height * Consts.Lobby_PlayerBox_ClickToJoinSize.y);

        #endregion

        //Background box.
        GUI.backgroundColor = Color.white;
        GUI.color = Color.white;
        GUI.Box(box, "", WorldConstants.GUIStyles.LobbyPlayerBox);

        //Draw player-specific stuff.
        if (data != null)
        {
            //Image.
            GUI.color = data.TeamCol;
            GUI.DrawTexture(characterImage, Owner.PlayerTexture);

            //Control scheme.
            GUI.color = Color.white;
            data.ControlScheme = (byte)MyGUI.Selector(new GUIContent(Owner.LeftArrowTexture),
                                                      new GUIContent(Owner.RightArrowTexture),
                                                      controlContents, data.ControlScheme, controlsLayout,
                                                      true, true, false,
                                                      WorldConstants.GUIStyles.SelectionsArrows, WorldConstants.GUIStyles.SelectionsSelection);

            //Team/ally selection. Only allow ally selection if at least one other player owns a team.
            GUI.color = Color.white;
            int result = -1;
            if (data.MadeTeam &&
                playerDatas.Where((dat, index) => (dat != null && dat.MadeTeam)).Count() == 1)
            {
                result = 0;
            }
            else
            {
                result = MyGUI.RadioButtons(new GUIContent[2] { new GUIContent("Make new team"), new GUIContent("Ally with player") },
                                            (data.MadeTeam ? 0 : 1), teamOptions, false,
                                            WorldConstants.GUIStyles.SettingsRadioButtons);
            }

            //Options for team/ally selection.
            switch (result)
            {
                #region RGB team selection

                case 0:

                    //Make a new team if this player didn't already have his own team.
                    if (!data.MadeTeam)
                    {
                        data.MakeOwnTeam();
                    }

                    //Do the RGB slider to select a team color.
                    data.ChangeOwnTeamColor(MyGUI.RGBSlider(data.TeamCol, makeTeamSliders, true));

                    break;

                #endregion

                #region Ally selection

                case 1:

                    //Set the ally if this player didn't already have an ally.
                    PlayerData pd;
                    if (data.MadeTeam)
                    {
                        Func<PlayerData, bool> pred = (dat) =>
                        {
                            bool b1 = dat != null;
                            bool b2 = dat != data;
                            bool b3 = (b1 && dat.MadeTeam);
                            return b1 && b2 && b3;
                        };
                        pd = playerDatas.First(pred);
                        data.JoinOtherPlayer(pd);
                    }

                    //Get the list of potential allies, and find the current ally.
                    List<GUIContent> potentialAlliesContent = new List<GUIContent>();
                    List<int> potentialAlliesIndex = new List<int>();
                    int currentAlly = -1;
                    for (int i = 0; i < playerDatas.Length; ++i)
                    {
                        if (playerDatas[i] != null && playerDatas[i] != data)
                        {
                            //Prevent infinite loops of alliances by only allowing alliances with team owners.
                            if (playerDatas[i].MadeTeam)
                            {
                                potentialAlliesContent.Add(new GUIContent("Player " + (i + 1).ToString()));
                                potentialAlliesIndex.Add(i);
                            }
                            if (playerDatas[i] == data.Ally)
                            {
                                currentAlly = potentialAlliesIndex.Count - 1;
                            }
                        }
                    }

                    int selectedAlly = -1;
                    try
                    {
                        selectedAlly = MyGUI.Selector(new GUIContent(Owner.LeftArrowTexture), new GUIContent(Owner.RightArrowTexture),
                                                      potentialAlliesContent.ToArray(),
                                                      currentAlly,
                                                      chooseAllySelector, true, true, false,
                                                      WorldConstants.GUIStyles.SelectionsArrows, WorldConstants.GUIStyles.SelectionsSelection);
                        data.JoinOtherPlayer(playerDatas[potentialAlliesIndex[selectedAlly]]);
                    }
                    catch { Debug.Log("Invalid ally! 'currentAlly' = " + currentAlly + ", 'selectedAlly' = " + selectedAlly); }
                    break;

                #endregion

                default: throw new InvalidOperationException();
            }

            GUI.color = Color.white;

            //Leaving the lobby. Only allow it if:
            //	1) There are more than the minimum number of players needed for a game.
            //  2) Nobody is allied to this player.
            if (playersInLobby > 2 && playerDatas.All(dat => (dat == null || dat == data || dat.Ally != data)))
            {
                if (GUI.Button(playerLeaveButton, Owner.XTexture))
                {
                    return null;
                }
            }
        }
        //Otherwise, invite the player to join the lobby.
        else
        {
            if (GUI.Button(clickToJoinButton, "Click here to join the lobby", WorldConstants.GUIStyles.SettingsRadioButtons))
            {
                return new PlayerData(GetUnusedControlScheme());
            }
        }

        return data;
    }

    /// <summary>
    /// Displays the window GUI for confirming to merge teams.
    /// </summary>
    private void MergeTeamsWarningWindow(int id)
    {
        //Get window size data.
        Vector2 sWndSize = new Vector2(Screen.width * Consts.Lobby_MergeTeamWarning_WindowSize.x, Screen.height * Consts.Lobby_MergeTeamWarning_WindowSize.y);
        Vector2 sWndPos = (new Vector2(Screen.width, Screen.height) - sWndSize) * 0.5f;


        //Label.

        Vector2 sLblOffset = new Vector2(Screen.width * Consts.Lobby_MergeTeamWarning_LabelOffset.x, Screen.height * Consts.Lobby_MergeTeamWarning_LabelOffset.y);
        Rect labelBounds = new Rect(sWndPos.x + sLblOffset.x, sWndPos.y + sLblOffset.y,
                                    sWndSize.x - (2.0f * sLblOffset.x), sWndSize.y - (2.0f * sLblOffset.y));

        GUI.Label(labelBounds, Consts.Lobby_MergeTeamWarning_Warning);


        //"OK" and "Cancel" buttons.

        Vector2 sBttnSize = new Vector2(Screen.width * Consts.Lobby_MergeTeamWarning_ButtonsSize.x, Screen.height * Consts.Lobby_MergeTeamWarning_ButtonsSize.y);
        float sBttnSpacing = Screen.width * Consts.Lobby_MergeTeamWarning_ButtonSpacing,
              sBttnYOffset = Screen.height * Consts.Lobby_MergeTeamWarning_ButtonYOffset;
        Rect button = new Rect(sWndPos.x + (0.5f * sWndSize.x) - (0.5f * sBttnSpacing) - sBttnSize.x,
                               sWndPos.y + sWndSize.y - sBttnYOffset - sBttnSize.y,
                               sBttnSize.x, sBttnSize.y);

        if (GUI.Button(button, "Cancel"))
        {
            confirmMergeTeams = false;
        }

        button.center += new Vector2(sBttnSpacing + sBttnSize.x, 0.0f);
        if (GUI.Button(button, "Continue"))
        {
            TryBuildMatchData(false);
            nextScreen = new GenerateLevelMenuScreen(MatchData, Consts, Owner);
        }
    }

    /// <summary>
    /// Tries to put the current player lobby data into the MatchData object.
    /// </summary>
    /// <param name="needConfirmation">Whether or not the players must be warned when teams will be merged.
    /// If "true", and they DO have to be merged, MatchData will not change.</param>
    /// <returns>Whether or not the match data was set successfully.
    /// If teams have to be merged and "needConfirmation" is "true", then it will NOT be successful.</returns>
    private bool TryBuildMatchData(bool needConfirmation)
    {
        //Try to build the match data. If it is successful, change MatchData to match it.

        Dictionary<Color, List<byte>> teams = new Dictionary<Color, List<byte>>();
        Dictionary<Color, bool> addedTeamLeaderYet = new Dictionary<Color, bool>();
        Dictionary<byte, byte> playerToControls = new Dictionary<byte, byte>();

        PlayerData player;
        byte id = 1;
        for (int i = 0; i < playerDatas.Length; ++i)
        {
            if (playerDatas[i] != null)
            {
                player = playerDatas[i];

                //Controls.
                playerToControls.Add(id, (byte)(player.ControlScheme + 1));

                //Team.

                if (player.TeamCol == ActorConstants.EnemiesTeam)
                {
                    player.TeamOwner.ChangeOwnTeamColor(new Color(player.TeamCol.r + 0.001f, player.TeamCol.g, player.TeamCol.b));
                }

                if (teams.ContainsKey(player.TeamCol))
                {
                    if (player.MadeTeam && addedTeamLeaderYet[player.TeamCol] && needConfirmation)
                    {
                        return false;
                    }
                    else
                    {
                        teams[player.TeamCol].Add(id);
                        if (player.MadeTeam)
                        {
                            addedTeamLeaderYet[player.TeamCol] = true;
                        }
                    }
                }
                else
                {
                    teams.Add(player.TeamCol, new List<byte>() { id });
                    addedTeamLeaderYet.Add(player.TeamCol, player.MadeTeam);
                }

                id += 1;
            }
        }

        //Building it was successful, so change MatchData.
        MatchData.PlayersOnTeams = teams;
        MatchData.PlayerControlSchemes = playerToControls;

        return true;
    }
}