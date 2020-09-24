using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class MatchEndScreen : MenuScreen
{
    public override bool DrawBackground { get { return false; } }

    private float elapsed;

    public MatchEndScreen(MatchStartData matchData, FrontEndConstants consts, ScreenUI owner)
        : base(matchData, consts, owner)
    {
        elapsed = 0.0f;
        WorldConstants.MatchController.GetComponent<InputManager>().DisableInput = true;
    }

    protected override MenuScreen ProtectedUpdate(MenuScreen.ScreenLayout layout)
    {
        elapsed += Time.deltaTime;

        if (elapsed >= WorldConstants.MatchEndWaitTIme.TotalSeconds)
        {
            return new MatchStatsScreen(MatchData, Consts, Owner);
        }

        return this;
    }
}
