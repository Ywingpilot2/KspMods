using System.Collections.Generic;
using ProgrammableMod.Modules;
using ProgrammableMod.Scripting.Exceptions;
using SteelLanguage.Exceptions;
using SteelLanguage.Library.System.Terms.Literal;
using SteelLanguage.Token.Fields;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;

namespace ProgrammableMod.Scripting.Terms.VesselTerms.Parts;

internal class PartTerm : BaseTerm
{
    public override string ValueType => "Part";
    private Part _value;

    #region Cache

    private static readonly KSPActionParam ActiveParams = new KSPActionParam(KSPActionGroup.None, KSPActionType.Activate);
    private static readonly KSPActionParam DeactiveParams = new KSPActionParam(KSPActionGroup.None, KSPActionType.Deactivate);
    private static readonly KSPActionParam ToggleParams = new KSPActionParam(KSPActionGroup.None, KSPActionType.Toggle);

    private Dictionary<string, PartField> _fields = new();
    private Dictionary<string, BaseAction> _actions = new();
    private uint _flightId = 0;

    private void GenerateCacheFromPart(Part part)
    {
        // to avoid regenerating the entire field cache every game tick, we double check the flight id isnt changing
        if (part.flightID == _flightId)
            return;
        
        _fields.Clear();
        _actions.Clear();
        _flightId = part.flightID;
        
        foreach (PartModule module in part.Modules)
        {
            foreach (BaseField field in module.Fields)
            {
                if (!field.guiActive || field.uiControlFlight is UI_Label)
                    continue;
                
                if (_fields.ContainsKey(field.guiName))
                    continue; // TODO: throw some kind of error
                    
                _fields.Add(field.guiName, new PartField(field, module.Fields));
            }

            foreach (BaseAction action in module.Actions)
            {
                if (_actions.ContainsKey(action.guiName))
                    continue; // TODO: throw some kind of error
                
                _actions.Add(action.guiName, action);
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

        if (value is null)
        {
            _value = null;
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
            _actions = partTerm._actions;
            _flightId = partTerm._flightId;
            
            return true;
        }

        if (term is NullTerm)
        {
            _value = null;
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

        string name = "";
        if (_value != null && _value.HasModuleImplementing<PartNameModule>())
            name = _value.FindModuleImplementing<PartNameModule>().partName;

        yield return new TermField("name", "string", name);
        yield return new TermField("part_name", "string", _value != null ? _value.protoPartSnapshot.partInfo.title : null);
        yield return new TermField("mass", "float", _value != null ? _value.mass : null);
        yield return new TermField("author", "string", _value != null ? _value.protoPartSnapshot.partInfo.author : null);
        yield return new TermField("description", "string", _value != null ? _value.protoPartSnapshot.partInfo.description : null);
        yield return new TermField("manufacturer", "string", _value != null ? _value.protoPartSnapshot.partInfo.manufacturer : null);
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
        yield return new Function("has_value", "bool",
            terms => new ReturnValue(_fields.ContainsKey(terms[0].CastToStr()), "bool"), "string");
        yield return new Function("highlight", terms =>
        {
            _value.Highlight(terms[0].CastToBool());
        }, "bool");
        yield return new Function("activate_action", terms =>
        {
            string value = terms[0].CastToStr();
            if (!_actions.ContainsKey(value))
                throw new ActionNotFoundException(0, value);

            _actions[value].Invoke(ActiveParams);
        }, "string", "bool");
        yield return new Function("toggle_action", terms =>
        {
            string value = terms[0].CastToStr();
            if (!_actions.ContainsKey(value))
                throw new ActionNotFoundException(0, value);

            _actions[value].Invoke(ToggleParams);
        }, "string");
        yield return new Function("has_action", "bool",
            terms => new ReturnValue(_actions.ContainsKey(terms[0].CastToStr()), "bool"), "string");
        yield return new Function("has_module", "bool", terms =>
        {
            foreach (PartModule module in _value.Modules)
            {
                if (module.GetModuleDisplayName() == terms[0].CastToStr())
                    return new ReturnValue(true, "bool");
            }

            return new ReturnValue(false, "bool");
        }, "string");
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