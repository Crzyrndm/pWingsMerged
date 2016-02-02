using KSP;
using KSP.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;

namespace ProceduralWings
{
    //class ControlProcedural : WingProcedural
    //{
    //    public override bool isCtrlSrf
    //    {
    //        get { return true; }
    //    }

    //    [KSPField]
    //    public float ctrlFraction = 1f;

    //    public const float costDensityControl = 6500f;

    //    /// <summary>
    //    /// control surfaces cant carry fuel
    //    /// </summary>
    //    public override bool canBeFueled
    //    {
    //        get
    //        {
    //            return false;
    //        }
    //    }

    //    public override float updateCost()
    //    {
    //        return (float)Math.Round(wingMass * (1f + ArSweepScale / 4f) * (costDensity * (1f - ctrlFraction) + costDensityControl * ctrlFraction), 1);
    //    }

    //    // Some handy bools
    //    [KSPField]
    //    public bool isWingAsCtrlSrf = false;


    //    #region Shared properties / Limits and increments
    //    public virtual Vector2d GetLimitsFromType(Vector4 set)
    //    {
    //        return new Vector2d(set.z, set.w);
    //    }

    //    public override float GetIncrementFromType(float incrementWing, float incrementCtrl)
    //    {
    //        return incrementCtrl;
    //    }
    //    #endregion

    //    #region Default values
    //    public override void ReplaceDefault(ref Vector4 set, float value)
    //    {
    //        set = new Vector4(set.z, value, set.z, set.w);
    //    }

    //    public override void RestoreDefault(ref Vector4 set)
    //    {
    //        set = new Vector4(set.z, set.w, set.z, set.w);
    //    }

    //    public override float GetDefault(Vector4 set)
    //    {
    //        return set.y;
    //    }
    //    #endregion

    //    #region Geometry
    //    public override void UpdateGeometry(bool updateAerodynamics)
    //    {
    //        float ctrlOffsetRootClamped = (float)Clamp(sharedBaseOffsetRoot, sharedBaseOffsetLimits.z, sharedBaseOffsetLimits.w + 0.15f); // Mathf.Clamp (sharedBaseOffsetRoot, sharedBaseOffsetLimits.z, ctrlOffsetRootLimit - 0.075f);
    //        float ctrlOffsetTipClamped = (float)Clamp(sharedBaseOffsetTip, Math.Max(sharedBaseOffsetLimits.z - 0.15f, ctrlOffsetRootClamped - sharedBaseLength), sharedBaseOffsetLimits.w); // Mathf.Clamp (sharedBaseOffsetTip, -ctrlOffsetTipLimit + 0.075f, sharedBaseOffsetLimits.w);

    //        float ctrlThicknessDeviationRoot = sharedBaseThicknessRoot / 0.24f;
    //        float ctrlThicknessDeviationTip = sharedBaseThicknessTip / 0.24f;

    //        float ctrlEdgeWidthDeviationRoot = sharedEdgeWidthTrailingRoot / 0.24f;
    //        float ctrlEdgeWidthDeviationTip = sharedEdgeWidthTrailingTip / 0.24f;

    //        if (meshFilterCtrlFrame != null)
    //        {
    //            int length = meshReferenceCtrlFrame.vp.Length;
    //            Vector3[] vp = new Vector3[length];
    //            Array.Copy(meshReferenceCtrlFrame.vp, vp, length);
    //            Vector3[] nm = new Vector3[length];
    //            Array.Copy(meshReferenceCtrlFrame.nm, nm, length);
    //            Vector2[] uv = new Vector2[length];
    //            Array.Copy(meshReferenceCtrlFrame.uv, uv, length);
    //            Color[] cl = new Color[length];
    //            Vector2[] uv2 = new Vector2[length];

    //            if (WPDebug.logUpdateGeometry) DebugLogWithID("UpdateGeometry", "Control surface frame | Passed array setup");
    //            for (int i = 0; i < vp.Length; ++i)
    //            {
    //                // Thickness correction (X), edge width correction (Y) and span-based offset (Z)
    //                if (vp[i].z < 0f) vp[i] = new Vector3(vp[i].x * ctrlThicknessDeviationTip, vp[i].y, vp[i].z + 0.5f - sharedBaseLength / 2f); // if (vp[i].z < 0f) vp[i] = new Vector3 (vp[i].x * ctrlThicknessDeviationTip, ((vp[i].y + 0.5f) * ctrlEdgeWidthDeviationTip), vp[i].z + 0.5f - sharedBaseLength / 2f);
    //                else vp[i] = new Vector3(vp[i].x * ctrlThicknessDeviationRoot, vp[i].y, vp[i].z - 0.5f + sharedBaseLength / 2f); // else vp[i] = new Vector3 (vp[i].x * ctrlThicknessDeviationRoot, ((vp[i].y + 0.5f) * ctrlEdgeWidthDeviationRoot), vp[i].z - 0.5f + sharedBaseLength / 2f);

    //                // Left/right sides
    //                if (nm[i] == new Vector3(0f, 0f, 1f) || nm[i] == new Vector3(0f, 0f, -1f))
    //                {
    //                    // Filtering out trailing edge cross sections
    //                    if (uv[i].y > 0.185f)
    //                    {
    //                        // Filtering out root neighbours
    //                        if (vp[i].y < -0.01f)
    //                        {
    //                            if (vp[i].z < 0f)
    //                            {
    //                                vp[i] = new Vector3(vp[i].x, -sharedBaseWidthTip, vp[i].z);
    //                                uv[i] = new Vector2(sharedBaseWidthTip, uv[i].y);
    //                            }
    //                            else
    //                            {
    //                                vp[i] = new Vector3(vp[i].x, -sharedBaseWidthRoot, vp[i].z);
    //                                uv[i] = new Vector2(sharedBaseWidthRoot, uv[i].y);
    //                            }
    //                        }
    //                    }
    //                }

    //                // Root (only needs UV adjustment)
    //                else if (nm[i] == new Vector3(0f, 1f, 0f))
    //                {
    //                    if (vp[i].z < 0f) uv[i] = new Vector2(sharedBaseLength, uv[i].y);
    //                }

