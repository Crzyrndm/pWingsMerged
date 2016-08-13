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
    /// methods and properties common to both wing variants. Some implementation details will be specific to the wing type
    /// </summary>
    abstract public class Base_ProceduralWing : PartModule, IPartCostModifier, IPartMassModifier, IPartSizeModifier
    {
        public virtual bool isCtrlSrf
        {
            get { return false; }
        }

        public override string GetInfo()
        {
            return "this is a PWing and GetInfo needs to be overridden...";
        }

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

        public virtual double minSpan
        {
            get { return length.min; }
        }
        public virtual Vector3 rootPos
        {
            get { return Vector3.zero; }
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
        public virtual double MAC
        {
            get { return (TipWidth + RootWidth) / 2; }
        }
        public virtual string FarModuleName
        {
            get { return "FARWingAerodynamicModel"; }
        }
        public virtual string WindowTitle
        {
            get
            {
                return "Wing";
            }
        }
        public virtual double Scale // scale all parameters of this part AND any children attached to it.
        {
            set
            {
                throw new NotImplementedException();
            }
        }

        // active assemblies
        public static bool assembliesChecked;
        public static bool FARactive;
        public static bool RFactive;
        public static bool MFTactive;

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
            
            StartCoroutine(flightAeroSetup());

            isStarted = true;
        }

        public virtual void OnDestroy()
        {
            GameEvents.onGameSceneLoadRequested.Remove(OnSceneSwitch);
        }

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

                if (vesselList != null)
                    vesselList.Find(vs => vs.vessel == vessel).isUpdated = false;
            }
            catch
            {
                Log("Failed to save settings");
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

        public abstract void SetupGeometryAndAppearance();

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
        #endregion

        #region geometry
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

        public virtual void OnAttach()
        {
            UpdateGeometry();
        }

        public virtual void OnDetach()
        {
            Base_ProceduralWing parentWing = part?.parent?.Modules.GetModule<Base_ProceduralWing>();
            if (parentWing != null)
            {
                parentWing.CalculateAerodynamicValues();
            }
        }

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
                    vesselListInclusive = true;
                    vesselStatusIndex = i;
                }
            }

            // If it was not included, we add it to the list
            // Correct index is then fairly obvious
            if (!vesselListInclusive)
            {
                vesselList.Add(new VesselStatus(vessel, false));
                vesselStatusIndex = vesselList.Count - 1;
            }

            // Using the index for the status list we obtained, we check whether it was updated yet. So that only one part can run the following part
            if (!vesselList[vesselStatusIndex].isUpdated)
            {
                vesselList[vesselStatusIndex].isUpdated = true;
                List<Base_ProceduralWing> moduleList = new List<Base_ProceduralWing>();

                for (int i = 0; i < vessel.parts.Count; ++i) // First we get a list of all relevant parts in the vessel. Found modules are added to a list
                    moduleList.AddRange(vessel.parts[i].Modules.GetModules<Base_ProceduralWing>());

                // After that we make two separate runs through that list. First one setting up all geometry and second one setting up aerodynamic values
                for (int i = 0; i < moduleList.Count; ++i)
                    moduleList[i].Setup();

                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();

                for (int i = 0; i < moduleList.Count; ++i)
                    moduleList[i].CalculateAerodynamicValues();
            }
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

            aeroStatVolume = length.value * MAC * (rootThickness.value + tipThickness.value) / 2;

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
                Base_ProceduralWing wing = part.symmetryCounterparts[s].Modules.GetModule<Base_ProceduralWing>();
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
        /// all wings need to be able to calc aero values but implementations can be different. Use a blank method for panels
        /// </summary>
        public virtual void CalculateAerodynamicValues()
        {
            double midChordSweep = (Utils.Rad2Deg * Math.Atan((rootPos.x - tipPos.x) / length.value));
            double taperRatio = tipWidth.value / rootWidth.value;
            double aspectRatio = 2.0 * length.value / MAC;

            ArSweepScale = Math.Pow(aspectRatio / Math.Cos(Utils.Deg2Rad * midChordSweep), 2.0) + 4.0;
            ArSweepScale = 2.0 + Math.Sqrt(ArSweepScale);
            ArSweepScale = (2.0 * Math.PI) / ArSweepScale * aspectRatio;

            wingMass = Math.Max(0.01, massFudgeNumber * MAC * Length * ((ArSweepScale * 2.0) / (3.0 + ArSweepScale)) * ((1.0 + taperRatio) / 2));

            Cd = dragBaseValue / ArSweepScale * dragMultiplier;
            Cl = liftFudgeNumber * MAC * Length * ArSweepScale;
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
                setFARModuleParams(midChordSweep, taperRatio, midChordOffsetFromOrigin);

            StartCoroutine(updateAeroDelayed());
        }

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

        public virtual void SetStockModuleParams()
        {
            // numbers for lift from: http://forum.kerbalspaceprogram.com/threads/118839-Updating-Parts-to-1-0?p=1896409&viewfull=1#post1896409
            float stockLiftCoefficient = (float)(Length * MAC / 3.52);
            part.CoMOffset.Set(Vector3.Dot(tipPos - rootPos, part.transform.right) / 2, Vector3.Dot(tipPos - rootPos, part.transform.up) / 2, 0); // CoL/P matches CoM unless otherwise specified
            part.Modules.GetModule<ModuleLiftingSurface>().deflectionLiftCoeff = stockLiftCoefficient;
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

        public virtual void GatherChildrenCl()
        {
            ChildrenCl = 0;

            // Add up the Cl and ChildrenCl of all our children to our ChildrenCl
            foreach (Part p in this.part.children)
            {
                Base_ProceduralWing child = p.Modules.GetModule<Base_ProceduralWing>();
                if (child != null)
                {
                    ChildrenCl += child.Cl;
                    ChildrenCl += child.ChildrenCl;
                }
            }

            // If parent is a pWing, trickle the call to gather ChildrenCl down to them.
            if (this.part.parent != null)
            {
                Base_ProceduralWing Parent = this.part.parent.Modules.GetModule<Base_ProceduralWing>();
                if (Parent != null)
                    Parent.GatherChildrenCl();
            }
        }

        #endregion

        #region Wing deformation

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


        public KeyCode uiKeyCodeEdit = KeyCode.J;

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