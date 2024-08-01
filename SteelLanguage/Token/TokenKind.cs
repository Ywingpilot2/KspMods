namespace SteelLanguage.Token;

public enum TokenKind
{
    Invalid = 0,
    Constant = 1,
    Term = 2,
    Function = 3,
    LocalFunc = 4,
    LocalField = 5,
    SpecialFunc = 6,
    Operator = 7,
    Type = 8,
    StaticField = 9
}

public enum OperatorKind
{
    Math = 0,
    Comparison = 1, 
    Bool = 2,
    Indexer = 3
}

public enum SpecialFuncKind
{
    Not = 0,
    As = 2,
    New = 3
}