using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using System.Reflection;

namespace ProceduralWings
{
    public class WingManipulator : ProceduralWing
    {
        // PartModule Dimensions
        [KSPField]
        public float modelChordLenght = 2f;

        [KSPField]
        public float modelControlSurfaceFraction = 1f;

        [KSPField]
        public float modelMinimumSpan = 0.05f;

        [KSPField]
        public Vector3 TipSpawnOffset = Vector3.forward;

        // PartModule Part type
        [KSPField]
        public bool symmetricMovement = false;

        [KSPField]
        public bool doNotParticipateInParentSnapping = false;

        [KSPField]
        public bool updateChildren = true;

        [KSPField(isPersistant = true)]
        public bool relativeThicknessScaling = true;

        // PartModule Tuning parameters
        [KSPField]
        public float liftFudgeNumber = 0.0775f;

        [KSPField]
        public float massFudgeNumber = 0.015f;

        [KSPField]
        public float dragBaseValue = 0.6f;

        [KSPField]
        public float dragMultiplier = 3.3939f;

        [KSPField]
        public float connectionFactor = 150f;

        [KSPField]
        public float connectionMinimum = 50f;

        [KSPField]
        public float costDensity = 5300f;

        [KSPField]
        public float costDensityControl = 6500f;

        // Commong config
        public static bool loadedConfig;
        public static KeyCode keyTranslation = KeyCode.G;
        public static KeyCode keyTipScale = KeyCode.T;
        public static KeyCode keyRootScale = KeyCode.B; // was r, stock uses r now though
        public static float moveSpeed = 5.0f;
        public static float scaleSpeed = 0.25f;

        // Internals
        public Transform Tip;
        public Transform Root;

        private Mesh baked;

        public SkinnedMeshRenderer wingSMR;
        public Transform wingTransform;
        public Transform SMRcontainer;

        private bool justDetached = false;

        // Internal Fields

        [KSPField(isPersistant = true)]
        public Vector3 tipScale = Vector3.one;

        [KSPField(isPersistant = true)]
        public Vector3 tipPosition = Vector3.zero;

        [KSPField(isPersistant = true)]
        public Vector3 rootPosition = Vector3.zero;

        [KSPField(isPersistant = true)]
        public Vector3 rootScale = Vector3.one;

        [KSPField(isPersistant = true)]
        public bool IgnoreSnapping = false;

        [KSPField(isPersistant = true)]
        public bool SegmentRoot = true;

        [KSPField(isPersistant = true)]
        public bool IsAttached = false;

        [KSPEvent(guiName = "Match Taper Ratio")]
        public void MatchTaperEvent()
        {
            // Check for a valid parent
                // Get parents taper
            WingManipulator parentWing = part.parent.Modules.OfType<WingManipulator>().FirstOrDefault();
            if (parentWing == null)
                return;
            Vector3 changeTipScale = (float)(b_2 / parentWing.b_2) * (parentWing.tipScale - parentWing.rootScale);

            // Scale the tip
            tipScale.Set(
                Mathf.Max(rootScale.x + changeTipScale.x, 0.01f),
                Mathf.Max(rootScale.y + changeTipScale.y, 0.01f),
                Mathf.Max(rootScale.z + changeTipScale.z, 0.01f));

            // Update part and children
            UpdateAllCopies(true);
        }

        #region aerodynamics

        [KSPField(guiActiveEditor = false, guiName = "Coefficient of Drag", guiFormat = "F3")]
        public float guiCd;

        [KSPField(guiActiveEditor = false, guiName = "Coefficient of Lift", guiFormat = "F3")]
        public float guiCl;

        [KSPField(guiActiveEditor = false, guiName = "Mass", guiFormat = "F3", guiUnits = "t")]
        public float guiWingMass;

        [KSPField(guiActiveEditor = false, guiName = "Mean Aerodynamic Chord", guiFormat = "F3", guiUnits = "m")]
        public float guiMAC;

        [KSPField(guiActiveEditor = false, guiName = "Semi-Span", guiFormat = "F3", guiUnits = "m")]
        public float guiB_2;

