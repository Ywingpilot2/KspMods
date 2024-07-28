using System.Collections.Generic;
using SteelLanguage.Exceptions;
using SteelLanguage.Token.Fields;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;
using SteelLanguage.Utils;

namespace ProgrammableMod.Scripting.Terms.Vessel.Parts;

public class PartTerm : BaseTerm
{
    public override string ValueType => "Part";
    private Part _value;

    #region Fields Cache

    private Dictionary<string, PartField> _fields = new();
    private uint _flightId = 0;

    private void GenerateCacheFromPart(Part part)
    {
        // to avoid regenerating the entire field cache every game tick, we double check the flight id isnt changing
        if (part.flightID == _flightId)
            return;
        
        _fields.Clear();
        _flightId = part.flightID;
        
        foreach (PartModule module in part.Modules)
        {
            foreach (BaseField field in module.Fields)
            {
                if (!field.guiActive || field.uiControlFlight == null)
                    continue;
                
                if (_fields.ContainsKey(field.guiName))
                    continue; // TODO: throw some kind of error
                    
                _fields.Add(field.guiName, new PartField(field, module.Fields));
            }
        }
    }

    #endregion

    #region Value

    public override bool SetValue(object value)
    {
        if (value is Part part)
        {
            _value = part;
            GenerateCacheFromPart(part);
            return true;
        }

        return false;
    }

    public override bool CopyFrom(BaseTerm term)
    {
        if (term is PartTerm partTerm)
        {
            _value = partTerm._value;
            _fields = partTerm._fields;
            _flightId = partTerm._flightId;
            
            return true;
        }

        return false;
    }

    public override object GetValue()
    {
        return _value;
    }

    #endregion

    #region Fields

    public override IEnumerable<TermField> GetFields()
    {
        foreach (TermField field in base.GetFields())
        {
            yield return field;
        }
        
        yield return new TermField("name", "string", _value != null ? _value.protoPartSnapshot.partInfo.title : null);
        yield return new TermField("name", "string", _value != null ? _value.mass : null);
        yield return new TermField("name", "string", _value != null ? _value.protoPartSnapshot.partInfo.author : null);
        yield return new TermField("name", "string", _value != null ? _value.protoPartSnapshot.partInfo.description : null);
        yield return new TermField("name", "string", _value != null ? _value.protoPartSnapshot.partInfo.manufacturer : null);
        yield return new TermField("name", "string", _value != null ? _value.protoPartSnapshot.partInfo.tags : null);
    }

    #endregion

    #region Functions

    public override IEnumerable<IFunction> GetFunctions()
    {
        foreach (IFunction function in base.GetFunctions())
        {
            yield return function;
        }

        yield return new Function("get_value", "term", terms => GetPartField(terms[0].CastToStr()), "string");
        yield return new Function("set_value", terms =>
        {
            SetPartField(terms[0].CastToStr(), terms[1]);
        }, "string", "term");
    }

    #endregion

    private ReturnValue GetPartField(string name)
    {
        if (!_fields.ContainsKey(name))
            throw new InvalidActionException(0, $"Could not find field of name \"{name}\" on {_value.partName}");

        PartField field = _fields[name];
        object value = field.GetValue();
        switch (value)
        {
            case int:
            {
                return new ReturnValue(value, "int");
            }
            case double:
            {
                return new ReturnValue(value, "double");
            }
            case float:
            {
                return new ReturnValue(value, "float");
            }
            case string:
            {
                return new ReturnValue(value, "string");
            }
            case bool:
            {
                return new ReturnValue(value, "bool");
            }
            default:
                throw new InvalidActionException(0, $"Type {value.GetType()} is not supported");
        }
    }

    private void SetPartField(string name, BaseTerm term)
    {
        if (!_fields.ContainsKey(name))
            throw new InvalidActionException(0, $"Could not find field of name \"{name}\" on {_value.partName}");

        PartField field = _fields[name];
        object value = term.CastToType(field.TypeNameFromControl());
        field.SetValue(value);
    }
}