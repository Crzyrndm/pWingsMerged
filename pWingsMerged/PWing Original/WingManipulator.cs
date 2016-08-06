using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using System.Reflection;

namespace ProceduralWings.Original
{
    public class WingManipulator : Base_ProceduralWing
    {
        public override double TipThickness
        {
            get { return 0.2 * tipScale.z; }
            set { tipScale.z = 5 * (float)value; }
        }

        public override double TipWidth
        {
            get { return tipScale.x * modelChordLength; }
            set { tipScale.x = (float)value / modelChordLength; }
        }

        public override Vector3 tipPos
        {
            get { return tipPosition; }
            set { tipPosition = value; }
        }

        public override double TipOffset
        {
            get { return -tipPosition.x; }
            set { tipPosition.x = (float)-value; }
        }

        public override double RootThickness
        {
            get { return 0.2 * rootScale.z; }
            set { rootScale.z = (float)value * 5; }
        }

        public override double RootWidth
        {
            get { return rootScale.x * modelChordLength; }
            set { rootScale.x = (float)value / modelChordLength; }
        }

        public override double minSpan
        {
            get { return modelMinimumSpan; }
        }

        public override Vector3 rootPos
        {
            get
            {
                return Root.position;
            }
        }

        public override double Scale
        {
            set
            {
                throw new NotImplementedException();
            }
        }

        public override double Length
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
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
            Vector3 changeTipScale = (float)(Length / parentWing.Length) * (parentWing.tipScale - parentWing.rootScale);

            // Scale the tip
            tipScale.Set(
                Mathf.Max(rootScale.x + changeTipScale.x, 0.01f),
                Mathf.Max(rootScale.y + changeTipScale.y, 0.01f),
                Mathf.Max(rootScale.z + changeTipScale.z, 0.01f));
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
                TriggerFARColliderUpdate();
        }

        public override void TriggerFARColliderUpdate()
        {
            CalculateAerodynamicValues();
            PartModule FARmodule = null;
            if (part.Modules.Contains(FarModuleName))
                FARmodule = part.Modules[FarModuleName];
            if (FARmodule != null)
            {
                Type FARtype = FARmodule.GetType();
                FARtype.GetMethod("TriggerPartColliderUpdate").Invoke(FARmodule, null);
            }
        }

        public override void UpdateGeometry()
        {
            Tip.localScale = tipScale;
            Root.localScale = rootScale;

            Tip.localPosition = tipPosition + TipSpawnOffset;
            if (IsAttached && this.part.parent != null && this.part.parent.Modules.OfType<Base_ProceduralWing>().Any() && !IgnoreSnapping)
            {
                Base_ProceduralWing Parent = part.parent.Modules.OfType<Base_ProceduralWing>().FirstOrDefault();
                part.transform.position = Parent.tipPos + 0.1f * Parent.transform.right; // set the new part inward just a little bit
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
            if (part.parent.Modules.OfType<Base_ProceduralWing>().Any())
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
            if (IsAttached && this.part.parent != null && this.part.parent.Modules.OfType<Base_ProceduralWing>().Any())
                Events["MatchTaperEvent"].guiActiveEditor = true;
        }
        #endregion
    }
}