using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

/// <summary>
/// Handles reading/writing level generation data from/to the level generation XML file.
/// </summary>
public class LevelGenerationReadWrite : XMLReadWrite
{
    public IEnumerable<string> Levels
    {
        get
        {
            foreach (XmlNode node in levels)
            {
                foreach (XmlAttribute a in node.Attributes)
                {
                    if (a.Name == "name")
                    {
                        yield return a.Value;
                        break;
                    }
                }
            }
        }
    }
    private XmlNodeList levels;

    public LevelGenerationReadWrite(TextAsset asset, string fileName)
        : base(asset, fileName, "generations")
    {
        if (ErrorMessage != "")
        {
            return;
        }

        try
        {
            levels = RootNode.ChildNodes;
        }
        catch (Exception e)
        {
            levels = null;
            ErrorMessage = e.Message;
        }
    }
    public LevelGenerationReadWrite(XmlDocument loadedDocument)
        : base(loadedDocument, "generations")
    {
        if (ErrorMessage != "")
        {
            return;
        }

        try
        {
            levels = RootNode.ChildNodes;
        }
        catch (Exception e)
        {
            levels = null;
            ErrorMessage = e.Message;
        }
    }

    /// <summary>
    /// Gets the generator with the given name.
    /// Throws an ArgumentException if there is no generator with the given name, or an
    /// XmlException if there is a problem reading in the XML data.
    /// </summary>
    public Generator ReadGenerator(string name)
    {
        XmlNode levelN = null;
        for (int i = 0; i < levels.Count; ++i)
            if (GetAttribute(levels[i], "name") == name)
            {
                levelN = levels[i];
            }
        if (levelN == null)
            throw new ArgumentException("The given level generation setting does not exist.");

        //Try to get all the match settings.
        Generator ret = null;
        //Track the current property/value in case there's an XML error.
        string propertyName = "";
        string var = "";
        string type = "";
        //These two functions are just shortcuts to cut down on typing.
        Func<string> GetAtt, GetCh;
        try
        {
            foreach (XmlNode property in levelN.ChildNodes)
            {
                propertyName = property.Name;
                GetAtt = () => GetAttribute(property, var);
                GetCh = () => GetChild(property, var).InnerText;

                switch (propertyName)
                {
                    case ("generator"):

                        var = "type";
                        type = GetAtt();
                        ret = GetBaseGenerator(type, property);

                        break;

                    case ("fill"):

                        var = "type";
                        type = GetAtt();
                        ret.GenSettings.FillPatterns.Add(GetFillPattern(type, property));

                        break;
					
					case ("description"):
					
						ret.Description = property.InnerText;
						
						break;
                }
            }
        }
        catch (Exception e)
        {
            throw new XmlException("Couldn't read XML data correctly: " + e.Message);
        }

        return ret;
    }

