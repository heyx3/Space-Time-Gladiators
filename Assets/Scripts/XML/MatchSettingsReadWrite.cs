using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

/// <summary>
/// Handles reading/writing a set of match settings from/to the match XML file.
/// </summary>
public class MatchSettingsReadWrite : XMLReadWrite
{
    /// <summary>
    /// The different matches contained in the XML file.
    /// </summary>
    public IEnumerable<string> Matches
    {
        get
        {
            foreach (XmlNode node in matches)
                foreach (XmlAttribute a in node.Attributes)
                    if (a.Name == "name")
                    {
                        yield return a.Value;
                        break;
                    }
        }
    }
    private XmlNodeList matches;

    /// <summary>
    /// Reads in the given XML match settings file
    /// from the resources directory (without the ".xml" extension).
    /// </summary>
    public MatchSettingsReadWrite(TextAsset file, string fileName)
        : base(file, fileName, "matches")
    {
        if (ErrorMessage != "")
        {
            return;
        }

        try
        {
            matches = RootNode.ChildNodes;
        }
        catch (Exception e)
        {
            matches = null;
            Debug.Log("Error getting match settings: " + e.Message);
            ErrorMessage = e.Message;
        }
    }
    public MatchSettingsReadWrite(XmlDocument loadedDocument)
        : base(loadedDocument, "matches")
    {
        if (ErrorMessage != "")
        {
            return;
        }

        try
        {
            matches = RootNode.ChildNodes;
        }
        catch (Exception e)
        {
            matches = null;
            Debug.Log("Error getting match settings: " + e.Message);
            ErrorMessage = e.Message;
        }
    }

    /// <summary>
    /// Writes the given match into the match XML file.
    /// </summary>
    public void WriteMatch(Rules match, string name)
    {
        XmlNode n = RootNode.AppendChild(Document.CreateElement("match"));

        XmlAttribute nameAtt = Document.CreateAttribute(name);
        nameAtt.Value = name;
        n.Attributes.Append(nameAtt);

        Document.Save(FullPath);
    }

    /// <summary>
    /// Gets the match with the given name.
    /// Throws an ArgumentException if the name doesn't exist
    /// or an XmlException if the XML data is missing.
    /// Will also throw exceptions from attempting to parse the data itself.
    /// </summary>
    public Rules ReadMatch(string name)
    {
        //Get the correct match node.
        XmlNode matchN = null;
        for (int i = 0; i < matches.Count; ++i)
            if (GetAttribute(matches[i], "name") == name)
            {
                matchN = matches[i];
                break;
            }
        if (matchN == null)
            throw new XmlException();

        //Try to get all the match settings.
        Rules ret = new Rules();
        //Track the current property/value in case there's an XML error.
        string propertyN = "";
        string var = "";
        float importance;
        //These two functions are just shortcuts to cut down on typing.
        Func<string> GetAtt;
        Func<string> GetCh;
        try
        {
            foreach (XmlNode property in matchN.ChildNodes)
            {
                propertyN = property.Name;
                GetAtt = () => GetAttribute(property, var);
                GetCh = () => GetChild(property, var).InnerText;

                switch (propertyN)
                {
					#region Description
					
					case ("description"):
					
						ret.Description = property.InnerText;
					
						break;
					
					#endregion
					
                    #region Basic Data

                    case ("basics"):

                        var = "score";
                        ret.ScoreGoal = Single.Parse(GetCh());
                        var = "length";
                        ret.MatchLength = TimeSpan.FromMinutes(Double.Parse(GetCh()));
                        var = "powerupSpawnInterval";
                        ret.PowerupSpawnInterval = TimeSpan.FromSeconds(Double.Parse(GetCh()));
                        var = "enemiesArePeopleToo";
                        ret.EnemiesArePeopleToo = Boolean.Parse(GetCh());

                        break;

                    #endregion

                    #region Pain

                    case ("pain"):

                        var = "importance";
                        importance = Single.Parse(GetAtt());

                        var = "";
                        ret.SetPainRules(new PainRules(), importance);

                        break;

                    #endregion

                    #region CTF

                    case ("ctf"):

                            var = "homeToScore";
                            bool homeToScore = Boolean.Parse(GetCh());
                            var = "carrySpeedScale";
                            float speedScale = Single.Parse(GetCh());
                            var = "carryStrengthScale";
                            float strengthScale = Single.Parse(GetCh());
                            var = "droppedResetTime";
                            double timeToReset = Double.Parse(GetCh());
                            var = "painReceivedDrop";
                            float painReceivedDrop = Single.Parse(GetCh());
                            var = "flagRespawnDelay";
                            float respawnTime = Single.Parse(GetCh());
                            var = "touchToReturn";
                            bool touchToReturn = Boolean.Parse(GetCh());
                            var = "importance";
                            importance = Single.Parse(GetAtt());

                            var = "";
                            ret.SetCTFRules(new CTFRules(homeToScore, speedScale, strengthScale, painReceivedDrop, timeToReset, respawnTime, touchToReturn), importance);

                        break;

                    #endregion

                    #region Bullseye

                    case ("bullseye"):

                            var = "nextTargetSelection";
                            BullseyeRules.NextTargetSelection chooser = ParseBulls(GetCh());
                            var = "targetChangeTime";
                            double changeTime = Double.Parse(GetCh());
                            var = "targetChangePainDealt";
                            float painDealThresh = Single.Parse(GetCh());
                            var = "targetChangePainReceived";
                            float painReceiveThresh = Single.Parse(GetCh());
                            var = "importance";
                            importance = Single.Parse(GetAtt());

                            var = "";
                            ret.SetBullseyeRules(new BullseyeRules(chooser, changeTime, painReceiveThresh, painDealThresh), importance);

                        break;

                    #endregion

                    #region Powerup Hunt

                    case ("powerup"):

                            var = "importance";
                            importance = Single.Parse(GetAtt());

                            var = "";
                            ret.SetPowerupHuntRules(new PowerupHuntRules(), importance);

                        break;

                    #endregion

                    #region Waypoint Race

                    case ("waypoint"):

                            var = "newWaypointDelay";
                            float delay = Single.Parse(GetCh());

                            var = "importance";
                            importance = Single.Parse(GetAtt());

                            var = "";
                            ret.SetWaypointFightRules(new WaypointFightRules(delay), importance);

                        break;

                    #endregion

                    default:
                        throw new NotImplementedException();
                }
            }
        }
        catch (Exception e)
        {
			if (var == "")
				throw new ArgumentException("Something went wrong when initializing a game-type!");
            throw new XmlException("The node or attribute '" + var + "' in '" + propertyN + "' doesn't exist or is invalid!");
        }

        return ret;
    }
    /// <summary>
    /// Gets the method for selecting a new Target given the XML string representation of the method.
    /// </summary>
    private BullseyeRules.NextTargetSelection ParseBulls(string val)
    {
        switch (val)
        {
            case ("random"): return BullseyeRules.NextTargetSelection.Random;
            case ("lowestScore"): return BullseyeRules.NextTargetSelection.LowestScore;
            case ("highestScore"): return BullseyeRules.NextTargetSelection.HighestScore;
            case ("fewestTimes"): return BullseyeRules.NextTargetSelection.FewestTimesAsVIP;

            default: throw new ArgumentException();
        }
    }
}