﻿using SteelLanguage.Exceptions;

namespace SteelLanguage.Token.Terms.Technical;

/// <summary>
/// Represents the base class for a <see cref="BaseTerm"/>
/// TODO: This should also be used for classes
/// </summary>
public class Term : BaseTerm
{
    public override string ValueType => "term";
    public override bool Parse(string value)
    {
        throw new System.NotImplementedException();
    }

    public override bool SetValue(object value)
    {
        throw new System.NotImplementedException();
    }

    public override bool CopyFrom(BaseTerm term)
    {
        throw new System.NotImplementedException();
    }

    public override object GetValue()
    {
        throw new TypeNotConstructableException(0, "term");
    }
}