using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using ActionScript.Exceptions;
using ActionScript.Extensions;
using ActionScript.Library;
using ActionScript.Token;
using ActionScript.Token.Functions;
using ActionScript.Token.Interaction;
using ActionScript.Token.Terms;

namespace ActionScript.Utils;

public enum ConditionType
{
    Or = 0,
    And = 1
}

public enum ComparisonType
{
    Equal = 0,
    NotEqual = 1,
    Greater = 2,
    GreaterEqual = 3,
    Lesser = 4,
    LesserEqual = 5
}

public enum AssignmentKind
{
    Constant = 0,
    Term = 1,
    Assignment = 2,
    Function = 3,
}

public static class CompileUtils
{
    public static readonly string[] InvalidNames = 
    {
        "|",
        "&",
        "!",
        "=",
        "!=",
        ")",
        "==",
        "<",
        ">",
        "<=",
        ">="
    };

    public static string[] GetTermValues(string token)
    {
        string[] split = token.SanitizedSplit('=', 2);
        string[] typeName = split[0].Trim().Split(' ');

        if (typeName.Length == 2)
        {
            string[] term = new string[3];
            term.SetValue(typeName[0].Trim(), 0);
            term.SetValue(typeName[1].Trim(), 1);
            term.SetValue(split[1].Trim(), 2);

            return term;
        }
        else
        {
            string[] term = new string[2];
            term.SetValue(split[0].Trim(), 0);
            term.SetValue(split[1].Trim(), 1);

            return term;
        }
    }

    public static AssignmentKind DetermineAssignment(string token, ITokenHolder holder)
    {
        string[] split = token.SanitizedSplit('=', 2);
        string[] typeName = split[0].Trim().Split(' ');
        
        if (typeName.Length == 1) // This is assigning an existing variable
        {
            if (!holder.HasTerm(typeName[0]))
                throw new TermNotExistException(0, typeName[0]);

            return AssignmentKind.Assignment;
        }
        
        string name = typeName[1].Trim();
        if (InvalidNames.Any(s => name.Contains(s)))
            throw new InvalidCompilationException(0, $"Cannot name a term {typeName[1]} as it contains operators");

        string type = typeName[0].Trim();
        string value = split[1].Trim();
        if (value.SanitizedContains("&") || value.SanitizedContains("|") || value.SanitizedContains("==") || value.SanitizedContains("!=")
            || value.SanitizedContains("<=") || value.SanitizedContains(">=") || value.StartsWith("!"))
        {
            if (type != "bool")
                throw new InvalidCompilationException(0, $"Cannot use boolean operations on type {type}");

            return AssignmentKind.Function;
        }

        if (value.SanitizedContains(" as "))
        {
            return AssignmentKind.Function;
        }
        
        if (value.EndsWith(")"))
        {
            return AssignmentKind.Function;
        }

        if (holder.HasTerm(value))
        {
            return AssignmentKind.Term;
        }
        
        return AssignmentKind.Constant;
    }

