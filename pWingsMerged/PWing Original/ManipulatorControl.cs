using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralWings.Original
{
    class ManipulatorControl : ManipulatorWing
    {
        public override bool IsCtrlSrf
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
        public override bool CanBeFueled
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
                Tip.localPosition = new Vector3(0, 0, ((float)Length - TipSpawnOffset.z) / 2);
                Root.localPosition = -Tip.localPosition;
            }
        }

        public override void setFARModuleParams(double midChordSweep, double taperRatio, Vector3 midChordOffset)
        {
            base.setFARModuleParams(midChordSweep, taperRatio, midChordOffset);
            if (aeroFARFieldInfoControlSurfaceFraction != null)
            {
                aeroFARFieldInfoControlSurfaceFraction.SetValue(aeroFARModuleReference, ctrlFraction);
                aeroFARMethodInfoUsed.Invoke(aeroFARModuleReference, null);
            }
        }

        public override void SetStockModuleParams()
        {
            // numbers for lift from: http://forum.kerbalspaceprogram.com/threads/118839-Updating-Parts-to-1-0?p=1896409&viewfull=1#post1896409
            part.CoMOffset.Set(Vector3.Dot(tipPos - rootPos, part.transform.right) / 2, Vector3.Dot(tipPos - rootPos, part.transform.up) / 2, 0); // COP matches COM
            ModuleControlSurface mCtrlSrf = part.Modules.GetModule<ModuleControlSurface>();
            if (mCtrlSrf != null)
            {
                mCtrlSrf.deflectionLiftCoeff = (float)(Length * MAC / 3.52);
                part.mass = mCtrlSrf.deflectionLiftCoeff * 0.1f * (1 + ctrlFraction);
                mCtrlSrf.ctrlSurfaceArea = ctrlFraction;
            }
        }
    }
}
