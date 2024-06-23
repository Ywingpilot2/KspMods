using System;
using System.Collections.Generic;
using System.Linq;
using ActionLanguage.Exceptions;
using ActionLanguage.Extensions;
using ActionLanguage.Library;
using ActionLanguage.Token;
using ActionLanguage.Token.Functions;
using ActionLanguage.Token.Interaction;
using ActionLanguage.Token.Terms;

namespace ActionLanguage.Utils;

public enum ComparisonType
{
    Equal = 0,
    NotEqual = 1,
    Greater = 2,
    GreaterEqual = 3,
    Lesser = 4,
    LesserEqual = 5
}

public enum OperatorKind
{
    And = 0,
    Or = 1,
    Add = 2,
    Subtract = 3,
    Multiply = 4,
    Divide = 5,
    Power = 6,
    Remaining = 7
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
    #region operator stuff

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

    public static readonly string[] Comparisons =
    {
        "==",
        "!=",
        "<",
        ">",
        "<=",
        ">="
    };

    public static readonly string[] Operators =
    {
        "|",
        "&",
        "+",
        "-",
        "/",
        "*",
        "^",
        "%"
    };

    #endregion

    #region Terms

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
        TokenKind kind = GetTokenKind(split[1].Trim(), holder);
        return kind switch
        {
            TokenKind.Constant => AssignmentKind.Constant,
            TokenKind.Term => AssignmentKind.Term,
            TokenKind.LocalField => AssignmentKind.Function,
            TokenKind.Function => AssignmentKind.Function,
            TokenKind.LocalFunc => AssignmentKind.Function,
            TokenKind.SpecialFunc => AssignmentKind.Function,
            TokenKind.Operator => AssignmentKind.Function,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    #endregion

    #region Handling
    
    public static Input HandleToken(string token, string expectedType, ITokenHolder holder, ActionCompiler compiler)
    {
        TokenKind kind = GetTokenKind(token, holder);
        TermType type = GetTypeFromToken(token, holder, kind);
        if (type.Name != expectedType && !type.CanImplicitCastTo(expectedType) && !type.IsSubclassOf(expectedType))
            throw new InvalidTermCastException(compiler.CurrentLine, type.Name, expectedType);
        
        switch (kind)
        {
            case TokenKind.Constant:
            {
                BaseTerm term = type.Construct(Guid.NewGuid().ToString(), compiler.CurrentLine, holder.GetLibraryManager());
                term.Parse(token);
                
                return new Input(term);
            }
            case TokenKind.Term:
            {
                return new Input(holder.GetTerm(token));
            }
            case TokenKind.LocalFunc:
            case TokenKind.Function:
            {
                TokenCall call = compiler.ParseFunctionCall(token, holder);
                return new Input(holder, call);
            }
            case TokenKind.SpecialFunc:
            {
                SpecialFuncKind specialKind = GetSpecialKind(token);
                switch (specialKind)
                {
                    case SpecialFuncKind.Not:
                    {
                        FunctionCall call = new FunctionCall(holder, holder.GetFunction("not"), compiler.CurrentLine, HandleToken(token.Remove(0,1), "bool", holder, compiler));
                        return new Input(holder, call);
                    }
                    case SpecialFuncKind.Comparison:
                    {
                        FunctionCall call = HandleComparisonOperation(token, GetComparisonFromToken(token), holder, compiler);
                        return new Input(holder, call);
                    }
                    case SpecialFuncKind.As:
                    {
                        string san = token.SanitizeQuotes();
                        string[] split = san.Split(new[] { " as " }, 2, StringSplitOptions.RemoveEmptyEntries);
                        if (split.Length != 2)
                            throw new InvalidParametersException(0, new[] { "term", "type" });

                        Input convert = HandleToken(split[0].Trim(), "term", holder, compiler);
                        CastCall call = new CastCall(holder, compiler.CurrentLine, convert, type.Name);
                        return new Input(holder, call);
                    }
                    case SpecialFuncKind.New:
                    {
                        string[] split = token.SanitizedSplit(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                        string[] typePrms = split[1].SanitizedSplit('(', 2);
                        string prms = typePrms[1].Trim(')'); // TODO: Trimming is expensive and wasteful!
                        List<string> inputTokens = compiler.ParseCallInputs(prms);

                        string[] types = new string[inputTokens.Count];
                        for (int i = 0; i < inputTokens.Count; i++)
                        {
                            types.SetValue(GetTypeFromToken(inputTokens[i], holder, GetTokenKind(inputTokens[i], holder)).Name, i);
                        }
                        
                        string sig = string.Join(" ", types);
                        if (!type.HasConstructor(sig))
                            throw new ConstructorNotFoundException(compiler.CurrentLine, sig);
                        
                        Input[] inputs = new Input[inputTokens.Count];
                        for (int i = 0; i < inputTokens.Count; i++)
                        {
                            inputs.SetValue(HandleToken(inputTokens[i], types[i], holder, compiler), i);
                        }

                        ConstructorCall call = new ConstructorCall(holder, compiler.CurrentLine, sig, type, inputs);
                        return new Input(holder, call);
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            case TokenKind.Operator:
            {
                OperatorKind op = GetOperator(token);
                if (!type.AllowedOperators.Contains(op))
                    throw new InvalidCompilationException(compiler.CurrentLine, $"Type {type.Name} does not support ");

                return new Input(holder, HandleOperation(token, op, holder, compiler, type.Name));
            }
            case TokenKind.LocalField:
            {
                string[] split = token.SanitizedSplit('.', 2, StringSplitOptions.RemoveEmptyEntries, ScanDirection.RightToLeft);
                string termToken = split[0].Trim();
                TermType termType = GetTypeFromToken(termToken, holder, GetTokenKind(termToken, holder));
                Input input = HandleToken(termToken, termType.Name, holder, compiler);
                FieldCall call = new FieldCall(holder, compiler.CurrentLine, input, split[1].Trim());
                return new Input(holder, call);
            }
            default:
                throw new ArgumentOutOfRangeException();
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
                callName = "not_equal";
                comparison = "!=";
            } break;
            case ComparisonType.Greater:
            {
                callName = "greater";
                comparison = ">";
            } break;
            case ComparisonType.GreaterEqual:
            {
                callName = "greater_equal";
                comparison = ">=";
            } break;
            case ComparisonType.Lesser:
            {
                callName = "lesser";
                comparison = "<";
            } break;
            case ComparisonType.LesserEqual:
            {
                callName = "lesser_equal";
                comparison = "<=";
            } break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, "Comparison operation was invalid");
        }

        string[] split = token.SplitAt(sanitized.IndexOf(comparison, StringComparison.Ordinal), 2);
        if (split.Length != 2)
            throw new InvalidParametersException(compiler.CurrentLine, new []{"term","term"});

        FunctionCall call = new FunctionCall(holder, holder.GetFunction(callName), compiler.CurrentLine,
            HandleToken(split[0].Remove(split[0].Length - 1).Trim(), "term", holder, compiler),
            HandleToken(split[1].Remove(0,1).Trim(), "term", holder, compiler));
        return call;
    }

    public static OperatorCall HandleOperation(string token, OperatorKind kind, ITokenHolder holder,
        ActionCompiler compiler, string expectedType)
    {
        char splitter = GetOperatorChar(kind);
        string[] split = token.SanitizedSplit(splitter, 2, StringSplitOptions.RemoveEmptyEntries);
        if (split.Length == 1)
            throw new InvalidCompilationException(compiler.CurrentLine, $"{token} lacks a second operator argument!");

        Input a = HandleToken(split[0].Trim(), expectedType, holder, compiler);
        Input b = HandleToken(split[1].Trim(), expectedType, holder, compiler);
        return new OperatorCall(holder, compiler.CurrentLine, a, b, kind);
    }

    #endregion

    #region Type Solving

    public static TermType GetTypeFromField(string token, ITokenHolder holder)
    {
        string san = token.SanitizeQuotes();
        string[] split = san.SanitizedSplit('.', 2, StringSplitOptions.RemoveEmptyEntries, ScanDirection.RightToLeft);
        TermType type = GetTypeFromToken(split[0].Trim(), holder, GetTokenKind(split[0].Trim(), holder));

        string typeName = type.GetField(split[1].Trim()).Value.Type;
        return holder.GetLibraryManager().GetTermType(typeName);
    }

    public static TermType GetTypeFromConstant(string token, ITokenHolder holder)
    {
        LibraryManager manager = holder.GetLibraryManager();
        
        if (token.StartsWith("\"") && token.EndsWith("\"")) // is a string
        {
            return manager.GetTermType("string");
        }
        else if (int.TryParse(token, out _))
        {
            return manager.GetTermType("int");
        }
        else if (float.TryParse(token, out _))
        {
            return manager.GetTermType("float");
        }
        else if (bool.TryParse(token, out _))
        {
            return manager.GetTermType("bool");
        }
        else
        {
            throw new InvalidCompilationException(0, "Unable to parse specified value, either this constant is of the incorrect type or the value cannot be parsed");
        }
    }

    public static TermType GetTypeFromTerm(string token, ITokenHolder holder)
    {
        BaseTerm term = holder.GetTerm(token);
        return term.GetTermType();
    }

    public static TermType GetTypeFromFunction(string token, ITokenHolder holder)
    {
        string name = token.SanitizedSplit('(', 2, StringSplitOptions.RemoveEmptyEntries)[0];
        IFunction function = holder.GetFunction(name);
        
        return holder.GetLibraryManager().GetTermType(function.ReturnType);
    }

    public static TermType GetTypeFromLocalFunc(string token, ITokenHolder holder)
    {
        string[] split = token.SanitizedSplit('.', 2, StringSplitOptions.RemoveEmptyEntries);

        string termToken = split[0].Trim();
        TermType termType = GetTypeFromToken(termToken, holder, GetTokenKind(termToken, holder));

        string funcName = split[1].SanitizedSplit('(', 2)[0].Trim();
        IFunction func = termType.GetFunction(funcName);
        return holder.GetLibraryManager().GetTermType(func.ReturnType);
    }

    #endregion

    public static TermType GetTypeFromToken(string token, ITokenHolder holder, TokenKind kind)
    {
        switch (kind)
        {
            case TokenKind.Constant:
            {
                return GetTypeFromConstant(token, holder);
            }
            case TokenKind.Term:
            {
                return GetTypeFromTerm(token, holder);
            }
            case TokenKind.Function:
            {
                return GetTypeFromFunction(token, holder);
            }
            case TokenKind.LocalFunc:
            {
                return GetTypeFromLocalFunc(token, holder);
            }
            case TokenKind.SpecialFunc:
            {
                SpecialFuncKind specialKind = GetSpecialKind(token);
                switch (specialKind)
                {
                    case SpecialFuncKind.Not:
                    case SpecialFuncKind.Comparison:
                    {
                        return holder.GetLibraryManager().GetTermType("bool");
                    }
                    case SpecialFuncKind.As:
                    {
                        string san = token.SanitizeQuotes();
                        string[] split = san.Split(new[] { " as " }, 2, StringSplitOptions.RemoveEmptyEntries);
                        if (split.Length != 2)
                            throw new InvalidParametersException(0, new[] { "term", "type" });

                        TermType type = holder.GetLibraryManager().GetTermType(split[1].Trim());
                        return type;
                    }
                    case SpecialFuncKind.New:
                    {
                        string[] split = token.SanitizedSplit(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                        if (split.Length != 2)
                            throw new InvalidParametersException(0, new[] { "type" });

                        string typeName = split[1].Trim().SanitizedSplit('(', 2, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
                        TermType type = holder.GetLibraryManager().GetTermType(typeName);
                        return type;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            case TokenKind.Operator:
            {
                OperatorKind operatorKind = GetOperator(token);
                char split = GetOperatorChar(operatorKind);
                string opToken = token.SanitizedSplit(split, 2, StringSplitOptions.RemoveEmptyEntries)[0].Trim();

                TokenKind opToKind = GetTokenKind(opToken, holder);
                return GetTypeFromToken(opToken, holder, opToKind);
            }
            case TokenKind.LocalField:
            {
                return GetTypeFromField(token, holder);
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
        }
    }

    #region Operator

    public static OperatorKind GetOperator(string token)
    {
        string san = token.SanitizeQuotes();
        string op = Operators.FirstOrDefault(san.Contains);
        return op switch
        {
            "|" => OperatorKind.Or,
            "&" => OperatorKind.And,
            "+" => OperatorKind.Add,
            "-" => OperatorKind.Subtract,
            "/" => OperatorKind.Divide,
            "*" => OperatorKind.Multiply,
            "^" => OperatorKind.Power,
            "%" => OperatorKind.Remaining,
            _ => throw new InvalidCompilationException(0, $"Operator in token {token} is invalid")
        };
    }

    public static char GetOperatorChar(OperatorKind kind)
    {
        return kind switch
        {
            OperatorKind.And => '&',
            OperatorKind.Or => '|',
            OperatorKind.Add => '+',
            OperatorKind.Subtract => '-',
            OperatorKind.Multiply => '*',
            OperatorKind.Divide => '/',
            OperatorKind.Power => '^',
            OperatorKind.Remaining => '%',
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
    }

    #endregion

    #region Comparison

    public static ComparisonType GetComparisonFromToken(string token)
    {
        string san = token.SanitizeQuotes();
        string op = Comparisons.FirstOrDefault(s => san.Contains(s));
        switch (op)
        {
            case "==":
            {
                return ComparisonType.Equal;
            }
            case "!=":
            {
                return ComparisonType.NotEqual;
            }
            case ">":
            {
                return ComparisonType.Greater;
            }
            case ">=":
            {
                return ComparisonType.GreaterEqual;
            }
            case "<":
            {
                return ComparisonType.Lesser;
            }
            case "<=":
            {
                return ComparisonType.LesserEqual;
            } 
        }

        throw new InvalidCompilationException(0, $"Unable to determine comparison operator from {token}");
    }

    #endregion

    #region Kind Solving

    public static SpecialFuncKind GetSpecialKind(string token)
    {
        string san = token.SanitizeQuotes();

        if (san.StartsWith("!"))
            return SpecialFuncKind.Not;

        if (Comparisons.Any(s => san.SanitizeParenthesis().Contains($" {s} ")))
            return SpecialFuncKind.Comparison;

        if (san.Contains(" as "))
            return SpecialFuncKind.As;

        if (san.StartsWith("new "))
            return SpecialFuncKind.New;

        throw new InvalidCompilationException(0, $"Token {token} is not valid");
    }

    public static TokenKind GetTokenKind(string token, ITokenHolder holder)
    {
        string san = token.SanitizeQuotes();

        if (san.StartsWith("!"))
        {
            return TokenKind.SpecialFunc;
        }
        
        // there needs to be a space between the operator in order to prevent conflicts with type containers
        if (Operators.Any(s => san.SanitizeParenthesis().Contains($" {s} ")))
        {
            return TokenKind.Operator;
        }
        
        if (Comparisons.Any(s => san.SanitizeParenthesis().Contains($" {s} ")))
        {
            return TokenKind.SpecialFunc;
        }

        if (TokenIsConstant(token))
        {
            return TokenKind.Constant;
        }
        
        if (san.StartsWith("new "))
        {
            return TokenKind.SpecialFunc;
        }

        if (san.EndsWith(")"))
        {
            string noPrms = san.SanitizeParenthesis();
            if (noPrms.Contains('.'))
                return TokenKind.LocalFunc;
            else
                return TokenKind.Function;
        }

        if (san.Contains(" as "))
        {
            return TokenKind.SpecialFunc;
        }

        if (san.Contains('.'))
        {
            string[] split = san.SanitizedSplit('.', 2, StringSplitOptions.RemoveEmptyEntries, ScanDirection.RightToLeft);
            TokenKind inKind = GetTokenKind(split[0].Trim(), holder);
            if (inKind != TokenKind.Invalid)
                return TokenKind.LocalField;
        }

        if (holder.HasTerm(token))
        {
            return TokenKind.Term;
        }

        return TokenKind.Invalid;
    }

    public static bool TokenIsConstant(string token)
    {
        if (token.StartsWith("\"") && token.EndsWith("\"")) // is a string
        {
            return true;
        }
        else if (int.TryParse(token, out _))
        {
            return true;
        }
        else if (float.TryParse(token, out _))
        {
            return true;
        }
        else if (bool.TryParse(token, out _))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    #endregion
}