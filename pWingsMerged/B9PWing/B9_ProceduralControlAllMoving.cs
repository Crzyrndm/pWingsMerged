using UnityEngine;

namespace ProceduralWings.B9PWing
{
    /// <summary>
    /// while its a control surface, to this plugin it mostly acts like a normal wing
    /// </summary>
    internal class B9_ProceduralControlAllMoving : B9_ProceduralWing
    {
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

        public override void setFARModuleParams(double midChordSweep, double taperRatio, Vector3 midChordOffset)
        {
            base.setFARModuleParams(midChordSweep, taperRatio, midChordOffset);
            if (aeroFARFieldInfoControlSurfaceFraction != null)
            {
                aeroFARFieldInfoControlSurfaceFraction.SetValue(aeroFARModuleReference, 1);
                aeroFARMethodInfoUsed.Invoke(aeroFARModuleReference, null);
            }
        }
    }
}