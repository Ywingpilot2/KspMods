using System;
using System.Collections.Generic;
using System.Linq;
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
        private Term _term;

        public Term GetValue()
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

                    return new Term(Guid.NewGuid().ToString(), value.Value.ToString(), Call.Function.ValueType);

                } break;
                default:
                {
                    return null;
                } break;
            }
        }

        public Input(FunctionCall call)
        {
            Type = InputType.Call;
            Call = call;
            _term = null;
        }

        public Input(Term term)
        {
            Type = InputType.Term;
            _term = term;
            Call = null;
        }

        public Input(string value)
        {
            Type = InputType.Constant;
            _term = new Term(Guid.NewGuid().ToString(), value.ToString());
            Call = null;
        }
    }

    public abstract class TokenCall : BaseToken
    {
        public abstract ReturnValue Call();

        protected TokenCall(ActionScript script) : base(script)
        {
        }
    }
    
    public class FunctionCall : TokenCall
    {
        public List<Input> Inputs { get; }
        public Function Function { get; }

        public override ReturnValue Call()
        {
            List<Term> terms = new List<Term>();
            foreach (Input input in Inputs)
            {
                terms.Add(input.GetValue());
            }

            return Function.ExecuteAction(terms.ToArray());
        }

        public FunctionCall(ActionScript script, Function function, IEnumerable<Input> inputs) : base(script)
        {
            Function = function;
            Inputs = new List<Input>(inputs);
        }

        public FunctionCall(ActionScript script, Function function, params Input[] inputs) : base(script)
        {
            Function = function;
            Inputs = new List<Input>(inputs);
        }
    }

    public class AssignmentCall : TokenCall
    {
        private Input Input { get; }
        private string _term;

        public override ReturnValue Call()
        {
            GetTerm(_term).CopyFrom(Input.GetValue());
            return new ReturnValue();
        }

        public AssignmentCall(Term term, Input input, ActionScript script) : base(script)
        {
            _term = term.Name;
            Input = input;
        }
    }
}