using System;

namespace ProgrammableMod.Scripting.Terms;

/// <summary>
/// TODO: Term interfaces/type attributes
/// </summary>
public interface IStashableTerm
{
    public bool Save(ConfigNode node);
    public bool Load(ConfigNode node);
}