using System.Linq;
using UnityEngine;
using System.Xml;
using System.IO;

/// <summary>
/// A Screen representing the "create match" menu.
/// </summary>
public class CreateMatchMenuScreen : MenuScreen
{
    MatchSettingsReadWrite matchSettings;
    LevelGenerationReadWrite levelGeneration;

    private GUIContent[] levels, matches;
    private Generator[] levelGens;
    private Rules[] matchSets;

    private int selectedMatchIndex, selectedLevelGenIndex;
    private string selectedMatch, selectedLevelGen;

    public CreateMatchMenuScreen(MatchStartData matchStartData, FrontEndConstants consts, ScreenUI owner)
        : base(matchStartData, consts, owner)
    {
        //Initialize data.

        //Debug.Log("Reading matches:");
        XmlDocument matchDoc = new XmlDocument();
        matchDoc.LoadXml(Owner.MatchesFile.text);
        //matchDoc.LoadXml(Owner.MatchesFile.text.Substring(1, Owner.MatchesFile.text.Length - 1));
        matchSettings = new MatchSettingsReadWrite(matchDoc);
        //Debug.Log("Read matches");

        //Debug.Log("Reading levels:");
        XmlDocument levelsDoc = new XmlDocument();
        levelsDoc.LoadXml(Owner.LevelsFile.text);
        //levelsDoc.LoadXml(Owner.LevelsFile.text.Substring(1, Owner.LevelsFile.text.Length - 1));
        levelGeneration = new LevelGenerationReadWrite(levelsDoc);
        //Debug.Log("Read levels");

        selectedMatch = "Unreadable";
        selectedLevelGen = "Unreadable";
        selectedMatchIndex = 0;
		selectedLevelGenIndex = 0;

        //Read matches.

        if (matchSettings.ErrorMessage == "")
        {
            matches = new GUIContent[matchSettings.Matches.Count()];
			matchSets = new Rules[matches.Length];
            int count = 0;
            foreach (string rulesN in matchSettings.Matches)
            {
                matchSets[count] = matchSettings.ReadMatch(rulesN);

                if (matchSettings.ErrorMessage != "")
                {
                    matches = null;
                    matchSets = null;
                    break;
                }

                matches[count] = new GUIContent(rulesN, matchSets[count].Description);
                count += 1;
            }

            selectedMatchIndex = Search(Convert(matches, m => m.text), "Brawl", (s1, s2) => s1 == s2);
            selectedMatch = matches[selectedMatchIndex].text;
            MatchData.MatchSettings = matchSets[selectedMatchIndex];
        }
        else
        {
            matches = null;
            matchSets = null;
        }

        //Read levels.

        if (levelGeneration.ErrorMessage == "")
        {
            levels = new GUIContent[levelGeneration.Levels.Count()];
			levelGens = new Generator[levels.Length];
            int count = 0;
            foreach (string levelN in levelGeneration.Levels)
            {
                levelGens[count] = levelGeneration.ReadGenerator(levelN);

                if (levelGeneration.ErrorMessage != "")
                {
                    levels = null;
                    levelGens = null;
                    break;
                }

                levels[count] = new GUIContent(levelN, levelGens[count].Description);
                count += 1;
            }

            selectedLevelGenIndex = Search(Convert(levels, l => l.text), "Corridor", (s1, s2) => s1 == s2);
            selectedLevelGen = levels[selectedLevelGenIndex].text;
            MatchData.GeneratedLevel = levelGens[selectedLevelGenIndex];
            MatchData.GenerateLevelAndSpawns();
        }
        else
        {
            levels = null;
            levelGens = null;
        }
    }

    private T[] Convert<O, T>(O[] array, System.Func<O, T> converter)
    {
        T[] ret = new T[array.Length];

        for (int i = 0; i < array.Length; ++i)
        {
            ret[i] = converter(array[i]);
        }

        return ret;
    }
    private int Search<T>(T[] array, T obj, System.Func<T, T, bool> comparer)
    {
        for (int i = 0; i < array.Length; ++i)
        {
            if (comparer(obj, array[i]))
            {
                return i;
            }
        }

        return -1;
    }