    public static Input HandleToken(string input, string expectedType, ITokenHolder holder, ActionCompiler compiler)
    {
        if (input.SanitizedContains("&"))
        {
            if (expectedType != "bool")
            {
                TermType bType = holder.GetTermType("bool");
                if (!bType.CanImplicitCastTo(expectedType) && !bType.IsSubclassOf(expectedType))
                    throw new InvalidParametersException(compiler.CurrentLine);
            }
            
            FunctionCall call = HandleConditionOperation(input, ConditionType.And, holder, compiler);
            return new Input(holder, call);
        }

        if (input.SanitizedContains("|"))
        {
            if (expectedType != "bool")
            {
                TermType bType = holder.GetTermType("bool");
                if (!bType.CanImplicitCastTo(expectedType) && !bType.IsSubclassOf(expectedType))
                    throw new InvalidParametersException(compiler.CurrentLine);
            }
            
            FunctionCall call = HandleConditionOperation(input, ConditionType.Or, holder, compiler);
            return new Input(holder, call);
        }

        if (input.SanitizedContains("!="))
        {
            if (expectedType != "bool")
            {
                TermType bType = holder.GetTermType("bool");
                if (!bType.CanImplicitCastTo(expectedType) && !bType.IsSubclassOf(expectedType))
                    throw new InvalidParametersException(compiler.CurrentLine);
            }

            return new Input(holder, HandleComparisonOperation(input, ComparisonType.NotEqual, holder, compiler));
        }
        
        if (input.SanitizedContains("=="))
        {
            if (expectedType != "bool")
            {
                TermType bType = holder.GetTermType("bool");
                if (!bType.CanImplicitCastTo(expectedType) && !bType.IsSubclassOf(expectedType))
                    throw new InvalidParametersException(compiler.CurrentLine);
            }

            return new Input(holder, HandleComparisonOperation(input, ComparisonType.Equal, holder, compiler));
        }
        
        if (input.SanitizedContains(">="))
        {
            if (expectedType != "bool")
            {
                TermType bType = holder.GetTermType("bool");
                if (!bType.CanImplicitCastTo(expectedType) && !bType.IsSubclassOf(expectedType))
                    throw new InvalidParametersException(compiler.CurrentLine);
            }

            return new Input(holder, HandleComparisonOperation(input, ComparisonType.GreaterEqual, holder, compiler));
        }
        
        if (input.SanitizedContains("<="))
        {
            if (expectedType != "bool")
            {
                TermType bType = holder.GetTermType("bool");
                if (!bType.CanImplicitCastTo(expectedType) && !bType.IsSubclassOf(expectedType))
                    throw new InvalidParametersException(compiler.CurrentLine);
            }

            return new Input(holder, HandleComparisonOperation(input, ComparisonType.LesserEqual, holder, compiler));
        }
        
        if (input.SanitizedContains(">"))
        {
            if (expectedType != "bool")
            {
                TermType bType = holder.GetTermType("bool");
                if (!bType.CanImplicitCastTo(expectedType) && !bType.IsSubclassOf(expectedType))
                    throw new InvalidParametersException(compiler.CurrentLine);
            }

            return new Input(holder, HandleComparisonOperation(input, ComparisonType.Greater, holder, compiler));
        }
        
        if (input.SanitizedContains("<"))
        {
            if (expectedType != "bool")
            {
                TermType bType = holder.GetTermType("bool");
                if (!bType.CanImplicitCastTo(expectedType) && !bType.IsSubclassOf(expectedType))
                    throw new InvalidParametersException(compiler.CurrentLine);
            }

            return new Input(holder, HandleComparisonOperation(input, ComparisonType.Lesser, holder, compiler));
        }

        if (input.SanitizedContains(" as "))
        {
            string[] split = input.SanitizeQuotes().Split(new[] { " as " }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length != 2)
                throw new InvalidParametersException(compiler.CurrentLine, new[] { "base-term","string" });

            TermType type = holder.GetTermType(split[1].Trim());
            Input convert = HandleToken(split[0].Trim(), "term", holder, compiler);
            CastCall castCall = new CastCall(holder, compiler.CurrentLine, convert, type.Name);
            return new Input(holder, castCall);
        }
        
        if (input.StartsWith("!"))
        {
            if (expectedType != "bool")
            {
                TermType bType = holder.GetTermType("bool");
                if (!bType.CanImplicitCastTo(expectedType) && !bType.IsSubclassOf(expectedType))
                    throw new InvalidParametersException(compiler.CurrentLine);
            }

            FunctionCall call = new FunctionCall(holder, holder.GetFunction("not"), compiler.CurrentLine, HandleToken(input.Remove(0,1), "bool", holder, compiler));
            return new Input(holder, call);
        }

        if (input.EndsWith(")"))
        {
            FunctionCall call = compiler.ParseFunctionCall(input, holder);

            if (call.Function.ReturnType != expectedType)
            {
                TermType type = holder.GetTermType(call.Function.ReturnType);
                if (!type.CanImplicitCastTo(expectedType) && !type.IsSubclassOf(expectedType))
                    throw new InvalidParametersException(compiler.CurrentLine, call.Function.InputTypes);
            }

            return new Input(holder, call);
        }

        if (!holder.HasTerm(input))
        {
            TermType type;
            if (expectedType == "term") // we are gonna try to parse it as a literal
            {
                // TODO: We should put this into its own class or something
                type = GetTypeFromConstant(input, compiler);
            }
            else
            {
                type = compiler.GetTermType(expectedType);
            }
                
            BaseTerm term = type.Construct(Guid.NewGuid().ToString(), compiler.CurrentLine);
            if (!term.Parse(input))
                throw new InvalidCompilationException(compiler.CurrentLine,
                    "Unable to parse specified value, either this constant is of the incorrect type or the value cannot be parsed");
                
            return new Input(term);
        }
        else
        {
            BaseTerm term = holder.GetTerm(input);
            if (term.ValueType != expectedType)
            {
                if (!term.CanImplicitCastToType(expectedType) && !term.GetTermType().IsSubclassOf(expectedType))
                    throw new InvalidParametersException(compiler.CurrentLine);
            }
            return new Input(term);
        }
    }

