using UnityEngine;

namespace ProceduralWings.B9PWing
{
    /// <summary>
    /// while its a control surface, to this plugin it mostly acts like a normal wing
    /// </summary>
    internal class B9_ProceduralControlAllMoving : B9_ProceduralWing
    {
        [KSPField]
        public float ctrlFraction = 1f;

        public override string WindowTitle
        {
            get
            {
                return "All-Moving surface";
            }
        }

        public override bool IsCtrlSrf
        {
            get
            {
                return true;
            }
        }

        public override bool CanBeFueled
        {
            get
            {
                return false;
            }
        }

        public override void SetStockModuleParams()
        {
            base.SetStockModuleParams();
            wingMass *= 1 + ctrlFraction;
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
    }
}