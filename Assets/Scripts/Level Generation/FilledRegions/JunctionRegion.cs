using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Covers a tunnel junction from a Roguelike generator.
/// </summary>
public class JunctionRegion : FilledRegion
{
    public JunctionRegion(Region covering)
        : base(covering) { }

    public override string ToString()
    {
        return "Junction";
    }
}