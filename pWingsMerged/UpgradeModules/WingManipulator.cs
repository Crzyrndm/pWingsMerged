using UnityEngine;

namespace ProceduralWings.UpgradeModules
{
    using Original;

    /// <summary>
    /// A stub class that takes the saved values from old saves and recreates the new module as appropriate to upgrade vessels
    /// </summary>
    class WingManipulator : Module_DeprecatedWingModule
    {
        [KSPField]
        public float modelChordLength = 2f;
        [KSPField]
        public float modelThickness = 0.2f;
        [KSPField]
        public float modelMinimumSpan = 0.05f;
        [KSPField]
        public Vector3 TipSpawnOffset = Vector3.forward;

        [KSPField(isPersistant = true)]
        public Vector3 tipScale = Vector3.one;

        [KSPField(isPersistant = true)]
        public Vector3 tipPosition = Vector3.zero;

        [KSPField(isPersistant = true)]
        public Vector3 rootPosition = Vector3.zero;

        [KSPField(isPersistant = true)]
        public Vector3 rootScale = Vector3.one;

        [KSPField(isPersistant = true)] // otherwise revert to editor does silly things
        public int fuelSelectedTankSetup = -1;

        public override void UpgradeModule(Base_ProceduralWing newModule)
        {
            // assign all the variables
            newModule.RootWidth = rootScale.y * modelChordLength;
            newModule.RootThickness = rootScale.z * modelThickness;

            newModule.TipWidth = tipScale.y * modelChordLength;
            newModule.TipThickness = tipScale.z * modelThickness;

            newModule.TipOffset = tipPosition.x - TipSpawnOffset.x;
            newModule.Length = tipPosition.z - TipSpawnOffset.z;

            newModule.fuelSelectedTankSetup = fuelSelectedTankSetup;
        }
    }
}
