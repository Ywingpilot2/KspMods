namespace ActionScript.Token;

public enum TokenKind
{
    Invalid = -1,
    Constant = 0,
    Term = 1,
    Function = 2,
    LocalFunc = 3,
    SpecialFunc = 4,
    Operator = 5
}

public enum SpecialFuncKind
{
    Not = 0,
    Comparison = 1,
    As = 2
}