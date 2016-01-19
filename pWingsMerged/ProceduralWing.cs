using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace ProceduralWings
{
    /// <summary>
    /// methods and properties common to both wing variants. Some implementations will be specific to the wing type
    /// </summary>
    abstract public class ProceduralWing : PartModule, IPartCostModifier, IPartMassModifier, IPartSizeModifier
    {
        // Part module parameters
        [KSPField]
        public bool isWing = true;
        [KSPField]
        public bool isCtrlSrf = false;

        // active assemblies
        public static bool assembliesChecked;
        public static bool FARactive;
        public static bool RFactive;
        public static bool MFTactive;

        // aero parameters
        public double b_2;
        public double MAC;
        public double midChordSweep;
        public double Cd;
        public double Cl;
        public double ChildrenCl;
        public double wingMass;
        public double connectionForce;
        public double taperRatio;
        public double surfaceArea;
        public double aspectRatio;
        public double ArSweepScale;

        public double tipThickness, rootThickness;

        // fuel parameters
        [KSPField(isPersistant = true)]
        public int fuelSelectedTankSetup = -1;
        public double aeroStatVolume;

        // module cost variables
        public float wingCost;
        public float costDensity = 5300f;
        public float costDensityControl = 6500f;

        public abstract float ctrlFraction { get; }

        #region Fuel configuration switching
        // Has to be situated here as this KSPEvent is not correctly added Part.Events otherwise
        [KSPEvent(guiActive = false, guiActiveEditor = true, guiName = "Next configuration", active = true)]
        public void NextConfiguration()
        {
            if (!(canBeFueled && useStockFuel))
                return;
            fuelSelectedTankSetup = ++fuelSelectedTankSetup % ProceduralWingManager.wingTankConfigurations.Count;
            FuelTankTypeChanged();
        }

        public void FuelUpdateVolume()
        {
            if (!canBeFueled || !HighLogic.LoadedSceneIsEditor)
                return;

            aeroStatVolume = b_2 * MAC * (rootThickness + tipThickness) / 2;

            for (int i = 0; i < part.Resources.Count; ++i)
            {
                PartResource res = part.Resources[i];
                double fillPct = res.maxAmount > 0 ? res.amount / res.maxAmount : 1.0;
                res.maxAmount = ProceduralWingManager.wingTankConfigurations[fuelSelectedTankSetup].resources[res.resourceName].unitsPerVolume * aeroStatVolume;
                res.amount = res.maxAmount * fillPct;
            }
            part.Resources.UpdateList();
        }

        /// <summary>
        /// set resources in this tank and all symmetry counterparts
        /// </summary>
        public void FuelTankTypeChanged()
        {
            FuelSetResources();
            for (int s = 0; s < part.symmetryCounterparts.Count; s++)
            {
                if (part.symmetryCounterparts[s] == null) // fixes nullref caused by removing mirror sym while hovering over attach location
                    continue;
                ProceduralWing wing = part.symmetryCounterparts[s].Modules.OfType<ProceduralWing>().FirstOrDefault();
                if (wing != null)
                {
                    wing.fuelSelectedTankSetup = fuelSelectedTankSetup;
                    wing.FuelSetResources();
                }
            }
        }

        /// <summary>
        /// takes a volume in m^3 and sets up amounts for RF/MFT
        /// </summary>
        public void FuelSetResources()
        {
            if (!(canBeFueled && HighLogic.LoadedSceneIsEditor))
                return;

            if (!useStockFuel)
            {
                PartModule module = part.Modules["ModuleFuelTanks"];
                if (module == null)
                    return;

                Type type = module.GetType();

                double volumeRF = aeroStatVolume;
                if (RFactive)
                    volumeRF *= 1000;     // RF requests units in liters instead of cubic meters
                else // assemblyMFTUsed
                    volumeRF *= 173.9;  // MFT requests volume in units
                type.GetField("volume").SetValue(module, volumeRF);
                type.GetMethod("ChangeVolume").Invoke(module, new object[] { volumeRF });
            }
            else
            {
                part.Resources.list.Clear();
                PartResource[] partResources = part.GetComponents<PartResource>();
                for (int i = 0; i < partResources.Length; i++)
                    DestroyImmediate(partResources[i]);

                foreach (KeyValuePair<string, WingTankResource> kvp in ProceduralWingManager.wingTankConfigurations[fuelSelectedTankSetup].resources)
                {
                    ConfigNode newResourceNode = new ConfigNode("RESOURCE");
                    newResourceNode.AddValue("name", kvp.Value.resource.name);
                    newResourceNode.AddValue("amount", kvp.Value.unitsPerVolume * aeroStatVolume);
                    newResourceNode.AddValue("maxAmount", kvp.Value.unitsPerVolume * aeroStatVolume);
                    part.AddResource(newResourceNode);
                }
                part.Resources.UpdateList();
            }
        }

        public bool canBeFueled
        {
            get
            {
                return !isCtrlSrf && ProceduralWingManager.wingTankConfigurations.Count > 0;
            }
        }

        public bool useStockFuel
        {
            get
            {
                return !RFactive && !MFTactive;
            }
        }

        public float FuelGetAddedCost()
        {
            float result = 0f;
            foreach (KeyValuePair<string, WingTankResource> kvp in ProceduralWingManager.wingTankConfigurations[fuelSelectedTankSetup].resources)
            {
                result += kvp.Value.resource.unitCost * kvp.Value.unitsPerVolume * (float)aeroStatVolume;
            }
            return result;
        }
        #endregion

        float updateTimeDelay = 0;
        /// <summary>
        /// Handle all the really expensive stuff once we are no longer actively modifying the wing. Doing it continuously causes lag spikes for lots of people
        /// </summary>
        /// <returns></returns>
        public IEnumerator updateAeroDelayed()
        {
            bool running = updateTimeDelay > 0;
            updateTimeDelay = 0.5f;
            if (running)
                yield break;
            while (updateTimeDelay > 0)
            {
                updateTimeDelay -= TimeWarp.deltaTime;
                yield return null;
            }
            if (FARactive)
            {
                if (part.Modules.Contains("FARWingAerodynamicModel"))
                {
                    PartModule FARmodule = part.Modules["FARWingAerodynamicModel"];
                    Type FARtype = FARmodule.GetType();
                    FARtype.GetMethod("StartInitialization").Invoke(FARmodule, null);
                }
                part.SendMessage("GeometryPartModuleRebuildMeshData"); // notify FAR that geometry has changed
            }
            else
            {
                DragCube DragCube = DragCubeSystem.Instance.RenderProceduralDragCube(part);
                part.DragCubes.ClearCubes();
                part.DragCubes.Cubes.Add(DragCube);
                part.DragCubes.ResetCubeWeights();
            }
            FuelUpdateVolume();

            if (HighLogic.LoadedSceneIsEditor)
                GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);
            updateTimeDelay = 0;
        }

        public void CheckAssemblies()
        {
            if (!assembliesChecked)
            {
                FARactive = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name.Equals("FerramAerospaceResearch", StringComparison.InvariantCultureIgnoreCase));
                RFactive = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name.Equals("RealFuels", StringComparison.InvariantCultureIgnoreCase));
                MFTactive = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name.Equals("modularFuelTanks", StringComparison.InvariantCultureIgnoreCase));
                assembliesChecked = true;
            }
        }

        public void GatherChildrenCl()
        {
            ChildrenCl = 0;

            // Add up the Cl and ChildrenCl of all our children to our ChildrenCl
            foreach (Part p in this.part.children)
            {
                ProceduralWing child = p.Modules.OfType<ProceduralWing>().FirstOrDefault();
                if (child != null)
                {
                    ChildrenCl += child.Cl;
                    ChildrenCl += child.ChildrenCl;
                }
            }

            // If parent is a pWing, trickle the call to gather ChildrenCl down to them.
            if (this.part.parent != null)
            {
                ProceduralWing Parent = this.part.parent.Modules.OfType<ProceduralWing>().FirstOrDefault();
                if (Parent != null)
                    Parent.GatherChildrenCl();
            }
        }

        public void updateCost()
        {
            // Values always set
            if (!isCtrlSrf)
                wingCost = (float)Math.Round(wingMass * (1f + ArSweepScale / 4f) * costDensity, 1);
            else // ctrl surfaces
                wingCost = (float)Math.Round(wingMass * (1f + ArSweepScale / 4f) * (costDensity * (1f - ctrlFraction) + costDensityControl * ctrlFraction), 1);
        }

        #region Interfaces
        public float GetModuleCost(float defaultCost)
        {
            return wingCost;
        }

        public float GetModuleMass(float defaultMass)
        {
            return part.mass - part.partInfo.partPrefab.mass;
        }

        public Vector3 GetModuleSize(Vector3 defaultSize)
        {
            return Vector3.zero; // should do this properly at some point
        }
        #endregion

        public const double Deg2Rad = Math.PI / 180;
        public const double Rad2Deg = 180 / Math.PI;

        public static T Clamp<T>(T val, T min, T max) where T : IComparable
        {
            if (val.CompareTo(min) < 0) // val less than min
                return min;
            if (val.CompareTo(max) > 0) // val greater than max
                return max;
            return val;
        }
    }
}