    private Generator GetBaseGenerator(string genType, XmlNode genNode)
    {
        string var = "";
        bool wrapX, wrapY;
        Func<string> GetAtt = () => GetAttribute(genNode, var),
                     GetCh = () => GetChild(genNode, var).InnerText,
                     GetAtt2, GetCh2;

        Generator ret;

        switch (genType)
        {
            case ("RDMaze"):

                ret = new RDMazeGen();

                var = "sizeX";
                int sizeX = Int32.Parse(GetCh());
                var = "sizeY";
                int sizeY = Int32.Parse(GetCh());


                RDMazeGenSettings sett = new RDMazeGenSettings(new Location(sizeX, sizeY));
                sett.FillPatterns.Clear();

                var = "wrapX";
                wrapX = Boolean.Parse(GetCh());
                sett.WrapX = wrapX;

                var = "wrapY";
                wrapY = Boolean.Parse(GetCh());
                sett.WrapY = wrapY;


                XmlNode igRegNode = GetChild(genNode, "ignoreRegionChance");

                GetAtt2 = () => GetAttribute(igRegNode, var);
                GetCh2 = () => GetChild(igRegNode, var).InnerText;

                var = "type";
                string igType = GetAtt2();
                sett.IgnoreRegionChance = GetIgnoreRegionChance(GetAtt2, GetCh2, igType, ref var);
                

                XmlNode splitDirNode = GetChild(genNode, "splitDirection");

                GetAtt2 = () => GetAttribute(splitDirNode, var);
                GetCh2 = () => GetChild(splitDirNode, var).InnerText;

                var = "type";
                string splDrType = GetAtt2();
                sett.ShouldSplitHorizontally = GetSplitDir(GetAtt2, GetCh2, splDrType, ref var);


                XmlNode splitLocationNode = GetChild(genNode, "splitLocation");

                GetAtt2 = () => GetAttribute(splitLocationNode, var);
                GetCh2 = () => GetChild(splitLocationNode, var).InnerText;

                var = "type";
                string splLocType = GetAtt2();
                sett.SplitLocation = GetSplitLocation(GetAtt2, GetCh2, splLocType, ref var);


                XmlNode numbHolesNode = GetChild(genNode, "numbHoles");

                GetAtt2 = () => GetAttribute(numbHolesNode, var);
                GetCh2 = () => GetChild(numbHolesNode, var).InnerText;

                var = "type";
                string getNumbType = GetAtt2();

                sett.HolesInLine = GetHolesInLine(Math.Max(sizeX, sizeY), GetAtt2, GetCh2, getNumbType, ref var);


                ret.SetSettings(sett);
                return ret;

            case ("Roguelike"):

                ret = new RoguelikeGen();

                RoguelikeGenSettings set = new RoguelikeGenSettings();
                set.FillPatterns.Clear();

                var = "wrapX";
                set.WrapAroundX = Boolean.Parse(GetCh());
                var = "wrapY";
                set.WrapAroundY = Boolean.Parse(GetCh());

                var = "roomColumns";
                int roomCols = Int32.Parse(GetCh());
                var = "roomRows";
                int roomRows = Int32.Parse(GetCh());
                set.NumberOfNodes = new Location(roomCols, roomRows);

                var = "roomWidth";
                int roomWidth = Int32.Parse(GetCh());
                var = "roomHeight";
                int roomHeight = Int32.Parse(GetCh());
                set.RoomDimensions = new Location(roomWidth, roomHeight);

                var = "roomWidthVarianceMin";
                int roomWidthVarianceMin = Int32.Parse(GetCh());
                var = "roomWidthVarianceMax";
                int roomWidthVarianceMax = Int32.Parse(GetCh());
                set.RoomXVariance = new Interval(roomWidthVarianceMin, roomWidthVarianceMax, true, 0);

                var = "roomHeightVarianceMin";
                int roomHeightVarianceMin = Int32.Parse(GetCh());
                var = "roomHeightVarianceMax";
                int roomHeightVarianceMax = Int32.Parse(GetCh());
                set.RoomYVariance = new Interval(roomHeightVarianceMin, roomHeightVarianceMax, true, 0);

                var = "verticalTunnelThickness";
                int verticalTunnelThickness = Int32.Parse(GetCh());
                var = "horizontalTunnelThickness";
                int horizontalTunnelThickness = Int32.Parse(GetCh());
                set.TunnelThickness = new Location(horizontalTunnelThickness, verticalTunnelThickness);

                var = "verticalTunnelLength";
                int verticalTunnelLength = Int32.Parse(GetCh());
                var = "horizontalTunnelLength";
                int horizontalTunnelLength = Int32.Parse(GetCh());
                set.TunnelLength = new Location(horizontalTunnelLength, verticalTunnelLength);

                var = "roomChance";
                set.PercentRooms = Single.Parse(GetCh());

                ret.SetSettings(set);

                return ret;

            default: throw new ArgumentException();
        }
    }

