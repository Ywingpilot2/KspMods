using System.IO;
using ActionScript.Library;

namespace ActionScript
{
    public static class ActionExecution
    {
        public static bool ExecuteAction(string action, params ILibrary[] libraries)
        {
            ActionCompiler compiler = new ActionCompiler();
            ActionScript script = compiler.CompileScript(action, libraries);
            script.Execute();
            
            return true;
        }

        public static bool ExecuteFromFile(string path, params ILibrary[] libraries)
        {
            if (!File.Exists(path))
                return false;
            
            StreamReader reader = new StreamReader(path);
            string script = reader.ReadToEnd();
            reader.Dispose();
            return ExecuteAction(script, libraries);
        }
    }
}