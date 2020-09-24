using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Holds some compound GUI controls.
/// </summary>
public static class MyGUI
{
    /// <summary>
    /// The recommended height for a slider in pixels.
    /// </summary>
    public const int SliderHeight = 10;
    /// <summary>
    /// The recommended height for a label in pixels.
    /// </summary>
    public const int LabelHeight = 20;

    //TODO: Implement "Equals()" and "ToString()" for all the layout classes for the ConstantsWriter script.

    //RGB Slider.

    /// <summary>
    /// Holds all data needed to position/format an RGB slider.
    /// </summary>
    [Serializable]
    public class RGBLayout
    {
        public Vector2 ScreenPos;

        public Vector2 LabelOffset;
        public float SliderWidth;
        public float SliderYSpacing;

        public Vector2 BackgroundBorderSize;

        public RGBLayout(Vector2 screenPos, Vector2 labelOffset, float sliderWidth, float sliderYSpacing, Vector2 backgroundBorderSize)
        {
            ScreenPos = screenPos;

            LabelOffset = labelOffset;
            SliderWidth = sliderWidth;
            SliderYSpacing = sliderYSpacing;

            BackgroundBorderSize = backgroundBorderSize;
        }
        public RGBLayout(RGBLayout copy)
            : this(copy.ScreenPos, copy.LabelOffset, copy.SliderWidth, copy.SliderYSpacing, copy.BackgroundBorderSize) { }
    }

    private const int RGBSliderTextHeight = 50;
    /// <summary>
    /// Draws an RGB slider using the GUI and the given arguments.
    /// </summary>
    /// <param name="scaleToScreen">If true, the values of "layout" will be scaled by the screen dimensions.</param>
    public static Color RGBSlider(Color currentRGB, RGBLayout layout, bool scaleToScreen)
    {
        Color oldColor = GUI.contentColor;

        //Layout data.

        if (scaleToScreen)
        {
            layout = new RGBLayout(layout);
            layout.ScreenPos = new Vector2(Screen.width * layout.ScreenPos.x, Screen.height * layout.ScreenPos.y);
            layout.LabelOffset = new Vector2(Screen.width * layout.LabelOffset.x, Screen.height * layout.LabelOffset.y);
            layout.SliderWidth = Screen.width * layout.SliderWidth;
            layout.SliderYSpacing = Screen.height * layout.SliderYSpacing;
            layout.BackgroundBorderSize = new Vector2(Screen.width * layout.BackgroundBorderSize.x, Screen.height * layout.BackgroundBorderSize.y);
        }

        Vector2 screenPos = new Vector2(layout.ScreenPos.x, layout.ScreenPos.y);
        Vector2 labelOffset = new Vector2(layout.LabelOffset.x, layout.LabelOffset.y);

        float width = layout.SliderWidth;
        float sliderYSpacing = layout.SliderYSpacing;
        Vector2 backgroundBorderSize = new Vector2(layout.BackgroundBorderSize.x, layout.BackgroundBorderSize.y);


        //Get screen rectangles.

        Rect backgroundRect = new Rect(screenPos.x - backgroundBorderSize.x, screenPos.y - backgroundBorderSize.y,
                                       width + (2.0f * backgroundBorderSize.x),
                                       (2.0f * backgroundBorderSize.y) + (2.0f * (SliderHeight - SliderHeight + sliderYSpacing)));

        Rect sliderRect = new Rect(screenPos.x, screenPos.y, width, SliderHeight);


        //Draw the background.
        GUI.backgroundColor = Color.white;
        GUI.Box(backgroundRect, "", WorldConstants.GUIStyles.RGBBoxes);

        //Draw the sliders.

        GUI.color = new Color(currentRGB.r, 0.0f, 0.0f);
        GUI.Label(new Rect(sliderRect.x + labelOffset.x, sliderRect.y + labelOffset.y, sliderRect.width, RGBSliderTextHeight), "Red");
        currentRGB.r = GUI.HorizontalSlider(sliderRect, currentRGB.r, 0.0f, 1.0f, WorldConstants.GUIStyles.RGBSliders, WorldConstants.GUIStyles.RGBSliderNubs);

        sliderRect.y += sliderYSpacing;
        GUI.color = new Color(0.0f, currentRGB.g, 0.0f);
        GUI.Label(new Rect(sliderRect.x + labelOffset.x, sliderRect.y + labelOffset.y, sliderRect.width, RGBSliderTextHeight), "Green");
        currentRGB.g = GUI.HorizontalSlider(sliderRect, currentRGB.g, 0.0f, 1.0f, WorldConstants.GUIStyles.RGBSliders, WorldConstants.GUIStyles.RGBSliderNubs);

        sliderRect.y += sliderYSpacing;
        GUI.color = new Color(0.0f, 0.0f, currentRGB.b);
        GUI.Label(new Rect(sliderRect.x + labelOffset.x, sliderRect.y + labelOffset.y, sliderRect.width, RGBSliderTextHeight), "Blue");
        currentRGB.b = GUI.HorizontalSlider(sliderRect, currentRGB.b, 0.0f, 1.0f, WorldConstants.GUIStyles.RGBSliders, WorldConstants.GUIStyles.RGBSliderNubs);

        GUI.contentColor = oldColor;
        return currentRGB;
    }