    private Func<Region, double> GetIgnoreRegionChance(Func<string> GetAtt, Func<string> GetCh, string igType, ref string var)
    {
        switch (igType)
        {
            case ("idealRegionDims"):

                var = "idealWidth";
                int idealWidth = Int32.Parse(GetCh());

                var = "idealHeight";
                int idealHeight = Int32.Parse(GetCh());

                var = "minimumWidth";
                int minimumWidth = Int32.Parse(GetCh());

                var = "minimumHeight";
                int minimumHeight = Int32.Parse(GetCh());

                var = "ignoreWidthBelow";
                int ignoreWidthBelow = Int32.Parse(GetCh());

                var = "ignoreHeightBelow";
                int ignoreHeightBelow = Int32.Parse(GetCh());

                return RDMazeGenSettings.IdealRegionFunc(new Location(idealWidth, idealHeight), new Location(minimumWidth, minimumHeight), new Location(ignoreWidthBelow, ignoreHeightBelow));

            case ("idealRegionArea"):

                var = "idealArea";
                int idealArea = Int32.Parse(GetCh());

                var = "minimumArea";
                int minimumArea = Int32.Parse(GetCh());

                var = "ignoredAreaBelow";
                int ignoreBelow = Int32.Parse(GetCh());

                return RDMazeGenSettings.IdealRegionFunc(idealArea, minimumArea, ignoreBelow);

            case ("ratio"):

                var = "idealRatio";
                float idealRatio = Single.Parse(GetCh());

                var = "farthestRatio";
                float farthestRatio = Single.Parse(GetCh());

                return RDMazeGenSettings.IdealDimensionsFunc(idealRatio, farthestRatio);

            default: throw new ArgumentException("Not a valid \"Ignore Region Chance\" structure.");
        }
    }
    private Func<Location, bool> GetSplitDir(Func<string> GetAtt, Func<string> GetCh, string splitDirType, ref string var)
    {
        switch (splitDirType)
        {
            case ("constantSplitChance"):

                var = "chanceHorizontal";
                float chanceHor = Single.Parse(GetCh());

                return RDMazeGenSettings.ConstantSplitChance(chanceHor);

            case ("splitLargerDimension"):

                return RDMazeGenSettings.SplitLargerDimension();

            default: throw new ArgumentException("Not a valid way to choose the split direction.");
        }
    }
    private Func<int, int, int> GetSplitLocation(Func<string> GetAtt, Func<string> GetCh, string splitLocType, ref string var)
    {
        switch (splitLocType)
        {
            case ("randomWithBorder"):

                var = "border";
                int border = Int32.Parse(GetCh());

                return RDMazeGenSettings.RandomSplitLocation(border);

            case ("random"):

                return RDMazeGenSettings.RandomSplitLocation();

            case ("centerAsRegionShrinks"):

                var = "maxSize";
                int maxSize = Int32.Parse(GetCh());

                var = "variation";
                int variation = Int32.Parse(GetCh());

                var = "shouldReverse";
                bool shouldReverse = Boolean.Parse(GetCh());

                return RDMazeGenSettings.CenterSplitAsRegionShrinks(maxSize, variation, shouldReverse);

            default: throw new ArgumentException("Not a valid way to get the split location.");
        }
    }
    private Func<int, int> GetHolesInLine(int largestDimension, Func<string> GetAtt, Func<string> GetCh, string getNumbType, ref string var)
    {
        switch (getNumbType)
        {
            case ("manyForMedium"):

                var = "max";
                int max = Int32.Parse(GetCh());

                return RDMazeGenSettings.ManyHolesForMediumLine(4, largestDimension);

            case ("oneHole"):

                return RDMazeGenSettings.OneHoleInLine;

            case ("constantHoles"):

                var = "numbHoles";
                int numb = Int32.Parse(GetCh());

                return RDMazeGenSettings.ConstantHolesInLine(numb);

            default: throw new ArgumentException("Not a valid way to get the number of holes in a line.");
        }
    }

