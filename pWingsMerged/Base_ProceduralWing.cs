/* A procedural wing class that implements all the common Unity/KSP interfacing and a number of common features
 * Including:
 *  - Symmetry updating
 *  - Editing through UI
 *  - Geometry manipulation through hover and mouse drag
 *  - Fuel carrying capabilities
 *  - Stock and FAR aero interop
 *  - Save/load methods
 *  - KSP interfaces for mass/cost/size
 *
 * Common methods to override for derivatives include:
 *  - UpdateGeometry()
 *      * Manipulation of physical parameters and appearance
 *  - SetupGeometryAndAppearance()
 *      * Initialisation for any model/visual requirements
 *  - CreateUI()
 *      * Specify the appearance of the UI window for this module class
 *  - ShowEditorUI()
 *      * Changes the wing target for the UI and updates property values to match the new wing
 *  - LoadWingProperty()
 *  - SaveWingProperty()
 *      * The serialisation methods using WingProperty nodes
 *  - SetupProperties()
 *      * Initialisation of wing properties for this type
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ProceduralWings
{
    using Fuel;
    using UI;
    using Utility;

    /// <summary>
    /// methods and properties common to all wing variants. Some implementation details will be specific to the wing type
    /// </summary>
    abstract public class Base_ProceduralWing : PartModule, IPartCostModifier, IPartMassModifier, IPartSizeModifier, IModuleInfo
    {
        [KSPField(isPersistant = true)]
        public int lastLoadedVersion = 0;

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

        #region physical dimensions

        public WingProperty scale;
        public virtual double Scale // scale all parameters of this part AND any children attached to it.
        {
            get
            {
                return scale.Value;
            }
            set
            {
                scale.Value = value;
                if (part.parent?.Modules != null && part.parent.Modules.Contains<Base_ProceduralWing>())
                {
                    part.parent.Modules.GetModule<Base_ProceduralWing>().Scale = value;
                }
                StartCoroutine(updateScaleLimited());
            }
        }

        private bool scaleModifiedRunning = false;
        private bool scaleModifiedRepeat;
        private const float RATE_LIMIT = 1 / 10;
        /// <summary>
        /// run the scale update at a limited rate
        /// </summary>
        /// <returns></returns>
        private IEnumerator updateScaleLimited()
        {
            scaleModifiedRepeat = true;
            if (scaleModifiedRunning)
            {
                yield break;
            }
            scaleModifiedRunning = true;
            while (scaleModifiedRepeat)
            {
                scaleModifiedRepeat = false;
                OnScaleModified();
                yield return new WaitForSeconds(RATE_LIMIT);
            }
            scaleModifiedRunning = false;
        }

        /// <summary>
        /// should always be called through updateScaleLimited()
        /// </summary>
        protected virtual void OnScaleModified()
        {
            if (WindowManager.Window.wing == this)
            {
                WindowManager.Window.FindPropertyGroup("Base").UpdatePropertyValues(length, rootWidth, tipWidth, tipOffset, rootThickness, tipThickness); //, scale, trailingAngle, leadingAngle);
            }
            StartCoroutine(UpdateSymmetricGeometry());
        }

        protected WingProperty length;
        public virtual double Length
        {
            get
            {
                return length.Value * Scale;
            }
            set
            {
                length.Value = value / Scale;
                StartCoroutine(UpdateSymmetricGeometry());
                // force updates
                double dummy = LeadingAngle;
                dummy = TrailingAngle;
                WindowManager.Window.FindPropertyGroup("Base").UpdatePropertyValues(leadingAngle, trailingAngle);
            }
        }

        // Properties for aero calcs
        protected WingProperty tipWidth;

        public virtual double TipWidth
        {
            get
            {
                return tipWidth.Value * Scale;
            }
            set
            {
                tipWidth.Value = value / Scale;
                StartCoroutine(UpdateSymmetricGeometry());
                // force updates
                double dummy = LeadingAngle;
                dummy = TrailingAngle;
                WindowManager.Window.FindPropertyGroup("Base").UpdatePropertyValues(leadingAngle, trailingAngle);
            }
        }

        protected WingProperty tipThickness;
        public virtual double TipThickness
        {
            get
            {
                return tipThickness.Value * Scale;
            }
            set
            {
                tipThickness.Value = value / Scale;
                StartCoroutine(UpdateSymmetricGeometry());
            }
        }

        protected WingProperty tipOffset;
        public virtual double TipOffset
        {
            get
            {
                return tipOffset.Value * Scale;
            }
            set
            {
                tipOffset.Value = value / Scale;
                StartCoroutine(UpdateSymmetricGeometry());
                // force updates
                double dummy = LeadingAngle;
                dummy = TrailingAngle;
                WindowManager.Window.FindPropertyGroup("Base").UpdatePropertyValues(leadingAngle, trailingAngle);
            }
        }

        protected WingProperty rootWidth;
        public virtual double RootWidth
        {
            get
            {
                return rootWidth.Value * Scale;
            }
            set
            {
                rootWidth.Value = value / Scale;
                StartCoroutine(UpdateSymmetricGeometry());
                // force updates
                double dummy = LeadingAngle;
                dummy = TrailingAngle;
                WindowManager.Window.FindPropertyGroup("Base").UpdatePropertyValues(leadingAngle, trailingAngle);
            }
        }

        protected WingProperty rootThickness;
        public virtual double RootThickness
        {
            get
            {
                return rootThickness.Value * Scale;
            }
            set
            {
                rootThickness.Value = value / Scale;
                StartCoroutine(UpdateSymmetricGeometry());
            }
        }

        public virtual Vector3 tipPos
        {
            get
            {
                return new Vector3(-(float)tipOffset.Value, 0, (float)length.Value);
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

        // NOT PERSISTENT
        protected WingProperty leadingAngle;

        public virtual double LeadingAngle
        {
            get
            {
                leadingAngle.Value = Math.Atan2(TipOffset + (RootWidth - TipWidth) / 2, Length) * Utils.Rad2Deg;
                return leadingAngle.Value;
            }
            set
            {
                leadingAngle.Value = value;

                double currentTrailingOffset = TipOffset + TipWidth / 2;
                double newWidth = RootWidth / 2 + currentTrailingOffset - Length * Math.Tan(value * Utils.Deg2Rad);
                tipWidth.Value = Math.Max(0, newWidth) / Scale;
                tipOffset.Value = (currentTrailingOffset - TipWidth / 2 - Math.Min(newWidth, 0)) / Scale;
                trailingAngle.Value = TrailingAngle;

                WindowManager.Window.FindPropertyGroup("Base").UpdatePropertyValues(trailingAngle, tipOffset, tipWidth);
                StartCoroutine(UpdateSymmetricGeometry());
            }
        }

        // NOT PERSISTENT
        protected WingProperty trailingAngle;

        public virtual double TrailingAngle
        {
            get
            {
                trailingAngle.Value = Math.Atan2(TipOffset - (RootWidth - TipWidth) / 2, Length) * Utils.Rad2Deg;
                return trailingAngle.Value;
            }
            set
            {
                trailingAngle.Value = value;
                double currentLeadingOffset = TipOffset - TipWidth / 2;
                double newWidth = RootWidth / 2 - currentLeadingOffset + Length * Math.Tan(value * Utils.Deg2Rad);
                tipWidth.Value = Math.Max(0, newWidth);
                tipOffset.Value = currentLeadingOffset + TipWidth / 2 + Math.Min(newWidth, 0);
                leadingAngle.Value = LeadingAngle;

                WindowManager.Window.FindPropertyGroup("Base").UpdatePropertyValues(leadingAngle, tipOffset, tipWidth);
                StartCoroutine(UpdateSymmetricGeometry());
            }
        }

        #endregion physical dimensions

        #region entry points

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
                CheckAndUpgradeVersion();

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
                    scale.Save(node);
                }
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
            if (WindowManager.Window != null)
                WindowManager.Window.Visible = false;
        }

        #endregion entry points

        #region Setting up

        public bool CheckAndUpgradeVersion()
        {
            if (lastLoadedVersion > 0)
            {
                if (lastLoadedVersion < StaticWingGlobals.version)
                {
                    // do save upgrading from a previous version of this plugin
                }
            }
            else
            {
                for (int i = part.Modules.Count - 1; i >= 0; --i)
                {
                    UpgradeModules.IDeprecatedWingModule dw = part.Modules[i] as UpgradeModules.IDeprecatedWingModule;
                    if (dw != null)
                    {
                        dw.UpgradeModule(this);
                        break;
                    }
                }
            }
            lastLoadedVersion = StaticWingGlobals.version;
            return true;
        }

        /// <summary>
        /// calls all required setup elements at the correct time (delayed in flight scene)
        /// </summary>
        public virtual void Setup()
        {
            useGUILayout = false; // no use of GUILayout.xx or GUI.Window, no need for legacy layout

            SetupProperties();
            SetupGeometryAndAppearance();

            UpdateGeometry();

            if (fuelSelectedTankSetup < 0)
            {
                fuelSelectedTankSetup = 0;
                FuelTankTypeChanged();
            }

            WindowManager.GetWindow(this);

            isStarted = true;
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
                case nameof(scale):
                    scale.Load(n);
                    break;

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
                scale = new WingProperty("Scale", nameof(scale), 1.0, 2, 0.1, 10, "Set the scale for this PWing and all of it's attached PWings");
                length = new WingProperty("Length", nameof(length), 4, 2, 0.05, 16, "Lateral measurement of the wing, \nalso referred to as semispan");
                tipOffset = new WingProperty("Offset (tip)", nameof(tipOffset), 0, 2, -8, 8, "Distance between midpoints of the cross \nsections on the longitudinal axis");
                rootWidth = new WingProperty("Width (root)", nameof(rootWidth), 4, 2, 0.05, 16, "Longitudinal measurement of the wing \nat the root cross section");
                tipWidth = new WingProperty("Width (tip)", nameof(tipWidth), 4, 2, 0.05, 16, "Longitudinal measurement of the wing \nat the tip cross section");
                rootThickness = new WingProperty("Thickness (root)", nameof(rootThickness), 0.2, 2, 0.01, 1, "Thickness at the root cross section \nUsually kept proportional to edge width");
                tipThickness = new WingProperty("Thickness (tip)", nameof(tipThickness), 0.2, 2, 0.01, 1, "Thickness at the tip cross section \nUsually kept proportional to edge width");
                leadingAngle = new WingProperty("Leading Edge Angle", nameof(leadingAngle), 0, 2, -85, 85, "");
                trailingAngle = new WingProperty("Trailing Edge Angle", nameof(trailingAngle), 0, 2, -85, 85, "");
            }
            else
            {
                Base_ProceduralWing wp = part.symmetryCounterparts[0].Modules.GetModule<Base_ProceduralWing>();
                scale = wp.scale;
                length = wp.length;
                tipOffset = wp.tipOffset;
                rootWidth = wp.rootWidth;
                tipWidth = wp.tipWidth;
                rootThickness = wp.rootThickness;
                tipThickness = wp.tipThickness;
                leadingAngle = wp.leadingAngle;
                trailingAngle = wp.trailingAngle;
            }
        }

        #endregion Setting up

        #region geometry

        /// <summary>
        /// calls UpdateGeometry() on this module and any symmetry counterparts. Should replace all calls to UpdateGeometry()
        /// locked to prevent geometry update being called multiple times per frame
        /// </summary>
        private bool geometryUpdateLock;

        private UIPartActionWindow partWindow;

        public virtual IEnumerator UpdateSymmetricGeometry()
        {
            if (!isStarted || geometryUpdateLock)
                yield break;
            geometryUpdateLock = true;
            yield return null;

            UpdateGeometry();
            if (partWindow == null)
            {
                partWindow = part.GetComponent<UIPartActionWindow>();
            }
            if (partWindow != null)
            {
                partWindow.displayDirty = true;
            }

            for (int i = part.symmetryCounterparts.Count - 1; i >= 0; --i)
            {
                part.symmetryCounterparts[i].Modules.GetModule<Base_ProceduralWing>().UpdateGeometry();
            }
            geometryUpdateLock = false;
        }

        /// <summary>
        /// makes all the neccesary geometry alterations and then updates the aerodynamics to match
        /// </summary>
        public virtual void UpdateGeometry()
        {
            foreach (Part p in part.children)
            {
                Base_ProceduralWing childWing = p.Modules.GetModule<Base_ProceduralWing>();
                if (childWing != null)
                {
                    childWing.transform.position = part.transform.position + childWing.parentWingOffset * (float)Scale;
                    childWing.Scale = Scale;
                }
            }
        }

        private Vector3 parentWingOffset;
        /// <summary>
        /// called when the part is connected to a vessel in the editor. Forces a geometry rebuild
        /// </summary>
        public virtual void OnAttach()
        {
            UpdateGeometry();

            Base_ProceduralWing parentWing = part.parent.Modules.GetModule<Base_ProceduralWing>();
            if (parentWing != null)
            {
                Scale = parentWing.Scale;
                parentWingOffset = (part.transform.position - part.parent.transform.position) / (float)parentWing.Scale;
            }
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

        #endregion geometry

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
                return !(StaticWingGlobals.RFactive || StaticWingGlobals.MFTactive);
            }
        }

        /// <summary>
        /// calculate the volume of the wing fuel tank and updates and existing resources. Independent of all other fuel methods
        /// </summary>
        public virtual void FuelUpdateVolume()
        {
            if (!HighLogic.LoadedSceneIsEditor)
                return;
            if (!CanBeFueled)
            {
                fuelVolume = 0;
                return;
            }

            fuelVolume = 0.7 * Length * MAC * (RootThickness + TipThickness) / 2;
            WingTankConfiguration wtc = StaticWingGlobals.wingTankConfigurations[fuelSelectedTankSetup];
            for (int i = part.Resources.Count - 1; i >= 0; --i)
            {
                PartResource res = part.Resources[i];
                WingTankResource wres;
                if (wtc.resources.TryGetValue(res.resourceName, out wres))
                {
                    double fillPct = res.maxAmount > 0 ? res.amount / res.maxAmount : 1.0;

                    res.maxAmount = 1000 * wres.fraction * fuelVolume / wres.resource.volume;
                    res.amount = res.maxAmount * fillPct;
                }
            }
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

            if (useStockFuel)
            {
                for (int i = part.Resources.Count - 1; i >= 0; --i)
                {
                    part.Resources.Remove(part.Resources[i]);
                }

                foreach (KeyValuePair<string, WingTankResource> kvp in StaticWingGlobals.wingTankConfigurations[fuelSelectedTankSetup].resources)
                {
                    ConfigNode newResourceNode = new ConfigNode("RESOURCE");
                    newResourceNode.AddValue("name", kvp.Value.resource.name);
                    newResourceNode.AddValue("amount", 1000 * fuelVolume * kvp.Value.fraction / kvp.Value.resource.volume);
                    newResourceNode.AddValue("maxAmount", 1000 * fuelVolume * kvp.Value.fraction / kvp.Value.resource.volume);

                    part.AddResource(newResourceNode);
                }
            }
            else
            {
                PartModule module = part.Modules["ModuleFuelTanks"];
                if (module == null)
                    return;

                Type type = module.GetType();

                double volumeRF = fuelVolume * (StaticWingGlobals.RFactive ? 1000 : 173.9);
                type.GetField("volume").SetValue(module, volumeRF);
                type.GetMethod("ChangeVolume").Invoke(module, new object[] { volumeRF });
            }
        }

        /// <summary>
        /// get the additional cost of the resources in this wing
        /// </summary>
        /// <returns></returns>
        public virtual float FuelGetAddedCost()
        {
            float result = 0f;
            foreach (PartResource pr in part.Resources)
            {
                result += (float)pr.amount * PartResourceLibrary.Instance.resourceDefinitions[pr.resourceName].unitCost;
            }
            return result;
        }

        #endregion Fuel configuration switching

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
            double midChordSweep = (Utils.Rad2Deg * Math.Atan((rootPos.x - tipPos.x) / Length));
            double taperRatio = TipWidth / RootWidth;
            double aspectRatio = 2.0 * Length / MAC;
            ArSweepScale = (2.0 * Math.PI) / (2.0 + Math.Sqrt(Math.Pow(aspectRatio / Math.Cos(Utils.Deg2Rad * midChordSweep), 2.0) + 4.0)) * aspectRatio;

            Cd = dragBaseValue * dragMultiplier / ArSweepScale;
            Cl = liftFudgeNumber * MAC * Length * ArSweepScale;
            GatherChildrenCl();

            connectionForce = Math.Round(Math.Max(Math.Sqrt(Cl + ChildrenCl) * connectionFactor, connectionMinimum), 0);

            updateCost();
            part.CoMOffset = part.CoLOffset = part.CoPOffset = (tipPos - rootPos) / 2;

            // should really do something about the joint torque here, not just its limits
            part.breakingForce = Mathf.Round((float)connectionForce);
            part.breakingTorque = Mathf.Round((float)connectionForce);

            part.CoMOffset = new Vector3((float)Length / 2f, -(float)TipOffset / 2f, 0);
            // Stock-only values
            if (!StaticWingGlobals.FARactive)
                setFARModuleParams(midChordSweep, taperRatio, midChordOffsetFromOrigin);
            else
            {
                SetStockModuleParams();
            }

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
            {
                aeroFARModuleType = aeroFARModuleReference.GetType();
                aeroFARFieldInfoSemispan = aeroFARModuleType.GetField("b_2");
                aeroFARFieldInfoSemispan_Actual = aeroFARModuleType.GetField("b_2_actual");
                aeroFARFieldInfoMAC = aeroFARModuleType.GetField("MAC");
                aeroFARFieldInfoMAC_Actual = aeroFARModuleType.GetField("MAC_actual");
                aeroFARFieldInfoMidChordSweep = aeroFARModuleType.GetField("MidChordSweep");
                aeroFARFieldInfoTaperRatio = aeroFARModuleType.GetField("TaperRatio");
                aeroFARFieldInfoControlSurfaceFraction = aeroFARModuleType.GetField("ctrlSurfFrac");
                aeroFARFieldInfoRootChordOffset = aeroFARModuleType.GetField("rootMidChordOffsetFromOrig");

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

        /// <summary>
        /// setup stock aero module parameters
        /// </summary>
        public virtual void SetStockModuleParams()
        {
            // numbers for lift from: http://forum.kerbalspaceprogram.com/threads/118839-Updating-Parts-to-1-0?p=1896409&viewfull=1#post1896409
            part.Modules.GetModule<ModuleLiftingSurface>().deflectionLiftCoeff = (float)(Length * MAC / 3.52);
            wingMass = Length * MAC / 30.52; // 100kg per 0.2481m2
        }

        private float updateTimeDelay = 0;

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
            if (StaticWingGlobals.FARactive)
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

        #endregion aero stuff

        #region Wing deformation

        public virtual void OnMouseOver()
        {
            if (!(HighLogic.LoadedSceneIsEditor && isAttached))
                return;

            if (Input.GetKeyDown(StaticWingGlobals.uiKeyCodeEdit))
            {
                ShowEditorUI();
            }

            if (!deformWing)
            {
                lastMousePos = Input.mousePosition;
                if (Input.GetKeyDown(StaticWingGlobals.keyTranslation))
                    StartCoroutine(translateTip());
                else if (Input.GetKeyDown(StaticWingGlobals.keyTipScale))
                    StartCoroutine(scaleTip());
                else if (Input.GetKeyDown(StaticWingGlobals.keyRootScale))
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
            while (Input.GetKey(StaticWingGlobals.keyTranslation))
            {
                yield return null;
                diff = UpdateMouseDiff(false);

                TipOffset -= diff.x * Vector3.Dot(EditorCamera.Instance.GetComponentCached(ref editorCam).transform.right, part.transform.up) + diff.y * Vector3.Dot(EditorCamera.Instance.GetComponentCached(ref editorCam).transform.up, part.transform.up);
                Length += diff.x * Vector3.Dot(EditorCamera.Instance.GetComponentCached(ref editorCam).transform.right, part.transform.right) + diff.y * Vector3.Dot(EditorCamera.Instance.GetComponentCached(ref editorCam).transform.up, part.transform.right);
                length.Value = Math.Max(length.Value, minSpan); // Clamp z to minimumSpan to prevent turning the model inside-out
            }
            deformWing = false;
        }

        public virtual IEnumerator scaleTip()
        {
            deformWing = true;
            Vector3 diff;
            while (Input.GetKey(StaticWingGlobals.keyTipScale))
            {
                yield return null;
                diff = UpdateMouseDiff(true);

                TipWidth += diff.x * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.right, -part.transform.up) + diff.y * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.up, -part.transform.up);
                tipWidth.Value = Math.Max(tipWidth.Value, 0.01);
                TipThickness += diff.x * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.right, part.transform.forward) + diff.y * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.up, part.transform.forward);
                tipThickness.Value = Math.Max(tipThickness.Value, 0.01);
            }
            deformWing = false;
        }

        public virtual IEnumerator scaleRoot()
        {
            // root scale requires that we aren't the child part of a PWing
            if (part.parent != null && part.parent.Modules.GetModule<Base_ProceduralWing>() != null)
                yield break;

            deformWing = true;
            // Root scaling
            // only if the root part is not a pWing, in which case the root will snap to the parent tip
            Vector3 diff;
            while (Input.GetKey(StaticWingGlobals.keyRootScale))
            {
                yield return null;
                diff = UpdateMouseDiff(true);

                RootWidth += diff.x * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.right, -part.transform.up) + diff.y * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.up, -part.transform.up);
                rootWidth.Value = Math.Max(rootWidth.Value, 0.01);
                RootThickness += diff.x * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.right, part.transform.forward) + diff.y * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.up, part.transform.forward);
                RootThickness = Math.Max(RootThickness, 0.01);
            }
            deformWing = false;
        }

        public Vector3 lastMousePos;

        public virtual Vector3 UpdateMouseDiff(bool scaleMode)
        {
            float depth = EditorCamera.Instance.GetComponentCached(ref editorCam).WorldToScreenPoint(tipPos).z; // distance of tip transform from camera
            Vector3 diff = (scaleMode ? StaticWingGlobals.scaleSpeed * 20 : StaticWingGlobals.moveSpeed) * depth * (Input.mousePosition - lastMousePos) / 4500;
            lastMousePos = Input.mousePosition;
            return diff;
        }

        #endregion Wing deformation

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

        public virtual float GetModuleMass(float defaultMass, ModifierStagingSituation sit)
        {
            if (StaticWingGlobals.FARactive)
                return 0;
            return (float)wingMass - defaultMass;
        }

        public ModifierChangeWhen GetModuleMassChangeWhen()
        {
            return ModifierChangeWhen.FIXED;
        }

        public virtual Vector3 GetModuleSize(Vector3 defaultSize, ModifierStagingSituation sit)
        {
            // todo : implement size interface...
            return Vector3.zero;
        }

        public ModifierChangeWhen GetModuleSizeChangeWhen()
        {
            return ModifierChangeWhen.FIXED;
        }

        public override string GetInfo()
        {
            return "override required";
        }

        public virtual string GetModuleTitle()
        {
            return "Override the Title";
        }

        public virtual Callback<Rect> GetDrawModulePanelCallback()
        {
            return null;
        }

        public virtual string GetPrimaryField()
        {
            return "Test Primary Field";
        }

        #endregion Stock Interfaces

        #region Parent matching

        public virtual void inheritShape()
        {
            inheritBase();
            Base_ProceduralWing parent = part.parent.Modules.GetModule<Base_ProceduralWing>();
            if (parent != null)
            {
                TipWidth = RootWidth + ((parent.TipWidth - parent.RootWidth) / (parent.Length)) * Length;
                TipOffset = Length / parent.Length * parent.TipOffset;
                TipThickness = RootThickness + ((parent.TipThickness - parent.RootThickness) / parent.Length) * Length;
            }
        }

        public virtual void inheritBase()
        {
            Base_ProceduralWing parent = part.parent.Modules.GetModule<Base_ProceduralWing>();
            if (parent != null)
            {
                RootWidth = parent.TipWidth;
                RootThickness = parent.TipThickness;
            }
        }

        #endregion Parent matching

        #region UI stuff

        public static Color uiColorSliderBase = new Color(0.30f, 0.40f, 0.2f, 1f);

        public virtual void ShowEditorUI()
        {
            WindowManager.GetWindow(this);

            WindowManager.Window.FindPropertyGroup("Base").UpdatePropertyValues(length, rootWidth, tipWidth, tipOffset, rootThickness, tipThickness, scale, leadingAngle, trailingAngle);
            WindowManager.Window.Visible = true;
        }

        public virtual EditorWindow CreateMainWindow()
        {
            EditorWindow window = new EditorWindow(WindowTitle);

            PropertyGroup basegroup = window.AddPropertyGroup("Base", uiColorSliderBase);
            basegroup.AddProperty(new WingProperty(scale), x => window.wing.Scale = x); // always have scale first since other methods depend on it
            basegroup.AddProperty(new WingProperty(length), x => window.wing.Length = x * window.wing.Scale, true);
            basegroup.AddProperty(new WingProperty(rootWidth), x => window.wing.RootWidth = x * window.wing.Scale, true);
            basegroup.AddProperty(new WingProperty(tipWidth), x => window.wing.TipWidth = x * window.wing.Scale, true);
            basegroup.AddProperty(new WingProperty(tipOffset), x => window.wing.TipOffset = x * window.wing.Scale, true);
            basegroup.AddProperty(new WingProperty(rootThickness), x => window.wing.RootThickness = x * window.wing.Scale, true);
            basegroup.AddProperty(new WingProperty(tipThickness), x => window.wing.TipThickness = x * window.wing.Scale, true);
            basegroup.AddProperty(new WingProperty(leadingAngle), x => window.wing.LeadingAngle = x);
            basegroup.AddProperty(new WingProperty(trailingAngle), x => window.wing.TrailingAngle = x);

            return window;
        }

        public virtual GameObject AddMatchingButtons(EditorWindow window)
        {
            GameObject go = new GameObject();
            go.transform.SetParent(window.mainPanel.transform, false);
            UnityEngine.UI.LayoutElement layout = go.AddComponent<UnityEngine.UI.LayoutElement>();
            layout.minWidth = layout.preferredWidth = 330;
            layout.minHeight = layout.preferredHeight = 20;

            UnityEngine.UI.HorizontalLayoutGroup hrzt = go.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            hrzt.spacing = 5;
            hrzt.padding = new RectOffset(10, 10, 0, 0);

            WindowManager.AddButtonComponentToUI(go, "Match Base", () => WindowManager.Window.wing.inheritBase());
            WindowManager.AddButtonComponentToUI(go, "Match Shape", () => WindowManager.Window.wing.inheritShape());
            return go;
        }

        #endregion UI stuff

        public void Log(object formatted)
        {
            Debug.Log($"[PW Plugin: {moduleName}] " + formatted);
        }

        public void Log(string toBeFormatted, params object[] args)
        {
            Debug.Log($"[PW Plugin: {moduleName}] " + string.Format(toBeFormatted, args));
        }
    }
}