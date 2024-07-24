using System;
using System.Collections.Generic;
using System.IO;

namespace ProgrammableMod.Scripting.Config.ScriptLibrary;

public class ScriptLibrary
{
    public string GetCurrentScriptsPath()
    {
        return $@"{KSPUtil.ApplicationRootPath}\saves\{HighLogic.SaveFolder}\Scripts\";
    }
    
    public ScriptDirectory GetScriptCrafts()
    {
        if (!Directory.Exists(GetCurrentScriptsPath()))
            Directory.CreateDirectory(GetCurrentScriptsPath());

        return SearchDirectory(GetCurrentScriptsPath());
    }

    private static ScriptDirectory SearchDirectory(string searchDir)
    {
        List<ScriptCraft> crafts = new List<ScriptCraft>();
        foreach (string file in Directory.EnumerateFiles(searchDir, "*.act", SearchOption.TopDirectoryOnly))
        {
            FileInfo info = new FileInfo(file);

            StreamReader reader = new StreamReader(file);
            string script = reader.ReadToEnd();
            reader.Dispose();
            crafts.Add(new ScriptCraft(info.Name.Replace(".act",""), script, searchDir));
        }

        List<ScriptDirectory> directories = new List<ScriptDirectory>();
        foreach (string directory in Directory.EnumerateDirectories(searchDir, "*", SearchOption.TopDirectoryOnly))
        {
            directories.Add(SearchDirectory(directory));
        }

        return new ScriptDirectory(searchDir, crafts, directories);
    }

    public void SaveScript(ScriptCraft craft)
    {
        if (!Directory.Exists(GetCurrentScriptsPath()))
            Directory.CreateDirectory(GetCurrentScriptsPath());

        StreamWriter writer = new StreamWriter($@"{craft.Directory}\{craft.Name}.act");
        writer.Write(craft.Script);
        writer.Dispose();
    }
}

public record ScriptDirectory(string Directory, List<ScriptCraft> Crafts, List<ScriptDirectory> Directories)
{
    public string Directory { get; } = Directory;
    public List<ScriptCraft> Crafts { get; } = Crafts;
    public List<ScriptDirectory> Directories { get; } = Directories;

    public ScriptDirectory(string directory) : this(directory, new List<ScriptCraft>(), new List<ScriptDirectory>())
    {
    }
}

[Serializable]
public record struct ScriptCraft (string Name, string Script, string Directory);