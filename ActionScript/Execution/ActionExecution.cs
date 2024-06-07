using System.IO;
using ActionScript.Library;

namespace ActionScript.Execution
{
    public static class ActionExecution
    {
        public static bool ExecuteAction(string action, params ILibrary[] libraries)
        {
            ActionScript script = new ActionScript(libraries);
            script.ParseTokens(action);
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