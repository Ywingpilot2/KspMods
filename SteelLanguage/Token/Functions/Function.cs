using System;
using System.Collections.Generic;
using System.Linq;
using SteelLanguage.Exceptions;
using SteelLanguage.Library.System.Terms.Complex.Enumerators;
using SteelLanguage.Reflection.Library;
using SteelLanguage.Reflection.Type;
using SteelLanguage.Token.Functions.Single;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;

namespace SteelLanguage.Token.Functions;

/// <summary>
/// Base implementation of a token capable of executing instructions
/// </summary>
public interface IExecutable
{
    /// <summary>
    /// Begin execution
    /// </summary>
    /// <param name="terms">Input terms given to this by a <see cref="TokenCall"/></param>
    /// <returns>A <see cref="ReturnValue"/>, <c>return new ReturnValue()</c> if void</returns>
    public ReturnValue Execute(params BaseTerm[] terms);
    /// <summary>
    /// Begin execution without any arguments
    /// </summary>
    /// <returns>A <see cref="ReturnValue"/>, <c>return new ReturnValue()</c> if void</returns>
    public ReturnValue Execute();

    /// <summary>
    /// Occurs just before <see cref="Execute(SteelLanguage.Token.Terms.BaseTerm[])"/> is called
    /// </summary>
    public void PreExecution();
    /// <summary>
    /// Occurs just after <see cref="Execute(SteelLanguage.Token.Terms.BaseTerm[])"/> is called
    /// </summary>
    public void PostExecution();
    /// <summary>
    /// Occurs when the <see cref="SteelCompiler"/> has finished compiling the <see cref="SteelScript"/>
    /// </summary>
    public void PostCompilation();
}

/// <summary>
/// Base implementation of a function which can be directly called through user code through a <see cref="FunctionCall"/>
/// </summary>
public interface IFunction : IExecutable
{
    /// <summary>
    /// The name of the function
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// The return type of the function
    /// </summary>
    public string ReturnType { get; }
    /// <summary>
    /// The types of this functions inputs
    /// </summary>
    public string[] InputTypes { get; }

    // TODO: this is a cool idea, why aren't we using it?
    // public bool InputIsValid(string type, int idx, ITokenHolder holder);
}

/// <inheritdoc />
/// <summary>
/// A function natively handled in C# which can be called by Steel code through a <see cref="T:SteelLanguage.Token.Functions.FunctionCall" />
/// </summary>
///
/// <seealso cref="UserFunction"/>
public record Function : IFunction
{
    private enum FunctionKind
    {
        InReturn,
        Return,
        YieldReturn,
        In,
        None
    }
    
    public string Name { get; }
    public string ReturnType { get; }
    public string[] InputTypes { get; }
    
    private Func<BaseTerm[], ReturnValue> InReturn { get; }
    private Func<ReturnValue> Return { get; }
    private readonly string _yieldType;
    private Func<IEnumerable<ReturnValue>> YieldReturn { get; }
    private Action<BaseTerm[]> In { get; }
    private Action None { get; }

    private readonly FunctionKind _kind;
    private static readonly BaseTerm[] Empty = new BaseTerm[0]; // array declaration is expensive!

