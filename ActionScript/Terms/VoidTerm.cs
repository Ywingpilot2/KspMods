﻿using System.Collections.Generic;
using ActionScript.Exceptions;
using ActionScript.Functions;

namespace ActionScript.Terms;

/// <summary>
/// used by method calls to signal that there is no return value
/// </summary>
public class VoidTerm : BaseTerm
{
    public override string ValueType => "void";
    public override IEnumerable<IFunction> Functions { get; }
    public override bool Parse(string value)
    {
        throw new TypeNotConstructableException(0, ValueType);
    }

    public override bool SetValue(object value)
    {
        throw new TypeNotConstructableException(0, ValueType);
    }

    public override bool CopyFrom(BaseTerm term)
    {
        throw new TypeNotConstructableException(0, ValueType);
    }
    
    public override object GetValue()
    {
        throw new TypeNotConstructableException(0, "term");
    }
}