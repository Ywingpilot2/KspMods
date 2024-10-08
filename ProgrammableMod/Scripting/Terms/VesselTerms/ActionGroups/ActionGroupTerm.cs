﻿using SteelLanguage.Library.System.Terms.Complex;

namespace ProgrammableMod.Scripting.Terms.VesselTerms.ActionGroups;

internal class ActionGroupTerm : EnumTerm
{
    public override string ValueType => "action";

    protected override string[] Values => new[]
    {
        "gear",
        "light",
        "rcs",
        "sas",
        "brakes",
        "abort",
        "1",
        "2",
        "3",
        "4",
        "5",
        "6",
        "7",
        "8",
        "9",
        "10"
    };
}