    //Selector.

    /// <summary>
    /// All the data specifying the layout of a Selector GUI control.
    /// </summary>
    [Serializable]
    public class SelectorLayout
    {
        public Vector2 CenterLeftPos;
        public float SpaceBetweenElements,
                     ArrowWidth, ArrowHeight;
        public float SelectedWidth, SelectedHeight;

        public SelectorLayout(Vector2 centerLeftPos, float spaceBetween, float arrowWidth, float arrowHeight, float selectedWidth, float selectedHeight)
        {
            CenterLeftPos = centerLeftPos;
            SpaceBetweenElements = spaceBetween;

            ArrowWidth = arrowWidth;
            ArrowHeight = arrowHeight;

            SelectedWidth = selectedWidth;
            SelectedHeight = selectedHeight;
        }
        public SelectorLayout(SelectorLayout copy)
            : this(copy.CenterLeftPos, copy.SpaceBetweenElements, copy.ArrowWidth, copy.ArrowHeight, copy.SelectedWidth, copy.SelectedHeight) { }
    }
    /// <summary>
    /// Creates a "selector" -- left/right arrow buttons with the currently-selected option displayed in between.
    /// Returns the new selected object based on player input.
    /// </summary>
    public static int Selector(GUIContent leftArrow, GUIContent rightArrow, GUIContent[] options, int currentOption,
                               SelectorLayout layout, bool wrapAroundBeginning, bool wrapAroundEnd, bool scaleToScreen,
                               GUIStyle arrowStyle = null, GUIStyle selectionStyle = null)
    {
        if (scaleToScreen)
        {
            layout = new SelectorLayout(layout);
            layout.CenterLeftPos = new Vector2(layout.CenterLeftPos.x * Screen.width, layout.CenterLeftPos.y * Screen.height);
            layout.SpaceBetweenElements *= Screen.width;
            layout.ArrowWidth *= Screen.width;
            layout.ArrowHeight *= Screen.height;
            layout.SelectedWidth *= Screen.width;
            layout.SelectedHeight *= Screen.height;
        }

        //The button/label areas.

        Rect leftArea = new Rect(layout.CenterLeftPos.x,
                                 layout.CenterLeftPos.y - (0.5f * layout.ArrowHeight),
                                 layout.ArrowWidth, layout.ArrowHeight);
        Rect centerArea = new Rect(leftArea.xMax + layout.SpaceBetweenElements,
                                   layout.CenterLeftPos.y - (0.5f * layout.SelectedHeight),
                                   layout.SelectedWidth, layout.SelectedHeight);
        Rect rightArea = new Rect(centerArea.xMax + layout.SpaceBetweenElements,
                                  leftArea.yMin,
                                  layout.ArrowWidth, layout.ArrowHeight);

        //Draw the current option.
        if (selectionStyle == null)
        {
            GUI.Box(centerArea, options[currentOption]);
        }
        else
        {
            GUI.Box(centerArea, options[currentOption], selectionStyle);
        }

        //Left arrow button.
        if ((arrowStyle == null && GUI.Button(leftArea, leftArrow)) ||
            (arrowStyle != null && GUI.Button(leftArea, leftArrow, arrowStyle)))
        {
            currentOption -= 1;
            if (currentOption < 0)
            {
                if (wrapAroundBeginning)
                {
                    currentOption += options.Length;
                }
                else
                {
                    currentOption = 0;
                }
            }
        }

        //Right arrow button.
        if ((arrowStyle == null && GUI.Button(rightArea, rightArrow)) ||
            (arrowStyle != null && GUI.Button(rightArea, rightArrow, arrowStyle)))
        {
            currentOption += 1;
            if (currentOption >= options.Length)
            {
                if (wrapAroundEnd)
                {
                    currentOption -= options.Length;
                }
                else
                {
                    currentOption = options.Length - 1;
                }
            }
        }

        return currentOption;
    }

