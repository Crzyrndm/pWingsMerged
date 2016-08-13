using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProceduralWings.B9PWing
{
    class B9_ProceduralPanel : B9_ProceduralWing
    {
        /// <summary>
        /// Panels are not lifting surfaces
        /// </summary>
        public override void CalculateAerodynamicValues()
        {
            if (part.Modules.Contains(FarModuleName))
                part.Modules.Remove(part.Modules[FarModuleName]);
            if (part.Modules.Contains<ModuleLiftingSurface>())
                part.Modules.Remove(part.Modules.GetModule<ModuleLiftingSurface>());
        }
    }
}
