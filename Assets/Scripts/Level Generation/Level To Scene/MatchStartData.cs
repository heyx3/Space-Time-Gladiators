using System.Collections.Generic;
using UnityEngine;
using System.Collections;

/// <summary>
/// Holds data needed to start a match.
/// </summary>
public class MatchStartData : MonoBehaviour
{
	//An ugly short-term solution to a fustrating problem.
	public static bool IsGeneratingOpen = false;
	
    public Generator GeneratedLevel;
    public Rules MatchSettings;
	public SpawnCreator Spawns;

    //TODO: Data for remote (online) players.
    //TODO: Data for each player's character/stats.

    /// <summary>
    /// Each player ID in the match, indexed by team color.
    /// </summary>
    public Dictionary<Color, List<byte>> PlayersOnTeams;

    /// <summary>
    /// The control scheme for each local player, organized by his player ID (from 1 to 4).
    /// </summary>
    public Dictionary<byte, byte> PlayerControlSchemes;
	
	void Awake()
	{
		WorldConstants.MatchData = this;
	}
	
    void Start()
    {
        MatchSettings = new Rules();
        MatchSettings.SetPainRules(new PainRules(), 1.0f);
        MatchSettings.ScoreGoal = 300;

        PlayersOnTeams = new Dictionary<Color,List<byte>>()
        {
            { Color.red, new List<byte>() { 1 } },
            { Color.blue, new List<byte>() { 2 } },
            { Color.green, new List<byte>() { 3 } },
        };

        PlayerControlSchemes = new Dictionary<byte, byte>()
        {
            { 1, 1 },
            { 2, 2 },
            { 3, 3 },
        };
		
        GeneratedLevel = new RoguelikeGen();
        GeneratedLevel.SetSettings(new RoguelikeGenSettings());
        GenerateLevelAndSpawns();
    }

    public void GenerateLevelAndSpawns()
    {
        GeneratedLevel.FullGenerate();
        GeneratorSettings.FlipYs(GeneratedLevel);

        Spawns = new SpawnCreator(GeneratedLevel, PlayersOnTeams.Count);
        while (!Spawns.GenerateSpawns())
        {
            GeneratedLevel.FullGenerate();
            GeneratorSettings.FlipYs(GeneratedLevel);
			Spawns = new SpawnCreator(GeneratedLevel, PlayersOnTeams.Count);
        }
    }
}