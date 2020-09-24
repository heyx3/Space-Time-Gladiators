using UnityEngine;
using System.Collections;

/// <summary>
/// Holds all GUI styles for the game's UI elements.
/// </summary>
public class GUIStyles : MonoBehaviour
{
	void Awake()
	{
		WorldConstants.GUIStyles = this;
	}

    public GUIStyle ScreenTitles;
    public GUIStyle MenuButtons;

    public GUIStyle MainMenuButtons;

    public GUIStyle[] Credits = new GUIStyle[5];

    public GUIStyle SubtitleLabels;

    public GUIStyle SettingsRadioButtons;
    public GUIStyle SettingsTooltip;

	public GUIStyle ScoreSliderBar, ScoreSliderButton;
    public Texture2D ScoreSliderBackground;

    public GUIStyle RGBBoxes;
    public GUIStyle RGBSliders;
    public GUIStyle RGBSliderNubs;

    public GUIStyle LobbyPlayerBox;
    public GUIStyle SelectionsArrows;
    public GUIStyle SelectionsSelection;

    public GUIStyle GameStatsText;
}
