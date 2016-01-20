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
        public override double tipThickness
        {
            get { return 0.2 * tipScale.z; }
            set { tipScale.z = 5 * (float)value; }
        }

        public override double tipWidth
        {
            get { return tipScale.x * modelChordLength; }
            set { tipScale.x = (float)value / modelChordLength; }
        }

        public override Vector3 tipPos
        {
            get { return tipPosition; }
            set { tipPosition = value; }
        }

        public override double rootThickness
        {
            get { return 0.2 * rootScale.z; }
            set { rootScale.z = (float)value * 5; }
        }

        public override double rootWidth
        {
            get { return rootScale.x * modelChordLength; }
            set { rootScale.x = (float)value / modelChordLength; }
        }

        public override double minSpan
        {
            get { return modelMinimumSpan; }
        }

        // PartModule Dimensions
        [KSPField]
        public float modelChordLength = 2f;
        [KSPField]
        public float modelMinimumSpan = 0.05f;
        [KSPField]
        public Vector3 TipSpawnOffset = Vector3.forward;

        public bool updateChildren = true;
        // Internals
        public Transform Tip;
        public Transform Root;
        private Mesh baked;
        public SkinnedMeshRenderer wingSMR;
        public Transform wingTransform;
        public Transform SMRcontainer;

        // Internal Fields

        [KSPField(isPersistant = true)]
        public Vector3 tipScale = Vector3.one;

        [KSPField(isPersistant = true)]
        public Vector3 tipPosition = Vector3.zero;

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
            UpdateCounterparts();
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

        public virtual string FarModuleName
        {
            get { return "FARWingAerodynamicModel"; }
        }

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

        #endregion

        #region Common Methods

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

        #warning yea, this ain't implemented...
        public override bool CheckForGeometryChanges()
        {
            return false;
        }

        public override void UpdateGeometry()
        {
            // If we're snapping, match relative thickness scaling with root
            //SetThicknessScalingTypeToRoot();

            Tip.localScale = tipScale;
            Root.localScale = rootScale;

            Tip.localPosition = tipPosition + TipSpawnOffset;

            if (IsAttached && this.part.parent != null && this.part.parent.Modules.OfType<ProceduralWing>().Any() && !IgnoreSnapping)
            {
                ProceduralWing Parent = part.parent.Modules.OfType<ProceduralWing>().FirstOrDefault();
                part.transform.position = Parent.tipPos + 0.1f * Parent.transform.right; // set the new part inward just a little bit
                //rootScale = Parent.tipScale;
            }

            tipPosition.y = Root.localPosition.y;
        }

        public override void UpdateCounterparts()
        {
            UpdateGeometry();
            SetupCollider();

            if (updateChildren)
                UpdateChildren();

            CalculateAerodynamicValues();

            for (int i = 0; i < part.symmetryCounterparts.Count; ++i)
            {
                Part p = part.symmetryCounterparts[i];
                var clone = p.Modules.OfType<WingManipulator>().FirstOrDefault();

                clone.rootScale = rootScale;
                clone.tipScale = tipScale;
                clone.tipPosition = tipPosition;

                clone.UpdateGeometry();
                clone.SetupCollider();

                if (updateChildren)
                    clone.UpdateChildren();
                clone.CalculateAerodynamicValues();
            }
        }

        // Updates child pWings
        public void UpdateChildren()
        {
            // Get the list of child parts
            for (int i = 0; i < part.children.Count; ++i)
            {
                Part p = part.children[i];
                // Check that it is a pWing and that it is affected by parent snapping
                WingManipulator wing = p.Modules.OfType<WingManipulator>().FirstOrDefault();
                if (wing != null && !wing.IgnoreSnapping)
                {
                    // Update its positions and refresh the collider
                    wing.UpdateGeometry();
                    wing.SetupCollider();
                    // If its a wing, refresh its aerodynamic values
                    wing.CalculateAerodynamicValues();
                }
            }
        }

        // Fires when the part is attached
        public override void OnAttach()
        {
            base.OnAttach();
            if (part.parent.Modules.OfType<ProceduralWing>().Any())
                Events["MatchTaperEvent"].guiActiveEditor = false;
        }

        public override void OnDetach()
        {
            base.OnDetach();
            Events["MatchTaperEvent"].guiActiveEditor = false;
        }

        #endregion

        #region PartModule

        public override void SetupGeometryAndAppearance()
        {
            Tip = part.FindModelTransform("Tip");
            Root = part.FindModelTransform("Root");
            SMRcontainer = part.FindModelTransform("Collider");
            wingSMR = SMRcontainer.GetComponent<SkinnedMeshRenderer>();

            SetupCollider();

            // Enable root-matching events
            if (IsAttached && this.part.parent != null && this.part.parent.Modules.OfType<ProceduralWing>().Any())
                Events["MatchTaperEvent"].guiActiveEditor = true;
        }


        //public override void DeformWing()
        //{
        //    if (this.part.parent == null || !IsAttached || state == 0)
        //        return;

        //    float depth = EditorCamera.Instance.camera.WorldToScreenPoint(state != 3 ? Tip.position : Root.position).z; // distance of tip transform from camera
        //    Vector3 diff = (state == 1 ? moveSpeed : scaleSpeed * 20) * depth * (Input.mousePosition - lastMousePos) / 4500;
        //    lastMousePos = Input.mousePosition;

        //    // Translation
        //    if (state == 1)
        //    {
        //        if (!Input.GetKey(keyTranslation))
        //        {
        //            state = 0;
        //            return;
        //        }

        //        if (symmetricMovement == true)
        //        { // Symmetric movement (for wing edge control surfaces)
        //            tipPosition.z -= diff.x * Vector3.Dot(EditorCamera.Instance.camera.transform.right, part.transform.right) + diff.y * Vector3.Dot(EditorCamera.Instance.camera.transform.up, part.transform.right);
        //            tipPosition.z = Mathf.Max(tipPosition.z, modelMinimumSpan / 2 - TipSpawnOffset.z); // Clamp z to modelMinimumSpan/2 to prevent turning the model inside-out
        //            tipPosition.x = tipPosition.y = 0;

        //            rootPosition.z += diff.x * Vector3.Dot(EditorCamera.Instance.camera.transform.right, part.transform.right) + diff.y * Vector3.Dot(EditorCamera.Instance.camera.transform.up, part.transform.right);
        //            rootPosition.z = Mathf.Max(rootPosition.z, modelMinimumSpan / 2 - TipSpawnOffset.z); // Clamp z to modelMinimumSpan/2 to prevent turning the model inside-out
        //            rootPosition.x = rootPosition.y = 0;
        //        }
        //        else
        //        { // Normal, only tip moves
        //            tipPosition.x += diff.x * Vector3.Dot(EditorCamera.Instance.camera.transform.right, part.transform.up) + diff.y * Vector3.Dot(EditorCamera.Instance.camera.transform.up, part.transform.up);
        //            tipPosition.z += diff.x * Vector3.Dot(EditorCamera.Instance.camera.transform.right, part.transform.right) + diff.y * Vector3.Dot(EditorCamera.Instance.camera.transform.up, part.transform.right);
        //            tipPosition.z = Mathf.Max(tipPosition.z, modelMinimumSpan - TipSpawnOffset.z); // Clamp z to modelMinimumSpan to prevent turning the model inside-out
        //            tipPosition.y = 0;
        //        }
        //    }
        //    // Tip scaling
        //    else if (state == 2)
        //    {
        //        if (!Input.GetKey(keyTipScale))
        //        {
        //            state = 0;
        //            return;
        //        }
        //        tipScale.x += diff.x * Vector3.Dot(EditorCamera.Instance.camera.transform.right, -part.transform.up) + diff.y * Vector3.Dot(EditorCamera.Instance.camera.transform.up, -part.transform.up);
        //        tipScale.y = tipScale.x = Mathf.Max(tipScale.x, 0.01f);
        //        tipScale.z += diff.x * Vector3.Dot(EditorCamera.Instance.camera.transform.right, part.transform.forward) + diff.y * Vector3.Dot(EditorCamera.Instance.camera.transform.up, part.transform.forward);
        //        tipScale.z = Mathf.Max(tipScale.z, 0.01f);
        //    }
        //    // Root scaling
        //    // only if the root part is not a pWing,
        //    // or we were told to ignore snapping,
        //    // or the part is set to ignore snapping (wing edge control surfaces, tipically)
        //    else if (state == 3 && (!this.part.parent.Modules.Contains("WingManipulator") || IgnoreSnapping || doNotParticipateInParentSnapping))
        //    {
        //        if (!Input.GetKey(keyRootScale))
        //        {
        //            state = 0;
        //            return;
        //        }
        //        rootScale.x += diff.x * Vector3.Dot(EditorCamera.Instance.camera.transform.right, -part.transform.up) + diff.y * Vector3.Dot(EditorCamera.Instance.camera.transform.up, -part.transform.up);
        //        rootScale.y = rootScale.x = Mathf.Max(rootScale.x, 0.01f);
        //        rootScale.z += diff.x * Vector3.Dot(EditorCamera.Instance.camera.transform.right, part.transform.forward) + diff.y * Vector3.Dot(EditorCamera.Instance.camera.transform.up, part.transform.forward);
        //        rootScale.z = Mathf.Max(rootScale.z, 0.01f);
        //    }
        //   UpdateAllCopies(true);
        //}
        #endregion
    }
}