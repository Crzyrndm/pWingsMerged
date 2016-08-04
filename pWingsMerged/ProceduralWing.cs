using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace ProceduralWings
{
    using UI;
    using Utility;
    using B9;
    using Original;
    using KSP.UI.Screens;

    /// <summary>
    /// methods and properties common to both wing variants. Some implementations will be specific to the wing type
    /// </summary>
    abstract public class ProceduralWing : PartModule, IPartCostModifier, IPartMassModifier, IPartSizeModifier
    {
        public virtual bool isCtrlSrf
        {
            get { return false; }
        }

        public override string GetInfo()
        {
            return "this is a PWing";
        }

        // Properties for aero calcs
        public abstract Vector3 tipPos { get; set; }
        public abstract double tipWidth { get; set; }
        public abstract double tipThickness { get; set; }
        public abstract double tipOffset { get; set; }
        
        public virtual Vector3 rootPos
        {
            get { return Vector3.zero; }
        }
        public abstract double rootWidth { get; set; }
        public abstract double rootThickness { get; set; }
        public abstract double minSpan { get; }

        // active assemblies
        public static bool assembliesChecked;
        public static bool FARactive;
        public static bool RFactive;
        public static bool MFTactive;

        // aero parameters
        public double length;
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
        public Vector3d midChordOffsetFromOrigin = Vector3.zero; // used to calculate the impact of edges on the wing center

        public const float liftFudgeNumber = 0.0775f;
        public const float massFudgeNumber = 0.015f;
        public const float dragBaseValue = 0.6f;
        public const float dragMultiplier = 3.3939f;
        public const float connectionFactor = 150f;
        public const float connectionMinimum = 50f;

        // config vars
        public static bool loadedConfig;
        public static KeyCode keyTranslation = KeyCode.G;
        public static KeyCode keyTipScale = KeyCode.T;
        public static KeyCode keyRootScale = KeyCode.B; // was r, stock uses r now though
        public static float moveSpeed = 5.0f;
        public static float scaleSpeed = 0.25f;        

        // fuel parameters
        [KSPField(isPersistant = true)]
        public int fuelSelectedTankSetup;
        public double aeroStatVolume;

        // module cost variables
        public float wingCost;
        public const float costDensity = 5300f;

        public bool isStarted; // helper bool that prevents anything running when the start sequence hasn't fired yet (happens for various events -.-)
        public bool isAttached
        {
            get { return part.isAttached; }
        }

        #region entry points
        /// <summary>
        /// runs only in the editor scene
        /// </summary>
        public virtual void Start()
        {
            GameEvents.onGameSceneLoadRequested.Add(OnSceneSwitch);

            if (!HighLogic.LoadedSceneIsEditor)
                return;
            Setup();

            part.OnEditorAttach += new Callback(OnAttach);
            part.OnEditorDetach += new Callback(OnDetach);

            CreateEditorUI();

            isStarted = true;
        }

        /// <summary>
        /// runs only in the flight scene
        /// </summary>
        /// <param name="state"></param>
        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);

            if (!HighLogic.LoadedSceneIsFlight)
                return;
            Setup();
            StartCoroutine(flightAeroSetup());

            isStarted = true;
        }

        public virtual void Update()
        {
            if (!HighLogic.LoadedSceneIsEditor)
                return;

            DeformWing();
            if (CheckForGeometryChanges())
            {
                UpdateGeometry();
                UpdateCounterparts();
            }
        }

        public virtual void OnDestroy()
        {
            GameEvents.onGameSceneLoadRequested.Remove(OnSceneSwitch);
        }

        // unnecesary save/load. config is static so it will be initialised as you pass through the space center, and there is no way to change options in the editor scene
        // may resolve errors reported by Hodo
        public override void OnSave(ConfigNode node)
        {
            if (WPDebug.logEvents)
                DebugLogWithID("OnSave", "Invoked");
            try
            {
                vesselList.FirstOrDefault(vs => vs.vessel == vessel).isUpdated = false;
                ProceduralWingDebug.SaveConfigs();
            }
            catch
            {
                Debug.Log("B9 PWings - Failed to save settings");
            }
        }

        public void OnSceneSwitch(GameScenes scene)
        {
            isStarted = false; // fixes annoying nullrefs when switching scenes and things haven't been destroyed yet
        }
        #endregion

        #region Setting up

        public static void CheckAssemblies()
        {
            if (!assembliesChecked)
            {
                FARactive = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name.Equals("FerramAerospaceResearch", StringComparison.InvariantCultureIgnoreCase));
                RFactive = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name.Equals("RealFuels", StringComparison.InvariantCultureIgnoreCase));
                MFTactive = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name.Equals("modularFuelTanks", StringComparison.InvariantCultureIgnoreCase));
                assembliesChecked = true;
            }
        }

        public virtual void Setup()
        {
            CheckAssemblies();
            SetupGeometryAndAppearance();
            RefreshGeometry();

            if (fuelSelectedTankSetup < 0)
            {
                fuelSelectedTankSetup = 0;
                FuelTankTypeChanged();
            }
        }

        public abstract void SetupGeometryAndAppearance();
        #endregion

        #region geometry
        /// <summary>
        /// makes all the neccesary geometry alterations and then updates the aerodynamics to match
        /// </summary>
        public abstract void UpdateGeometry();

        public virtual void OnAttach()
        {
            UpdateGeometry();
        }

        public virtual void OnDetach()
        {
            ProceduralWing parentWing = part.parent.Modules.OfType<ProceduralWing>().FirstOrDefault();
            if (parentWing != null)
            {
                parentWing.FuelUpdateVolume(); // why am I doing this...?
                parentWing.CalculateAerodynamicValues();
            }
        }

        /// <summary>
        /// pass all changes to sym counterparts
        /// </summary>
        public abstract void UpdateCounterparts();

        public class VesselStatus
        {
            public Vessel vessel = null;
            public bool isUpdated = false;

            public VesselStatus(Vessel v, bool state)
            {
                vessel = v;
                isUpdated = state;
            }
        }
        public static List<VesselStatus> vesselList;

        /// <summary>
        /// setup the wing ready for flight
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerator flightAeroSetup()
        {
            if (vesselList == null)
                vesselList = new List<VesselStatus>();
            // First we need to determine whether the vessel this part is attached to is included into the status list
            // If it's included, we need to fetch it's index in that list

            bool vesselListInclusive = false;
            int vesselID = vessel.GetInstanceID();
            int vesselStatusIndex = 0;
            int vesselListCount = vesselList.Count;
            for (int i = 0; i < vesselListCount; ++i)
            {
                if (vesselList[i].vessel.GetInstanceID() == vesselID)
                {
                    if (WPDebug.logFlightSetup)
                        DebugLogWithID("SetupReorderedForFlight", "Vessel " + vesselID + " found in the status list");
                    vesselListInclusive = true;
                    vesselStatusIndex = i;
                }
            }

            // If it was not included, we add it to the list
            // Correct index is then fairly obvious

            if (!vesselListInclusive)
            {
                if (WPDebug.logFlightSetup)
                    DebugLogWithID("SetupReorderedForFlight", "Vessel " + vesselID + " was not found in the status list, adding it");
                vesselList.Add(new VesselStatus(vessel, false));
                vesselStatusIndex = vesselList.Count - 1;
            }

            // Using the index for the status list we obtained, we check whether it was updated yet
            // So that only one part can run the following part

            if (!vesselList[vesselStatusIndex].isUpdated)
            {
                if (WPDebug.logFlightSetup)
                    DebugLogWithID("SetupReorderedForFlight", "Vessel " + vesselID + " was not updated yet (this message should only appear once)");
                vesselList[vesselStatusIndex].isUpdated = true;
                List<ProceduralWing> moduleList = new List<ProceduralWing>();

                // First we get a list of all relevant parts in the vessel
                // Found modules are added to a list
                for (int i = 0; i < vessel.parts.Count; ++i)
                    moduleList.AddRange(vessel.parts[i].Modules.OfType<ProceduralWing>());

                // After that we make two separate runs through that list
                // First one setting up all geometry and second one setting up aerodynamic values
                for (int i = 0; i < moduleList.Count; ++i)
                    moduleList[i].Setup();

                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();

                if (WPDebug.logFlightSetup)
                    DebugLogWithID("SetupReorderedForFlight", "Vessel " + vesselID + " waited for updates, starting aero value calculation");
                for (int i = 0; i < moduleList.Count; ++i)
                    moduleList[i].CalculateAerodynamicValues();
            }
        }

        /// <summary>
        /// check if the wing shape/appearance has changed since last update
        /// </summary>
        /// <returns></returns>
        public abstract bool CheckForGeometryChanges();

        /// <summary>
        /// call during setup and when updating sym counterparts
        /// </summary>
        public virtual void RefreshGeometry()
        {
            UpdateGeometry();
        }
        #endregion

        #region Fuel configuration switching
        // Has to be situated here as this KSPEvent is not correctly added Part.Events otherwise
        [KSPEvent(guiActive = false, guiActiveEditor = true, guiName = "Next configuration", active = true)]
        public void NextConfiguration()
        {
            if (!(canBeFueled && useStockFuel))
                return;
            fuelSelectedTankSetup = ++fuelSelectedTankSetup % StaticWingGlobals.wingTankConfigurations.Count;
            FuelTankTypeChanged();
        }

        public void FuelUpdateVolume()
        {
            if (!canBeFueled || !HighLogic.LoadedSceneIsEditor)
                return;

            aeroStatVolume = length * MAC * (rootThickness + tipThickness) / 2;

            for (int i = 0; i < part.Resources.Count; ++i)
            {
                PartResource res = part.Resources[i];
                double fillPct = res.maxAmount > 0 ? res.amount / res.maxAmount : 1.0;
                res.maxAmount = StaticWingGlobals.wingTankConfigurations[fuelSelectedTankSetup].resources[res.resourceName].unitsPerVolume * aeroStatVolume;
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

                foreach (KeyValuePair<string, WingTankResource> kvp in StaticWingGlobals.wingTankConfigurations[fuelSelectedTankSetup].resources)
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

        public virtual bool canBeFueled
        {
            get
            {
                return StaticWingGlobals.wingTankConfigurations.Count > 0;
            }
        }

        public virtual bool useStockFuel
        {
            get
            {
                return !RFactive && !MFTactive;
            }
        }

        public virtual float FuelGetAddedCost()
        {
            float result = 0f;
            foreach (KeyValuePair<string, WingTankResource> kvp in StaticWingGlobals.wingTankConfigurations[fuelSelectedTankSetup].resources)
            {
                result += kvp.Value.resource.unitCost * kvp.Value.unitsPerVolume * (float)aeroStatVolume;
            }
            return result;
        }
        #endregion

        #region aero stuff
        /// <summary>
        /// all wings need to be able to calc aero values but implementations are all different. Use a blank method for panels
        /// </summary>
        public virtual void CalculateAerodynamicValues()
        {
            // Calculate intemediate values
            //print(part.name + ": Calc Aero values");
            length = tipPos.z - rootPos.z;
            MAC = (tipWidth + rootWidth);
            midChordSweep = (Utils.Rad2Deg * Math.Atan((rootPos.x - tipPos.x) / length));
            taperRatio = tipWidth / rootWidth;
            surfaceArea = MAC * length;
            aspectRatio = 2.0 * length / MAC;

            ArSweepScale = Math.Pow(aspectRatio / Math.Cos(Utils.Deg2Rad * midChordSweep), 2.0) + 4.0;
            ArSweepScale = 2.0 + Math.Sqrt(ArSweepScale);
            ArSweepScale = (2.0 * Math.PI) / ArSweepScale * aspectRatio;

            wingMass = Math.Max(0.01, massFudgeNumber * surfaceArea * ((ArSweepScale * 2.0) / (3.0 + ArSweepScale)) * ((1.0 + taperRatio) / 2));

            Cd = dragBaseValue / ArSweepScale * dragMultiplier;
            Cl = liftFudgeNumber * surfaceArea * ArSweepScale;
            GatherChildrenCl();

            connectionForce = Math.Round(Math.Max(Math.Sqrt(Cl + ChildrenCl) * connectionFactor, connectionMinimum), 0);

            updateCost();

            // should really do something about the joint torque here, not just its limits
            part.breakingForce = Mathf.Round((float)connectionForce);
            part.breakingTorque = Mathf.Round((float)connectionForce);

            // Stock-only values
            if (!FARactive)
                SetStockModuleParams();
            else
                setFARModuleParams();

            StartCoroutine(updateAeroDelayed());
        }

        public virtual void setFARModuleParams()
        {
            if (part.Modules.Contains("FARWingAerodynamicModel"))
            {
                PartModule FARmodule = part.Modules["FARWingAerodynamicModel"];
                Type FARtype = FARmodule.GetType();
                FARtype.GetField("b_2").SetValue(FARmodule, length);
                FARtype.GetField("b_2_actual").SetValue(FARmodule, length);
                FARtype.GetField("MAC").SetValue(FARmodule, MAC);
                FARtype.GetField("MAC_actual").SetValue(FARmodule, MAC);
                FARtype.GetField("S").SetValue(FARmodule, surfaceArea);
                FARtype.GetField("MidChordSweep").SetValue(FARmodule, midChordSweep);
                FARtype.GetField("TaperRatio").SetValue(FARmodule, taperRatio);
                FARtype.GetField("rootMidChordOffsetFromOrig").SetValue(FARmodule, (Vector3)midChordOffsetFromOrigin);
            }
        }

        public virtual void SetStockModuleParams()
        {
            // numbers for lift from: http://forum.kerbalspaceprogram.com/threads/118839-Updating-Parts-to-1-0?p=1896409&viewfull=1#post1896409
            float stockLiftCoefficient = (float)(surfaceArea / 3.52);
            part.CoMOffset.Set(Vector3.Dot(tipPos - rootPos, part.transform.right) / 2, Vector3.Dot(tipPos - rootPos, part.transform.up) / 2, 0); // CoL/P matches CoM unless otherwise specified
            part.Modules.GetModules<ModuleLiftingSurface>().FirstOrDefault().deflectionLiftCoeff = stockLiftCoefficient;
            part.mass = stockLiftCoefficient * 0.1f;
        }

        public virtual void TriggerFARColliderUpdate() { }

        float updateTimeDelay = 0;
        /// <summary>
        /// Handle all the really expensive stuff once we are no longer actively modifying the wing. Doing it continuously causes lag spikes for lots of people
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerator updateAeroDelayed()
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

        public virtual void GatherChildrenCl()
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

        #endregion

        #region Wing deformation

        public Vector3 lastMousePos;
        public int state = 0; // 0 == nothing, 1 == translate, 2 == tipScale, 3 == rootScale
        public Vector3 tempVec = Vector3.zero;
        public virtual void OnMouseOver()
        {
            DebugValues();
            

            if (!(HighLogic.LoadedSceneIsEditor && isAttached))
                return;
            
            if (state == 0)
            {
                lastMousePos = Input.mousePosition;
                if (Input.GetKeyDown(keyTranslation))
                    state = 1;
                else if (Input.GetKeyDown(keyTipScale))
                    state = 2;
                else if (Input.GetKeyDown(keyRootScale))
                    state = 3;
            }
        }

        /// <summary>
        /// respond to key/mouse input used to shape the wing
        /// </summary>
        public static Camera editorCam;
        public virtual void DeformWing()
        {
            if (!isAttached || state == 0)
                return;

            float depth = EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).WorldToScreenPoint(state != 3 ? tipPos : rootPos).z; // distance of tip transform from camera
            Vector3 diff = (state == 1 ? moveSpeed : scaleSpeed * 20) * depth * (Input.mousePosition - lastMousePos) / 4500;
            lastMousePos = Input.mousePosition;

            switch (state)
            {
                case 1: // translation
                    translateTip(diff);
                    break;
                case 2: // tip
                    scaleTip(diff);
                    break;
                case 3: // root
                    scaleRoot(diff);
                    break;
            }
        }

        public virtual void translateTip(Vector3 diff)
        {
            if (!Input.GetKey(keyTranslation))
            {
                state = 0;
                return;
            }
            tempVec = tipPos;
            tempVec.x += diff.x * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.right, part.transform.up) + diff.y * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.up, part.transform.up);
            tempVec.z += diff.x * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.right, part.transform.right) + diff.y * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.up, part.transform.right);
            tempVec.z = Mathf.Max(tempVec.z, (float)minSpan); // Clamp z to minimumSpan to prevent turning the model inside-out
            tempVec.y = 0;
            tipPos = tempVec;
        }

        public virtual void scaleTip(Vector3 diff)
        {
            if (!Input.GetKey(keyTipScale))
            {
                state = 0;
                return;
            }
            tipWidth += diff.x * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.right, -part.transform.up) + diff.y * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.up, -part.transform.up);
            tipWidth = Math.Max(tipWidth, 0.01);
            tipThickness += diff.x * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.right, part.transform.forward) + diff.y * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.up, part.transform.forward);
            tipThickness = Math.Max(tipThickness, 0.01);
        }

        public virtual void scaleRoot(Vector3 diff)
        {
            if (part.parent.Modules.OfType<ProceduralWing>().Any())
                return;
            // Root scaling
            // only if the root part is not a pWing, in which case the root will snap to the parent tip
            if (!Input.GetKey(keyRootScale))
            {
                state = 0;
                return;
            }
            rootWidth += diff.x * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.right, -part.transform.up) + diff.y * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.up, -part.transform.up);
            rootWidth = Math.Max(rootWidth, 0.01);
            rootThickness += diff.x * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.right, part.transform.forward) + diff.y * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.up, part.transform.forward);
            rootThickness = Math.Max(rootThickness, 0.01);
        }

        #endregion

        #region Interfaces
        public virtual float updateCost()
        {
            // Values always set
            return (float)Math.Round(wingMass * (1f + ArSweepScale / 4f) * costDensity, 1);
        }

        public float GetModuleCost(float defaultCost, ModifierStagingSituation sit)
        {
            return FuelGetAddedCost() + updateCost() - defaultCost;
        }

        public ModifierChangeWhen GetModuleCostChangeWhen()
        {
            return ModifierChangeWhen.FIXED;
        }

        public float GetModuleMass(float defaultMass, ModifierStagingSituation sit)
        {
            if (FARactive)
                return 0;
            return (float)wingMass - defaultMass;
        }

        public ModifierChangeWhen GetModuleMassChangeWhen()
        {
            return ModifierChangeWhen.FIXED;
        }

        public Vector3 GetModuleSize(Vector3 defaultSize, ModifierStagingSituation sit)
        {
            return Vector3.zero;
        }

        public ModifierChangeWhen GetModuleSizeChangeWhen()
        {
            return ModifierChangeWhen.FIXED;
        }
        #endregion

        #region debug
        public struct DebugMessage
        {
            public string message;
            public string interval;

            public DebugMessage(string m, string i)
            {
                message = m;
                interval = i;
            }
        }

        public DateTime debugTime;
        public DateTime debugTimeLast;
        public List<DebugMessage> debugMessageList = new List<DebugMessage>();
        
        /// <summary>
        /// Print debug values when 'O' is pressed.
        /// </summary>
        public void DebugValues()
        {
            if (Input.GetKeyDown(KeyCode.O))
            {
                print("tipScaleModified " + tipWidth);
                print("rootScaleModified " + rootWidth);
                print("Mass " + wingMass);
                print("ConnectionForce " + connectionForce);
                print("DeflectionLift " + Cl);
                print("ChildrenDeflectionLift " + ChildrenCl);
                print("DeflectionDrag " + Cd);
                print("Aspectratio " + aspectRatio);
                print("ArSweepScale " + ArSweepScale);
                print("Surfacearea " + surfaceArea);
                print("taperRatio " + taperRatio);
                print("MidChordSweep " + midChordSweep);
                print("MAC " + MAC);
                print("b_2 " + length);
                print("FARactive " + FARactive);
            }
        }

        public void DebugLogWithID(string method, string message)
        {
            debugTime = DateTime.UtcNow;
            string m = "WP | ID: " + part.gameObject.GetInstanceID() + " | " + method + " | " + message;
            string i = (debugTime - debugTimeLast).TotalMilliseconds + " ms.";
            if (debugMessageList.Count <= 150)
                debugMessageList.Add(new DebugMessage(m, i));
            debugTimeLast = DateTime.UtcNow;
            Debug.Log(m);
        }

        ArrowPointer pointer;
        void DrawArrow(Vector3 dir)
        {
            if (pointer == null)
                pointer = ArrowPointer.Create(part.partTransform, Vector3.zero, dir, 30, Color.red, true);
            else
                pointer.Direction = dir;
        }

        void destroyArrow()
        {
            if (pointer != null)
            {
                Destroy(pointer);
                pointer = null;
            }
        }
        #endregion

        #region Parent matching

        public virtual void inheritShape(ProceduralWing parent)
        {
            inheritBase(parent);

            tipWidth = rootWidth + ((parent.tipWidth - parent.rootWidth) / (parent.length)) * length;
            tipOffset = length / parent.length * parent.tipOffset;
            tipThickness = rootThickness + ((parent.tipThickness - parent.rootThickness) / parent.length) * length;
        }

        public virtual void inheritBase(ProceduralWing parent)
        {
            rootWidth = parent.tipWidth;
            rootThickness = parent.tipThickness;
        }

        #endregion



        #region UI stuff

        public KeyCode uiKeyCodeEdit = KeyCode.J;

        public void CreateEditorUI()
        {
            Debug.Log("creating UI");
            EditorWindow window = EditorWindow.Instance;
            PropertyGroup basegroup = window.AddPropertyGroup("Base", new Color(0.25f, 0.5f, 0.4f, 1f));
            PropertySlider p = basegroup.AddProperty("Length");
            p.Max = 8.0;
            p.Min = 0.05;
            p.onValueChanged += SetLength;
        }

        public static void SetLength(float value)
        {
            EditorWindow.currentWing.tipPos = new Vector3(EditorWindow.currentWing.tipPos.x, EditorWindow.currentWing.tipPos.y, (float)value);
        }


        #endregion
    }
}