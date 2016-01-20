using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProceduralWings
{
    class ControlManipulator //: WingManipulator
    {
        public bool updateChildren = true;
        public bool SegmentRoot = true;

        [KSPField]
        public float ctrlFraction;
        [KSPField]
        public bool symmetricMovement = true;
        [KSPField(isPersistant = true)]
        public Vector3 rootPosition = Vector3.zero;

        //public void UpdatePositions()
        //{
        //    // If we're snapping, match relative thickness scaling with root
        //    //SetThicknessScalingTypeToRoot();

        //    Tip.localScale = tipScale;
        //    Root.localScale = rootScale;

        //    Tip.localPosition = tipPosition + TipSpawnOffset;

        //    if (IsAttached &&
        //        this.part.parent != null &&
        //        this.part.parent.Modules.Contains("WingManipulator") &&
        //        !IgnoreSnapping)
        //    {
        //        WingManipulator Parent = part.parent.Modules.OfType<WingManipulator>().FirstOrDefault();
        //        part.transform.position = Parent.Tip.position + 0.1f * Parent.Tip.right; // set the new part inward just a little bit
        //        rootScale = Parent.tipScale;
        //    }

        //    if (symmetricMovement == false)
        //        tipPosition.y = Root.localPosition.y;
        //    else
        //    {
        //        tipPosition.y = 0f;
        //        tipPosition.x = 0f;
        //        rootPosition.x = 0f;
        //        rootPosition.y = 0f;

        //        Root.localPosition = -(tipPosition + TipSpawnOffset);
        //    }
        //}

        //// This method calculates part values such as mass, lift, drag and connection forces, as well as all intermediates.
        //public void CalculateAerodynamicValues()
        //{
        //    // Calculate intemediate values
        //    //print(part.name + ": Calc Aero values");
        //    b_2 = tipPosition.z - Root.localPosition.z + 1.0;

        //    MAC = (tipScale.x + rootScale.x) * modelChordLenght / 2.0;

        //    midChordSweep = (Rad2Deg * Math.Atan((Root.localPosition.x - tipPosition.x) / b_2));

        //    taperRatio = tipScale.x / rootScale.x;

        //    surfaceArea = MAC * b_2;

        //    aspectRatio = 2.0 * b_2 / MAC;

        //    ArSweepScale = Math.Pow(aspectRatio / Math.Cos(Deg2Rad * midChordSweep), 2.0) + 4.0;
        //    ArSweepScale = 2.0 + Math.Sqrt(ArSweepScale);
        //    ArSweepScale = (2.0 * Math.PI) / ArSweepScale * aspectRatio;

        //    wingMass = Math.Max(0.01, massFudgeNumber * surfaceArea * ((ArSweepScale * 2.0) / (3.0 + ArSweepScale)) * ((1.0 + taperRatio) / 2));

        //    Cd = dragBaseValue / ArSweepScale * dragMultiplier;

        //    Cl = liftFudgeNumber * surfaceArea * ArSweepScale;

        //    //print("Gather Children");
        //    GatherChildrenCl();

        //    connectionForce = Math.Round(Math.Max(Math.Sqrt(Cl + ChildrenCl) * connectionFactor, connectionMinimum), 0);

        //    updateCost();

        //    // should really do something about the joint torque here, not just its limits
        //    part.breakingForce = Mathf.Round((float)connectionForce);
        //    part.breakingTorque = Mathf.Round((float)connectionForce);

        //    // Stock-only values
        //    if (!FARactive)
        //    {
        //        // numbers for lift from: http://forum.kerbalspaceprogram.com/threads/118839-Updating-Parts-to-1-0?p=1896409&viewfull=1#post1896409
        //        float stockLiftCoefficient = (float)(surfaceArea / 3.52);
        //        // CoL/P matches CoM unless otherwise specified
        //        part.CoMOffset = new Vector3(Vector3.Dot(Tip.position - Root.position, part.transform.right) / 2, Vector3.Dot(Tip.position - Root.position, part.transform.up) / 2, 0);
        //        ModuleControlSurface mCtrlSrf = part.Modules.OfType<ModuleControlSurface>().FirstOrDefault();
        //        if (mCtrlSrf != null)
        //        {
        //            mCtrlSrf.deflectionLiftCoeff = stockLiftCoefficient;
        //            mCtrlSrf.ctrlSurfaceArea = ctrlFraction;
        //            part.mass = stockLiftCoefficient * (1 + ctrlFraction) * 0.1f;
        //        }
        //        //guiCd = (float)Math.Round(Cd, 2);
        //        //guiCl = (float)Math.Round(Cl, 2);
        //        //guiWingMass = part.mass;
        //    }
        //    else
        //    {
        //        if (part.Modules.Contains("FARControllableSurface"))
        //        {
        //            PartModule FARmodule = part.Modules["FARControllableSurface"];
        //            Type FARtype = FARmodule.GetType();
        //            FARtype.GetField("b_2").SetValue(FARmodule, b_2);
        //            FARtype.GetField("b_2_actual").SetValue(FARmodule, b_2);
        //            FARtype.GetField("MAC").SetValue(FARmodule, MAC);
        //            FARtype.GetField("MAC_actual").SetValue(FARmodule, MAC);
        //            FARtype.GetField("S").SetValue(FARmodule, surfaceArea);
        //            FARtype.GetField("MidChordSweep").SetValue(FARmodule, midChordSweep);
        //            FARtype.GetField("TaperRatio").SetValue(FARmodule, taperRatio);
        //            FARtype.GetField("ctrlSurfFrac").SetValue(FARmodule, ctrlFraction);
        //            //print("Set fields");

        //        }

        //        if (!triggerUpdate)
        //            TriggerUpdateAllWings();
        //        triggerUpdate = false;
        //    }

        //    //guiMAC = (float)MAC;
        //    //guiB_2 = (float)b_2;
        //    //guiMidChordSweep = (float)midChordSweep;
        //    //guiTaperRatio = (float)taperRatio;
        //    //guiSurfaceArea = (float)surfaceArea;
        //    //guiAspectRatio = (float)aspectRatio;

        //    StartCoroutine(updateAeroDelayed());
        //}

        //public override void setFARModuleParams()
        //{
        //    if (part.Modules.Contains("FARControllableSurface"))
        //    {
        //        PartModule FARmodule = part.Modules["FARControllableSurface"];
        //        Type FARtype = FARmodule.GetType();
        //        FARtype.GetField("b_2").SetValue(FARmodule, b_2);
        //        FARtype.GetField("b_2_actual").SetValue(FARmodule, b_2);
        //        FARtype.GetField("MAC").SetValue(FARmodule, MAC);
        //        FARtype.GetField("MAC_actual").SetValue(FARmodule, MAC);
        //        FARtype.GetField("S").SetValue(FARmodule, surfaceArea);
        //        FARtype.GetField("MidChordSweep").SetValue(FARmodule, midChordSweep);
        //        FARtype.GetField("TaperRatio").SetValue(FARmodule, taperRatio);
        //        FARtype.GetField("ctrlSurfFrac").SetValue(FARmodule, ctrlFraction);
        //    }

        //    if (!triggerUpdate)
        //        TriggerUpdateAllWings();
        //    triggerUpdate = false;
        //}

        //public override void SetStockModuleParams()
        //{
        //    // numbers for lift from: http://forum.kerbalspaceprogram.com/threads/118839-Updating-Parts-to-1-0?p=1896409&viewfull=1#post1896409
        //    part.CoMOffset.Set(Vector3.Dot(tipPos - rootPos, part.transform.right) / 2, Vector3.Dot(tipPos - rootPos, part.transform.up) / 2, 0); // COP matches COM
        //    ModuleControlSurface mCtrlSrf = part.Modules.OfType<ModuleControlSurface>().FirstOrDefault();
        //    if (mCtrlSrf != null)
        //    {
        //        mCtrlSrf.deflectionLiftCoeff = (float)surfaceArea / 3.52f;
        //        mCtrlSrf.ctrlSurfaceArea = ctrlFraction;
        //        part.mass = (float)surfaceArea * (1 + ctrlFraction) / 35.2f; // multiply by 0.1, divide by 3.52
        //    }
        //}

        //public void SetupCollider()
        //{
        //    baked = new Mesh();
        //    wingSMR.BakeMesh(baked);
        //    wingSMR.enabled = false;
        //    Transform modelTransform = transform.FindChild("model");
        //    if (modelTransform.GetComponent<MeshCollider>() == null)
        //        modelTransform.gameObject.AddComponent<MeshCollider>();
        //    MeshCollider meshCol = modelTransform.GetComponent<MeshCollider>();
        //    meshCol.sharedMesh = null;
        //    meshCol.sharedMesh = baked;
        //    meshCol.convex = true;
        //    if (FARactive)
        //    {
        //        CalculateAerodynamicValues();
        //        PartModule FARmodule = null;
        //        if (part.Modules.Contains("FARControllableSurface"))
        //            FARmodule = part.Modules["FARControllableSurface"];
        //        if (FARmodule != null)
        //        {
        //            Type FARtype = FARmodule.GetType();
        //            FARtype.GetMethod("TriggerPartColliderUpdate").Invoke(FARmodule, null);
        //        }
        //    }
        //}

        //// Updates child pWings
        //public void UpdateChildren()
        //{
        //    // Get the list of child parts
        //    for (int i = 0; i < part.children.Count; ++i)
        //    {
        //        Part p = part.children[i];
        //        // Check that it is a pWing and that it is affected by parent snapping
        //        WingManipulator wing = p.Modules.OfType<WingManipulator>().FirstOrDefault();
        //        if (wing != null && !wing.IgnoreSnapping)
        //        {
        //            // Update its positions and refresh the collider
        //            wing.UpdateGeometry();
        //            wing.SetupCollider();
        //            // If its a wing, refresh its aerodynamic values
        //            wing.CalculateAerodynamicValues();
        //        }
        //    }
        //}

        //Vector3 lastMousePos;
        //int state = 0; // 0 == nothing, 1 == translate, 2 == tipScale, 3 == rootScale
        //public void DeformWing()
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
        //    UpdateAllCopies(true);
        //}

        //void OnMouseOver()
        //{
        //    DebugValues();
        //    if (!HighLogic.LoadedSceneIsEditor || state != 0)
        //        return;

        //    lastMousePos = Input.mousePosition;
        //    if (Input.GetKeyDown(keyTranslation))
        //        state = 1;
        //    else if (Input.GetKeyDown(keyTipScale))
        //        state = 2;
        //    else if (Input.GetKeyDown(keyRootScale))
        //        state = 3;
        //}
    }
}