        [KSPField(guiActiveEditor = false, guiName = "Mid-Chord Sweep", guiFormat = "F3", guiUnits = "deg.")]
        public float guiMidChordSweep;

        [KSPField(guiActiveEditor = false, guiName = "Taper Ratio", guiFormat = "F3")]
        public float guiTaperRatio;

        [KSPField(guiActiveEditor = false, guiName = "Surface Area", guiFormat = "F3", guiUnits = "m²")]
        public float guiSurfaceArea;

        [KSPField(guiActiveEditor = false, guiName = "Aspect Ratio", guiFormat = "F3")]
        public float guiAspectRatio;

        protected bool triggerUpdate = false; // if this is true, an update will be done and it set false.
        // this will set the triggerUpdate field true on all wings on the vessel.
        public void TriggerUpdateAllWings()
        {
            List<Part> plist = new List<Part>();
            if (HighLogic.LoadedSceneIsEditor)
                plist = EditorLogic.SortedShipList;
            else
                plist = part.vessel.Parts;
            for (int i = 0; i < plist.Count; i++)
            {
                WingManipulator wing = plist[i].Modules.OfType<WingManipulator>().FirstOrDefault();
                if (wing != null)
                    wing.triggerUpdate = true;
            }
        }

        // This method calculates part values such as mass, lift, drag and connection forces, as well as all intermediates.
        public void CalculateAerodynamicValues()
        {
            if (!isWing && !isCtrlSrf)
                return;
            // Calculate intemediate values
            //print(part.name + ": Calc Aero values");
            b_2 = tipPosition.z - Root.localPosition.z + 1.0;

            MAC = (tipScale.x + rootScale.x) * modelChordLenght / 2.0;

            midChordSweep = (Rad2Deg * Math.Atan((Root.localPosition.x - tipPosition.x) / b_2));

            taperRatio = tipScale.x / rootScale.x;

            surfaceArea = MAC * b_2;

            aspectRatio = 2.0 * b_2 / MAC;

            ArSweepScale = Math.Pow(aspectRatio / Math.Cos(Deg2Rad * midChordSweep), 2.0) + 4.0;
            ArSweepScale = 2.0 + Math.Sqrt(ArSweepScale);
            ArSweepScale = (2.0 * Math.PI) / ArSweepScale * aspectRatio;

            wingMass = Math.Max(0.01, massFudgeNumber * surfaceArea * ((ArSweepScale * 2.0) / (3.0 + ArSweepScale)) * ((1.0 + taperRatio) / 2));

            Cd = dragBaseValue / ArSweepScale * dragMultiplier;

            Cl = liftFudgeNumber * surfaceArea * ArSweepScale;

            //print("Gather Children");
            GatherChildrenCl();

            connectionForce = Math.Round(Math.Max(Math.Sqrt(Cl + ChildrenCl) * connectionFactor, connectionMinimum), 0);

            // Values always set
            if (isWing)
                wingCost = (float)Math.Round(wingMass * (1f + ArSweepScale / 4f) * costDensity, 1);
            else // ctrl surfaces
                wingCost = (float)Math.Round(wingMass * (1f + ArSweepScale / 4f) * (costDensity * (1f - modelControlSurfaceFraction) + costDensityControl * modelControlSurfaceFraction), 1);

            // should really do something about the joint torque here, not just its limits
            part.breakingForce = Mathf.Round((float)connectionForce);
            part.breakingTorque = Mathf.Round((float)connectionForce);

            // Stock-only values
            if (!FARactive)
            {
                // numbers for lift from: http://forum.kerbalspaceprogram.com/threads/118839-Updating-Parts-to-1-0?p=1896409&viewfull=1#post1896409
                float stockLiftCoefficient = (float)(surfaceArea / 3.52);
                // CoL/P matches CoM unless otherwise specified
                part.CoMOffset = new Vector3(Vector3.Dot(Tip.position - Root.position, part.transform.right) / 2, Vector3.Dot(Tip.position - Root.position, part.transform.up) / 2, 0);
                if (isWing && !isCtrlSrf)
                {
                    part.Modules.GetModules<ModuleLiftingSurface>().FirstOrDefault().deflectionLiftCoeff = stockLiftCoefficient;
                    part.mass = stockLiftCoefficient * 0.1f;
                }
                else
                {
                    ModuleControlSurface mCtrlSrf = part.Modules.OfType<ModuleControlSurface>().FirstOrDefault();
                    if (mCtrlSrf != null)
                    {
                        mCtrlSrf.deflectionLiftCoeff = stockLiftCoefficient;
                        mCtrlSrf.ctrlSurfaceArea = modelControlSurfaceFraction;
                        part.mass = stockLiftCoefficient * (1 + modelControlSurfaceFraction) * 0.1f;
                    }
                }
                guiCd = (float)Math.Round(Cd, 2);
                guiCl = (float)Math.Round(Cl, 2);
                guiWingMass = part.mass;
            }
            else
            {
                if (part.Modules.Contains("FARControllableSurface"))
                {
                    PartModule FARmodule = part.Modules["FARControllableSurface"];
                    Type FARtype = FARmodule.GetType();
                    FARtype.GetField("b_2").SetValue(FARmodule, b_2);
                    FARtype.GetField("b_2_actual").SetValue(FARmodule, b_2);
                    FARtype.GetField("MAC").SetValue(FARmodule, MAC);
                    FARtype.GetField("MAC_actual").SetValue(FARmodule, MAC);
                    FARtype.GetField("S").SetValue(FARmodule, surfaceArea);
                    FARtype.GetField("MidChordSweep").SetValue(FARmodule, midChordSweep);
                    FARtype.GetField("TaperRatio").SetValue(FARmodule, taperRatio);
                    FARtype.GetField("ctrlSurfFrac").SetValue(FARmodule, modelControlSurfaceFraction);
                    //print("Set fields");

                }
                else if (part.Modules.Contains("FARWingAerodynamicModel"))
                {
                    PartModule FARmodule = part.Modules["FARWingAerodynamicModel"];
                    Type FARtype = FARmodule.GetType();
                    FARtype.GetField("b_2").SetValue(FARmodule, b_2);
                    FARtype.GetField("b_2_actual").SetValue(FARmodule, b_2);
                    FARtype.GetField("MAC").SetValue(FARmodule, MAC);
                    FARtype.GetField("MAC_actual").SetValue(FARmodule, MAC);
                    FARtype.GetField("S").SetValue(FARmodule, surfaceArea);
                    FARtype.GetField("MidChordSweep").SetValue(FARmodule, midChordSweep);
                    FARtype.GetField("TaperRatio").SetValue(FARmodule, taperRatio);
                }
                
                if (!triggerUpdate)
                    TriggerUpdateAllWings();
                triggerUpdate = false;
            }

            guiMAC = (float)MAC;
            guiB_2 = (float)b_2;
            guiMidChordSweep = (float)midChordSweep;
            guiTaperRatio = (float)taperRatio;
            guiSurfaceArea = (float)surfaceArea;
            guiAspectRatio = (float)aspectRatio;

            StartCoroutine(updateAeroDelayed());
        }

