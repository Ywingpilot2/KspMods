using SteelLanguage.Exceptions;
using SteelLanguage.Reflection.Type;

namespace SteelLanguage.Token.Terms;

public record TermHolder(string Type)
{
    public string Name { get; set; }
    public string Type { get; } = Type;

    private BaseTerm _term;

    public void SetTerm(BaseTerm term)
    {
        TermType type = term.GetTermType();
        if (type.Name != Type && !type.IsSubclassOf(Type))
            throw new InvalidAssignmentException(0, Name, Type);

        term.Name = Name;
        _term = term;
    }

    public BaseTerm GetTerm() => _term;

    public TermHolder(BaseTerm term) : this(term, term.ValueType)
    {
    }

    public TermHolder(BaseTerm term, string type) : this(type)
    {
        Name = term.Name;
        SetTerm(term);
    }
}