    //                // Trailing edge
    //                else
    //                {
    //                    // Filtering out root neighbours
    //                    if (vp[i].y < -0.1f)
    //                    {
    //                        if (vp[i].z < 0f) vp[i] = new Vector3(vp[i].x, vp[i].y + 0.5f - sharedBaseWidthTip, vp[i].z);
    //                        else vp[i] = new Vector3(vp[i].x, vp[i].y + 0.5f - sharedBaseWidthRoot, vp[i].z);
    //                    }
    //                }

    //                // Offset-based distortion
    //                if (vp[i].z < 0f)
    //                {
    //                    vp[i] = new Vector3(vp[i].x, vp[i].y, vp[i].z + vp[i].y * ctrlOffsetTipClamped);
    //                    if (nm[i] != new Vector3(0f, 0f, 1f) && nm[i] != new Vector3(0f, 0f, -1f)) uv[i] = new Vector2(uv[i].x - (vp[i].y * ctrlOffsetTipClamped) / 4f, uv[i].y);
    //                }
    //                else
    //                {
    //                    vp[i] = new Vector3(vp[i].x, vp[i].y, vp[i].z + vp[i].y * ctrlOffsetRootClamped);
    //                    if (nm[i] != new Vector3(0f, 0f, 1f) && nm[i] != new Vector3(0f, 0f, -1f)) uv[i] = new Vector2(uv[i].x - (vp[i].y * ctrlOffsetRootClamped) / 4f, uv[i].y);
    //                }

    //                // Just blanks
    //                cl[i] = new Color(0f, 0f, 0f, 0f);
    //                uv2[i] = Vector2.zero;
    //            }

    //            meshFilterCtrlFrame.mesh.vertices = vp;
    //            meshFilterCtrlFrame.mesh.uv = uv;
    //            meshFilterCtrlFrame.mesh.uv2 = uv2;
    //            meshFilterCtrlFrame.mesh.colors = cl;
    //            meshFilterCtrlFrame.mesh.RecalculateBounds();

    //            MeshCollider meshCollider = meshFilterCtrlFrame.gameObject.GetComponent<MeshCollider>();
    //            if (meshCollider == null)
    //                meshCollider = meshFilterCtrlFrame.gameObject.AddComponent<MeshCollider>();
    //            meshCollider.sharedMesh = null;
    //            meshCollider.sharedMesh = meshFilterCtrlFrame.mesh;
    //            meshCollider.convex = true;
    //            if (WPDebug.logUpdateGeometry)
    //                DebugLogWithID("UpdateGeometry", "Control surface frame | Finished");
    //        }

    //        // Next, time for edge types
    //        // Before modifying geometry, we have to show the correct objects for the current selection
    //        // As UI only works with floats, we have to cast selections into ints too

    //        int ctrlEdgeTypeInt = Mathf.RoundToInt(sharedEdgeTypeTrailing - 1);
    //        for (int i = 0; i < meshTypeCountEdgeCtrl; ++i)
    //        {
    //            if (i != ctrlEdgeTypeInt)
    //                meshFiltersCtrlEdge[i].gameObject.SetActive(false);
    //            else
    //                meshFiltersCtrlEdge[i].gameObject.SetActive(true);
    //        }

    //        // Now we can modify geometry
    //        // Copy-pasted frame deformation sequence at the moment, to be pruned later

    //        if (meshFiltersCtrlEdge[ctrlEdgeTypeInt] != null)
    //        {
    //            MeshReference meshReference = meshReferencesCtrlEdge[ctrlEdgeTypeInt];
    //            int length = meshReference.vp.Length;
    //            Vector3[] vp = new Vector3[length];
    //            Array.Copy(meshReference.vp, vp, length);
    //            Vector3[] nm = new Vector3[length];
    //            Array.Copy(meshReference.nm, nm, length);
    //            Vector2[] uv = new Vector2[length];
    //            Array.Copy(meshReference.uv, uv, length);
    //            Color[] cl = new Color[length];
    //            Vector2[] uv2 = new Vector2[length];

    //            if (WPDebug.logUpdateGeometry) DebugLogWithID("UpdateGeometry", "Control surface edge | Passed array setup");
    //            for (int i = 0; i < vp.Length; ++i)
    //            {
    //                // Thickness correction (X), edge width correction (Y) and span-based offset (Z)
    //                if (vp[i].z < 0f) vp[i] = new Vector3(vp[i].x * ctrlThicknessDeviationTip, ((vp[i].y + 0.5f) * ctrlEdgeWidthDeviationTip) - 0.5f, vp[i].z + 0.5f - sharedBaseLength / 2f);
    //                else vp[i] = new Vector3(vp[i].x * ctrlThicknessDeviationRoot, ((vp[i].y + 0.5f) * ctrlEdgeWidthDeviationRoot) - 0.5f, vp[i].z - 0.5f + sharedBaseLength / 2f);

    //                // Left/right sides
    //                if (nm[i] == new Vector3(0f, 0f, 1f) || nm[i] == new Vector3(0f, 0f, -1f))
    //                {
    //                    if (vp[i].z < 0f) vp[i] = new Vector3(vp[i].x, vp[i].y + 0.5f - sharedBaseWidthTip, vp[i].z);
    //                    else vp[i] = new Vector3(vp[i].x, vp[i].y + 0.5f - sharedBaseWidthRoot, vp[i].z);
    //                }

    //                // Trailing edge
    //                else
    //                {
    //                    // Filtering out root neighbours
    //                    if (vp[i].y < -0.1f)
    //                    {
    //                        if (vp[i].z < 0f) vp[i] = new Vector3(vp[i].x, vp[i].y + 0.5f - sharedBaseWidthTip, vp[i].z);
    //                        else vp[i] = new Vector3(vp[i].x, vp[i].y + 0.5f - sharedBaseWidthRoot, vp[i].z);
    //                    }
    //                }

    //                // Offset-based distortion
    //                if (vp[i].z < 0f)
    //                {
    //                    vp[i] = new Vector3(vp[i].x, vp[i].y, vp[i].z + vp[i].y * ctrlOffsetTipClamped);
    //                    if (nm[i] != new Vector3(0f, 0f, 1f) && nm[i] != new Vector3(0f, 0f, -1f)) uv[i] = new Vector2(uv[i].x - (vp[i].y * ctrlOffsetTipClamped) / 4f, uv[i].y);
    //                }
    //                else
    //                {
    //                    vp[i] = new Vector3(vp[i].x, vp[i].y, vp[i].z + vp[i].y * ctrlOffsetRootClamped);
    //                    if (nm[i] != new Vector3(0f, 0f, 1f) && nm[i] != new Vector3(0f, 0f, -1f)) uv[i] = new Vector2(uv[i].x - (vp[i].y * ctrlOffsetRootClamped) / 4f, uv[i].y);
    //                }

