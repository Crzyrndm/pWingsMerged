using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Reflection;

namespace ProceduralWings
{
    using UI;
    using Utility;
    using Fuel;

    /// <summary>
    /// methods and properties common to all wing variants. Some implementation details will be specific to the wing type
    /// </summary>
    abstract public class Base_ProceduralWing : PartModule, IPartCostModifier, IPartMassModifier, IPartSizeModifier
    {
        public virtual bool IsCtrlSrf
        {
            get { return false; }
        }

        public virtual string WindowTitle
        {
            get
            {
                return "Wing";
            }
        }

        public override string GetInfo()
        {
            return "this is a PWing and GetInfo needs to be overridden...";
        }

        #region physical dimensions
        protected WingProperty length;
        public virtual double Length
        {
            get
            {
                return length.value;
            }
            set
            {
                length.value = value;
                UpdateSymmetricGeometry();
            }
        }

        // Properties for aero calcs
        protected WingProperty tipWidth;
        public virtual double TipWidth
        {
            get
            {
                return tipWidth.value;
            }
            set
            {
                tipWidth.value = value;
                UpdateSymmetricGeometry();
            }
        }

        protected WingProperty tipThickness;
        public virtual double TipThickness
        {
            get
            {
                return tipThickness.value;
            }
            set
            {
                tipThickness.value = value;
                UpdateSymmetricGeometry();
            }
        }

        protected WingProperty tipOffset;
        public virtual double TipOffset
        {
            get
            {
                return tipOffset.value;
            }
            set
            {
                tipOffset.value = value;
                UpdateSymmetricGeometry();
            }
        }

        protected WingProperty rootWidth;
        public virtual double RootWidth
        {
            get
            {
                return rootWidth.value;
            }
            set
            {
                rootWidth.value = value;
                UpdateSymmetricGeometry();
            }
        }

        protected WingProperty rootThickness;
        public virtual double RootThickness
        {
            get
            {
                return rootThickness.value;
            }
            set
            {
                rootThickness.value = value;
                UpdateSymmetricGeometry();
            }
        }

        public virtual Vector3 tipPos
        {
            get
            {
                return new Vector3(-(float)tipOffset.value, 0, (float)length.value);
            }
            set
            {
                Length = value.z;
                TipOffset = -value.x;
            }
        }

        public virtual Vector3 rootPos
        {
            get { return Vector3.zero; }
        }

        public virtual double minSpan
        {
            get { return length.min; }
        }

        public virtual double MAC
        {
            get { return (TipWidth + RootWidth) / 2; }
        }

        // TODO: implement scale property
        public virtual double Scale // scale all parameters of this part AND any children attached to it.
        {
            set
            {
                throw new NotImplementedException();
            }
        }
        #endregion        

        #region entry points
        public static bool loadedConfig;

        /// <summary>
        /// helper bool that prevents anything running when the start sequence hasn't fired yet (happens for various events -.-)
        /// </summary>
        public bool isStarted;

        /// <summary>
        /// shortcut to part.isattached
        /// </summary>
        public bool isAttached
        {
            get { return part.isAttached; }
        }

        /// <summary>
        /// entry point for the module
        /// </summary>
        public virtual void Start()
        {
            GameEvents.onGameSceneLoadRequested.Add(OnSceneSwitch);

            if (HighLogic.LoadedSceneIsEditor)
            {
                Setup();

                part.OnEditorAttach += new Callback(OnAttach);
                part.OnEditorDetach += new Callback(OnDetach);
            }
            else if (HighLogic.LoadedSceneIsFlight)
            {
                StartCoroutine(flightAeroSetup());
            }
            isStarted = true;
        }

        /// <summary>
        /// exit point for the module
        /// </summary>
        public virtual void OnDestroy()
        {
            GameEvents.onGameSceneLoadRequested.Remove(OnSceneSwitch);
        }

        /// <summary>
        /// called before onstart when loading a vessel
        /// </summary>
        /// <param name="node"></param>
        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);

