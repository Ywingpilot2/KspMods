namespace ProgrammableMod.Modules.Computers
{
    public interface IComputer
    {
        [KSPEvent(active = true, guiActive = true, guiName = "Compile")]
        public void CompileScript();
        
        /// <summary>
        /// Validates the current script
        /// </summary>
        /// <returns>Whether the script is valid or not</returns>
        public bool ValidateScript();

        [KSPEvent(active = true, guiActive = true, guiName = "Start execution")]
        public void Execute();
        
        [KSPEvent(active = true, guiActive = false, guiName = "Stop execution")]
        public void StopExecuting();
    }
}