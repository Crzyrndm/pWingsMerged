using System;
using System.Collections;
using UnityEngine;

namespace ProceduralWings.Original
{
    public class ManipulatorWing : Base_ProceduralWing
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

        #region Geometry

        public void SetupCollider()
        {
            baked = new Mesh();
            wingSMR.BakeMesh(baked);
            wingSMR.enabled = true;

            Transform modelTransform = transform.FindChild("model");
            if (modelTransform.GetComponent<MeshCollider>() == null)
                modelTransform.gameObject.AddComponent<MeshCollider>();
            MeshCollider meshCol = modelTransform.GetComponent<MeshCollider>();
            meshCol.sharedMesh = null;
            meshCol.sharedMesh = baked;
            meshCol.convex = true;

            wingSMR.updateWhenOffscreen = true;
        }

        public override void UpdateGeometry()
        {
            Root.localScale = new Vector3(0, (float)RootWidth / modelChordLength, (float)RootThickness / modelThickness);
            Tip.localScale = new Vector3(0, (float)TipWidth / modelChordLength, (float)TipThickness / modelThickness);
            Tip.localPosition = new Vector3((float)TipOffset + TipSpawnOffset.x, 0, (float)Length - TipSpawnOffset.z);

            if (part?.parent?.Modules.GetModule<Base_ProceduralWing>() != null && !IgnoreSnapping)
            {
                Base_ProceduralWing Parent = part.parent.Modules.GetModule<Base_ProceduralWing>();
                part.transform.position = Parent.tipPos + 0.1f * Parent.transform.right; // set the new part inward just a little bit
            }

            SetupCollider();

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
                ManipulatorWing wing = p?.Modules.GetModule<ManipulatorWing>();
                if (wing != null && !wing.IgnoreSnapping)
                {
                    // Update its positions and refresh the collider
                    wing.UpdateGeometry();
                }
            }
        }

        public override void SetupGeometryAndAppearance()
        {
            Tip = part.FindModelTransform("Tip");
            Root = part.FindModelTransform("Root");
            SMRcontainer = part.FindModelTransform("Collider");
            wingSMR = SMRcontainer.GetComponent<SkinnedMeshRenderer>();
        }

        public void MatchTaperEvent()
        {
            // Check for a valid parent
            // Get parents taper
            ManipulatorWing parentWing = part.parent?.Modules.GetModule<ManipulatorWing>();
            if (parentWing == null)
                return;

            TipThickness = RootThickness * (Length / parentWing.Length) * (parentWing.TipThickness - parentWing.RootThickness);
            TipWidth = RootWidth * (Length / parentWing.Length) * (parentWing.TipWidth - parentWing.RootWidth);
        }
        #endregion

        public override IEnumerator translateTip()
        {
            deformWing = true;
            Vector3 diff;
            while (Input.GetKey(keyTranslation))
            {
                yield return null;
                diff = UpdateMouseDiff(false);

                TipOffset += diff.x * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.right, part.transform.up) + diff.y * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.up, part.transform.up);
                length.value += diff.x * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.right, part.transform.right) + diff.y * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.up, part.transform.right);
                Length = Math.Max(length.value, minSpan); // Clamp z to minimumSpan to prevent turning the model inside-out
            }
            deformWing = false;
        }
    }
}