    //                // Trailing edge (UV adjustment, has to be the last as it's based on cumulative vertex positions)
    //                if (nm[i] != new Vector3(0f, 1f, 0f) && nm[i] != new Vector3(0f, 0f, 1f) && nm[i] != new Vector3(0f, 0f, -1f) && uv[i].y < 0.3f)
    //                {
    //                    if (vp[i].z < 0f) uv[i] = new Vector2(vp[i].z, uv[i].y);
    //                    else uv[i] = new Vector2(vp[i].z, uv[i].y);

    //                    // Color has to be applied there to avoid blanking out cross sections
    //                    cl[i] = GetVertexColor(2);
    //                    uv2[i] = GetVertexUV2(sharedMaterialET);
    //                }
    //            }

    //            meshFiltersCtrlEdge[ctrlEdgeTypeInt].mesh.vertices = vp;
    //            meshFiltersCtrlEdge[ctrlEdgeTypeInt].mesh.uv = uv;
    //            meshFiltersCtrlEdge[ctrlEdgeTypeInt].mesh.uv2 = uv2;
    //            meshFiltersCtrlEdge[ctrlEdgeTypeInt].mesh.colors = cl;
    //            meshFiltersCtrlEdge[ctrlEdgeTypeInt].mesh.RecalculateBounds();
    //            if (WPDebug.logUpdateGeometry)
    //                DebugLogWithID("UpdateGeometry", "Control surface edge | Finished");
    //        }

    //        // Finally, simple top/bottom surface changes

    //        if (meshFilterCtrlSurface != null)
    //        {
    //            int length = meshReferenceCtrlSurface.vp.Length;
    //            Vector3[] vp = new Vector3[length];
    //            Array.Copy(meshReferenceCtrlSurface.vp, vp, length);
    //            Vector2[] uv = new Vector2[length];
    //            Array.Copy(meshReferenceCtrlSurface.uv, uv, length);
    //            Color[] cl = new Color[length];
    //            Vector2[] uv2 = new Vector2[length];

    //            if (WPDebug.logUpdateGeometry) DebugLogWithID("UpdateGeometry", "Control surface top | Passed array setup");
    //            for (int i = 0; i < vp.Length; ++i)
    //            {
    //                // Span-based shift
    //                if (vp[i].z < 0f)
    //                {
    //                    vp[i] = new Vector3(vp[i].x, vp[i].y, vp[i].z + 0.5f - sharedBaseLength / 2f);
    //                    uv[i] = new Vector2(0f, uv[i].y);
    //                }
    //                else
    //                {
    //                    vp[i] = new Vector3(vp[i].x, vp[i].y, vp[i].z - 0.5f + sharedBaseLength / 2f);
    //                    uv[i] = new Vector2(sharedBaseLength / 4f, uv[i].y);
    //                }

    //                // Width-based shift
    //                if (vp[i].y < -0.1f)
    //                {
    //                    if (vp[i].z < 0f)
    //                    {
    //                        vp[i] = new Vector3(vp[i].x, vp[i].y + 0.5f - sharedBaseWidthTip, vp[i].z);
    //                        uv[i] = new Vector2(uv[i].x, sharedBaseWidthTip / 4f);
    //                    }
    //                    else
    //                    {
    //                        vp[i] = new Vector3(vp[i].x, vp[i].y + 0.5f - sharedBaseWidthRoot, vp[i].z);
    //                        uv[i] = new Vector2(uv[i].x, sharedBaseWidthRoot / 4f);
    //                    }
    //                }
    //                else uv[i] = new Vector2(uv[i].x, 0f);

    //                // Offsets & thickness
    //                if (vp[i].z < 0f)
    //                {
    //                    vp[i] = new Vector3(vp[i].x * ctrlThicknessDeviationTip, vp[i].y, vp[i].z + vp[i].y * ctrlOffsetTipClamped);
    //                    uv[i] = new Vector2(uv[i].x + (vp[i].y * ctrlOffsetTipClamped) / 4f, uv[i].y);
    //                }
    //                else
    //                {
    //                    vp[i] = new Vector3(vp[i].x * ctrlThicknessDeviationRoot, vp[i].y, vp[i].z + vp[i].y * ctrlOffsetRootClamped);
    //                    uv[i] = new Vector2(uv[i].x + (vp[i].y * ctrlOffsetRootClamped) / 4f, uv[i].y);
    //                }

    //                // Colors
    //                if (vp[i].x > 0f)
    //                {
    //                    cl[i] = GetVertexColor(0);
    //                    uv2[i] = GetVertexUV2(sharedMaterialST);
    //                }
    //                else
    //                {
    //                    cl[i] = GetVertexColor(1);
    //                    uv2[i] = GetVertexUV2(sharedMaterialSB);
    //                }
    //            }
    //            meshFilterCtrlSurface.mesh.vertices = vp;
    //            meshFilterCtrlSurface.mesh.uv = uv;
    //            meshFilterCtrlSurface.mesh.uv2 = uv2;
    //            meshFilterCtrlSurface.mesh.colors = cl;
    //            meshFilterCtrlSurface.mesh.RecalculateBounds();
    //            if (WPDebug.logUpdateGeometry)
    //                DebugLogWithID("UpdateGeometry", "Control surface top | Finished");
    //        }
    //        if (WPDebug.logUpdateGeometry)
    //            DebugLogWithID("UpdateGeometry", "Finished");
    //        if (updateAerodynamics)
    //            CalculateAerodynamicValues();
    //    }

    //    #endregion

