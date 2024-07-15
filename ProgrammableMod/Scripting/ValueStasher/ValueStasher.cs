using System;
using System.Collections.Generic;
using ProgrammableMod.Scripting.Exceptions;
using ProgrammableMod.Scripting.Terms;
using SteelLanguage.Extensions;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;
using UnityEngine;

namespace ProgrammableMod.Scripting.ValueStasher;

[Serializable]
public class ValueStasher : IConfigNode
{
    private readonly Dictionary<string, ProtoStash> _stashedValues;

    public void StashValue(string name, BaseTerm term)
    {
        if (name.Contains(" "))
            name = name.Replace(' ', '_');
        
        if (HasValue(name))
        {
            _stashedValues.Remove(name);
        }

        if (term is IStashableTerm configTerm)
        {
            ConfigNode cfg = new ConfigNode(name);
            cfg.AddValue("basic_type", false);
            configTerm.Save(cfg);
            _stashedValues.Add(name, new ProtoStash(name, term.ValueType, cfg));
        }
        
        object value = term.GetValue();
        switch (value)
        {
            case int:
            case double:
            case uint:
            case string:
            case bool:
            {
                ConfigNode cfg = new ConfigNode($"{term.ValueType}-{name}");
                cfg.AddValue("value", KerbinSuperComputer.Clean(value.ToString()));
                cfg.AddValue("basic_type", true);
                _stashedValues.Add(name, new ProtoStash(name, term.ValueType, cfg));
            } break;
        }
    }

    public ProtoStash GetValue(string name)
    {
        if (name.Contains(" "))
            name = name.Replace(' ', '_');

        if (!HasValue(name))
            throw new StashableNotFoundException(0, name);

        return _stashedValues[name];
    }

    public bool HasValue(string name) => _stashedValues.ContainsKey(name.Replace(' ', '_'));

    public void Load(ConfigNode node)
    {
        foreach (ConfigNode configNode in node.GetNodes())
        {
            string[] split = configNode.name.SanitizedSplit('-', 2);
            string typeName = split[0].Trim();
            string name = split[1].Trim();
            
            _stashedValues.Add(name, new ProtoStash(name, typeName, configNode));
        }
    }

    public void Save(ConfigNode node)
    {
        foreach (KeyValuePair<string, ProtoStash> valuePair in _stashedValues)
        {
            string name = valuePair.Key;
            ProtoStash stash = valuePair.Value;
            node.AddNode($"{stash.ValueType}-{name}", stash.Node);
        }
    }

    public bool CanStashType(BaseTerm term)
    {
        if (term is IStashableTerm)
            return true;
        
        object value = term.GetValue();
        switch (value)
        {
            case int:
            case double:
            case uint:
            case string:
            case bool:
            {
                return true;
            }
        }

        return false;
    }

    public ValueStasher()
    {
        _stashedValues = new Dictionary<string, ProtoStash>();
    }
}