    public ReturnValue Execute(params BaseTerm[] terms)
    {
        switch (_kind)
        {
            case FunctionKind.InReturn:
            {
                return InReturn.Invoke(terms);
            }
            case FunctionKind.Return:
            {
                return Return.Invoke();
            }
            case FunctionKind.YieldReturn:
            {
                return new ReturnValue(new FuncEnumerable(YieldReturn.Invoke(), _yieldType), "FuncEnumerator");
            }
            case FunctionKind.In:
            {
                In.Invoke(terms);
            } break;
            case FunctionKind.None:
            {
                None.Invoke();
            } break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return ReturnValue.None;
    }

    public ReturnValue Execute()
    {
        switch (_kind)
        {
            case FunctionKind.InReturn:
            {
                return InReturn.Invoke(Empty);
            }
            case FunctionKind.Return:
            {
                return Return.Invoke();
            }
            case FunctionKind.YieldReturn:
            {
                return new ReturnValue(new FuncEnumerable(YieldReturn.Invoke(), _yieldType), "FuncEnumerator");
            }
            case FunctionKind.In:
            {
                In.Invoke(Empty);
            } break;
            case FunctionKind.None:
            {
                None.Invoke();
            } break;
        }

        return ReturnValue.None;
    }

    #region Events

    public void PreExecution()
    {
    }

    public void PostExecution()
    {
    }

    public void PostCompilation()
    {
    }

    #endregion

    #region Construction

    private Function(string name, string returnType, string[] inputs)
    {
        Name = name;
        ReturnType = returnType;
        InputTypes = inputs;
    }

    /// <summary>
    /// Creates a function with inputs and a return value
    /// </summary>
    /// <param name="name">The name of the function</param>
    /// <param name="returnType">The name of the TermType the returned type belongs to</param>
    /// <param name="action">The action to execute</param>
    /// <param name="inputTypes">The TermType names of the inputted types</param>
    public Function(string name, string returnType, Func<BaseTerm[], ReturnValue> action, params string[] inputTypes) : this(name, returnType, inputTypes)
    {
        InReturn = action;
        _kind = FunctionKind.InReturn;
    }

    /// <summary>
    /// Creates a function with inputs, but no return value
    /// </summary>
    /// <param name="name">The name of the function</param>
    /// <param name="action">The action to execute</param>
    /// <param name="inputTypes">The TermType names of the inputted types</param>
    public Function(string name, Action<BaseTerm[]> action, params string[] inputTypes) : this(name, "void", inputTypes)
    {
        In = action;
        _kind = FunctionKind.In;
    }
    
    /// <summary>
    /// Creates a function with no inputs but a return value
    /// </summary>
    /// <param name="name">The name of the function</param>
    /// <param name="returnType">The name of the TermType the returned type belongs to</param>
    /// <param name="action">The action to execute</param>
    public Function(string name, string returnType, Func<ReturnValue> action) : this(name, returnType, new string[0])
    {
        Return = action;
        _kind = FunctionKind.Return;
    }

    public Function(string name, Action action) : this(name, "void", new string[0])
    {
        None = action;
        _kind = FunctionKind.None;
    }
    
    public Function(string name, Func<IEnumerable<ReturnValue>> action, string containedType) : this(name, "FuncEnumerator", new string[0])
    {
        YieldReturn = action;
        _yieldType = containedType;
        _kind = FunctionKind.YieldReturn;
    }

    #endregion
}

/// <summary>
/// Represents a function written in Steel
/// </summary>
/// <seealso cref="Function"/>
public record UserFunction : BaseExecutable, IFunction
{
    public string Name { get; }
    public string ReturnType { get; }
    public string[] InputTypes { get; }
    private readonly string[] _inputNames;

    public override ReturnValue Execute(params BaseTerm[] terms)
    {
        for (int i = 0; i < _inputNames.Length; i++)
        {
            string name = _inputNames[i];
            BaseTerm term = GetTerm(name);
            term.CopyFrom(terms[i]);
        }

        return Execute();
    }

    public override ReturnValue Execute()
    {
        foreach (TokenCall call in Calls)
        {
            if (call is ReturnCall)
                return call.Call();
            
            call.PreExecution();
            ReturnValue returnValue = call.Call();
            call.PostExecution();

            if (returnValue.HasValue)
            {
                if (returnValue.Value is ReturnCall returnCall)
                    return returnCall.Call();
            }
        }

        if (ReturnType == "void")
        {
            return ReturnValue.None;
        }
        else
            throw new FunctionLacksReturnException(0, this);
    }

    public UserFunction(ITokenHolder holder, string name, string returnType, Dictionary<string,string> inputs) : base(holder)
    {
        Name = name;
        ReturnType = returnType;
        InputTypes = new string[inputs.Count];
        _inputNames = new string[inputs.Count];

        int idx = 0;
        foreach (KeyValuePair<string,string> input in inputs)
        {
            InputTypes.SetValue(input.Value, idx);
            _inputNames.SetValue(input.Key, idx);

            LibraryManager manager = GetLibraryManager();
            TermType type;

            if (input.Value.StartsWith("params"))
            {
                type = manager.GetTermType($"Array<{input.Value.Split(' ')[1].Trim()}>");
            }
            else
            {
                type = manager.GetTermType(input.Value);
            }

            AddTerm(type.Construct(input.Key, 0, manager));
            idx++;
        }
    }

    public override IEnumerable<BaseTerm> EnumerateTerms()
    {
        foreach (BaseTerm term in BaseTerms.Values)
        {
            yield return term;
        }
    }

    public override BaseTerm GetTerm(string name)
    {
        if (!BaseTerms.ContainsKey(name) && GetLibraryManager().HasGlobalTerm(name))
            return GetLibraryManager().GetGlobalTerm(name);
            
        return BaseTerms[name];
    }

    public override bool HasTerm(string name)
    {
        return BaseTerms.ContainsKey(name) || GetLibraryManager().HasGlobalTerm(name);
    }
}