    public static FunctionCall HandleConditionOperation(string token, ConditionType type, ITokenHolder holder, ActionCompiler compiler)
    {
        if (type == ConditionType.And)
        {
            string[] split = token.SanitizedSplit('&', 2);
            Input a = HandleToken(split[0].Trim(), "bool", holder, compiler);
            Input b = HandleToken(split[1].Trim(), "bool", holder, compiler);

            return new FunctionCall(holder, holder.GetFunction("and"), compiler.CurrentLine, a, b);
        }
        else
        {
            string[] split = token.SanitizedSplit('|', 2);
            Input a = HandleToken(split[0].Trim(), "bool", holder, compiler);
            Input b = HandleToken(split[1].Trim(), "bool", holder, compiler);

            return new FunctionCall(holder, holder.GetFunction("or"), compiler.CurrentLine, a, b);
        }
    }

    public static FunctionCall HandleComparisonOperation(string token, ComparisonType type, ITokenHolder holder,
        ActionCompiler compiler)
    {
        string sanitized = token.SanitizeQuotes();
        string callName;
        string comparison;
        switch (type)
        {
            case ComparisonType.Equal:
            {
                callName = "equal";
                comparison = "==";
            } break;
            case ComparisonType.NotEqual:
            {
                callName = "not-equal";
                comparison = "!=";
            } break;
            case ComparisonType.Greater:
            {
                callName = "greater";
                comparison = ">";
            } break;
            case ComparisonType.GreaterEqual:
            {
                callName = "greater-equal";
                comparison = ">=";
            } break;
            case ComparisonType.Lesser:
            {
                callName = "lesser";
                comparison = "<";
            } break;
            case ComparisonType.LesserEqual:
            {
                callName = "lesser-equal";
                comparison = "<=";
            } break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, "Comparison operation was invalid");
        }
        
        string[] split = sanitized.Split(new[] { comparison }, 2, StringSplitOptions.RemoveEmptyEntries);
        if (split.Length != 2)
            throw new InvalidParametersException(compiler.CurrentLine, new []{"term","term"});

        FunctionCall call = new FunctionCall(holder, compiler.GetFunction(callName), compiler.CurrentLine,
            HandleToken(split[0].Trim(), "term", holder, compiler),
            HandleToken(split[1].Trim(), "term", holder, compiler));
        return call;
    }

    public static TermType GetTypeFromConstant(string token, ITokenHolder holder)
    {
        if (token.StartsWith("\"") && token.EndsWith("\"")) // is a string
        {
            return holder.GetTermType("string");
        }
        else if (int.TryParse(token, out _))
        {
            return holder.GetTermType("int");
        }
        else if (float.TryParse(token, out _))
        {
            return holder.GetTermType("float");
        }
        else if (bool.TryParse(token, out _))
        {
            return holder.GetTermType("bool");
        }
        else
        {
            throw new InvalidCompilationException(0, "Unable to parse specified value, either this constant is of the incorrect type or the value cannot be parsed");
        }
    }
}