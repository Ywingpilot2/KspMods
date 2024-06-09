using System;
using System.Collections.Generic;
using System.Linq;
using ActionScript.Exceptions;
using ActionScript.Library;
using ActionScript.Terms;
using ActionScript.Token;

namespace ActionScript.Functions
{
    public enum InputType
    {
        Null,
        Constant,
        Term,
        Call
    }
    
    public struct Input
    {
        public InputType Type { get; }
        private FunctionCall Call { get; }
        private BaseTerm _term;
        private ITokenHolder _script;

        public BaseTerm GetValue()
        {
            switch (Type)
            {
                case InputType.Constant:
                case InputType.Term:
                {
                    return _term;
                }break;
                case InputType.Call:
                {
                    ReturnValue value = Call.Call();
                    if (!value.HasValue)
                        return null;
                    
                    //return new BaseTerm(Guid.NewGuid().ToString(), value.Value.ToString(), Call.Function.ValueType);
                    if (!_script.TermTypeExists(value.Type))
                        throw new TypeNotExistException(Call.Line, value.Type);

                    TermType type = _script.GetTermType(value.Type);
                    BaseTerm term = type.Construct(Guid.NewGuid().ToString(), Call.Line);
                    term.SetValue(value.Value.ToString());

                    return term;
                } break;
                default:
                {
                    return null;
                } break;
            }
        }

        public Input(ITokenHolder script, FunctionCall call)
        {
            Type = InputType.Call;
            Call = call;
            _term = null;
            _script = script;
        }

        public Input(BaseTerm term)
        {
            Type = InputType.Term;
            _term = term;
            Call = null;
        }
    }

    public abstract class TokenCall : BaseToken
    {
        public abstract ReturnValue Call();

        protected TokenCall(ITokenHolder script, int line) : base(script, line)
        {
        }
    }
    
    public class FunctionCall : TokenCall
    {
        protected List<Input> _inputs;
        public IFunction Function { get; }

        public override ReturnValue Call()
        {
            List<BaseTerm> terms = new List<BaseTerm>();
            foreach (Input input in _inputs)
            {
                terms.Add(input.GetValue());
            }

            return Function.Execute(terms.ToArray());
        }

        public FunctionCall(ITokenHolder script, IFunction function, IEnumerable<Input> inputs, int line) : base(script, line)
        {
            Function = function;
            _inputs = new List<Input>(inputs);
        }

        public FunctionCall(ITokenHolder script, IFunction function, int line, params Input[] inputs) : base(script, line)
        {
            Function = function;
            _inputs = new List<Input>(inputs);
        }
    }

    public class TermCall : FunctionCall
    {
        private readonly string _term;

        public override ReturnValue Call()
        {
            List<BaseTerm> terms = new List<BaseTerm>();
            terms.Add(GetTerm(_term));
            foreach (Input input in _inputs)
            {
                terms.Add(input.GetValue());
            }

            return Function.Execute(terms.ToArray());
        }


        public TermCall(ITokenHolder script, int line, IFunction function, string term, IEnumerable<Input> inputs) : base(script, function, inputs, line)
        {
            _term = term;
        }

        public TermCall(ITokenHolder script, int line, IFunction function, string term, params Input[] inputs) : base(script, function, line, inputs)
        {
            _term = term;
        }
    }

    public class AssignmentCall : TokenCall
    {
        private Input Input { get; }
        private string _term;

        public override ReturnValue Call()
        {
            if (!GetTerm(_term).CopyFrom(Input.GetValue()))
            {
                throw new InvalidAssignmentException(Line, GetTerm(_term));
            }
            return new ReturnValue();
        }

        public AssignmentCall(BaseTerm term, Input input, ITokenHolder script, int line) : base(script, line)
        {
            _term = term.Name;
            Input = input;
        }
    }
}