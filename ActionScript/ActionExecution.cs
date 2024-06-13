using System.IO;
using ActionLanguage.Library;

namespace ActionLanguage
{
    public static class ActionExecution
    {
        public static bool ExecuteAction(string action, params ILibrary[] libraries)
        {
            ActionCompiler compiler = new ActionCompiler(action, libraries);
            ActionScript script = compiler.CompileScript();
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