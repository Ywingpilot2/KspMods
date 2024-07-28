﻿using SteelLanguage.Library.System.Terms.Complex;

namespace ProgrammableMod.Scripting.Terms.Vessel.SAS;

public class SASTypeTerm : EnumTerm
{
    public override string ValueType => "sas_mode";

    protected override string[] Values => new[]
    {
        "stability",
        "prograde",
        "retrograde",
        "normal",
        "antinormal",
        "radialin",
        "radialout",
        "target",
        "antitarget",
        "maneuver"
    };
}