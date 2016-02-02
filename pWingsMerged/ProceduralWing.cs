﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

using ProceduralWings.UI;

namespace ProceduralWings
{
    /// <summary>
    /// methods and properties common to both wing variants. Some implementations will be specific to the wing type
    /// </summary>
    abstract public class ProceduralWing : PartModule, IPartCostModifier, /*IPartMassModifier, */IPartSizeModifier
    {
        public Dictionary<string, UIDragField> wingVars = new Dictionary<string, UIDragField>();
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
        #warning nullrefs and incorrect positions...
        public virtual Vector3 rootPos
        {
            get { return isAttached ? part.attachJoint.transform.position : part.transform.position; }
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
        public int fuelSelectedTankSetup = -1;
        public double aeroStatVolume;

        // module cost variables
        public float wingCost;
        public const float costDensity = 5300f;

        public bool isStarted; // helper bool that prevents anything running when the start sequence hasn't fired yet
        public bool isAttached
        {
            get { return part.isAttached; }
        }

        List<UIFieldGroup> UIGroups;

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

            SetupUI();

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
            UpdateUI();
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
                ProceduralWingManager.SaveConfigs();
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

        public virtual void SetupUI() 
        {
            UIGroups = new List<UIFieldGroup>(); // only need to init this in the editor

            UIFieldGroup baseGroup = new UIFieldGroup();
            baseGroup.Label = "Base";

            baseGroup.fieldsToDraw.Add(new UIDragField("Length", "Lateral measurement of the wing, \nalso referred to as semispan", uiLengthLimit, new Vector2d(incrementMain, 1.0), 2.0));
            baseGroup.fieldsToDraw.Add(new UIDragField("Width (root)", "Longitudinal measurement of the wing \nat the root cross section", uiLengthLimit, new Vector2d(incrementMain, 1.0), 2.0));
            baseGroup.fieldsToDraw.Add(new UIDragField("Width (tip)", "Longitudinal measurement of the wing \nat the tip cross section", uiLengthLimit, new Vector2d(incrementMain, 1.0), 2.0));
            baseGroup.fieldsToDraw.Add(new UIDragField("Offset (tip)", "Distance between midpoints of the cross \nsections on the longitudinal axis", uiLengthLimit, new Vector2d(incrementMain, 1.0), 2.0));
            baseGroup.fieldsToDraw.Add(new UIDragField("Thickness (root)", "Thickness at the root cross section \nUsually kept proportional to edge width", uiLengthLimit, new Vector2d(incrementMain, 1.0), 2.0));
            baseGroup.fieldsToDraw.Add(new UIDragField("Thickness (tip)", "Thickness at the tip cross section \nUsually kept proportional to edge width", uiLengthLimit, new Vector2d(incrementMain, 1.0), 2.0));

            UIGroups.Add(baseGroup);
        }

        public abstract void SetupGeometryAndAppearance();
        #endregion

        /// <summary>
        /// handles any UI changes if neccesary
        /// </summary>
        public virtual void UpdateUI() { }

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
            UpdateUI();
        }

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

            aeroStatVolume = length * MAC * (rootThickness + tipThickness) / 2;

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

