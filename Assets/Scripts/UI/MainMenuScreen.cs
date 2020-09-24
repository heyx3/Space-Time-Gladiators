using UnityEngine;

/// <summary>
/// A Screen representing the main menu.
/// </summary>
public class MainMenuScreen : MenuScreen
{
    public override bool DrawBackground { get { return Owner.Logos.Length == 0; } }

    public MainMenuScreen(MatchStartData matchData, FrontEndConstants consts, ScreenUI owner)
        : base(matchData, consts, owner) { }

    bool started = false;
    protected override MenuScreen ProtectedUpdate(ScreenLayout layout)
    {
        //The music looping system.

        if (!started)
        {
            started = true;

            //Start playing the main menu music if it isn't playing already.
            if (Owner.MusicLooper == null ||
                Owner.MusicLooper.Loop != Owner.MenuMusic)
            {
                if (Owner.MusicLooper != null)
                {
                    Owner.MusicLooper.EndLoop();
                }

                Owner.MusicLooper = new FadeLoopNoise(Owner.MenuMusic, "Menu Music");
                Owner.MusicLooper.StartLoop();
            }
        }
        else
        {
            Owner.MusicLooper.UpdateLoop();
        }


        //The actual main menu.

        if (!DrawBackground)
        {
            Texture2D logo = Owner.Logos[Owner.LogoToUse];

            float logoWidth = Screen.width;
            float logoHeight = (logo.height / (float)logo.width) * logoWidth;
            float yOffset = (Screen.height - logoHeight) * 0.5f;

            GUI.DrawTexture(new Rect(0.0f, yOffset, logoWidth, logoHeight), logo);
        }

        GUI.color = Color.white;

        return DrawButtonMenu(layout.ButtonMenu,
                              new GUIContent[]
                              {
                                  new GUIContent("Quit"),
                                  new GUIContent("Credits"),
                                  new GUIContent("Create Game"),
                              },
                              new GetData<MenuScreen>[]
                              {
                                  () => { Application.Quit(); return this; },
                                  () => new CreditsScreen(Owner, MatchData, Consts),
                                  () => new CreateMatchMenuScreen(MatchData, Consts, Owner),
                              },
                              this, WorldConstants.GUIStyles.MainMenuButtons);
    }
}