        #endregion

        #region Common Methods

        // Print debug values when 'O' is pressed.
        public void DebugValues()
        {
            if (Input.GetKeyDown(KeyCode.O))
            {
                print("tipScaleModified " + tipScale);
                print("rootScaleModified " + rootScale);
                print("isControlSurface " + isCtrlSrf);
                print("DoNotParticipateInParentSnapping " + doNotParticipateInParentSnapping);
                print("IgnoreSnapping " + IgnoreSnapping);
                print("SegmentRoot " + SegmentRoot);
                print("IsAttached " + IsAttached);
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
                print("b_2 " + b_2);
                print("FARactive " + FARactive);
            }
        }

        public void SetupCollider()
        {
            baked = new Mesh();
            wingSMR.BakeMesh(baked);
            wingSMR.enabled = false;
            Transform modelTransform = transform.FindChild("model");
            if (modelTransform.GetComponent<MeshCollider>() == null)
                modelTransform.gameObject.AddComponent<MeshCollider>();
            MeshCollider meshCol = modelTransform.GetComponent<MeshCollider>();
            meshCol.sharedMesh = null;
            meshCol.sharedMesh = baked;
            meshCol.convex = true;
            if (FARactive)
            {
                CalculateAerodynamicValues();
                PartModule FARmodule = null;
                if (part.Modules.Contains("FARControllableSurface"))
                    FARmodule = part.Modules["FARControllableSurface"];
                else if (part.Modules.Contains("FARWingAerodynamicModel"))
                    FARmodule = part.Modules["FARWingAerodynamicModel"];
                if (FARmodule != null)
                {
                    Type FARtype = FARmodule.GetType();
                    FARtype.GetMethod("TriggerPartColliderUpdate").Invoke(FARmodule, null);
                }
            }
        }

