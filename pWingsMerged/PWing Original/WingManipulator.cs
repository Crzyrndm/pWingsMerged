using System;
using UnityEngine;

namespace ProceduralWings.Original
{
    public class WingManipulator : Base_ProceduralWing
    {
        // PartModule Dimensions
        [KSPField]
        public float modelChordLength = 2f;
        [KSPField]
        public float modelThickness = 0.2f;
        [KSPField]
        public float modelMinimumSpan = 0.05f;
        [KSPField]
        public Vector3 TipSpawnOffset = Vector3.forward;

        // Internals
        public Transform Tip;
        public Transform Root;
        private Mesh baked;
        public SkinnedMeshRenderer wingSMR;
        public Transform wingTransform;
        public Transform SMRcontainer;

        [KSPField()]
        public bool IgnoreSnapping = false;

        public void MatchTaperEvent()
        {
            // Check for a valid parent
                // Get parents taper
            WingManipulator parentWing = part.parent?.Modules.GetModule<WingManipulator>();
            if (parentWing == null)
                return;

            TipThickness = RootThickness * (Length / parentWing.Length) * (parentWing.TipThickness - parentWing.RootThickness);
            TipWidth = RootWidth * (Length / parentWing.Length) * (parentWing.TipWidth - parentWing.RootWidth);
        }

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
            Root.localScale = new Vector3(0, (float)RootWidth / modelChordLength, (float)RootThickness / modelThickness);
            Tip.localScale = new Vector3(0, (float)TipWidth / modelChordLength, (float)TipThickness / modelThickness);
            Tip.localPosition = new Vector3(-(float)TipOffset + TipSpawnOffset.x, 0, (float)Length - TipSpawnOffset.z);

            if (part?.parent?.Modules.GetModule<Base_ProceduralWing>() != null && !IgnoreSnapping)
            {
                Base_ProceduralWing Parent = part.parent.Modules.GetModule<Base_ProceduralWing>();
                part.transform.position = Parent.tipPos + 0.1f * Parent.transform.right; // set the new part inward just a little bit
            }

            CalculateAerodynamicValues();
        }

        // Updates child pWings
        public void UpdateChildren()
        {
            // Get the list of child parts
            for (int i = 0; i < part.children.Count; ++i)
            {
                Part p = part.children[i];
                // Check that it is a pWing and that it is affected by parent snapping
                WingManipulator wing = p?.Modules.GetModule<WingManipulator>();
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
            if (part?.parent?.Modules.GetModule<Base_ProceduralWing>() != null)
                Events["MatchTaperEvent"].guiActiveEditor = true;
        }
        #endregion
    }
}