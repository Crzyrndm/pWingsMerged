using UnityEngine;

namespace ProceduralWings.UpgradeModules
{
    using Original;

    /// <summary>
    /// A stub class that takes the saved values from old saves and recreates the new module as appropriate to upgrade vessels
    /// </summary>
    class WingManipulator : PartModule
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

        public void Start()
        {
            ManipulatorWing wing = part.Modules.GetModule<ManipulatorWing>();
            // assign all the variables
            wing.RootWidth = rootScale.y * modelChordLength;
            wing.RootThickness = rootScale.z * modelThickness;

            wing.TipWidth = tipScale.y * modelChordLength;
            wing.TipThickness = tipScale.z * modelThickness;

            wing.TipOffset = tipPosition.x - TipSpawnOffset.x;
            wing.Length = tipPosition.z - TipSpawnOffset.z;

            wing.fuelSelectedTankSetup = fuelSelectedTankSetup;
        }
    }
}