        public void UpdatePositions()
        {
            // If we're snapping, match relative thickness scaling with root
            //SetThicknessScalingTypeToRoot();

            Tip.localScale = tipScale;
            Root.localScale = rootScale;

            Tip.localPosition = tipPosition + TipSpawnOffset;

            if (IsAttached &&
                this.part.parent != null &&
                this.part.parent.Modules.Contains("WingManipulator") &&
                !IgnoreSnapping &&
                !doNotParticipateInParentSnapping)
            {
                WingManipulator Parent = part.parent.Modules.OfType<WingManipulator>().FirstOrDefault();
                part.transform.position = Parent.Tip.position + 0.1f * Parent.Tip.right; // set the new part inward just a little bit
                rootScale = Parent.tipScale;
            }

            if (symmetricMovement == false)
            {
                tipPosition.y = Root.localPosition.y;
            }
            else
            {
                tipPosition.y = 0f;
                tipPosition.x = 0f;
                rootPosition.x = 0f;
                rootPosition.y = 0f;

                Root.localPosition = -(tipPosition + TipSpawnOffset);
            }
        }

        public void UpdateAllCopies(bool childrenNeedUpdate)
        {
            UpdatePositions();
            SetupCollider();

            if (updateChildren && childrenNeedUpdate)
                UpdateChildren();

            if (isWing || isCtrlSrf)
                CalculateAerodynamicValues();

            foreach (Part p in this.part.symmetryCounterparts)
            {
                var clone = p.Modules.OfType<WingManipulator>().FirstOrDefault();

                clone.rootScale = rootScale;
                clone.tipScale = tipScale;
                clone.tipPosition = tipPosition;

                clone.relativeThicknessScaling = relativeThicknessScaling;
                //clone.SetThicknessScalingEventName();

                clone.UpdatePositions();
                clone.SetupCollider();

                if (updateChildren && childrenNeedUpdate)
                    clone.UpdateChildren();

                if (isWing || isCtrlSrf)
                    clone.CalculateAerodynamicValues();
            }
        }

        // Updates child pWings
        public void UpdateChildren()
        {
            // Get the list of child parts
            foreach (Part p in this.part.children)
            {
                // Check that it is a pWing and that it is affected by parent snapping
                WingManipulator wing = p.Modules.OfType<WingManipulator>().FirstOrDefault();
                if (wing != null && !wing.IgnoreSnapping && !wing.doNotParticipateInParentSnapping)
                {
                    // Update its positions and refresh the collider
                    wing.UpdatePositions();
                    wing.SetupCollider();

                    // If its a wing, refresh its aerodynamic values
                    if (isWing || isCtrlSrf) // FIXME should this be child.isWing etc?
                        wing.CalculateAerodynamicValues();
                }
            }
        }

