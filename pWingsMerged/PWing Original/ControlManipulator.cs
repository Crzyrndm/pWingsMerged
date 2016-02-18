using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProceduralWings.Original
{
    class ControlManipulator : WingManipulator
    {
        public override bool isCtrlSrf
        {
            get { return true; }
        }

        [KSPField]
        public float ctrlFraction = 1f;

        public const float costDensityControl = 6500f;

        /// <summary>
        /// control surfaces cant carry fuel
        /// </summary>
        public override bool canBeFueled
        {
            get
            {
                return false;
            }
        }

        public override float updateCost()
        {
            return (float)Math.Round(wingMass * (1f + ArSweepScale / 4f) * (costDensity * (1f - ctrlFraction) + costDensityControl * ctrlFraction), 1);
        }

        [KSPField]
        public bool symmetricMovement = true;
        [KSPField(isPersistant = true)]
        public Vector3 rootPosition = Vector3.zero;

        public override void UpdateGeometry()
        {
            base.UpdateGeometry();

            if (symmetricMovement)
            {
                tipPosition.y = 0f;
                tipPosition.x = 0f;
                rootPosition.x = 0f;
                rootPosition.y = 0f;

                Root.localPosition = -(tipPosition + TipSpawnOffset);
            }
        }

        public override void setFARModuleParams()
        {
            if (part.Modules.Contains("FARControllableSurface"))
            {
                PartModule FARmodule = part.Modules["FARControllableSurface"];
                Type FARtype = FARmodule.GetType();
                FARtype.GetField("b_2").SetValue(FARmodule, length);
                FARtype.GetField("b_2_actual").SetValue(FARmodule, length);
                FARtype.GetField("MAC").SetValue(FARmodule, MAC);
                FARtype.GetField("MAC_actual").SetValue(FARmodule, MAC);
                FARtype.GetField("S").SetValue(FARmodule, surfaceArea);
                FARtype.GetField("MidChordSweep").SetValue(FARmodule, midChordSweep);
                FARtype.GetField("TaperRatio").SetValue(FARmodule, taperRatio);
                FARtype.GetField("ctrlSurfFrac").SetValue(FARmodule, ctrlFraction);
            }

            if (!triggerUpdate)
                TriggerUpdateAllWings();
            triggerUpdate = false;
        }

        public override void SetStockModuleParams()
        {
            // numbers for lift from: http://forum.kerbalspaceprogram.com/threads/118839-Updating-Parts-to-1-0?p=1896409&viewfull=1#post1896409
            part.CoMOffset.Set(Vector3.Dot(tipPos - rootPos, part.transform.right) / 2, Vector3.Dot(tipPos - rootPos, part.transform.up) / 2, 0); // COP matches COM
            ModuleControlSurface mCtrlSrf = part.Modules.OfType<ModuleControlSurface>().FirstOrDefault();
            if (mCtrlSrf != null)
            {
                mCtrlSrf.deflectionLiftCoeff = (float)surfaceArea / 3.52f;
                mCtrlSrf.ctrlSurfaceArea = ctrlFraction;
                part.mass = (float)surfaceArea * (1 + ctrlFraction) / 35.2f; // multiply by 0.1, divide by 3.52
            }
        }

        public override void TriggerFARColliderUpdate()
        {
            CalculateAerodynamicValues();
            PartModule FARmodule = null;
            if (part.Modules.Contains("FARControllableSurface"))
                FARmodule = part.Modules["FARControllableSurface"];
            if (FARmodule != null)
            {
                Type FARtype = FARmodule.GetType();
                FARtype.GetMethod("TriggerPartColliderUpdate").Invoke(FARmodule, null);
            }
        }

        public override void DeformWing()
        {
            if (isAttached || state == 0)
                return;

            float depth = EditorCamera.Instance.camera.WorldToScreenPoint(state != 3 ? tipPos : rootPos).z; // distance of tip transform from camera
            Vector3 diff = (state == 1 ? moveSpeed : scaleSpeed * 20) * depth * (Input.mousePosition - lastMousePos) / 4500;
            lastMousePos = Input.mousePosition;

            switch (state)
            {
                case 1: // translation
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
                    { // All movers
                        tipPosition.x += diff.x * Vector3.Dot(EditorCamera.Instance.camera.transform.right, part.transform.up) + diff.y * Vector3.Dot(EditorCamera.Instance.camera.transform.up, part.transform.up);
                        tipPosition.z += diff.x * Vector3.Dot(EditorCamera.Instance.camera.transform.right, part.transform.right) + diff.y * Vector3.Dot(EditorCamera.Instance.camera.transform.up, part.transform.right);
                        tipPosition.z = Mathf.Max(tipPosition.z, modelMinimumSpan - TipSpawnOffset.z); // Clamp z to modelMinimumSpan to prevent turning the model inside-out
                        tipPosition.y = 0;
                    }
                    break;
                case 2: // tip
                    if (!Input.GetKey(keyTipScale))
                    {
                        state = 0;
                        return;
                    }
                    tipWidth += diff.x * Vector3.Dot(EditorCamera.Instance.camera.transform.right, -part.transform.up) + diff.y * Vector3.Dot(EditorCamera.Instance.camera.transform.up, -part.transform.up);
                    tipWidth = Math.Max(tipWidth, 0.01);
                    tipThickness += diff.x * Vector3.Dot(EditorCamera.Instance.camera.transform.right, part.transform.forward) + diff.y * Vector3.Dot(EditorCamera.Instance.camera.transform.up, part.transform.forward);
                    tipThickness = Math.Max(tipThickness, 0.01);
                    break;
                case 3: // root
                    if (!Input.GetKey(keyRootScale))
                    {
                        state = 0;
                        return;
                    }
                    rootWidth += diff.x * Vector3.Dot(EditorCamera.Instance.camera.transform.right, -part.transform.up) + diff.y * Vector3.Dot(EditorCamera.Instance.camera.transform.up, -part.transform.up);
                    rootWidth = Math.Max(rootWidth, 0.01);
                    rootThickness += diff.x * Vector3.Dot(EditorCamera.Instance.camera.transform.right, part.transform.forward) + diff.y * Vector3.Dot(EditorCamera.Instance.camera.transform.up, part.transform.forward);
                    rootThickness = Math.Max(rootThickness, 0.01);
                    break;
            }
        }
    }
}
