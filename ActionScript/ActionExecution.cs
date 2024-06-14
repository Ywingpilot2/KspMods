using System.IO;
using ActionLanguage.Library;

namespace ActionLanguage
{
    public static class ActionExecution
    {
        public static ActionScript CompileScriptFromFile(string path, params ILibrary[] libraries)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Cannot compile file which does not exist", path);

            StreamReader reader = new StreamReader(path);
            ActionCompiler compiler = new ActionCompiler(reader, libraries);
            return compiler.CompileScript();
        }
        
        public static bool ExecuteCompileAction(string action, params ILibrary[] libraries)
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
            return ExecuteCompileAction(script, libraries);
        }
    }
}