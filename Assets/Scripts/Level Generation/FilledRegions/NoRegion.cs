using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class NoRegion : FilledRegion
{
    public NoRegion(Region covering)
        : base(covering)
    {

    }

    public override string ToString()
    {
        return "N/A";
    }
}
