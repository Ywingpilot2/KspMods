using System.Collections.Generic;
using System.Linq;
using ActionLanguage.Exceptions;
using ActionLanguage.Token.Interaction;
using ActionLanguage.Token.Terms;

namespace ActionLanguage.Token.Functions
{
    public abstract class TokenCall : BaseToken
    {
        public abstract ReturnValue Call();

        #region Pre/Post events

        public virtual void PreExecution()
        {
        }

        public virtual void PostExecution()
        {
        }

        public virtual void PostCompilation()
        {
        }

        #endregion

        protected TokenCall(ITokenHolder script, int line) : base(script, line)
        {
        }
    }
    
    public class FunctionCall : TokenCall
    {
        private Input[] Inputs;
        private IFunction Function { get; }

        public override ReturnValue Call()
        {
            BaseTerm[] terms = new BaseTerm[Inputs.Length];
            for (var i = 0; i < Inputs.Length; i++)
            {
                terms.SetValue(Inputs[i].GetValue(), i);
            }

            return Function.Execute(terms);
        }
        
        #region Pre/Post events

        public override void PreExecution()
        {
            Function.PreExecution();
        }

        public override void PostExecution()
        {
            Function.PostExecution();
        }

        public override void PostCompilation()
        {
            Function.PostCompilation();
            foreach (Input input in Inputs)
            {
                input.PostCompilation();
            }
        }

        #endregion

        public FunctionCall(ITokenHolder script, IFunction function, IEnumerable<Input> inputs, int line) : base(script, line)
        {
            Function = function;
            Inputs = inputs.ToArray();
        }

        public FunctionCall(ITokenHolder script, IFunction function, int line, params Input[] inputs) : base(script, line)
        {
            Function = function;
            Inputs = inputs;
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

        public override void PostCompilation()
        {
            Input.PostCompilation();
        }

        public AssignmentCall(BaseTerm term, Input input, ITokenHolder script, int line) : base(script, line)
        {
            _term = term.Name;
            Input = input;
        }
    }
}