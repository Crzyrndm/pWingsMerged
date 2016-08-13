using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ProceduralWings.B9PWing
{
    /// <summary>
    /// while its a control surface, to this plugin it mostly acts like a normal wing
    /// </summary>
    class B9_ProceduralControlAllMoving : B9_ProceduralWing
    {
        public override string WindowTitle
        {
            get
            {
                return "All-Moving surface";
            }
        }

        public override bool isCtrlSrf
        {
            get
            {
                return true;
            }
        }

        public override bool canBeFueled
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