    protected override MenuScreen ProtectedUpdate(MenuScreen.ScreenLayout layout)
    {
        DrawTitleString("Choose a game-type and level:", layout.Title);

        #region Draw matches and levels.

        //Some layout data.

        Rect body = layout.Body;

        float sSubtitleYOffset = body.height * Consts.Settings_SubtitleYOffset,
              sSubtitleHeight = body.height * Consts.Settings_SubtitleHeight;

        float sListYOffset = body.height * Consts.Settings_List_YOffset;
        float sListXOffset = body.width * Consts.Settings_List_XOffset;
        float sListSpacing = body.height * Consts.Settings_List_Spacing;
        Vector2 sListButtonSize = new Vector2(body.width * Consts.Settings_List_ButtonSize.x, body.height * Consts.Settings_List_ButtonSize.y);

        float sTooltipXOffset = body.width * Consts.Settings_TooltipXOffset,
              sTooltipYOffset = body.height * Consts.Settings_TooltipYOffset,
              sTooltipWidth = body.width * Consts.Settings_TooltipWidth;

        //Some layout rectangles.

        Rect matchesSubtitleRect = new Rect(body.xMin + sListXOffset, body.yMin + sSubtitleYOffset, sListButtonSize.x, sSubtitleHeight),
             levelsSubtitleRect = new Rect(body.xMax - sListXOffset - sListButtonSize.x, matchesSubtitleRect.yMin, matchesSubtitleRect.width, matchesSubtitleRect.height);

        MyGUI.RadioButtonsLayout matchesLayout = Consts.Settings_List_Layout(new Vector2(matchesSubtitleRect.xMin, matchesSubtitleRect.yMax + sListYOffset), body),
                                 levelsLayout = Consts.Settings_List_Layout(new Vector2(levelsSubtitleRect.xMin, levelsSubtitleRect.yMax + sListYOffset), body);

        Rect tooltipRect = new Rect(matchesSubtitleRect.xMax + sTooltipXOffset,
                                    body.yMin + sTooltipYOffset,
                                    sTooltipWidth,
                                    body.height - sTooltipYOffset);


        //Display the matches.
        if (matches == null)
        {
            GUI.color = Color.black;
            GUI.Label(body, "XML error when reading match settings:\n\"" + matchSettings.ErrorMessage + "\".");
        }
        else
        {
            GUI.Label(matchesSubtitleRect, "Game-types", WorldConstants.GUIStyles.SubtitleLabels);

            selectedMatchIndex = MyGUI.RadioButtons(matches, selectedMatchIndex, matchesLayout, false, WorldConstants.GUIStyles.SettingsRadioButtons);
            selectedMatch = matches[selectedMatchIndex].text;

            MatchData.MatchSettings = matchSets[selectedMatchIndex];
        }

        //Display the levels.
        if (levels == null)
        {
            GUI.color = Color.black;
            GUI.Label(body, "XML error when reading level settings:\n\"" + levelGeneration.ErrorMessage + "\".");
        }
        else
        {
            GUI.Label(levelsSubtitleRect, "Levels", WorldConstants.GUIStyles.SubtitleLabels);

            int old = selectedLevelGenIndex;
            selectedLevelGenIndex = MyGUI.RadioButtons(levels, selectedLevelGenIndex, levelsLayout, false, WorldConstants.GUIStyles.SettingsRadioButtons);
            selectedLevelGen = levels[selectedLevelGenIndex].text;
            if (selectedLevelGen == "Open")
            {
                MatchStartData.IsGeneratingOpen = true;
            }
            else
            {
                MatchStartData.IsGeneratingOpen = false;
            }

            if (old != selectedLevelGenIndex)
            {
                MatchData.GeneratedLevel = levelGens[selectedLevelGenIndex];
                MatchData.GenerateLevelAndSpawns();
            }
        }

        GUI.color = Color.white;
        GUI.Label(tooltipRect, GUI.tooltip, WorldConstants.GUIStyles.SettingsTooltip);

        #endregion

        return DrawButtonMenu(layout.ButtonMenu,
                              new GUIContent[]
                              {
                                  new GUIContent("Back"),
                                  new GUIContent("Continue")
                              },
                              new GetData<MenuScreen>[]
                              {
                                  () => new MainMenuScreen(MatchData, Consts, Owner),
                                  () => new LobbyMenuScreen(MatchData, Consts, Owner)
                              },
                              this);
    }
}