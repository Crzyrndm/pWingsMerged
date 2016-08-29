using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProceduralWings.UpgradeModules
{
    public interface IDeprecatedWingModule
    {
        void UpgradeModule(Base_ProceduralWing newModule);
    }
}
