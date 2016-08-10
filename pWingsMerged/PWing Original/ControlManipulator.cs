using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProceduralWings.Original
{
    class ControlManipulator : WingManipulator
    {
        public override bool isCtrlSrf
        {
            get { return true; }
        }

        public override Vector3 rootPos
        {
            get
            {
                return Root.localPosition;
            }
        }

        [KSPField]
        public float ctrlFraction = 1f;

        public const float costDensityControl = 6500f;

        /// <summary>
        /// control surfaces cant carry fuel
        /// </summary>
        public override bool canBeFueled
        {
            get
            {
                return false;
            }
        }

        public override float updateCost()
        {
            return (float)Math.Round(wingMass * (1f + ArSweepScale / 4f) * (costDensity * (1f - ctrlFraction) + costDensityControl * ctrlFraction), 1);
        }

        [KSPField]
        public bool symmetricMovement = true;
        [KSPField(isPersistant = true)]
        public Vector3 rootPosition = Vector3.zero;

        public override void UpdateGeometry()
        {
            base.UpdateGeometry();

            if (symmetricMovement)
            {
                Tip.localPosition = new Vector3(0, ((float)Length - TipSpawnOffset.z) / 2, 0);
                Root.localPosition = -Tip.localPosition;
            }
        }

        public override void setFARModuleParams(double midChordSweep, double taperRatio, Vector3 midChordOffset)
        {
            if (part.Modules.Contains(FarModuleName))
            {
                PartModule FARmodule = part.Modules[FarModuleName];
                Type FARtype = FARmodule.GetType();
                FARtype.GetField("b_2").SetValue(FARmodule, Length);
                FARtype.GetField("b_2_actual").SetValue(FARmodule, Length);
                FARtype.GetField("MAC").SetValue(FARmodule, MAC);
                FARtype.GetField("MAC_actual").SetValue(FARmodule, MAC);
                FARtype.GetField("S").SetValue(FARmodule, Length * MAC);
                FARtype.GetField("MidChordSweep").SetValue(FARmodule, midChordSweep);
                FARtype.GetField("TaperRatio").SetValue(FARmodule, taperRatio);
                FARtype.GetField("ctrlSurfFrac").SetValue(FARmodule, ctrlFraction);
            }
        }

        public override void SetStockModuleParams()
        {
            // numbers for lift from: http://forum.kerbalspaceprogram.com/threads/118839-Updating-Parts-to-1-0?p=1896409&viewfull=1#post1896409
            part.CoMOffset.Set(Vector3.Dot(tipPos - rootPos, part.transform.right) / 2, Vector3.Dot(tipPos - rootPos, part.transform.up) / 2, 0); // COP matches COM
            ModuleControlSurface mCtrlSrf = part.Modules.OfType<ModuleControlSurface>().FirstOrDefault();
            if (mCtrlSrf != null)
            {
                mCtrlSrf.deflectionLiftCoeff = (float)(Length * MAC / 3.52);
                part.mass = mCtrlSrf.deflectionLiftCoeff * 0.1f * (1 + ctrlFraction);
                mCtrlSrf.ctrlSurfaceArea = ctrlFraction;
            }
        }
    }
}