    //    #region Mesh Setup and Checking
    //    public override void SetupMeshFilters()
    //    {
    //        meshFilterCtrlFrame = CheckMeshFilter(meshFilterCtrlFrame, "frame");
    //        meshFilterCtrlSurface = CheckMeshFilter(meshFilterCtrlSurface, "surface");
    //        for (int i = 0; i < meshTypeCountEdgeCtrl; ++i)
    //        {
    //            MeshFilter meshFilterCtrlEdge = CheckMeshFilter("edge_type" + i);
    //            meshFiltersCtrlEdge.Add(meshFilterCtrlEdge);
    //        }
    //    }

    //    public override void SetupMeshReferences()
    //    {
    //        bool required = true;
    //        if (meshReferenceCtrlFrame != null && meshReferenceCtrlSurface != null && meshReferencesCtrlEdge[meshTypeCountEdgeCtrl - 1] != null)
    //        {
    //            if (meshReferenceCtrlFrame.vp.Length > 0 && meshReferenceCtrlSurface.vp.Length > 0 && meshReferencesCtrlEdge[meshTypeCountEdgeCtrl - 1].vp.Length > 0)
    //            {
    //                required = false;
    //            }
    //        }
    //        if (required)
    //            SetupMeshReferencesFromScratch();
    //    }

    //    public override void SetupMeshReferencesFromScratch()
    //    {
    //        if (WPDebug.logMeshReferences)
    //            DebugLogWithID("SetupMeshReferencesFromScratch", "No sources found, creating new references");

    //        WingProcedural.meshReferenceCtrlFrame = FillMeshRefererence(meshFilterCtrlFrame);
    //        WingProcedural.meshReferenceCtrlSurface = FillMeshRefererence(meshFilterCtrlSurface);
    //        for (int i = 0; i < meshTypeCountEdgeCtrl; ++i)
    //        {
    //            MeshReference meshReferenceCtrlEdge = FillMeshRefererence(meshFiltersCtrlEdge[i]);
    //            meshReferencesCtrlEdge.Add(meshReferenceCtrlEdge);
    //        }
    //    }
    //    #endregion

    //    #region Materials
    //    public override void UpdateMaterials()
    //    {
    //        if (materialLayeredSurface == null || materialLayeredEdge == null)
    //            SetMaterialReferences();
    //        if (materialLayeredSurface != null)
    //        {
    //            SetMaterial(meshFilterCtrlSurface, materialLayeredSurface);
    //            SetMaterial(meshFilterCtrlFrame, materialLayeredEdge);
    //            for (int i = 0; i < meshTypeCountEdgeCtrl; ++i)
    //            {
    //                SetMaterial(meshFiltersCtrlEdge[i], materialLayeredEdge);
    //            }
    //        }
    //        else if (WPDebug.logUpdateMaterials)
    //            DebugLogWithID("UpdateMaterials", "Material creation failed");
    //    }

    //    public override void SetMaterialReferences()
    //    {
    //        if (materialLayeredSurface == null)
    //            materialLayeredSurface = ResourceExtractor.GetEmbeddedMaterial("ProceduralWings.B9PWing.SpecularLayered.txt");
    //        if (materialLayeredEdge == null)
    //            materialLayeredEdge = ResourceExtractor.GetEmbeddedMaterial("ProceduralWings.B9PWing.SpecularLayered.txt");

    //        SetTextures(meshFilterCtrlSurface, meshFilterCtrlFrame);

    //        if (materialLayeredSurfaceTextureMain != null && materialLayeredSurfaceTextureMask != null)
    //        {
    //            materialLayeredSurface.SetTexture("_MainTex", materialLayeredSurfaceTextureMain);
    //            materialLayeredSurface.SetTexture("_Emissive", materialLayeredSurfaceTextureMask);
    //            materialLayeredSurface.SetFloat("_Shininess", materialPropertyShininess);
    //            materialLayeredSurface.SetColor("_SpecColor", materialPropertySpecular);
    //        }
    //        else if (WPDebug.logUpdateMaterials) DebugLogWithID("SetMaterialReferences", "Surface textures not found");

    //        if (materialLayeredEdgeTextureMain != null && materialLayeredEdgeTextureMask != null)
    //        {
    //            materialLayeredEdge.SetTexture("_MainTex", materialLayeredEdgeTextureMain);
    //            materialLayeredEdge.SetTexture("_Emissive", materialLayeredEdgeTextureMask);
    //            materialLayeredEdge.SetFloat("_Shininess", materialPropertyShininess);
    //            materialLayeredEdge.SetColor("_SpecColor", materialPropertySpecular);
    //        }
    //        else if (WPDebug.logUpdateMaterials) DebugLogWithID("SetMaterialReferences", "Edge textures not found");
    //    }

    //    #endregion

    //    #region Aero

    //    // Aerodynamics value calculation
    //    // More or less lifted from pWings, so credit goes to DYJ and Taverius
    //    public override void CalculateAerodynamicValues()
    //    {
    //        if (WPDebug.logCAV)
    //            DebugLogWithID("CalculateAerodynamicValues", "Started");
    //        CheckAssemblies();

    //        float sharedWidthTipSum = sharedBaseWidthTip + sharedEdgeWidthTrailingTip;
    //        float sharedWidthRootSum = sharedBaseWidthRoot + sharedEdgeWidthTrailingRoot;

    //        float ctrlOffsetRootLimit = (sharedBaseLength / 2f) / (sharedBaseWidthRoot + sharedEdgeWidthTrailingRoot);
    //        float ctrlOffsetTipLimit = (sharedBaseLength / 2f) / (sharedBaseWidthTip + sharedEdgeWidthTrailingTip);

    //        float ctrlOffsetRootClamped = Mathf.Clamp(sharedBaseOffsetRoot, -ctrlOffsetRootLimit, ctrlOffsetRootLimit);
    //        float ctrlOffsetTipClamped = Mathf.Clamp(sharedBaseOffsetTip, -ctrlOffsetTipLimit, ctrlOffsetTipLimit);

    //        // Base four values
    //        length = (double)sharedBaseLength;
    //        taperRatio = (double)(sharedBaseLength + sharedWidthTipSum * ctrlOffsetTipClamped - sharedWidthRootSum * ctrlOffsetRootClamped) / (double)sharedBaseLength;
    //        MAC = (double)(sharedWidthTipSum + sharedWidthRootSum) / 2.0;
    //        midChordSweep = Math.Atan((double)Mathf.Abs(sharedWidthRootSum - sharedWidthTipSum) / (double)sharedBaseLength) * Rad2Deg;

