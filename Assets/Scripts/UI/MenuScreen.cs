using UnityEngine;

/// <summary>
/// Represents a menu screen.
/// </summary>
public abstract class MenuScreen
{
    public virtual bool DrawBackground { get { return true; } }

    protected MatchStartData MatchData { get; private set; }
    protected FrontEndConstants Consts { get; private set; }
    protected ScreenUI Owner { get; private set; }

    public MenuScreen(MatchStartData matchStartData, FrontEndConstants consts, ScreenUI owner)
    {
        MatchData = matchStartData;
        Consts = consts;
        Owner = owner;
    }

    /// <summary>
    /// Updates this screen and returns the next screen to move to, or "null" if the screen should be dropped.
    /// </summary>
    public MenuScreen Update()
    {
        float space1 = Screen.height * Consts.Layout_TitleBodySpace,
              space2 = Screen.height * Consts.Layout_BodyButtonMenuSpace,
              usableHeight = Screen.height  - space1 - space2;

        Rect title = new Rect(0, 0, Screen.width, Consts.Layout_TitleHeight * usableHeight);
        Rect body = new Rect(0, title.yMax + space1, Screen.width, Consts.Layout_BodyHeight * usableHeight);
        Rect buttonMenu = new Rect(0, body.yMax + space2, Screen.width, Consts.Layout_ButtonMenuHeight * usableHeight);

        return ProtectedUpdate(new ScreenLayout
                               {
                                   Title = title,
                                   Body = body,
                                   ButtonMenu = buttonMenu,
                               });
    }

    /// <summary>
    /// The rectangular regions (in pixels) for each of the different parts of the screen.
    /// </summary>
    protected struct ScreenLayout
    {
        public Rect Title, Body, ButtonMenu;
    }
    /// <summary>
    /// Draws the GUI objects for this screen and returns the new MenuScreen to use (return "this" if the screen shouldn't change).
    /// </summary>
    protected abstract MenuScreen ProtectedUpdate(ScreenLayout layout);

    /// <summary>
    /// Draws the given title inside the given screen region.
    /// </summary>
    protected void DrawTitleString(string screenTitle, Rect titleRect)
    {
        GUI.Label(titleRect, screenTitle, WorldConstants.GUIStyles.ScreenTitles);
    }

    /// <summary>
    /// Represents a method that gets some object without any input.
    /// </summary>
    protected delegate T GetData<T>();
    /// <summary>
    /// Draws a button menu, using the given buttons and the given actions for each button.
    /// If the two arrays are not of equal size, an ArgumentException will be thrown.
    /// If the array of buttons is empty, an ArgumentOutOfRangeException will be thrown.
    /// </summary>
    /// <returns>The value of the parallel function for the button that was clicked, or the given default value if no button was clicked.</returns>
    protected R DrawButtonMenu<R>(Rect buttonMenuRect, GUIContent[] buttons, GetData<R>[] buttonValues, R defaultValue, GUIStyle style = null)
    {
        GUIStyle st = (style == null) ? WorldConstants.GUIStyles.MenuButtons : style;

        //Error-checking.
        if (buttons.Length != buttonValues.Length)
        {
            throw new System.ArgumentException("The 'buttons' array and 'buttonValues' array have different lengths!");
        }
        if (buttons.Length < 1)
        {
            throw new System.ArgumentOutOfRangeException("Must use at least one button!");
        }

        //Remove the border from the button region.
        Rect noBorderButtonMenuRect = new Rect(buttonMenuRect.xMin + (buttonMenuRect.width * Consts.ButtonMenu_XBorder),
                                               buttonMenuRect.yMin + (buttonMenuRect.height * Consts.ButtonMenu_YBorder),
                                               buttonMenuRect.width - (buttonMenuRect.width * 2 * Consts.ButtonMenu_XBorder),
                                               buttonMenuRect.height - (buttonMenuRect.height * 2 * Consts.ButtonMenu_YBorder));

        //Layout data.
        float sSpacing = buttonMenuRect.width * Consts.ButtonMenu_Spacing;
        float sWidth = (noBorderButtonMenuRect.width - (sSpacing * (buttons.Length - 1))) / (float)buttons.Length;

        //Keep a counter for the current button's horizontal position.
        float currentX = noBorderButtonMenuRect.xMin;

        //If there's only one button, center it.
        if (buttons.Length == 0)
        {
            currentX = buttonMenuRect.center.x - (sSpacing * 0.5f);
        }
        
        //Try each button.
        for (int i = 0; i < buttons.Length; ++i)
        {
            if (GUI.Button(new Rect(currentX, noBorderButtonMenuRect.yMin, sWidth, noBorderButtonMenuRect.height), buttons[i], st))
            {
                return buttonValues[i]();
            }

            currentX += sWidth + sSpacing;
        }

        //No buttons were clicked.
        return defaultValue;
    }
}