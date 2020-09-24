using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

/// <summary>
/// Writes the current constants to a text file on scene exit.
/// </summary>
public class ConstantsWriter : MonoBehaviour
{
    CollectibleObjectiveConstants co;
    ActorConstants a;
    PlayerConstants p;

    /// <summary>
    /// The text file name, not including the ".txt" at the end.
    /// </summary>
    public string FileName = "Constants";
	
	public string FullFilePath { get { return Application.dataPath + "/Resources/" + FileName + ".txt"; } }

    /// <summary>
    /// Gets assigned through code. Reads the file.
    /// </summary>
    private TextAsset asset;

    /// <summary>
    /// This is the writer that writes to the file
    /// </summary>
    private StreamWriter writer;
	
	/// <summary>
	/// The initial values of all constants when this behavior starts up.
	/// </summary>
	private Dictionary<string, string> startingFieldValues;

    /// <summary>
    /// Whether or not each type to be used has static fields (as opposed to member fields).
    /// </summary>
    private Dictionary<Type, bool> typesAreStatic = new Dictionary<Type, bool>()
    {
        { typeof(WorldConstants), true },
        { typeof(CollectibleObjectiveConstants), false },
        { typeof(ActorConstants), false },
        { typeof(PlayerConstants), false },
        { typeof(CameraConstants), false },
        { typeof(FrontEndConstants), false },
    };
	/// <summary>
	/// Any fields that should be ignored by this behavior, indexed by class name.
	/// </summary>
	private Dictionary<Type, List<string>> ignoreFields = new Dictionary<Type, List<string>>()
	{
		{ typeof(WorldConstants), new List<string>() { "MatchWrapper", "MatchData", "ScreenUI", "ColTracker", "ConstantsOwner", "CrowdCheering", "Creator", "PlayPhysNoises", "Size", "MirrorContainer", "WallContainer", "MatchController", "LevelBounds", "MaxViewBounds" } },
		{ typeof(CollectibleObjectiveConstants), new List<string>() { } },
		{ typeof(ActorConstants), new List<string>() { "EnemiesTeam" } },
		{ typeof(PlayerConstants), new List<string>() { } },
        { typeof(CameraConstants), new List<string>() { } },
        { typeof(FrontEndConstants), new List<string>() { } },
	};
	
    void AppendString(string appendString)
    {
        writer = new StreamWriter(FullFilePath, true);
        writer.WriteLine(appendString);
		writer.Close ();
    }
	
	private bool checkedYet = false;
    void Update()
    {
        if (!checkedYet)
        {
            try
            {
                startingFieldValues = new Dictionary<string, string>();

                FieldInfo[] fields;
                object instance;
                foreach (Type t in typesAreStatic.Keys)
                {
                    if (typesAreStatic[t])
                    {
                        instance = null;
                        fields = t.GetFields(BindingFlags.Public | BindingFlags.Static);
                    }
                    else
                    {
                        instance = WorldConstants.ConstantsOwner.GetComponent(t);
                        fields = t.GetFields(BindingFlags.Public | BindingFlags.Instance);
                    }

                    foreach (FieldInfo f in fields)
                    {
                        if (!ignoreFields[t].Contains(f.Name))
                        {
                            startingFieldValues.Add(t.ToString() + " " + f.Name, f.GetValue(instance).ToString());
                        }
                    }
                }

                checkedYet = true;
            }
            catch (Exception e)
            {

            }
        }
    }
	
	void OnDestroy()
	{
		if (!written)
		{
			WriteConstants();	
		}
	}
	
    private bool written = false;
    /// <summary>
    /// Writes any changes to the constants if they haven't been written already.
    /// </summary>
    public void WriteConstants()
    {
        if (written || !checkedYet)
        {
            return;
        }
        written = true;

        //Build the output of constant values that have changed.

        StringBuilder sb = new StringBuilder();

        sb.AppendLine("\n\n\nChanges on " + System.DateTime.Now.ToString());
        bool changes = false;
        
        FieldInfo[] fields;
        object instance;
        foreach (Type t in typesAreStatic.Keys)
        {
            sb.AppendLine("\n----------------" + t.ToString() + "--------------------");

            if (typesAreStatic[t])
            {
                instance = null;
                fields = t.GetFields(BindingFlags.Public | BindingFlags.Static);
            }
            else
            {
                instance = WorldConstants.ConstantsOwner.GetComponent(t);
                fields = t.GetFields(BindingFlags.Public | BindingFlags.Instance);
            }

            foreach (FieldInfo f in fields)
            {
                if (!ignoreFields[t].Contains(f.Name) &&
                    startingFieldValues[t.ToString() + " " + f.Name] != f.GetValue(instance).ToString())
                {
                    sb.AppendLine("\t" + f.Name + ":" +
                                  "\n\t\t Old = " + startingFieldValues[t.ToString() + " " + f.Name] +
                                  "\n\t\t New = " + f.GetValue(instance).ToString());
                    changes = true;
                }
            }
        }

        //Output it to the file.
        if (changes)
		{
			string s = sb.ToString ();
			AppendString(s);
			
		}
    }
}