    //        // Derived values

    //        surfaceArea = MAC * length;
    //        aspectRatio = 2.0f * length / MAC;

    //        ArSweepScale = Math.Pow(aspectRatio / Math.Cos(Deg2Rad * midChordSweep), 2.0f) + 4.0f;
    //        ArSweepScale = 2.0f + Math.Sqrt(ArSweepScale);
    //        ArSweepScale = (2.0f * Math.PI) / ArSweepScale * aspectRatio;

    //        wingMass = Clamp(massFudgeNumber * surfaceArea * ((ArSweepScale * 2.0) / (3.0 + ArSweepScale)) * ((1.0 + taperRatio) / 2), 0.01, double.MaxValue);
    //        Cd = dragBaseValue / ArSweepScale * dragMultiplier;
    //        Cl = liftFudgeNumber * surfaceArea * ArSweepScale;
    //        GatherChildrenCl();
    //        connectionForce = Math.Round(Clamp(Math.Sqrt(Cl + ChildrenCl) * (double)connectionFactor, (double)connectionMinimum, double.MaxValue));
    //        if (WPDebug.logCAV)
    //            DebugLogWithID("CalculateAerodynamicValues", "Passed SR/AR/ARSS/mass/Cl/Cd/connection");

    //        // Shared parameters

    //        updateCost();
    //        part.CoMOffset = new Vector3(0f, -(sharedWidthRootSum + sharedWidthTipSum) / 4f, 0f);

    //        part.breakingForce = Mathf.Round((float)connectionForce);
    //        part.breakingTorque = Mathf.Round((float)connectionForce);
    //        if (WPDebug.logCAV)
    //            DebugLogWithID("CalculateAerodynamicValues", "Passed cost/force/torque");

    //        // Stock-only values
    //        if (!FARactive)
    //        {
    //            float stockLiftCoefficient = (float)surfaceArea / 3.52f;
    //            if (WPDebug.logCAV)
    //                DebugLogWithID("CalculateAerodynamicValues", "FAR/NEAR is inactive, calculating stock control surface module values");
    //            ModuleControlSurface mCtrlSrf = part.Modules.OfType<ModuleControlSurface>().FirstOrDefault();
    //            mCtrlSrf.deflectionLiftCoeff = (float)Math.Round(stockLiftCoefficient, 2);
    //            mCtrlSrf.ctrlSurfaceArea = ctrlFraction;
    //            part.mass = stockLiftCoefficient * (1 + mCtrlSrf.ctrlSurfaceArea) * 0.1f;

    //            aeroUICd = (float)Math.Round(Cd, 2);
    //            aeroUICl = (float)Math.Round(Cl, 2);
    //            aeroUIMass = part.mass;
    //        }
    //        else
    //        {
    //            if (WPDebug.logCAV)
    //                DebugLogWithID("CalculateAerodynamicValues", "FAR/NEAR | Entered segment");
    //            if (aeroFARModuleReference == null)
    //            {
    //                if (part.Modules.Contains("FARControllableSurface"))
    //                    aeroFARModuleReference = part.Modules["FARControllableSurface"];
    //                else if (part.Modules.Contains("FARWingAerodynamicModel"))
    //                    aeroFARModuleReference = part.Modules["FARWingAerodynamicModel"];
    //                if (WPDebug.logCAV)
    //                    DebugLogWithID("CalculateAerodynamicValues", "FAR/NEAR | Module reference was null, search performed, recheck result was " + (aeroFARModuleReference == null).ToString());
    //            }
    //            if (aeroFARModuleReference != null)
    //            {
    //                if (WPDebug.logCAV)
    //                    DebugLogWithID("CalculateAerodynamicValues", "FAR/NEAR | Module reference present");
    //                if (aeroFARModuleType == null)
    //                    aeroFARModuleType = aeroFARModuleReference.GetType();
    //                if (aeroFARModuleType != null)
    //                {
    //                    if (WPDebug.logCAV)
    //                        DebugLogWithID("CalculateAerodynamicValues", "FAR/NEAR | Module type present");
    //                    if (aeroFARFieldInfoSemispan == null)
    //                        aeroFARFieldInfoSemispan = aeroFARModuleType.GetField("b_2");
    //                    if (aeroFARFieldInfoSemispan_Actual == null)
    //                        aeroFARFieldInfoSemispan_Actual = aeroFARModuleType.GetField("b_2_actual");
    //                    if (aeroFARFieldInfoMAC == null)
    //                        aeroFARFieldInfoMAC = aeroFARModuleType.GetField("MAC");
    //                    if (aeroFARFieldInfoMAC_Actual == null)
    //                        aeroFARFieldInfoMAC_Actual = aeroFARModuleType.GetField("MAC_actual");
    //                    if (aeroFARFieldInfoSurfaceArea == null)
    //                        aeroFARFieldInfoSurfaceArea = aeroFARModuleType.GetField("S");
    //                    if (aeroFARFieldInfoMidChordSweep == null)
    //                        aeroFARFieldInfoMidChordSweep = aeroFARModuleType.GetField("MidChordSweep");
    //                    if (aeroFARFieldInfoTaperRatio == null)
    //                        aeroFARFieldInfoTaperRatio = aeroFARModuleType.GetField("TaperRatio");
    //                    if (aeroFARFieldInfoControlSurfaceFraction == null)
    //                        aeroFARFieldInfoControlSurfaceFraction = aeroFARModuleType.GetField("ctrlSurfFrac");
    //                    if (aeroFARFieldInfoRootChordOffset == null)
    //                        aeroFARFieldInfoRootChordOffset = aeroFARModuleType.GetField("rootMidChordOffsetFromOrig");
                        
    //                    if (WPDebug.logCAV)
    //                        DebugLogWithID("CalculateAerodynamicValues", "FAR/NEAR | Field checks and fetching passed");

