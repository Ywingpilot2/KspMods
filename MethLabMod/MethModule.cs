using UnityEngine;

namespace MethLabMod
{
    public class MethModule : ModuleResourceConverter
    {
        [KSPField(isPersistant = false, guiActive = true, guiName = "Sun Exposure")]
        public string SunExposure;

        [KSPField(isPersistant = true)]
        public bool shouldBeActive;
        
        [KSPField(isPersistant = true)]
        public bool isBlocked;

        public void Start()
        {
            Fields["SunExposure"].guiName = "Sun Exposure";
        }

        public override void OnStart(StartState state)
        {
            SunExposure = "";
            base.OnStart(state);
        }

        public override void FixedUpdate()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                string body = "";
                string part = "";

                if (SolarVisible(out body))
                {
                    if (!IsActivated && shouldBeActive)
                    {
                        shouldBeActive = true;
                        StopResourceConverter();
                        isBlocked = false;
                        
                        SunExposure = "Fully Exposed";
                        UpdateConverterStatus();
                    }
                }
                else
                {
                    if (IsActivated)
                    {
                        StartResourceConverter();
                    }

                    SunExposure = $"Blocked by {body}";
                    isBlocked = true;
                    status = "Not functioning";
                    
                    UpdateConverterStatus();
                    return; // Don't do anything else lol
                }
            }
            base.FixedUpdate();
        }

        /// <summary>
        /// This gets whether or not the sun is obscured by a celestial body
        /// </summary>
        /// <param name="refForm"></param>
        /// <param name="angle"></param>
        /// <param name="obscuringBody"></param>
        /// <returns>True if the sun is visible, false if it is obscured</returns>
        private bool SolarVisible(out string obscuringBody)
        {
            bool isVisible = true;
            obscuringBody = "nil";

            CelestialBody sun = FlightGlobals.Bodies[0];
            CelestialBody currentBody = FlightGlobals.currentMainBody;

            if (currentBody != sun)
            {
                Vector3 vesselT = sun.position - part.vessel.GetWorldPos3D();
                Vector3 sunT = currentBody.position - part.vessel.GetWorldPos3D();

                // I dont understand this math tbh
                // Its checking if the sun is behind the horizon of the planet
                if (Vector3.Dot(vesselT, sunT) > (sunT.sqrMagnitude - currentBody.Radius * currentBody.Radius))
                {
                    // if true, obscured
                    if ((Mathf.Pow(Vector3.Dot(vesselT, sunT), 2) / vesselT.sqrMagnitude) > (sunT.sqrMagnitude - currentBody.Radius * currentBody.Radius))
                    {
                        isVisible = false;
                        obscuringBody = currentBody.name;
                    }
                }
            }

            return isVisible;
        }
    }
}