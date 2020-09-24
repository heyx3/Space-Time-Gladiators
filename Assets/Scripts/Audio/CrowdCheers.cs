using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Provides functionality for creating crowd cheers.
/// </summary>
public class CrowdCheers : MonoBehaviour
{
    public const int CrowdCheerLevels = 7;

    private GameObject CheerContainer;

    //TODO: Hook all these events to crowd cheers. The ones that aren't hooked yet are marked with an empty comment next to them.
    /// <summary>
    /// Events that can trigger a crowd cheer.
    /// </summary>
    public enum Events
    {
        PlayerLightlyHurt,
        PlayerBadlyHurt,
        PlayerClusterFuck,//

        PlayerTeamHurt,

        FlagGrabbed,
        FlagReturned,
        FlagCaptured,

        BurstEnabled,//

        SmallPowerup,
        LargePowerup,

        NewVIP,

        WaypointSpawned,
        WaypointCaptured,

        GameStart,
        GameWin,//
    }
    private static Dictionary<Events, int> Severity = new Dictionary<Events, int>()
    {
        { Events.PlayerLightlyHurt, 0 },
        { Events.PlayerTeamHurt, 6 },
        { Events.PlayerBadlyHurt, 2 },
        { Events.PlayerClusterFuck, 3 },
        { Events.FlagGrabbed, 1 },
        { Events.FlagReturned, 1 },
        { Events.FlagCaptured, 2 },
        { Events.BurstEnabled, 1 },
        { Events.SmallPowerup, 1 },
        { Events.LargePowerup, 2 },
        { Events.NewVIP, 1 },
		{ Events.WaypointSpawned, 2},
        { Events.WaypointCaptured, 3 },
        { Events.GameStart, 4 },
        { Events.GameWin, 5 },
    };

    public GameObject BackgroundCheersPrefab;
    public GameObject HeightenedBackgroundCheersPrefab;
    public GameObject[] CrowdCheerSeveritiesPrefabs = new GameObject[CrowdCheerLevels];

    private FadeLoopNoise backgroundCheering;

    void Awake()
    {
        backgroundCheering = new FadeLoopNoise(BackgroundCheersPrefab, "Background Cheers");
    }

    private bool startedCheering = false;
    public void StartCheering()
    {
        startedCheering = true;
        backgroundCheering.StartLoop();
    }

    void Update()
    {
        if (!startedCheering) return;

        backgroundCheering.UpdateLoop();
    }

    void OnDestroy()
    {
        backgroundCheering.EndLoop();
    }

    /// <summary>
    /// Creates a crowd cheer with the appropriate severity for the given event.
    /// </summary>
    public void CreateCrowdCheer(Events gameEvent)
    {
        if (CrowdCheerSeveritiesPrefabs.Length - 1 < Severity[gameEvent])
        {
            throw new System.InvalidOperationException("There is not a high enough severity category for this event!");
        }

        if (CrowdCheerSeveritiesPrefabs[Severity[gameEvent]] == null)
        {
            return;
        }

        ((GameObject)Instantiate(CrowdCheerSeveritiesPrefabs[Severity[gameEvent]])).GetComponent<ControlledNoise>().StartClip();
    }
}