    private FillPattern GetFillPattern(string fillType, XmlNode pattNode)
    {
        string var = "";
        Func<string> GetCh = () => GetChild(pattNode, var).InnerText,
                     GetAtt = () => GetAttribute(pattNode, var);

        string maxSuitability = "very";
        if (GetAttribute(pattNode, "maxSuitability") != null)
        {
            maxSuitability = GetAttribute(pattNode, "maxSuitability");
        }
        Suitability maxSuit;
        switch (maxSuitability)
        {
            case ("little"):
                maxSuit = Suitability.Little;
                break;
            case ("moderate"):
                maxSuit = Suitability.Moderate;
                break;
            case ("very"):
                maxSuit = Suitability.Very;
                break;

            default: throw new ArgumentException("Not a valid max suitability for the " + fillType + " fill pattern.");
        }

        switch (fillType)
        {
            case ("blank"):
                BlankRegionPattern brp = new BlankRegionPattern();
                brp.MaxSuitability = maxSuit;
                return brp;

            case ("carvedPlus"):
                CarvedPlusPattern cpp = new CarvedPlusPattern();
                cpp.MaxSuitability = maxSuit;
                return cpp;

            case ("concentricSquares"):
                ConcentricSquaresPattern csp = new ConcentricSquaresPattern();
                csp.MaxSuitability = maxSuit;
                return csp;

            case ("line"):
                LinePattern lp = new LinePattern();
                lp.MaxSuitability = maxSuit;
                return lp;

            case ("alternatingSteps"):
                AlternatingStepsPattern asp = new AlternatingStepsPattern();
                asp.MaxSuitability = maxSuit;
                return asp;

            case ("x"):

                var = "minXToYRatio";
                float min = Single.Parse(GetCh());
                var = "maxXToYRatio";
                float max = Single.Parse(GetCh());

                XPattern xp = new XPattern(new Interval(min, max, true, 2));
                xp.MaxSuitability = maxSuit;
                return xp;

            case ("circle"):

                var = "radius";
                int radius = Int32.Parse(GetCh());

                CirclePattern cp = new CirclePattern(radius);
                cp.MaxSuitability = maxSuit;
                return cp;

            case ("platforms"):

                var = "spaceBetween";
                int spaceBetween = Int32.Parse(GetCh());

                var = "border";
                int border = Int32.Parse(GetCh());

                var = "proportionHoles";
                float proportionHoles = Single.Parse(GetCh());

                var = "minArea";
                int minArea = Int32.Parse(GetCh());

                var = "minAcceptableXToYRatio";
                float minRatio = Single.Parse(GetCh());
                var = "maxAcceptableXToYRatio";
                float maxRatio = Single.Parse(GetCh());

                PlatformsPattern pp = new PlatformsPattern(spaceBetween, border, proportionHoles, minArea, new Interval(minRatio, maxRatio, true, 3));
                pp.MaxSuitability = maxSuit;
                return pp;

            case ("steppedHallway"):

                var = "relativeHeight";
                float relHeight = Single.Parse(GetCh());

                return new SteppedHallwayPattern(GetPlateauGen(GetChild(pattNode, "plateau")), relHeight);

            case ("thickHurdle"):

                var = "averageRelativeHeight";
                float avRelHeight = Single.Parse(GetCh());

                var = "relativeHeightVariance";
                float relheightVariance = Single.Parse(GetCh());

                return new ThickHurdlePattern(GetPlateauGen(GetChild(pattNode, "plateau")), avRelHeight, relheightVariance);

            default: throw new ArgumentException("Not a valid fill pattern.");
        }
    }
    private PlateauGenerationProperties GetPlateauGen(XmlNode platNode)
    {
        string type = GetAttribute(platNode, "type");

        string var = "";
        Func<string> GetCh = () => GetChild(platNode, var).InnerText,
                     GetAtt = () => GetAttribute(platNode, var);

        var = "border";
        int border = Int32.Parse(GetCh());

        switch (type)
        {
            case ("numbAndSpace"):

                var = "numb";
                int numb = Int32.Parse(GetCh());

                var = "spaceBetween";
                int spaceBetween = Int32.Parse(GetCh());

                return new PlateauFixedNumbAndSpace(numb, spaceBetween, border);

            case ("widthAndSpace"):

                var = "width";
                int width = Int32.Parse(GetCh());

                var = "spaceBetween";
                int spaceBetween2 = Int32.Parse(GetCh());

                return new PlateauFixedWidthAndSpace(width, spaceBetween2, border);

            default: throw new ArgumentException("Not a valid plateau generation method.");
        }
    }
}