        public virtual bool canBeFueled
        {
            get
            {
                return ProceduralWingManager.wingTankConfigurations.Count > 0;
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
            foreach (KeyValuePair<string, WingTankResource> kvp in ProceduralWingManager.wingTankConfigurations[fuelSelectedTankSetup].resources)
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

            if (!uiEditModeTimeout)
            {
                if (uiEditMode && Input.GetKeyDown(KeyCode.Mouse1))
                {
                    uiEditMode = false;
                    uiEditModeTimeout = true;
                }
                else if (Input.GetKeyDown(uiKeyCodeEdit))
                {
                    uiInstanceIDTarget = part.GetInstanceID();
                    uiEditMode = true;
                    uiEditModeTimeout = true;
                    uiAdjustWindow = true;
                    uiWindowActive = true;
                    //stockButton.SetTrue(false);
                    // inheritance update
                }
            }
            
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
        public virtual void DeformWing()
        {
            if (!isAttached || state == 0)
                return;

            float depth = EditorCamera.Instance.camera.WorldToScreenPoint(state != 3 ? tipPos : rootPos).z; // distance of tip transform from camera
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
            tempVec.x += diff.x * Vector3.Dot(EditorCamera.Instance.camera.transform.right, part.transform.up) + diff.y * Vector3.Dot(EditorCamera.Instance.camera.transform.up, part.transform.up);
            tempVec.z += diff.x * Vector3.Dot(EditorCamera.Instance.camera.transform.right, part.transform.right) + diff.y * Vector3.Dot(EditorCamera.Instance.camera.transform.up, part.transform.right);
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
            tipWidth += diff.x * Vector3.Dot(EditorCamera.Instance.camera.transform.right, -part.transform.up) + diff.y * Vector3.Dot(EditorCamera.Instance.camera.transform.up, -part.transform.up);
            tipWidth = Math.Max(tipWidth, 0.01);
            tipThickness += diff.x * Vector3.Dot(EditorCamera.Instance.camera.transform.right, part.transform.forward) + diff.y * Vector3.Dot(EditorCamera.Instance.camera.transform.up, part.transform.forward);
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
            rootWidth += diff.x * Vector3.Dot(EditorCamera.Instance.camera.transform.right, -part.transform.up) + diff.y * Vector3.Dot(EditorCamera.Instance.camera.transform.up, -part.transform.up);
            rootWidth = Math.Max(rootWidth, 0.01);
            rootThickness += diff.x * Vector3.Dot(EditorCamera.Instance.camera.transform.right, part.transform.forward) + diff.y * Vector3.Dot(EditorCamera.Instance.camera.transform.up, part.transform.forward);
            rootThickness = Math.Max(rootThickness, 0.01);
        }

        #endregion

        #region Interfaces
        public virtual float updateCost()
        {
            // Values always set
            return (float)Math.Round(wingMass * (1f + ArSweepScale / 4f) * costDensity, 1);
        }

        public float GetModuleCost(float defaultCost)
        {
            return updateCost();
        }

        // is this doing silly stuff just for FAR or in stock as well? Need to spend some time investigating
        //public float GetModuleMass(float defaultMass)
        //{
        //    return part.mass - part.partInfo.partPrefab.mass;
        //}

        public Vector3 GetModuleSize(Vector3 defaultSize)
        {
            return Vector3.zero; // should do this properly at some point
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
        public bool isSetToDefaultValues = false;
        public static bool uiAdjustWindow = true;
        public virtual double SetupFieldValue(double value, Vector2d limits, double defaultValue)
        {
            if (!isSetToDefaultValues)
                return defaultValue;
            else
                return Utils.Clamp(value, limits.x, limits.y);
        }

        public static string uiLastFieldName = "";
        public static string uiLastFieldTooltip = "Additional info on edited \nproperties is displayed here";



        public static Vector2 GetVertexUV2(float selectedLayer)
        {
            if (selectedLayer == 0)
                return new Vector2(0f, 1f);
            else
                return new Vector2((selectedLayer - 1f) / 3f, 0f);
        }



        public virtual bool CheckFieldValue(float fieldValue, ref float fieldCache)
        {
            if (fieldValue != fieldCache)
            {
                if (WPDebug.logUpdate)
                    DebugLogWithID("Update", "Detected value change");
                fieldCache = fieldValue;
                return true;
            }

            return false;
        }

        public virtual void OnGUI()
        {
            if (!isStarted || !HighLogic.LoadedSceneIsEditor || !uiWindowActive)
                return;

            if (uiInstanceIDLocal == 0)
                uiInstanceIDLocal = part.GetInstanceID();
            if (uiInstanceIDTarget == uiInstanceIDLocal || uiInstanceIDTarget == 0)
            {
                if (!ProceduralWingManager.uiStyleConfigured)
                    ProceduralWingManager.ConfigureStyles();

                if (uiAdjustWindow)
                {
                    uiAdjustWindow = false;
                    if (WPDebug.logPropertyWindow)
                        DebugLogWithID("OnGUI", "Window forced to adjust");
                    ProceduralWingManager.uiRectWindowEditor = GUILayout.Window(273, ProceduralWingManager.uiRectWindowEditor, OnWindow, GetWindowTitle(), ProceduralWingManager.uiStyleWindow, GUILayout.Height(0));
                }
                else
                    ProceduralWingManager.uiRectWindowEditor = GUILayout.Window(273, ProceduralWingManager.uiRectWindowEditor, OnWindow, GetWindowTitle(), ProceduralWingManager.uiStyleWindow);

                // Thanks to ferram4
                // Following section lock the editor, preventing window clickthrough

                if (ProceduralWingManager.uiRectWindowEditor.Contains(UIUtility.GetMousePos()))
                {
                    EditorLogic.fetch.Lock(false, false, false, "WingProceduralWindow");
                    EditorTooltip.Instance.HideToolTip();
                }
                else
                    EditorLogic.fetch.Unlock("WingProceduralWindow");
            }
        }

        public virtual string GetWindowTitle()
        {
            return "Wing";
        }

        public static bool uiEditModeTimeout = false;
        public static bool uiEditMode = false;
        public float uiEditModeTimeoutDuration = 0.25f;
        public float uiEditModeTimer = 0f;
        public KeyCode uiKeyCodeEdit = KeyCode.J;

        public virtual void StopWindowTimeout()
        {
            uiAdjustWindow = true;
            uiEditModeTimeout = false;
            uiEditModeTimer = 0.0f;

        }

        public virtual void ExitEditMode()
        {
            uiEditMode = false;
            uiEditModeTimeout = true;
            uiAdjustWindow = true;
        }

        public static bool uiWindowActive = true;
        public static bool displayDimensions;
        public virtual void OnWindow(int window)
        {
            if (uiEditMode)
            {
                bool returnEarly = false;
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                if (uiLastFieldName.Length > 0)
                    GUILayout.Label("Last: " + uiLastFieldName, ProceduralWingManager.uiStyleLabelMedium);
                else
                    GUILayout.Label("Property editor", ProceduralWingManager.uiStyleLabelMedium);
                if (uiLastFieldTooltip.Length > 0)
                    GUILayout.Label(uiLastFieldTooltip + "\n_________________________", ProceduralWingManager.uiStyleLabelHint, GUILayout.MaxHeight(44f), GUILayout.MinHeight(44f)); // 58f for four lines
                GUILayout.EndVertical();
                if (GUILayout.Button("Close", ProceduralWingManager.uiStyleButton, GUILayout.MaxWidth(50f)))
                {
                    EditorLogic.fetch.Unlock("WingProceduralWindow");
                    uiWindowActive = false;
                    stockButton.SetFalse(false);
                    returnEarly = true;
                }
                GUILayout.EndHorizontal();
                if (returnEarly)
                    return;

                drawEditFields();

                GUILayout.Label("_________________________\n\nPress J to exit edit mode\nOptions below allow you to change default values", ProceduralWingManager.uiStyleLabelHint);
                if (canBeFueled && useStockFuel)
                {
                    if (GUILayout.Button(ProceduralWingManager.wingTankConfigurations[fuelSelectedTankSetup].ConfigurationName + " | Next tank setup", ProceduralWingManager.uiStyleButton))
                        NextConfiguration();
                }

                drawOptions();
            }
            else
            {
                if (uiEditModeTimeout)
                    GUILayout.Label("Exiting edit mode...\n", ProceduralWingManager.uiStyleLabelMedium);
                else
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Press J while pointing at a\nprocedural part to edit it", ProceduralWingManager.uiStyleLabelHint);
                    if (GUILayout.Button("Close", ProceduralWingManager.uiStyleButton, GUILayout.MaxWidth(50f)))
                    {
                        uiWindowActive = false;
                        stockButton.SetFalse(false);
                        uiAdjustWindow = true;
                        EditorLogic.fetch.Unlock("WingProceduralWindow");
                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUI.DragWindow();
        }

        public virtual void drawEditFields()
        {
            double[] val = new double[] { length, rootWidth, tipWidth, tipOffset, rootThickness, tipThickness };
            UIGroups[0].drawGroup(ref val);
            length = val[0];
            rootWidth = val[1];
            tipWidth = val[2];
            tipOffset = val[3];
            rootThickness = val[4];
            tipThickness = val[5];
        }

        public static double incrementMain = 0.125, incrementSmall = 0.04;
        public static Vector2d uiLengthLimit = new Vector2d(0.125, 16);
        public static Vector2d uiRootLimit = new Vector2d(0.125, 16);
        public static Vector2d uiTipLimit = new Vector2d(0.0000001, 16);
        public static Vector2d uiOffsetLimit = new Vector2d(-8, 8);
        public static Vector2d uiThicknessLimit = new Vector2d(0.04, 1);
        public static Vector4 baseColour = new Vector4(0.25f, 0.5f, 0.4f, 1f);


        public virtual void drawOptions()
        {
            //GUILayout.BeginHorizontal();
            //if (GUILayout.Button("Shape", ProceduralWingManager.uiStyleButton))
            //    InheritParentValues(0);
            //if (GUILayout.Button("Base", ProceduralWingManager.uiStyleButton))
            //    InheritParentValues(1);
            //if (GUILayout.Button("Edges", ProceduralWingManager.uiStyleButton))
            //    InheritParentValues(2);
            //GUILayout.EndHorizontal();
        }
        #endregion

        #region stockToolbar
        public static ApplicationLauncherButton stockButton = null;
        public static int uiInstanceIDTarget = 0, uiInstanceIDLocal = 0;

        public virtual void OnStockButtonSetup()
        {
            stockButton = ApplicationLauncher.Instance.AddModApplication(OnStockButtonClick, OnStockButtonClick, null, null, null, null, ApplicationLauncher.AppScenes.SPH, (Texture)GameDatabase.Instance.GetTexture("B9_Aerospace/Plugins/icon_stock", false));
        }

        public void OnStockButtonClick()
        {
            uiWindowActive = !uiWindowActive;
        }

        //public void editorAppDestroy()
        //{
        //    if (!HighLogic.LoadedSceneIsEditor)
        //        return;

        //    bool stockButtonCanBeRemoved = true;
        //    //WingProcedural[] components = GameObject.FindObjectsOfType<WingProcedural>();
        //    if (WPDebug.logEvents)
        //        DebugLogWithID("OnDestroy", "Invoked, with " + components.Length + " remaining components in the scene");
        //    for (int i = 0; i < components.Length; ++i)
        //    {
        //        if (components[i] != null)
        //            stockButtonCanBeRemoved = false;
        //    }
        //    if (stockButtonCanBeRemoved)
        //    {
        //        uiInstanceIDTarget = 0;
        //        if (stockButton != null)
        //        {
        //            ApplicationLauncher.Instance.RemoveModApplication(stockButton);
        //            stockButton = null;
        //        }
        //    }
        //}

        #endregion
    }
}