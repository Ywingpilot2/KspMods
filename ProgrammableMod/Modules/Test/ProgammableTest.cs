using System;
using ActionLanguage;
using ActionLanguage.Library;
using ProgrammableMod.Scripting.Library;

namespace ProgrammableMod.Modules.Test
{
    public class ProgammableTestModule : PartModule
    {
        [KSPField(isPersistant = false, guiActive = true, guiName = "Program Status")]
        public string Status;

        [KSPField(isPersistant = true)]
        private string _compilable = "log(\"test\")";

        [KSPField(isPersistant = true)]
        private bool _execute;
        private ActionScript _script;

        private static ILibrary _library = new KerbalLibrary(); 

        private void FixedUpdate()
        {
            if (_execute && HighLogic.LoadedSceneIsFlight)
            {
                try
                {
                    _script.Execute();
                }
                catch (Exception e)
                {
                    _execute = false;
                    Status = e.Message;
                    UpdateButtons();
                }
            }
        }

        [KSPEvent(active = true, guiActive = true, guiName = "Compile test script")]
        public void CompileScript()
        {
            try
            {
                ActionCompiler compiler = new ActionCompiler(_compilable, _library);
                _script = compiler.CompileScript();
                Status = "Operating properly";
            }
            catch (Exception e)
            {
                Status = $"{e.Message}";
            }
        }
        
        [KSPEvent(active = true, guiActive = true, guiName = "Start execution")]
        public void StartExecute()
        {
            _execute = true;
            UpdateButtons();
        }
        
        [KSPEvent(active = true, guiActive = false, guiName = "Stop execution")]
        public void StopExecute()
        {
            _execute = false;
            UpdateButtons();
        }

        public void UpdateButtons()
        {
            if (_execute)
            {
                Events["StopExecute"].active = true;
                Events["StartExecute"].active = false;
            }
            else
            {
                Events["StopExecute"].active = false;
                Events["StartExecute"].active = true;
            }
        }
    }
}