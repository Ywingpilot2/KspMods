using System;
using System.Collections.Generic;
using System.Linq;
using ActionLanguage.Exceptions;
using ActionLanguage.Extensions;
using ActionLanguage.Library;
using ActionLanguage.Reflection;
using ActionLanguage.Token;
using ActionLanguage.Token.Functions;
using ActionLanguage.Token.Functions.Modifier;
using ActionLanguage.Token.Functions.Operator;
using ActionLanguage.Token.Functions.Single;
using ActionLanguage.Token.Interaction;
using ActionLanguage.Token.Terms;

namespace ActionLanguage.Utils;

public enum BoolOperatorKind
{
    And = 0,
    Or = 1
}

public enum ComparisonOperatorKind
{
    Equal = 0,
    NotEqual = 1,
    Greater = 2,
    GreaterEqual = 3,
    Lesser = 4,
    LesserEqual = 5
}

public enum MathOperatorKind
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

    private static readonly string[] InvalidNames = 
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
        ">=",
        "&&",
        "||"
    };

    private static readonly string[] Comparisons =
    {
        "==",
        "!=",
        "<=",
        ">=",
        "<",
        ">",
    };

    private static readonly string[] MathOps =
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

    private static readonly string[] BoolOps =
    {
        "||",
        "&&"
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
                switch (GetOperatorKind(token))
                {
                    case OperatorKind.Math:
                    {
                        MathOperatorKind op = GetMathOp(token);
                        return new Input(holder, HandleMathOperation(token, op, holder, compiler, type.Name));
                    }
                    case OperatorKind.Comparison:
                    {
                        ComparisonOperatorKind op = GetComparisonFromToken(token);
                        return new Input(holder, HandleComparisonOperation(token, op, holder, compiler));
                    }
                    case OperatorKind.Bool:
                    {
                        BoolOperatorKind op = GetBoolOpFromToken(token);
                        return new Input(holder, HandleBoolOperation(token, op, holder, compiler));
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
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
    
    public static BoolOpCall HandleBoolOperation(string token, BoolOperatorKind operatorKind, ITokenHolder holder,
        ActionCompiler compiler)
    {
        string san = token.SanitizeQuotes().SanitizeParenthesis();
        string splitter = GetBoolOpStr(operatorKind);

        string[] split = token.SplitAt(san.IndexOf(splitter, StringComparison.Ordinal));

        string tokenA = split[0].Remove(split[0].Length - 1).Trim();
        string tokenB = split[1].Remove(0, 1).Trim();
        TermType typeA = GetTypeFromToken(tokenA, holder, GetTokenKind(tokenA, holder));

        if (!typeA.AllowedBoolOps.Contains(operatorKind))
            throw new InvalidCompilationException(compiler.CurrentLine, $"Type {typeA.Name} does not support {operatorKind}");

        Input a = HandleToken(tokenA, typeA.Name, holder, compiler);
        Input b = HandleToken(tokenB, typeA.Name, holder, compiler);
        return new BoolOpCall(holder, compiler.CurrentLine, a, b, operatorKind);
    }

    public static ComparisonCall HandleComparisonOperation(string token, ComparisonOperatorKind operatorKind, ITokenHolder holder,
        ActionCompiler compiler)
    {
        string san = token.SanitizeQuotes().SanitizeParenthesis();
        string splitter = GetComparisonStr(operatorKind);

        string[] split = token.SplitAt(san.IndexOf(splitter, StringComparison.Ordinal));

        string tokenA = split[0].Remove(split[0].Length - 1).Trim();
        string tokenB = split[1].Remove(0, 1).Trim();
        TermType typeA = GetTypeFromToken(tokenA, holder, GetTokenKind(tokenA, holder));

        if (!typeA.AllowedComparisons.Contains(operatorKind))
            throw new InvalidCompilationException(compiler.CurrentLine, $"Type {typeA.Name} does not support {operatorKind}");

        Input a = HandleToken(tokenA, typeA.Name, holder, compiler);
        Input b = HandleToken(tokenB, typeA.Name, holder, compiler);
        return new ComparisonCall(holder, compiler.CurrentLine, a, b, operatorKind);
    }

    public static MathCall HandleMathOperation(string token, MathOperatorKind kind, ITokenHolder holder,
        ActionCompiler compiler, string expectedType)
    {
        char splitter = GetMathChar(kind);
        string[] split = token.SanitizedSplit(splitter, 2, StringSplitOptions.RemoveEmptyEntries);
        if (split.Length == 1)
            throw new InvalidCompilationException(compiler.CurrentLine, $"{token} lacks a second operator argument!");

        Input a = HandleToken(split[0].Trim(), expectedType, holder, compiler);
        Input b = HandleToken(split[1].Trim(), expectedType, holder, compiler);
        return new MathCall(holder, compiler.CurrentLine, a, b, kind);
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
                switch (GetOperatorKind(token))
                {
                    case OperatorKind.Math:
                    {
                        MathOperatorKind mathOperatorKind = GetMathOp(token);
                        char split = GetMathChar(mathOperatorKind);
                        string opToken = token.SanitizedSplit(split, 2, StringSplitOptions.RemoveEmptyEntries)[0].Trim();

                        TokenKind opToKind = GetTokenKind(opToken, holder);
                        return GetTypeFromToken(opToken, holder, opToKind);
                    }
                    case OperatorKind.Comparison:
                    case OperatorKind.Bool:
                        return holder.GetLibraryManager().GetTermType("bool"); // TODO: Actually check the type lol
                    default:
                        throw new ArgumentOutOfRangeException();
                }
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

    #region Math

    public static MathOperatorKind GetMathOp(string token)
    {
        string san = token.SanitizeQuotes();
        
        // we do last or default because otherwise we may pickup on value negation
        // e.g -1 / 2 will be registered as subtraction because of the "-" sign
        string op = MathOps.LastOrDefault(san.Contains); 
        return op switch
        {
            "|" => MathOperatorKind.Or,
            "&" => MathOperatorKind.And,
            "+" => MathOperatorKind.Add,
            "-" => MathOperatorKind.Subtract,
            "/" => MathOperatorKind.Divide,
            "*" => MathOperatorKind.Multiply,
            "^" => MathOperatorKind.Power,
            "%" => MathOperatorKind.Remaining,
            _ => throw new InvalidCompilationException(0, $"Operator in token {token} is invalid")
        };
    }

    public static char GetMathChar(MathOperatorKind kind)
    {
        return kind switch
        {
            MathOperatorKind.And => '&',
            MathOperatorKind.Or => '|',
            MathOperatorKind.Add => '+',
            MathOperatorKind.Subtract => '-',
            MathOperatorKind.Multiply => '*',
            MathOperatorKind.Divide => '/',
            MathOperatorKind.Power => '^',
            MathOperatorKind.Remaining => '%',
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
    }

    #endregion

    #region Comparison

    public static ComparisonOperatorKind GetComparisonFromToken(string token)
    {
        string san = token.SanitizeQuotes();
        string op = Comparisons.FirstOrDefault(s => san.Contains(s));
        switch (op)
        {
            case "==":
            {
                return ComparisonOperatorKind.Equal;
            }
            case "!=":
            {
                return ComparisonOperatorKind.NotEqual;
            }
            case ">":
            {
                return ComparisonOperatorKind.Greater;
            }
            case ">=":
            {
                return ComparisonOperatorKind.GreaterEqual;
            }
            case "<":
            {
                return ComparisonOperatorKind.Lesser;
            }
            case "<=":
            {
                return ComparisonOperatorKind.LesserEqual;
            } 
        }

        throw new InvalidCompilationException(0, $"Unable to determine comparison operator from {token}");
    }

    public static string GetComparisonStr(ComparisonOperatorKind kind)
    {
        return kind switch
        {
            ComparisonOperatorKind.Equal => "==",
            ComparisonOperatorKind.NotEqual => "!=",
            ComparisonOperatorKind.Greater => ">",
            ComparisonOperatorKind.GreaterEqual => ">=",
            ComparisonOperatorKind.Lesser => "<",
            ComparisonOperatorKind.LesserEqual => "<=",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
    }

#endregion

    #region Boolean

    public static BoolOperatorKind GetBoolOpFromToken(string token)
    {
        string san = token.SanitizeQuotes();
        string op = BoolOps.FirstOrDefault(s => san.Contains(s));
        switch (op)
        {
            case "&&":
            {
                return BoolOperatorKind.And;
            }
            case "!||":
            {
                return BoolOperatorKind.Or;
            }
        }

        throw new InvalidCompilationException(0, $"Unable to determine comparison operator from {token}");
    }

    public static string GetBoolOpStr(BoolOperatorKind kind)
    {
        return kind switch
        {
            BoolOperatorKind.And => "&&",
            BoolOperatorKind.Or => "||",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
    }

    #endregion

    #endregion

    #region Kind Solving

    public static OperatorKind GetOperatorKind(string token)
    {
        string san = token.SanitizeQuotes();
        
        if (BoolOps.Any(s => san.SanitizeParenthesis().Contains($" {s} ")))
            return OperatorKind.Bool;
        
        if (Comparisons.Any(s => san.SanitizeParenthesis().Contains($" {s} ")))
            return OperatorKind.Comparison;

        // there needs to be a space between the operator in order to prevent conflicts with type containers
        if (MathOps.Any(s => san.SanitizeParenthesis().Contains($" {s} ")))
            return OperatorKind.Math;

        throw new InvalidCompilationException(0,"not a valid operator");
    }

    public static SpecialFuncKind GetSpecialKind(string token)
    {
        string san = token.SanitizeQuotes();

        if (san.StartsWith("!"))
            return SpecialFuncKind.Not;

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

        if (BoolOps.Any(s => san.SanitizeParenthesis().Contains($" {s} ")))
            return TokenKind.Operator;
        
        if (Comparisons.Any(s => san.SanitizeParenthesis().Contains($" {s} ")))
            return TokenKind.Operator;

        // there needs to be a space between the operator in order to prevent conflicts with type containers
        if (MathOps.Any(s => san.SanitizeParenthesis().Contains($" {s} ")))
            return TokenKind.Operator;

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