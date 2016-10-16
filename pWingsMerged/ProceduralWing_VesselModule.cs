using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ProceduralWings
{
    class ProceduralWing_VesselModule : VesselModule
    {
        public override Activation GetActivation()
        {
            return Activation.LoadedVessels;
        }

        public override bool ShouldBeActive()
        {
            return Vessel.loaded;
        }

        protected override void OnStart()
        {
            StartCoroutine(Startup());
        }

        public System.Collections.IEnumerator Startup()
        {
            Vessel vessel = GetComponent<Vessel>();

            Base_ProceduralWing w;

            // delay to ensure all part modules have started
            yield return new WaitForFixedUpdate();
            for (int i = vessel.parts.Count - 1; i >= 0; --i)
            {
                w = vessel.parts[i].Modules.GetModule<Base_ProceduralWing>();
                if (w != null)
                {
                    w.Setup();
                }
            }

            // delay to ensure FAR is initialised
            yield return new WaitForFixedUpdate();
            for (int i = vessel.parts.Count - 1; i >= 0; --i)
            {
                w = vessel.parts[i].Modules.GetModule<Base_ProceduralWing>();
                if (w != null)
                {
                    w.CalculateAerodynamicValues();
                }
            }
        }
    }
}
