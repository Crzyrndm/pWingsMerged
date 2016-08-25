using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProceduralWings.UpgradeModules
{
    public abstract class Module_DeprecatedWingModule : PartModule
    {
        public abstract void UpgradeModule(Base_ProceduralWing newModule);
    }
}