        // Fires when the part is attached
        public void UpdateOnEditorAttach()
        {
            // We are attached
            IsAttached = true;

            // If we were the root of a detached segment, check for the mouse state
            // and set snap override.
            if (SegmentRoot)
            {
                IgnoreSnapping = Input.GetKey(KeyCode.Mouse1);
                SegmentRoot = false;
            }

            // If we're snapping, match relative thickness scaling type with root
            //SetThicknessScalingTypeToRoot();

            // if snap is not ignored, lets update our dimensions.
            if (this.part.parent != null &&
                this.part.parent.Modules.Contains("WingManipulator") &&
                !IgnoreSnapping &&
                !doNotParticipateInParentSnapping)
            {
                UpdatePositions();
                SetupCollider();
                Events["MatchTaperEvent"].guiActiveEditor = true;
            }

            // Now redo aerodynamic values.
            if (isWing || isCtrlSrf)
                CalculateAerodynamicValues();

            // Enable relative scaling event
            //SetThicknessScalingEventState();
        }

        public void UpdateOnEditorDetach()
        {
            // If the root is not null and is a pWing, set its justDetached so it knows to check itself next Update
            if (this.part.parent != null && this.part.parent.Modules.Contains("WingManipulator"))
                this.part.parent.Modules.OfType<WingManipulator>().FirstOrDefault().justDetached = true;

            // We are not attached.
            IsAttached = false;
            justDetached = true;

            // Disable root-matching events
            Events["MatchTaperEvent"].guiActiveEditor = false;

            // Disable relative scaling event
            //SetThicknessScalingEventState();
        }

        #endregion

        #region PartModule

        private void Setup()
        {
            CheckAssemblies();

            Tip = part.FindModelTransform("Tip");
            Root = part.FindModelTransform("Root");
            SMRcontainer = part.FindModelTransform("Collider");
            wingSMR = SMRcontainer.GetComponent<SkinnedMeshRenderer>();

            UpdatePositions();
            SetupCollider();

            CalculateAerodynamicValues();

            // Enable root-matching events
            if (IsAttached &&
                this.part.parent != null &&
                this.part.parent.Modules.Contains("WingManipulator"))
            {
                Events["MatchTaperEvent"].guiActiveEditor = true;
            }

            this.part.OnEditorAttach += new Callback(UpdateOnEditorAttach);
            this.part.OnEditorDetach += new Callback(UpdateOnEditorDetach);

            if (fuelSelectedTankSetup < 0)
            {
                fuelSelectedTankSetup = 0;
                FuelTankTypeChanged();
            }
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            Setup();
        }

        public void Update()
        {
            if (!HighLogic.LoadedSceneIsEditor || wingSMR == null)
                return;

            DeformWing();

            //Sets the skinned meshrenderer to update even when culled for being outside the screen
            wingSMR.updateWhenOffscreen = true;

            // A pWing has just detached from us, or we have just detached
            if (justDetached)
            {
                if (!IsAttached)
                {
                    // We have just detached. Check if we're the root of the detached segment
                    SegmentRoot = (this.part.parent == null) ? true : false;
                }
                else
                {
                    // A pWing just detached from us, we need to redo the wing values.
                    if (isWing || isCtrlSrf)
                        CalculateAerodynamicValues();
                }

                // And set this to false so we only do it once.
                justDetached = false;
            }
            if (triggerUpdate)
                CalculateAerodynamicValues();
        }