            try
            {
                SetupProperties();

                foreach (ConfigNode n in node.GetNodes("WING_PROPERTY"))
                {
                    LoadWingProperty(n);
                }
            }
            catch
            {
                Log("failed to load wing properties");
            }
        }

        /// <summary>
        /// called before destruction to serialise the module
        /// </summary>
        /// <param name="node"></param>
        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
            try
            {
                if (length != null)
                {
                    length.Save(node);
                    tipOffset.Save(node);
                    rootWidth.Save(node);
                    tipWidth.Save(node);
                    rootThickness.Save(node);
                    tipThickness.Save(node);
                }

                if (HighLogic.LoadedSceneIsFlight)
                    vesselList.Remove(vessel.GetInstanceID());
            }
            catch
            {
                Log("Failed to save settings");
            }
        }

        /// <summary>
        /// called when a scene switch is initialised to shut down some functionality early
        /// </summary>
        /// <param name="scene"></param>
        public void OnSceneSwitch(GameScenes scene)
        {
            isStarted = false; // fixes annoying nullrefs when switching scenes and things haven't been destroyed yet
        }
        #endregion

        #region Setting up
        // active assemblies
        public static bool assembliesChecked;
        public static bool FARactive;
        public static bool RFactive;
        public static bool MFTactive;

        /// <summary>
        /// checks for presence of FAR, MFT, and RF
        /// </summary>
        public static void CheckAssemblies()
        {
            if (!assembliesChecked)
            {
                for (int i = AssemblyLoader.loadedAssemblies.Count - 1; i >= 0; --i)
                {
                    AssemblyLoader.LoadedAssembly test = AssemblyLoader.loadedAssemblies[i];
                    if (test.assembly.GetName().Name.Equals("FerramAerospaceResearch", StringComparison.InvariantCultureIgnoreCase))
                        FARactive = true;
                    else if (test.assembly.GetName().Name.Equals("RealFuels", StringComparison.InvariantCultureIgnoreCase))
                        RFactive = true;
                    else if (test.assembly.GetName().Name.Equals("modularFuelTanks", StringComparison.InvariantCultureIgnoreCase))
                        MFTactive = true;
                }
                assembliesChecked = true;
            }
        }

        /// <summary>
        /// calls all required setup elements at the correct time (delayed in flight scene)
        /// </summary>
        public virtual void Setup()
        {
            SetupProperties();
            CheckAssemblies();
            SetupGeometryAndAppearance();
            UpdateGeometry();

            if (fuelSelectedTankSetup < 0)
            {
                fuelSelectedTankSetup = 0;
                FuelTankTypeChanged();
            }
        }

        /// <summary>
        /// implementation specific first time geometry setup
        /// </summary>
        public abstract void SetupGeometryAndAppearance();

        /// <summary>
        /// handles loading from the save file
        /// </summary>
        /// <param name="n"></param>
        public virtual void LoadWingProperty(ConfigNode n)
        {
            switch (n.GetValue("ID"))
            {
                case nameof(length):
                    length.Load(n);
                    break;
                case nameof(tipOffset):
                    tipOffset.Load(n);
                    break;
                case nameof(rootWidth):
                    rootWidth.Load(n);
                    break;
                case nameof(rootThickness):
                    rootThickness.Load(n);
                    break;
                case nameof(tipWidth):
                    tipWidth.Load(n);
                    break;
                case nameof(tipThickness):
                    tipThickness.Load(n);
                    break;
                default:
                    Log($"No property of ID {n.GetValue("ID")} to load");
                    break;
            }
        }

        /// <summary>
        /// handles initialising the properties ready for use
        /// </summary>
        public virtual void SetupProperties()
        {
            if (length != null)
                return;
            if (part.symmetryCounterparts.Count == 0 || part.symmetryCounterparts[0].Modules.GetModule<Base_ProceduralWing>().length == null)
            {
                length = new WingProperty("Length", nameof(length), 4, 2, 0.05, 16);
                tipOffset = new WingProperty("Offset (tip)", nameof(tipOffset), 0, 2, -8, 8);
                rootWidth = new WingProperty("Width (root)", nameof(rootWidth), 4, 2, 0.05, 16);
                tipWidth = new WingProperty("Width (tip)", nameof(tipWidth), 4, 2, 0.05, 16);
                rootThickness = new WingProperty("Thickness (root)", nameof(rootThickness), 0.2, 2, 0.01, 1);
                tipThickness = new WingProperty("Thickness (tip)", nameof(tipThickness), 0.2, 2, 0.01, 1);
            }
            else
            {
                Base_ProceduralWing wp = part.symmetryCounterparts[0].Modules.GetModule<Base_ProceduralWing>();
                length = wp.length;
                tipOffset = wp.tipOffset;
                rootWidth = wp.rootWidth;
                tipWidth = wp.tipWidth;
                rootThickness = wp.rootThickness;
                tipThickness = wp.tipThickness;
            }
        }

        /// <summary>
        /// all vessels in flight that are loaded with this wing module are part of this set
        /// </summary>
        public static HashSet<int> vesselList = new HashSet<int>();

        /// <summary>
        /// setup the wing ready for flight
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerator flightAeroSetup()
        {
            // First we need to determine whether the vessel this part is attached to is updated
            int vesselID = vessel.GetInstanceID();
            if (vesselList.Contains(vesselID))
                yield break;
            else
                vesselList.Add(vesselID);

            Base_ProceduralWing w;
            for (int i = vessel.parts.Count - 1; i >= 0; --i)
            {
                w = vessel.parts[i].Modules.GetModule<Base_ProceduralWing>();
                if (w != null)
                {
                    w.Setup();
                }
            }

            // delay to ensure FAR is initialised
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            for (int i = vessel.parts.Count - 1; i >= 0; --i)
            {
                w = vessel.parts[i].Modules.GetModule<Base_ProceduralWing>();
                if (w != null)
                {
                    w.CalculateAerodynamicValues();
                }
            }
        }
        #endregion

        #region geometry

        /// <summary>
        /// calls UpdateGeometry() on this module and any symmetry counterparts. Should replace all calls to UpdateGeometry()
        /// </summary>
        public virtual void UpdateSymmetricGeometry()
        {
            UpdateGeometry();
            for (int i = part.symmetryCounterparts.Count - 1; i >=0; --i)
            {
                part.symmetryCounterparts[i].Modules.GetModule<Base_ProceduralWing>().UpdateGeometry();
            }
        }

        /// <summary>
        /// makes all the neccesary geometry alterations and then updates the aerodynamics to match
        /// </summary>
        public abstract void UpdateGeometry();

        /// <summary>
        /// called when the part is connected to a vessel in the editor. Forces a geometry rebuild
        /// </summary>
        public virtual void OnAttach()
        {
            UpdateGeometry();
        }

        /// <summary>
        /// called when the part is removed from the vessel in the editor. Forces an aero update of a procedural parent wing
        /// </summary>
        public virtual void OnDetach()
        {
            Base_ProceduralWing parentWing = part?.parent?.Modules.GetModule<Base_ProceduralWing>();
            if (parentWing != null)
            {
                parentWing.CalculateAerodynamicValues();
            }
        }

        #endregion

        #region Fuel configuration switching
        /// <summary>
        /// save the index of the selected fuel setup
        /// </summary>
        [KSPField(isPersistant = true)]
        public int fuelSelectedTankSetup;

        /// <summary>
        /// the volume of fuel tankage in m^3
        /// </summary>
        public double fuelVolume;

        /// <summary>
        /// if this wing can hold fuel
        /// </summary>
        public virtual bool CanBeFueled
        {
            get
            {
                return !IsCtrlSrf && StaticWingGlobals.wingTankConfigurations.Count > 0;
            }
        }

        /// <summary>
        /// true if RF and MFT are not detected
        /// </summary>
        public virtual bool useStockFuel
        {
            get
            {
                return !(RFactive || MFTactive);
            }
        }

        /// <summary>
        /// calculate the volume of the wing fuel tank and updates and existing resources. Independent of all other fuel methods
        /// </summary>
        public void FuelUpdateVolume()
        {
            if (!HighLogic.LoadedSceneIsEditor)
                return;
            if (!CanBeFueled)
            {
                fuelVolume = 0;
                return;
            }

            fuelVolume = Length * MAC * (RootThickness + TipThickness) / 2;
            for (int i = part.Resources.Count - 1; i >= 0; --i)
            {
                PartResource res = part.Resources[i];
                double fillPct = res.maxAmount > 0 ? res.amount / res.maxAmount : 1.0;
                res.maxAmount = 1000 * fuelVolume / PartResourceLibrary.Instance.resourceDefinitions[res.name].volume;
                res.amount = res.maxAmount * fillPct;
            }
            part.Resources.UpdateList();
        }

        /// <summary>
        /// advance one tank setup
        /// </summary>
        public void NextConfiguration()
        {
            if (!(CanBeFueled && useStockFuel))
                return;
            fuelSelectedTankSetup = ++fuelSelectedTankSetup % StaticWingGlobals.wingTankConfigurations.Count;
            FuelTankTypeChanged();
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
                Base_ProceduralWing wing = part.symmetryCounterparts[s].Modules.GetModule<Base_ProceduralWing>();
                if (wing != null)
                {
                    wing.fuelSelectedTankSetup = fuelSelectedTankSetup;
                    wing.FuelSetResources();
                }
            }
        }

        /// <summary>
        /// takes a volume in m^3 and sets up amounts for RF/MFT or stock based on whats installed
        /// </summary>
        public void FuelSetResources()
        {
            if (!(CanBeFueled && HighLogic.LoadedSceneIsEditor)) // resources cant be modified on an unfueled wing or outside the editor
                return;

            if (!useStockFuel)
            {
                PartModule module = part.Modules["ModuleFuelTanks"];
                if (module == null)
                    return;

                Type type = module.GetType();

                double volumeRF = fuelVolume * (RFactive ? 1000 : 173.9);
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
                    newResourceNode.AddValue("amount", kvp.Value.unitsPerVolume * fuelVolume);
                    newResourceNode.AddValue("maxAmount", kvp.Value.unitsPerVolume * fuelVolume);
                    part.AddResource(newResourceNode);
                }
                part.Resources.UpdateList();
            }
        }

        /// <summary>
        /// get the additional cost of the resources in this wing
        /// </summary>
        /// <returns></returns>
        public virtual float FuelGetAddedCost()
        {
            float result = 0f;
            foreach(PartResource pr in part.Resources)
            {
                result += (float)pr.amount * PartResourceLibrary.Instance.resourceDefinitions[pr.name].unitCost;
            }
            return result;
        }
        #endregion

        #region aero stuff
        // aero parameters
        public double ArSweepScale;
        public double Cd;
        public double Cl;
        public double ChildrenCl;
        public double wingMass;
        public double connectionForce;
        public Vector3d midChordOffsetFromOrigin = Vector3.zero; // used to calculate the impact of edges on the wing center

        public const float liftFudgeNumber = 0.0775f;
        public const float massFudgeNumber = 0.015f;
        public const float dragBaseValue = 0.6f;
        public const float dragMultiplier = 3.3939f;
        public const float connectionFactor = 150f;
        public const float connectionMinimum = 50f;

        public PartModule aeroFARModuleReference;
        public Type aeroFARModuleType;
        public MethodInfo aeroFARMethodInfoUsed;
        public FieldInfo aeroFARFieldInfoSemispan;
        public FieldInfo aeroFARFieldInfoSemispan_Actual; // to handle tweakscale, FARs wings have semispan (unscaled) and semispan_actual (tweakscaled). Need to set both (actual is the important one, and tweakscale isn't needed here, so only _actual actually needs to be set, but it would be silly to not set it)
        public FieldInfo aeroFARFieldInfoMAC;
        public FieldInfo aeroFARFieldInfoMAC_Actual; //  to handle tweakscale, FARs wings have MAC (unscaled) and MAC_actual (tweakscaled). Need to set both (actual is the important one, and tweakscale isn't needed here, so only _actual actually needs to be set, but it would be silly to not set it)
        public FieldInfo aeroFARFieldInfoMidChordSweep;
        public FieldInfo aeroFARFieldInfoTaperRatio;
        public FieldInfo aeroFARFieldInfoControlSurfaceFraction;
        public FieldInfo aeroFARFieldInfoRootChordOffset;

        public virtual string FarModuleName
        {
            get { return "FARWingAerodynamicModel"; }
        }

        /// <summary>
        /// all wings need to be able to calc aero values but implementations can be different. Use a blank method for panels
        /// </summary>
        public virtual void CalculateAerodynamicValues()
        {
            double midChordSweep = (Utils.Rad2Deg * Math.Atan((rootPos.x - tipPos.x) / length.value));
            double taperRatio = tipWidth.value / rootWidth.value;
            double aspectRatio = 2.0 * length.value / MAC;
            ArSweepScale = (2.0 * Math.PI) / (2.0 + Math.Sqrt(Math.Pow(aspectRatio / Math.Cos(Utils.Deg2Rad * midChordSweep), 2.0) + 4.0)) * aspectRatio;

            wingMass = Math.Max(0.01, massFudgeNumber * MAC * Length * ((ArSweepScale * 2.0) / (3.0 + ArSweepScale)) * ((1.0 + taperRatio) / 2));

            Cd = dragBaseValue * dragMultiplier / ArSweepScale;
            Cl = liftFudgeNumber * MAC * Length * ArSweepScale;
            GatherChildrenCl();

            connectionForce = Math.Round(Math.Max(Math.Sqrt(Cl + ChildrenCl) * connectionFactor, connectionMinimum), 0);

            updateCost();

            // should really do something about the joint torque here, not just its limits
            part.breakingForce = Mathf.Round((float)connectionForce);
            part.breakingTorque = Mathf.Round((float)connectionForce);

            // Stock-only values
            if (!FARactive)
                setFARModuleParams(midChordSweep, taperRatio, midChordOffsetFromOrigin);
            else
                SetStockModuleParams();

            StartCoroutine(updateAeroDelayed());
        }

        /// <summary>
        /// setup FAR aero module parameters
        /// </summary>
        /// <param name="midChordSweep"></param>
        /// <param name="taperRatio"></param>
        /// <param name="midChordOffset"></param>
        public virtual void setFARModuleParams(double midChordSweep, double taperRatio, Vector3 midChordOffset)
        {
            if (aeroFARModuleReference == null)
            {
                if (part.Modules.Contains(FarModuleName))
                    aeroFARModuleReference = part.Modules[FarModuleName];
            }
            if (aeroFARModuleReference == null)
                return;

            if (aeroFARModuleType == null)
                aeroFARModuleType = aeroFARModuleReference.GetType();
            if (aeroFARModuleType != null)
            {
                if (aeroFARFieldInfoSemispan == null)
                    aeroFARFieldInfoSemispan = aeroFARModuleType.GetField("b_2");
                if (aeroFARFieldInfoSemispan_Actual == null)
                    aeroFARFieldInfoSemispan_Actual = aeroFARModuleType.GetField("b_2_actual");
                if (aeroFARFieldInfoMAC == null)
                    aeroFARFieldInfoMAC = aeroFARModuleType.GetField("MAC");
                if (aeroFARFieldInfoMAC_Actual == null)
                    aeroFARFieldInfoMAC_Actual = aeroFARModuleType.GetField("MAC_actual");
                if (aeroFARFieldInfoMidChordSweep == null)
                    aeroFARFieldInfoMidChordSweep = aeroFARModuleType.GetField("MidChordSweep");
                if (aeroFARFieldInfoTaperRatio == null)
                    aeroFARFieldInfoTaperRatio = aeroFARModuleType.GetField("TaperRatio");
                if (aeroFARFieldInfoControlSurfaceFraction == null)
                    aeroFARFieldInfoControlSurfaceFraction = aeroFARModuleType.GetField("ctrlSurfFrac");
                if (aeroFARFieldInfoRootChordOffset == null)
                    aeroFARFieldInfoRootChordOffset = aeroFARModuleType.GetField("rootMidChordOffsetFromOrig");

                if (aeroFARMethodInfoUsed == null)
                {
                    aeroFARMethodInfoUsed = aeroFARModuleType.GetMethod("StartInitialization");
                }
                if (aeroFARMethodInfoUsed != null)
                {
                    aeroFARFieldInfoSemispan.SetValue(aeroFARModuleReference, length);
                    aeroFARFieldInfoSemispan_Actual.SetValue(aeroFARModuleReference, length);
                    aeroFARFieldInfoMAC.SetValue(aeroFARModuleReference, MAC);
                    aeroFARFieldInfoMAC_Actual.SetValue(aeroFARModuleReference, MAC);
                    aeroFARFieldInfoMidChordSweep.SetValue(aeroFARModuleReference, midChordSweep);
                    aeroFARFieldInfoTaperRatio.SetValue(aeroFARModuleReference, taperRatio);
                    aeroFARFieldInfoRootChordOffset.SetValue(aeroFARModuleReference, midChordOffset);

                    aeroFARMethodInfoUsed.Invoke(aeroFARModuleReference, null);
                }
            }
        }

        /// <summary>
        /// setup stock aero module parameters
        /// </summary>
        public virtual void SetStockModuleParams()
        {
            // numbers for lift from: http://forum.kerbalspaceprogram.com/threads/118839-Updating-Parts-to-1-0?p=1896409&viewfull=1#post1896409
            float stockLiftCoefficient = (float)(Length * MAC / 3.52);
            part.CoMOffset.Set(Vector3.Dot(tipPos - rootPos, part.transform.right) / 2, Vector3.Dot(tipPos - rootPos, part.transform.up) / 2, 0); // CoL/P matches CoM unless otherwise specified
            part.Modules.GetModule<ModuleLiftingSurface>().deflectionLiftCoeff = stockLiftCoefficient;
            part.mass = stockLiftCoefficient * 0.1f;
        }

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
                if (part.Modules.Contains(FarModuleName))
                {
                    PartModule FARmodule = part.Modules[FarModuleName];
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

        /// <summary>
        /// collect Cl of child wings
        /// </summary>
        public virtual void GatherChildrenCl()
        {
            ChildrenCl = 0;

            // Add up the Cl and ChildrenCl of all our children to our ChildrenCl
            foreach (Part p in part.children)
            {
                Base_ProceduralWing child = p.Modules.GetModule<Base_ProceduralWing>();
                if (child != null)
                {
                    ChildrenCl += child.Cl;
                    ChildrenCl += child.ChildrenCl;
                }
            }

            // If parent is a pWing, trickle the call to gather ChildrenCl down to them.
            if (part.parent != null)
            {
                Base_ProceduralWing Parent = part.parent.Modules.GetModule<Base_ProceduralWing>();
                if (Parent != null)
                    Parent.GatherChildrenCl();
            }
        }

        #endregion

        #region Wing deformation

        public static KeyCode keyTranslation = KeyCode.G;
        public static KeyCode keyTipScale = KeyCode.T;
        public static KeyCode keyRootScale = KeyCode.B;
        public static float moveSpeed = 5.0f;
        public static float scaleSpeed = 0.25f;

        public virtual void OnMouseOver()
        {
            if (!(HighLogic.LoadedSceneIsEditor && isAttached))
                return;

            if (Input.GetKeyDown(uiKeyCodeEdit))
            {
                ShowEditorUI();
            }
            
            if (!deformWing)
            {
                lastMousePos = Input.mousePosition;
                if (Input.GetKeyDown(keyTranslation))
                    StartCoroutine(translateTip());
                else if (Input.GetKeyDown(keyTipScale))
                    StartCoroutine(scaleTip());
                else if (Input.GetKeyDown(keyRootScale))
                    StartCoroutine(scaleRoot());
            }
        }

        /// <summary>
        /// respond to key/mouse input used to shape the wing
        /// </summary>
        public static Camera editorCam;
        public bool deformWing;
        public virtual IEnumerator translateTip()
        {
            deformWing = true;
            Vector3 diff;
            while (Input.GetKey(keyTranslation))
            {
                yield return null;
                diff = UpdateMouseDiff(false);
                
                TipOffset -= diff.x * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.right, part.transform.up) + diff.y * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.up, part.transform.up);
                length.value += diff.x * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.right, part.transform.right) + diff.y * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.up, part.transform.right);
                Length = Math.Max(length.value, minSpan); // Clamp z to minimumSpan to prevent turning the model inside-out
            }
            deformWing = false;
        }

        public virtual IEnumerator scaleTip()
        {
            deformWing = true;
            Vector3 diff;
            while (Input.GetKey(keyTipScale))
            {
                yield return null;
                diff = UpdateMouseDiff(true);

                tipWidth.value += diff.x * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.right, -part.transform.up) + diff.y * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.up, -part.transform.up);
                TipWidth = Math.Max(tipWidth.value, 0.01);
                tipThickness.value += diff.x * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.right, part.transform.forward) + diff.y * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.up, part.transform.forward);
                TipThickness = Math.Max(tipThickness.value, 0.01);
            }
            deformWing = false;
        }

        public virtual IEnumerator scaleRoot()
        {
            // root scale requires that we aren't the child part of a PWing
            if (part.parent == null || part.parent.Modules.GetModule<Base_ProceduralWing>() == null)
                yield break;
                
            deformWing = true;
            // Root scaling
            // only if the root part is not a pWing, in which case the root will snap to the parent tip
            Vector3 diff;
            while (Input.GetKey(keyRootScale))
            {
                yield return null;
                diff = UpdateMouseDiff(true);

                rootWidth.value += diff.x * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.right, -part.transform.up) + diff.y * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.up, -part.transform.up);
                RootWidth = Math.Max(rootWidth.value, 0.01);
                RootThickness += diff.x * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.right, part.transform.forward) + diff.y * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.up, part.transform.forward);
                RootThickness = Math.Max(RootThickness, 0.01);
            }
            deformWing = false;
        }

        public Vector3 lastMousePos;
        public virtual Vector3 UpdateMouseDiff(bool scaleMode)
        {
            float depth = EditorCamera.Instance.GetComponentCached(ref editorCam).WorldToScreenPoint(tipPos).z; // distance of tip transform from camera
            Vector3 diff = (scaleMode ? scaleSpeed * 20 : moveSpeed) * depth * (Input.mousePosition - lastMousePos) / 4500;
            lastMousePos = Input.mousePosition;
            return diff;
        }

        #endregion

        #region Stock Interfaces
        // module cost variables
        public float wingCost;
        public const float costDensity = 5300f;
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

        #region Parent matching

        public virtual void inheritShape(Base_ProceduralWing parent)
        {
            inheritBase(parent);

            TipWidth = RootWidth + ((parent.TipWidth - parent.RootWidth) / (parent.Length)) * Length;
            TipOffset = Length / parent.Length * parent.TipOffset;
            TipThickness = RootThickness + ((parent.TipThickness - parent.RootThickness) / parent.Length) * Length;
        }

        public virtual void inheritBase(Base_ProceduralWing parent)
        {
            RootWidth = parent.TipWidth;
            RootThickness = parent.TipThickness;
        }

        #endregion

        #region UI stuff
        public static KeyCode uiKeyCodeEdit = KeyCode.J;

        public static Vector4 uiColorSliderBase = new Vector4(0.25f, 0.5f, 0.4f, 1f);

        public virtual void ShowEditorUI()
        {
            WindowManager.GetWindow(this);

            WindowManager.Window.wing = this;
            WindowManager.Window.FindPropertyGroup("Base").UpdatePropertyValues(length, rootWidth, tipWidth, tipOffset, rootThickness, tipThickness);
            WindowManager.Window.Visible = true;
        }

        public virtual EditorWindow CreateWindow()
        {
            EditorWindow window = new EditorWindow();

            PropertyGroup basegroup = window.AddPropertyGroup("Base", UIUtility.ColorHSBToRGB(uiColorSliderBase));
            basegroup.AddProperty(new WingProperty(length), x => window.wing.Length = x);
            basegroup.AddProperty(new WingProperty(rootWidth), x => window.wing.RootWidth = x);
            basegroup.AddProperty(new WingProperty(tipWidth), x => window.wing.TipWidth = x);
            basegroup.AddProperty(new WingProperty(tipOffset), x => window.wing.TipOffset = x);
            basegroup.AddProperty(new WingProperty(rootThickness), x => window.wing.RootThickness = x);
            basegroup.AddProperty(new WingProperty(tipThickness), x => window.wing.TipThickness = x);

            return window;
        }
        #endregion

        public static void Log(object formatted)
        {
            Debug.Log("[B9PW] " + formatted);
        }

        public static void Log(string toBeFormatted, params object[] args)
        {
            Debug.Log("[B9PW] " + string.Format(toBeFormatted, args));
        }
    }
}