    //Radio Buttons.

    /// <summary>
    /// All the data specifying the layout of a Radio Button GUI control.
    /// </summary>
    [Serializable]
    public class RadioButtonsLayout
    {
        public Vector2 TopLeftChoicePos;
        public Vector2 ButtonSize;
        public Vector2 ButtonSpacing;
        public int ButtonsPerRow;

        public RadioButtonsLayout(Vector2 topLeftChoicePos, Vector2 buttonSize, Vector2 buttonSpacing, int buttonsPerRow)
        {
            TopLeftChoicePos = topLeftChoicePos;
            ButtonSize = buttonSize;
            ButtonSpacing = buttonSpacing;
            ButtonsPerRow = buttonsPerRow;
        }
        public RadioButtonsLayout(RadioButtonsLayout copy)
            : this(copy.TopLeftChoicePos, copy.ButtonSize, copy.ButtonSpacing, copy.ButtonsPerRow) { }

        /// <summary>
        /// Gets the top left position of the given option in relative screen coordinates: (0, 0) to (1, 1).
        /// </summary>
        public Vector2 GetOptionPos(int choiceNumbFromZero)
        {
            Vector2 ret = TopLeftChoicePos;
            Vector2 increment = ButtonSize + ButtonSpacing;

            ret.x += (choiceNumbFromZero % ButtonsPerRow) * increment.x;
            ret.y += (choiceNumbFromZero / ButtonsPerRow) * increment.y;

            return ret;
        }
    }

    /// <summary>
    /// Creates a GUI system identical to radio buttons by using toggle buttons.
    /// Giving 0 or fewer buttons per row in the layout object puts all choices on the first row.
    /// </summary>
    public static int RadioButtons(GUIContent[] items, int currentSelected, RadioButtonsLayout layout, bool scaleToScreen, GUIStyle style = null)
    {
        if (scaleToScreen)
        {
            layout = new RadioButtonsLayout(layout);
            layout.TopLeftChoicePos = new Vector2(layout.TopLeftChoicePos.x * Screen.width, layout.TopLeftChoicePos.y * Screen.height);
            layout.ButtonSize = new Vector2(layout.ButtonSize.x * Screen.width, layout.ButtonSize.y * Screen.height);
            layout.ButtonSpacing = new Vector2(layout.ButtonSpacing.x * Screen.width, layout.ButtonSpacing.y * Screen.height);
        }

        Rect buttonRect = new Rect(layout.TopLeftChoicePos.x, layout.TopLeftChoicePos.y, layout.ButtonSize.x, layout.ButtonSize.y);

        bool tempToggled;
        for (int i = 0; i < items.Length; ++i)
        {
            //Show the button.
            if (style == null)
            {
                tempToggled = GUI.Toggle(buttonRect, (currentSelected == i), items[i]);
            }
            else
            {
                tempToggled = GUI.Toggle(buttonRect, (currentSelected == i), items[i], style);
            }

            //React to it being pressed.
            if (tempToggled)
            {
                currentSelected = i;
            }

            //Move the position counter for the next button.
            buttonRect.x += layout.ButtonSpacing.x + buttonRect.width;
            if (layout.ButtonsPerRow > 0 && ((i + 1) % layout.ButtonsPerRow == 0))
            {
                buttonRect.x = layout.TopLeftChoicePos.x;
                buttonRect.y += layout.ButtonSize.y + layout.ButtonSpacing.y;
            }
        }

        return currentSelected;
    }
}