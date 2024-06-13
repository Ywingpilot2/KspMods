namespace ProgrammableMod.Modules.Test
{
    public class ThrottleTestModule : PartModule
    {
        [KSPField]
        public bool fullThrottle;

        [KSPField]
        public bool shouldThrottle;

        [KSPField] 
        private float currentThrottle;

        private void fullThorttle(FlightCtrlState s)
        {
            s.mainThrottle = currentThrottle;
        }

        [KSPEvent(active = true, guiActive = true, guiName = "Start Full Throttle")]
        public void StartFullThrottle()
        {
            Events["StopFullThrottle"].active = true;
            Events["StartFullThrottle"].active = false;

            currentThrottle = 1f;
            vessel.OnFlyByWire += fullThorttle;
            vessel.ActionGroups.SetGroup(KSPActionGroup.SAS, true);
            vessel.Autopilot.SetMode(VesselAutopilot.AutopilotMode.Normal);
        }
        
        [KSPEvent(active = false, guiActive = true, guiName = "Stop Full Throttle")]
        public void StopFullThrottle()
        {
            Events["StopFullThrottle"].active = false;
            Events["StartFullThrottle"].active = true;
            currentThrottle = 0f;
            vessel.OnFlyByWire -= fullThorttle;
        }
    }
}