using System;

namespace ActionLanguage.Token;

public enum TokenKind
{
    Invalid = -1,
    Constant = 0,
    Term = 1,
    Function = 2,
    LocalFunc = 3,
    LocalField = 4,
    SpecialFunc = 5,
    Operator = 6
}

public enum OperatorKind
{
    Math = 0,

    Comparison = 1, 
    Bool = 2
}

public enum SpecialFuncKind
{
    Not = 0,
    As = 2,
    New = 3
}