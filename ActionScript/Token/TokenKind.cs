namespace ActionScript.Token;

public enum TokenKind
{
    Constant = 0,
    Term = 1,
    Function = 2,
    SpecialFunc = 3,
    Operator = 4
}

public enum SpecialFuncKind
{
    Not = 0,
    Comparison = 1,
    As = 2,
}