    //                    if (aeroFARMethodInfoUsed == null)
    //                    {
    //                        aeroFARMethodInfoUsed = aeroFARModuleType.GetMethod("StartInitialization");
    //                    }
    //                    if (aeroFARMethodInfoUsed != null)
    //                    {
    //                        if (WPDebug.logCAV)
    //                            DebugLogWithID("CalculateAerodynamicValues", "FAR/NEAR | Method info present");
    //                        aeroFARFieldInfoSemispan.SetValue(aeroFARModuleReference, length);
    //                        aeroFARFieldInfoSemispan_Actual.SetValue(aeroFARModuleReference, length);
    //                        aeroFARFieldInfoMAC.SetValue(aeroFARModuleReference, MAC);
    //                        aeroFARFieldInfoMAC_Actual.SetValue(aeroFARModuleReference, MAC);
    //                        //aeroFARFieldInfoSurfaceArea.SetValue (aeroFARModuleReference, aeroStatSurfaceArea);
    //                        aeroFARFieldInfoMidChordSweep.SetValue(aeroFARModuleReference, midChordSweep);
    //                        aeroFARFieldInfoTaperRatio.SetValue(aeroFARModuleReference, taperRatio);
    //                        aeroFARFieldInfoControlSurfaceFraction.SetValue(aeroFARModuleReference, ctrlFraction);
    //                        aeroFARFieldInfoRootChordOffset.SetValue(aeroFARModuleReference, (Vector3)aeroStatRootMidChordOffsetFromOrigin);

    //                        if (WPDebug.logCAV)
    //                            DebugLogWithID("CalculateAerodynamicValues", "FAR/NEAR | All values set, invoking the method");
    //                        aeroFARMethodInfoUsed.Invoke(aeroFARModuleReference, null);
    //                    }
    //                }
    //            }
    //        }
    //        if (WPDebug.logCAV)
    //            DebugLogWithID("CalculateAerodynamicValues", "FAR/NEAR | Segment ended");

    //        // Update GUI values and finish
    //        aeroUIMeanAerodynamicChord = (float)MAC;
    //        aeroUISemispan = (float)length;
    //        aeroUIMidChordSweep = (float)midChordSweep;
    //        aeroUITaperRatio = (float)taperRatio;
    //        aeroUISurfaceArea = (float)surfaceArea;
    //        aeroUIAspectRatio = (float)aspectRatio;

    //        if (WPDebug.logCAV)
    //            DebugLogWithID("CalculateAerodynamicValues", "Finished");

    //        StartCoroutine(updateAeroDelayed());
    //    }

        
    //    #endregion

    //    #region Alternative UI/input

    //    public override void OnWindow(int window)
    //    {
    //        if (uiEditMode)
    //        {

    //            bool returnEarly = false;
    //            GUILayout.BeginHorizontal();
    //            GUILayout.BeginVertical();
    //            if (uiLastFieldName.Length > 0) GUILayout.Label("Last: " + uiLastFieldName, ProceduralWingManager.uiStyleLabelMedium);
    //            else GUILayout.Label("Property editor", ProceduralWingManager.uiStyleLabelMedium);
    //            if (uiLastFieldTooltip.Length > 0) GUILayout.Label(uiLastFieldTooltip + "\n_________________________", ProceduralWingManager.uiStyleLabelHint, GUILayout.MaxHeight(44f), GUILayout.MinHeight(44f)); // 58f for four lines
    //            GUILayout.EndVertical();
    //            if (GUILayout.Button("Close", ProceduralWingManager.uiStyleButton, GUILayout.MaxWidth(50f)))
    //            {
    //                EditorLogic.fetch.Unlock("WingProceduralWindow");
    //                uiWindowActive = false;
    //                stockButton.SetFalse(false);
    //                returnEarly = true;
    //            }
    //            GUILayout.EndHorizontal();
    //            if (returnEarly)
    //                return;

    //            DrawFieldGroupHeader(ref sharedFieldGroupBaseStatic, "Base");
    //            if (sharedFieldGroupBaseStatic)
    //            {
    //                sharedBaseLength = (float)DrawField(sharedBaseLength, GetIncrementFromType(sharedIncrementMain, sharedIncrementSmall), GetIncrementFromType(1f, 0.24f), GetLimitsFromType(sharedBaseLengthLimits), "Length", uiColorSliderBase, 0, 0);
    //                sharedBaseWidthRoot = (float)DrawField(sharedBaseWidthRoot, GetIncrementFromType(sharedIncrementMain, sharedIncrementSmall), GetIncrementFromType(1f, 0.24f), GetLimitsFromType(sharedBaseWidthRootLimits), "Width (root)", uiColorSliderBase, 1, 0);
    //                sharedBaseWidthTip = (float)DrawField(sharedBaseWidthTip, GetIncrementFromType(sharedIncrementMain, sharedIncrementSmall), GetIncrementFromType(1f, 0.24f), GetLimitsFromType(sharedBaseWidthTipLimits), "Width (tip)", uiColorSliderBase, 2, 0);
    //                sharedBaseOffsetTip = (float)DrawField(sharedBaseOffsetTip, GetIncrementFromType(sharedIncrementMain, sharedIncrementSmall), 1f, GetLimitsFromType(sharedBaseOffsetLimits), "Offset (tip)", uiColorSliderBase, 4, 0);
    //                sharedBaseThicknessRoot = (float)DrawField(sharedBaseThicknessRoot, sharedIncrementSmall, sharedIncrementSmall, sharedBaseThicknessLimits, "Thickness (root)", uiColorSliderBase, 5, 0);
    //                sharedBaseThicknessTip = (float)DrawField(sharedBaseThicknessTip, sharedIncrementSmall, sharedIncrementSmall, sharedBaseThicknessLimits, "Thickness (tip)", uiColorSliderBase, 6, 0);
    //            }

    //            DrawFieldGroupHeader(ref sharedFieldGroupEdgeTrailingStatic, "Edge (trailing)");
    //            if (sharedFieldGroupEdgeTrailingStatic)
    //            {
    //                sharedEdgeTypeTrailing = (float)DrawField(sharedEdgeTypeTrailing, sharedIncrementInt, sharedIncrementInt, GetLimitsFromType(sharedEdgeTypeLimits), "Shape", uiColorSliderEdgeT, 10, 2, false);
    //                sharedEdgeWidthTrailingRoot = (float)DrawField(sharedEdgeWidthTrailingRoot, sharedIncrementSmall, sharedIncrementSmall, GetLimitsFromType(sharedEdgeWidthLimits), "Width (root)", uiColorSliderEdgeT, 11, 0);
    //                sharedEdgeWidthTrailingTip = (float)DrawField(sharedEdgeWidthTrailingTip, sharedIncrementSmall, sharedIncrementSmall, GetLimitsFromType(sharedEdgeWidthLimits), "Width (tip)", uiColorSliderEdgeT, 12, 0);
    //            }