        Vector3 lastMousePos;
        int state = 0; // 0 == nothing, 1 == translate, 2 == tipScale, 3 == rootScale
        public void DeformWing()
        {
            if (this.part.parent == null || !IsAttached || state == 0)
                return;

            float depth = EditorCamera.Instance.camera.WorldToScreenPoint(state != 3 ? Tip.position : Root.position).z; // distance of tip transform from camera
            Vector3 diff = (state == 1 ? moveSpeed : scaleSpeed * 20) * depth * (Input.mousePosition - lastMousePos) / 4500;
            lastMousePos = Input.mousePosition;

            // Translation
            if (state == 1)
            {
                if (!Input.GetKey(keyTranslation))
                {
                    state = 0;
                    return;
                }

                if (symmetricMovement == true)
                { // Symmetric movement (for wing edge control surfaces)
                    tipPosition.z -= diff.x * Vector3.Dot(EditorCamera.Instance.camera.transform.right, part.transform.right) + diff.y * Vector3.Dot(EditorCamera.Instance.camera.transform.up, part.transform.right);
                    tipPosition.z = Mathf.Max(tipPosition.z, modelMinimumSpan / 2 - TipSpawnOffset.z); // Clamp z to modelMinimumSpan/2 to prevent turning the model inside-out
                    tipPosition.x = tipPosition.y = 0;

                    rootPosition.z += diff.x * Vector3.Dot(EditorCamera.Instance.camera.transform.right, part.transform.right) + diff.y * Vector3.Dot(EditorCamera.Instance.camera.transform.up, part.transform.right);
                    rootPosition.z = Mathf.Max(rootPosition.z, modelMinimumSpan / 2 - TipSpawnOffset.z); // Clamp z to modelMinimumSpan/2 to prevent turning the model inside-out
                    rootPosition.x = rootPosition.y = 0;
                }
                else
                { // Normal, only tip moves
                    tipPosition.x += diff.x * Vector3.Dot(EditorCamera.Instance.camera.transform.right, part.transform.up) + diff.y * Vector3.Dot(EditorCamera.Instance.camera.transform.up, part.transform.up);
                    tipPosition.z += diff.x * Vector3.Dot(EditorCamera.Instance.camera.transform.right, part.transform.right) + diff.y * Vector3.Dot(EditorCamera.Instance.camera.transform.up, part.transform.right);
                    tipPosition.z = Mathf.Max(tipPosition.z, modelMinimumSpan - TipSpawnOffset.z); // Clamp z to modelMinimumSpan to prevent turning the model inside-out
                    tipPosition.y = 0;
                }
            }
            // Tip scaling
            else if (state == 2)
            {
                if (!Input.GetKey(keyTipScale))
                {
                    state = 0;
                    return;
                }
                tipScale.x += diff.x * Vector3.Dot(EditorCamera.Instance.camera.transform.right, -part.transform.up) + diff.y * Vector3.Dot(EditorCamera.Instance.camera.transform.up, -part.transform.up);
                tipScale.y = tipScale.x = Mathf.Max(tipScale.x, 0.01f);
                tipScale.z += diff.x * Vector3.Dot(EditorCamera.Instance.camera.transform.right, part.transform.forward) + diff.y * Vector3.Dot(EditorCamera.Instance.camera.transform.up, part.transform.forward);
                tipScale.z = Mathf.Max(tipScale.z, 0.01f);
            }
            // Root scaling
            // only if the root part is not a pWing,
            // or we were told to ignore snapping,
            // or the part is set to ignore snapping (wing edge control surfaces, tipically)
            else if (state == 3 && (!this.part.parent.Modules.Contains("WingManipulator") || IgnoreSnapping || doNotParticipateInParentSnapping))
            {
                if (!Input.GetKey(keyRootScale))
                {
                    state = 0;
                    return;
                }
                rootScale.x += diff.x * Vector3.Dot(EditorCamera.Instance.camera.transform.right, -part.transform.up) + diff.y * Vector3.Dot(EditorCamera.Instance.camera.transform.up, -part.transform.up);
                rootScale.y = rootScale.x = Mathf.Max(rootScale.x, 0.01f);
                rootScale.z += diff.x * Vector3.Dot(EditorCamera.Instance.camera.transform.right, part.transform.forward) + diff.y * Vector3.Dot(EditorCamera.Instance.camera.transform.up, part.transform.forward);
                rootScale.z = Mathf.Max(rootScale.z, 0.01f);
            }
            UpdateAllCopies(true);
        }

        void OnMouseOver()
        {
            DebugValues();
            if (!HighLogic.LoadedSceneIsEditor || state != 0)
                return;

            lastMousePos = Input.mousePosition;
            if (Input.GetKeyDown(keyTranslation))
                state = 1;
            else if (Input.GetKeyDown(keyTipScale))
                state = 2;
            else if (Input.GetKeyDown(keyRootScale))
                state = 3;
        }
        #endregion
    }
}