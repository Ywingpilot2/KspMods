using System;
using System.Collections.Generic;
using SteelLanguage.Library.Collections.Terms;
using SteelLanguage.Library.System.Terms.Complex;
using SteelLanguage.Library.System.Terms.Complex.Enumerators;
using SteelLanguage.Library.System.Terms.Literal;
using SteelLanguage.Reflection.Library;
using SteelLanguage.Reflection.Type;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.KeyWords.Container;
using SteelLanguage.Token.KeyWords.Single;
using SteelLanguage.Token.Terms;
using SteelLanguage.Token.Terms.Technical;

namespace SteelLanguage.Library.System;

public class SystemLibrary : ILibrary
{
    public string Name => "system";

    public IEnumerable<IFunction> GlobalFunctions { get; } = new IFunction[]
    {
        new Function("equal", "bool", inputTypes: new[] { "term", "term" }, action: terms =>
        {
            object a = terms[0].GetValue();
            object b = terms[1].GetValue();

            if (a == null && b == null) // both values are null therefore technically equal
            {
                return new ReturnValue(true, "bool");
            }

            return new ReturnValue(Equals(a, b), "bool");
        }),
        new Function("to_string", "string", inputTypes: "term", action: terms => new ReturnValue(TermToString(terms[0]), "string")),
        new Function("concat", "string", terms =>
        {
            string str = terms[0].CastToStr();
            TermArray array = (TermArray)terms[1].GetValue();

            for (int i = 0; i < array.Length; i++)
            {
                BaseTerm term = array.GetValue(i);

                str = str.Replace($"{{{i}}}", TermToString(term));
            }

            return new ReturnValue(str, "string");
        }, "string", "params term"),
        // TODO: Remove this! This should instead just be an operator
        new Function("not", "bool", inputTypes:new []{"bool"}, action: terms => new ReturnValue(!terms[0].CastToBool(), "bool"))
    };

    public static string TermToString(BaseTerm term)
    {
        TermType strType = SteelCompiler.Library.TypeLibrary.GetTermType("string");
        if (term.CanImplicitCastToType(strType))
        {
            return term.CastToStr();
        }

        try
        {
            string str = term.GetValue().ToString();
            if (str == term.GetValue().GetType().FullName)
                return term.GetTermType().Name;

            return str;
        }
        catch (Exception)
        {
            return term.GetTermType().Name;
        }
    }

    public IEnumerable<GlobalTerm> GlobalTerms => new[]
    {
        new GlobalTerm("NULL", "null-type")
    };

    public IEnumerable<IKeyword> Keywords => new IKeyword[]
    {
        new FuncKeyword(),
        new WhileKeyword(),
        new BreakKeyword(),
        new ContinueKeyword(),
        new ReturnKeyword(),
        new ThrowKeyword(),
        new IfKeyword(),
        new ElseIfKeyword(),
        new ElseKeyword(),
        new ForeachKeyword(),
        new ImportKeyword(),
        new SwitchKeyword(),
        new MatchKeyword()
    };
    public TypeLibrary TypeLibrary { get; }
    
    public SystemLibrary()
    {
        TypeLibrary = new TypeLibrary();

        TermType baseType = new TermType(new Term(), isAbstract:true);
        TermType numberType = new TermType(new NumberTerm(), baseType, true);
        TermType enumerableType = new TermType(new EnumeratorTerm(), baseType, true);
        TermType enumType = new TermType(new EnumTerm(), baseType, true);
        TermType collectionType = new TermType(new CollectionTerm(), enumerableType, true, true);
        
        TypeLibrary.AddTermType(baseType);
        TypeLibrary.AddTermType(numberType);
        TypeLibrary.AddTermType(enumerableType);
        TypeLibrary.AddTermType(enumType);
        TypeLibrary.AddTermType(collectionType);
        TypeLibrary.AddTermType(new TermType(new VoidTerm(), isAbstract:true));
        TypeLibrary.AddTermType(new TermType(new TermI(), numberType));
        TypeLibrary.AddTermType(new TermType(new TermU(), numberType));
        TypeLibrary.AddTermType(new TermType(new TermF(), numberType));
        TypeLibrary.AddTermType(new TermType(new TermD(), numberType));
        TypeLibrary.AddTermType(new TermType(new StringTerm(), baseType, isNullable:true));
        TypeLibrary.AddTermType(new TermType(new BoolTerm(), baseType));
        TypeLibrary.AddTermType(new TermType(new ArrayTerm(), enumerableType, isNullable:true));
        TypeLibrary.AddTermType(new TermType(new NullTerm(), baseType, isNullable:true));
        TypeLibrary.AddTermType(new TermType(new ListTerm(), collectionType, isNullable:true));
        TypeLibrary.AddTermType(new TermType(new FuncEnumeratorTerm(), enumerableType, isNullable:true));
    }
}