    //            DrawFieldGroupHeader(ref sharedFieldGroupColorSTStatic, "Surface (top)");
    //            if (sharedFieldGroupColorSTStatic)
    //            {
    //                sharedMaterialST = (float)DrawField(sharedMaterialST, sharedIncrementInt, sharedIncrementInt, sharedMaterialLimits, "Material", uiColorSliderColorsST, 13, 1, false);
    //                sharedColorSTOpacity = (float)DrawField(sharedColorSTOpacity, sharedIncrementColor, sharedIncrementColorLarge, sharedColorLimits, "Opacity", uiColorSliderColorsST, 14, 0);
    //                sharedColorSTHue = (float)DrawField(sharedColorSTHue, sharedIncrementColor, sharedIncrementColorLarge, sharedColorLimits, "Hue", uiColorSliderColorsST, 15, 0);
    //                sharedColorSTSaturation = (float)DrawField(sharedColorSTSaturation, sharedIncrementColor, sharedIncrementColorLarge, sharedColorLimits, "Saturation", uiColorSliderColorsST, 16, 0);
    //                sharedColorSTBrightness = (float)DrawField(sharedColorSTBrightness, sharedIncrementColor, sharedIncrementColorLarge, sharedColorLimits, "Brightness", uiColorSliderColorsST, 17, 0);
    //            }

    //            DrawFieldGroupHeader(ref sharedFieldGroupColorSBStatic, "Surface (bottom)");
    //            if (sharedFieldGroupColorSBStatic)
    //            {
    //                sharedMaterialSB = (float)DrawField(sharedMaterialSB, sharedIncrementInt, sharedIncrementInt, sharedMaterialLimits, "Material", uiColorSliderColorsSB, 13, 1, false);
    //                sharedColorSBOpacity = (float)DrawField(sharedColorSBOpacity, sharedIncrementColor, sharedIncrementColorLarge, sharedColorLimits, "Opacity", uiColorSliderColorsSB, 14, 0);
    //                sharedColorSBHue = (float)DrawField(sharedColorSBHue, sharedIncrementColor, sharedIncrementColorLarge, sharedColorLimits, "Hue", uiColorSliderColorsSB, 15, 0);
    //                sharedColorSBSaturation = (float)DrawField(sharedColorSBSaturation, sharedIncrementColor, sharedIncrementColorLarge, sharedColorLimits, "Saturation", uiColorSliderColorsSB, 16, 0);
    //                sharedColorSBBrightness = (float)DrawField(sharedColorSBBrightness, sharedIncrementColor, sharedIncrementColorLarge, sharedColorLimits, "Brightness", uiColorSliderColorsSB, 17, 0);
    //            }

    //            DrawFieldGroupHeader(ref sharedFieldGroupColorETStatic, "Surface (trailing edge)");
    //            if (sharedFieldGroupColorETStatic)
    //            {
    //                sharedMaterialET = (float)DrawField(sharedMaterialET, sharedIncrementInt, sharedIncrementInt, sharedMaterialLimits, "Material", uiColorSliderColorsET, 13, 1, false);
    //                sharedColorETOpacity = (float)DrawField(sharedColorETOpacity, sharedIncrementColor, sharedIncrementColorLarge, sharedColorLimits, "Opacity", uiColorSliderColorsET, 14, 0);
    //                sharedColorETHue = (float)DrawField(sharedColorETHue, sharedIncrementColor, sharedIncrementColorLarge, sharedColorLimits, "Hue", uiColorSliderColorsET, 15, 0);
    //                sharedColorETSaturation = (float)DrawField(sharedColorETSaturation, sharedIncrementColor, sharedIncrementColorLarge, sharedColorLimits, "Saturation", uiColorSliderColorsET, 16, 0);
    //                sharedColorETBrightness = (float)DrawField(sharedColorETBrightness, sharedIncrementColor, sharedIncrementColorLarge, sharedColorLimits, "Brightness", uiColorSliderColorsET, 17, 0);
    //            }

    //            GUILayout.Label("_________________________\n\nPress J to exit edit mode\nOptions below allow you to change default values", ProceduralWingManager.uiStyleLabelHint);

    //            GUILayout.BeginHorizontal();
    //            if (GUILayout.Button("Save as default", ProceduralWingManager.uiStyleButton))
    //                ReplaceDefaults();
    //            if (GUILayout.Button("Restore default", ProceduralWingManager.uiStyleButton))
    //                RestoreDefaults();
    //            GUILayout.EndHorizontal();
    //            if (inheritancePossibleOnShape || inheritancePossibleOnMaterials)
    //            {
    //                GUILayout.Label("_________________________\n\nOptions options allow you to match the part properties to it's parent", ProceduralWingManager.uiStyleLabelHint);
    //                GUILayout.BeginHorizontal();
    //                if (inheritancePossibleOnShape)
    //                {
    //                    if (GUILayout.Button("Shape", ProceduralWingManager.uiStyleButton))
    //                        InheritParentValues(0);
    //                    if (GUILayout.Button("Base", ProceduralWingManager.uiStyleButton))
    //                        InheritParentValues(1);
    //                    if (GUILayout.Button("Edges", ProceduralWingManager.uiStyleButton))
    //                        InheritParentValues(2);
    //                }
    //                if (inheritancePossibleOnMaterials)
    //                {
    //                    if (GUILayout.Button("Color", ProceduralWingManager.uiStyleButton)) InheritParentValues(3);
    //                }
    //                GUILayout.EndHorizontal();
    //            }
    //        }
    //        else
    //        {
    //            if (uiEditModeTimeout)
    //                GUILayout.Label("Exiting edit mode...\n", ProceduralWingManager.uiStyleLabelMedium);
    //            else
    //            {
    //                GUILayout.BeginHorizontal();
    //                GUILayout.Label("Press J while pointing at a\nprocedural part to edit it", ProceduralWingManager.uiStyleLabelHint);
    //                if (GUILayout.Button("Close", ProceduralWingManager.uiStyleButton, GUILayout.MaxWidth(50f)))
    //                {
    //                    uiWindowActive = false;
    //                    stockButton.SetFalse(false);
    //                    uiAdjustWindow = true;
    //                    EditorLogic.fetch.Unlock("WingProceduralWindow");
    //                }
    //                GUILayout.EndHorizontal();
    //            }
    //        }
    //        GUI.DragWindow();
    //    }

