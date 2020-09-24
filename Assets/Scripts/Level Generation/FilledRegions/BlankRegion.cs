using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class BlankRegion : FilledRegion
{
    public BlankRegion(Region covering)
        : base(covering)
    {

    }

    public override string ToString()
    {
        return "Blank";
    }
}