using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Holds all constants relating to the layout of the front-end.
/// </summary>
public class FrontEndConstants : MonoBehaviour
{
    void Awake()
    {
        WorldConstants.FrontEndConsts = this;
    }


    //Outline layout.
    public float Layout_TitleHeight = 0.1f,
                 Layout_BodyHeight = 0.8f,
                 Layout_ButtonMenuHeight = 0.1f;
    public float Layout_TitleBodySpace = 0.05f,
                 Layout_BodyButtonMenuSpace = 0.00f;

    //Button Menu layout.
    public float ButtonMenu_Spacing = 0.3f,
                 ButtonMenu_XBorder = 0.1f,
                 ButtonMenu_YBorder = 0.15f;


    //"Main" menu screen.

    public float Main_ButtonWidth = 0.3f,
                 Main_ButtonHeight = 0.3f,
                 Main_ButtonSpacing = 0.1f,
                 Main_ButtonYBorder = 0.1f;

    //"Credits" menu screen.

    public string[] Credits = { "William Manning:\n\tLead Design, Software Design",
                                "William Herbert:\n\tWorld Artist, Design",
                                "Nolan Manning:\n\tCharacter/Object Artist, Design",
                                "Benjamin Leichter:\n\tSound Effects, Music, Sound Design",
                                "Thanks to Kevin MacLeod for \"Cut and Run\"\nhttp://incompetech.com/music/royalty-free/index.html?isrc=USUAN1100851"
                              };
    public Vector2[] Credits_Offsets = { new Vector2(0, 0),
                                         new Vector2(0.0f, 0.25f),
                                         new Vector2(0.7f, 0.25f),
                                         new Vector2(0.25f, 0.5f),
                                         new Vector2(0.0f, 0.8f)
                                      };

    //"Choose Game-type/Level Generation" menu screen.

    public float Settings_SubtitleYOffset = 0.025f,
                 Settings_SubtitleHeight = 0.025f;

    public float Settings_List_YOffset = 0.025f,
                 Settings_List_XOffset = 0.05f;
    public float Settings_List_Spacing = 0.025f;
    public Vector2 Settings_List_ButtonSize = new Vector2(0.2f, 0.085f);
    /// <summary>
    /// Gets the layout for the match/level selection. Already scaled to fit the screen.
    /// </summary>
    /// <param name="listTopLeft">The top-left position of the first item in the list.</param>
    /// <param name="bodyRect">The rectangle representing the Body area of the UI layout.</param>
    public MyGUI.RadioButtonsLayout Settings_List_Layout(Vector2 listTopLeft, Rect bodyRect)
    {
        return new MyGUI.RadioButtonsLayout(listTopLeft,
                                            new Vector2(Settings_List_ButtonSize.x * bodyRect.width, Settings_List_ButtonSize.y * bodyRect.height),
                                            new Vector2(1.0f, Settings_List_Spacing * bodyRect.height),
                                            1);
    }

    public float Settings_TooltipXOffset = 0.1f,
                 Settings_TooltipWidth = 0.25f;
    public float Settings_TooltipYOffset = 0.1f;

    //"Lobby" menu screen.

    public float Lobby_PlayersHeight = 0.8f;
    public float Lobby_PlayersXBorder = 0.05f,
                 Lobby_PlayersYBorder = 0.05f;
    public float Lobby_ButtonsXBorder = 0.3f,
                 Lobby_ButtonsYBorder = 0.05f;
    public float Lobby_PlayersSpacing = 0.1f,
                 Lobby_ButtonsSpacing = 0.2f;

    public float Lobby_PlayerBox_ImageXBorder = 0.05f,
                 Lobby_PlayerBox_ImageScale = 1.4f;
    public float Lobby_PlayerBox_ControlsSelectionXOffsetFromCenter = -0.1f,
                 Lobby_PlayerBox_ControlsSelectionSpacing = 0.005f;
    public float Lobby_PlayerBox_TeamChoiceXOffsetFromCenter = 0.1f,
                 Lobby_PlayerBox_TeamChoiceButtonWidth = 0.085f,
                 Lobby_PlayerBox_TeamChoiceButtonHeight = 0.085f,
                 Lobby_PlayerBox_TeamChoiceYOffset = 0.008f,
                 Lobby_PlayerBox_TeamChoiceSpacing = 0.1f;
    public Vector2 Lobby_PlayerBox_TeamChoiceRGBOffset = new Vector2(0.001f, 0.025f);
    public MyGUI.RGBLayout Lobby_PlayerBox_TeamChoiceRGBLayout = new MyGUI.RGBLayout(Vector2.zero, new Vector2(0.01f, -0.01f), 0.075f, 0.0025f, new Vector2(0.005f, 0.005f));
    public Vector2 Lobby_PlayerBox_TeamChoiceAllySelectionOffset = new Vector2(0.05f, 0.05f);
    public float Lobby_PlayerBox_TeamChoiceAllySelectionSpacing = 0.05f;
    public Vector2 Lobby_PlayerBox_TeamChoiceAllySelectionChoiceSize = new Vector2(64.16f, 20);
    public Vector2 Lobby_PlayerBox_PlayerLeaveLobbyOffset = new Vector2(-0.01f, -0.01f);

    public Vector2 Lobby_PlayerBox_ClickToJoinOffset = new Vector2(0.1f, -0.05f);
    public Vector2 Lobby_PlayerBox_ClickToJoinSize = new Vector2(0.3f, 0.1f);

    public Vector2 Lobby_MergeTeamWarning_WindowSize = new Vector2(0.75f, 0.75f);
    public Vector2 Lobby_MergeTeamWarning_ButtonsSize = new Vector2(0.3f, 0.3f);
    public float Lobby_MergeTeamWarning_ButtonSpacing = 0.15f,
                 Lobby_MergeTeamWarning_ButtonYOffset = 0.05f;
    public Vector2 Lobby_MergeTeamWarning_LabelOffset = new Vector2(0.1f, 0.2f);
    public string Lobby_MergeTeamWarning_Warning = "WARNING: At least two different teams have the same team color! Teams with the same color will be automatically merged. Is this OK?";

    //"Generate Match" menu screen.

    public Vector2 Generate_LevelPreviewMaxSize = new Vector2(0.8f, 0.8f);
    public float Generate_LevelPreviewYOffset = 0.05f;
    public float Generate_ButtonWidth = 0.175f;
    public float Generate_ButtonXSpacing = 0.15f;
    public float Generate_ButtonYBorder = 0.025f;

    //"Match Stats" menu screen.

    public float Stats_CategorySpacing = 0.1f,
                 Stats_LineSpacing = 0.04f;
    public Vector2 Stats_Offset = new Vector2(0.2f, 0.3f);

    public Vector2 Stats_ButtonSize = new Vector2(0.1f, 0.1f);
    public float Stats_ButtonYOffset = 0.05f;

    public float Stats_PlayerXBorder = 0.05f;

    public float Stats_ImageOffset = 0.05f;
}