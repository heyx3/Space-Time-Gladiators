using UnityEngine;
using System.Collections;

/// <summary>
/// The game.
/// </summary>
/// <remarks>I lost the game.</remarks>
public class MatchScreen : MenuScreen
{
    public override bool DrawBackground { get { return false; } }

    public MatchScreen(MatchStartData matchData, FrontEndConstants consts, ScreenUI owner)
        : base(matchData, consts, owner)
    {
        manager = WorldConstants.Creator.CreateMatchController().GetComponent<LevelManager>();
    }

    private LevelManager manager;
    private bool started = false;
    protected override MenuScreen ProtectedUpdate(MenuScreen.ScreenLayout layout)
    {
        if (!started)
        {
            manager.StartWorld(false);
            started = true;

            Owner.MusicLooper.EndLoop();
            Owner.MusicLooper = new FadeLoopNoise(Owner.MatchMusic, "Music");
            Owner.MusicLooper.StartLoop();
        }
        else
        {
            Owner.MusicLooper.UpdateLoop();

            if (manager.GameOver)
            {
                return new MatchEndScreen(MatchData, Consts, Owner);
            }
        }

        return this;
    }
}