    //    public override int GetFieldMode()
    //    {
    //        return 2;
    //    }

    //    public override string UpdateTooltipText(int fieldID)
    //    {
    //        switch (fieldID)
    //        {
    //            case 0: // sharedBaseLength))
    //                return "Lateral measurement of the control \nsurface at it's root";
    //            case 1: // sharedBaseWidthRoot))
    //                return "Longitudinal measurement of \nthe root chord";
    //            case 2: // sharedBaseWidthTip))
    //                return "Longitudinal measurement of \nthe tip chord";
    //            case 3: // sharedBaseOffsetRoot))
    //                return "Offset of the trailing edge \nroot corner on the lateral axis";
    //            case 4: // sharedBaseOffsetTip))
    //                return "Offset of the trailing edge \ntip corner on the lateral axis";
    //            case 5: // sharedBaseThicknessRoot))
    //                return "Thickness at the root cross section \nUsually kept proportional to edge width";
    //            case 6: // sharedBaseThicknessTip))
    //                return "Thickness at the tip cross section \nUsually kept proportional to edge width";
    //            case 7: // sharedEdgeTypeTrailing))
    //                return "Shape of the trailing edge cross \nsection (round/biconvex/sharp)";
    //            case 8: // sharedEdgeWidthTrailingRoot))
    //                return "Longitudinal measurement of the trailing \nedge cross section at with root";
    //            case 9: // sharedEdgeWidthTrailingTip))
    //                return "Longitudinal measurement of the trailing \nedge cross section at with tip";
    //            case 10: // sharedEdgeTypeLeading))
    //                return "Shape of the leading edge cross \nsection (round/biconvex/sharp)";
    //            case 11: // sharedEdgeWidthLeadingRoot))
    //                return "Longitudinal measurement of the leading \nedge cross section at wing root";
    //            case 12: // sharedEdgeWidthLeadingTip))
    //                return "Longitudinal measurement of the leading \nedge cross section at with tip";
    //            case 13:
    //                return "Surface material (uniform fill, plating, \nLRSI/HRSI tiles and so on)";
    //            case 14:
    //                return "Fairly self-explanatory, controls the paint \nopacity: no paint at 0, full coverage at 1";
    //            case 15:
    //                return "Controls the paint hue (HSB axis): \nvalues from zero to one make full circle";
    //            case 16:
    //                return "Controls the paint saturation (HSB axis): \ncolorless at 0, full color at 1";
    //            case 17:
    //                return "Controls the paint brightness (HSB axis): black at 0, white at 1, primary at 0.5";
    //            default:
    //                return "Unknown field\n";
    //        }
    //    }

    //    public override void translateTip(Vector3 diff)
    //    {
    //        if (!Input.GetKey(keyTranslation))
    //        {
    //            state = 0;
    //            return;
    //        }
    //        sharedBaseLength += 2 * diff.x * Vector3.Dot(EditorCamera.Instance.camera.transform.right, part.transform.right) + diff.y * Vector3.Dot(EditorCamera.Instance.camera.transform.up, part.transform.right);
    //        sharedBaseLength = (float)Clamp(sharedBaseLength, GetLimitsFromType(sharedBaseLengthLimits).x, GetLimitsFromType(sharedBaseLengthLimits).y);
    //    }

    //    public override void scaleTip(Vector3 diff)
    //    {
    //        if (!Input.GetKey(keyTipScale))
    //        {
    //            state = 0;
    //            return;
    //        }
    //        sharedBaseWidthTip += diff.x * Vector3.Dot(EditorCamera.Instance.camera.transform.right, -part.transform.up) + diff.y * Vector3.Dot(EditorCamera.Instance.camera.transform.up, -part.transform.up);
    //        sharedBaseWidthTip = (float)Clamp(sharedBaseWidthTip, GetLimitsFromType(sharedBaseWidthTipLimits).x, GetLimitsFromType(sharedBaseWidthTipLimits).y);
    //        sharedBaseThicknessTip += diff.x * Vector3.Dot(EditorCamera.Instance.camera.transform.right, -part.transform.forward) + diff.y * Vector3.Dot(EditorCamera.Instance.camera.transform.up, part.transform.forward * (part.isMirrored ? 1 : -1));
    //        sharedBaseThicknessTip = (float)Clamp(sharedBaseThicknessTip, sharedBaseThicknessLimits.x, sharedBaseThicknessLimits.y);
    //    }

    //    public override void scaleRoot(Vector3 diff)
    //    {
    //        if (part.parent.Modules.OfType<ProceduralWing>().Any())
    //            return;
    //        if (!Input.GetKey(keyRootScale))
    //        {
    //            state = 0;
    //            return;
    //        }
    //        sharedBaseWidthRoot += diff.x * Vector3.Dot(EditorCamera.Instance.camera.transform.right, -part.transform.up) + diff.y * Vector3.Dot(EditorCamera.Instance.camera.transform.up, -part.transform.up);
    //        sharedBaseWidthRoot = (float)Clamp(sharedBaseWidthRoot, GetLimitsFromType(sharedBaseWidthRootLimits).x, GetLimitsFromType(sharedBaseWidthRootLimits).y);
    //        sharedBaseThicknessRoot += diff.x * Vector3.Dot(EditorCamera.Instance.camera.transform.right, -part.transform.forward) + diff.y * Vector3.Dot(EditorCamera.Instance.camera.transform.up, part.transform.forward * (part.isMirrored ? 1 : -1));
    //        sharedBaseThicknessRoot = (float)Clamp(sharedBaseThicknessRoot, sharedBaseThicknessLimits.x, sharedBaseThicknessLimits.y);
    //    }

    //    public override string GetWindowTitle()
    //    {
    //        if (isWingAsCtrlSrf)
    //            return "All-moving control surface";
    //        else
    //            return "Control surface";
    //    }

    //    #endregion
    //}
}
