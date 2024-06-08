using System;
using ActionScript.Terms;

namespace ActionScript.Functions
{
    public struct ReturnValue
    {
        public bool HasValue => Value != null;
        public string Type { get; }
        public object Value { get; }

        public ReturnValue(object value, string type)
        {
            Value = value;
            Type = type;
        }
    }
    
    public struct Function
    {
        public string Name { get; }
        public string ValueType { get; set; }
        public string[] InputTypes { get; }
        public Func<BaseTerm[], ReturnValue> Action { get; }

        public ReturnValue ExecuteAction(params BaseTerm[] terms)
        {
            return Action.Invoke(terms);
        }

        public Function(string name, string valueType, Func<BaseTerm[], ReturnValue> action, params string[] inputTypes)
        {
            Name = name;
            ValueType = valueType;
            InputTypes = inputTypes;
            Action = action;
        }
    }
}