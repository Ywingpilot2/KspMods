using System;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;

namespace SteelLanguage.Token.Functions.Operator;

public class IndexerCall : TokenCall
{
    private readonly Input _input;
    private readonly Input _indexer;

    public override ReturnValue Call()
    {
        BaseTerm term = _input.GetValue();
        return term.ConductIndexingOperation(_indexer.GetValue());
    }
    
    public IndexerCall(ITokenHolder container, int line, Input input, Input indexer) : base(container, line)
    {
        _input = input;
        _indexer = indexer;
    }
}