using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralWings.B9PWing
{
    using Utility;
    using UI;
    class B9_ProceduralControl : B9_ProceduralWing
    {
        public override bool IsCtrlSrf
        {
            get { return true; }
        }

        #region physical dimensions
        [KSPField]
        public float ctrlFraction = 1f;

        protected WingProperty rootOffset;
        public double RootOffset
        {
            get { return rootOffset.Value; }
            set
            {
                rootOffset.Value = value;
                StartCoroutine(UpdateSymmetricGeometry());
            }

        }
        #endregion

        #region entry points
        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
            try
            {
                rootOffset.Save(node);
            }
            catch
            { }
        }

        #endregion

        #region setting up
        public override void SetupProperties()
        {
            if (length != null)
                return;
            if (part.symmetryCounterparts.Count == 0 || part.symmetryCounterparts[0].Modules.GetModule<B9_ProceduralControl>().length == null)
            {
                length = new WingProperty("Length", nameof(length), 1, 2, 0.05, 8, "Lateral measurement of the wing, \nalso referred to as semispan");
                rootOffset = new WingProperty("Offset (root)", nameof(rootOffset), 0, 2, -1, 1, "Offset of the trailing edge \nroot corner on the lateral axis");
                tipOffset = new WingProperty("Offset (tip)", nameof(tipOffset), 0, 2, -1, 1, "Offset of the trailing edge \ntip corner on the lateral axis");
                rootWidth = new WingProperty("Width (root)", nameof(rootWidth), 0.5, 2, 0.05, 1, "Longitudinal measurement of the wing \nat the root cross section");
                tipWidth = new WingProperty("Width (tip)", nameof(tipWidth), 0.5, 2, 0.05, 1, "Longitudinal measurement of the wing \nat the tip cross section");
                rootThickness = new WingProperty("Thickness (root)", nameof(rootThickness), 0.24, 2, 0.01, 1, "Thickness at the root cross section \nUsually kept proportional to edge width");
                tipThickness = new WingProperty("Thickness (tip)", nameof(tipThickness), 0.24, 2, 0.01, 1, "Thickness at the tip cross section \nUsually kept proportional to edge width");

                trailingEdgeType = new WingProperty("Shape", nameof(trailingEdgeType), 3, 0, 1, 3, "Shape of the trailing edge cross \nsection (round/biconvex/sharp)");
                rootTrailingEdge = new WingProperty("Width (root)", nameof(rootTrailingEdge), 0.48, 2, 0.01, 1.0, "Longitudinal measurement of the trailing \nedge cross section at wing root");
                tipTrailingEdge = new WingProperty("Width (tip)", nameof(tipTrailingEdge), 0.48, 2, 0.01, 1.0, "Longitudinal measurement of the trailing \nedge cross section at wing tip");

                surfTopMat = new WingProperty("Material", nameof(surfTopMat), 1, 0, 0, 4, "Surface material (uniform fill, plating, \nLRSI/HRSI tiles and so on)");
                surfTopOpacity = new WingProperty("Opacity", nameof(surfTopOpacity), 0, 2, 0, 1, "Fairly self-explanatory, controls the paint \nopacity: no paint at 0, full coverage at 1");
                surfTopHue = new WingProperty("Hue", nameof(surfTopHue), 0.1, 2, 0, 1, "Controls the paint hue (HSB axis): \nvalues from zero to one make full circle");
                surfTopSat = new WingProperty("Saturation", nameof(surfTopSat), 0.75, 2, 0, 1, "Controls the paint saturation (HSB axis): \ncolorless at 0, full color at 1");
                surfTopBright = new WingProperty("Brightness", nameof(surfTopBright), 0.6, 2, 0, 1, "Controls the paint brightness (HSB axis): black at 0, white at 1, primary at 0.5");

                surfBottomMat = new WingProperty("Material", nameof(surfBottomMat), 4, 0, 0, 4, "Surface material (uniform fill, plating, \nLRSI/HRSI tiles and so on)");
                surfBottomOpacity = new WingProperty("Opacity", nameof(surfBottomOpacity), 0, 2, 0, 1, "Fairly self-explanatory, controls the paint \nopacity: no paint at 0, full coverage at 1");
                surfBottomHue = new WingProperty("Hue", nameof(surfBottomHue), 0.1, 2, 0, 1, "Controls the paint hue (HSB axis): \nvalues from zero to one make full circle");
                surfBottomSat = new WingProperty("Saturation", nameof(surfBottomSat), 0.75, 2, 0, 1, "Controls the paint saturation (HSB axis): \ncolorless at 0, full color at 1");
                surfBottomBright = new WingProperty("Brightness", nameof(surfBottomBright), 0.6, 2, 0, 1, "Controls the paint brightness (HSB axis): black at 0, white at 1, primary at 0.5");

                surfTrailMat = new WingProperty("Material", nameof(surfTrailMat), 4, 0, 0, 4, "Surface material (uniform fill, plating, \nLRSI/HRSI tiles and so on)");
                surfTrailOpacity = new WingProperty("Opacity", nameof(surfTrailOpacity), 0, 2, 0, 1, "Fairly self-explanatory, controls the paint \nopacity: no paint at 0, full coverage at 1");
                surfTrailHue = new WingProperty("Hue", nameof(surfTrailHue), 0.1, 2, 0, 1, "Controls the paint hue (HSB axis): \nvalues from zero to one make full circle");
                surfTrailSat = new WingProperty("Saturation", nameof(surfTrailSat), 0.75, 2, 0, 1, "Controls the paint saturation (HSB axis): \ncolorless at 0, full color at 1");
                surfTrailBright = new WingProperty("Brightness", nameof(surfTrailBright), 0.6, 2, 0, 1, "Controls the paint brightness (HSB axis): black at 0, white at 1, primary at 0.5");
            }
            else
            {
                B9_ProceduralControl wp = part.symmetryCounterparts[0].Modules.GetModule<B9_ProceduralControl>();  // all properties for symmetry will be the same object. Yay for no need to update values :D
                length = wp.length;
                tipOffset = wp.tipOffset;
                rootOffset = wp.rootOffset;
                rootWidth = wp.rootWidth;
                tipWidth = wp.tipWidth;
                rootThickness = wp.rootThickness;
                tipThickness = wp.tipThickness;

                trailingEdgeType = wp.trailingEdgeType;
                rootTrailingEdge = wp.rootTrailingEdge;
                tipTrailingEdge = wp.tipTrailingEdge;

                surfTopMat = wp.surfTopMat;
                surfTopOpacity = wp.surfTopOpacity;
                surfTopHue = wp.surfTopHue;
                surfTopSat = wp.surfTopSat;
                surfTopBright = wp.surfTopBright;

                surfBottomMat = wp.surfBottomMat;
                surfBottomOpacity = wp.surfBottomOpacity;
                surfBottomHue = wp.surfBottomHue;
                surfBottomSat = wp.surfBottomSat;
                surfBottomBright = wp.surfBottomBright;

                surfTrailMat = wp.surfTrailMat;
                surfTrailOpacity = wp.surfTrailOpacity;
                surfTrailHue = wp.surfTrailHue;
                surfTrailSat = wp.surfTrailSat;
                surfTrailBright = wp.surfTrailBright;
            }
        }

        public override void LoadWingProperty(ConfigNode n)
        {
            switch (n.GetValue("ID"))
            {
                case nameof(rootOffset):
                    rootOffset.Load(n);
                    break;
                default:
                    base.LoadWingProperty(n);
                    break;
            }
        }

        #endregion

        #region Geometry
        public override void UpdateGeometry(bool updateAerodynamics)
        {
            float ctrlOffsetRootClamped = (float)Utils.Clamp(RootOffset, rootOffset.min, rootOffset.max);
            float ctrlOffsetTipClamped = (float)Utils.Clamp(TipOffset, Math.Max(rootOffset.min, ctrlOffsetRootClamped - Length), rootOffset.max);

            float ctrlThicknessDeviationRoot = (float)RootThickness / 0.24f;
            float ctrlThicknessDeviationTip = (float)TipThickness / 0.24f;

            float ctrlEdgeWidthDeviationRoot = (float)RootTrailingEdge / 0.24f;
            float ctrlEdgeWidthDeviationTip = (float)TipTrailingEdge / 0.24f;

            if (meshFilterWingSection != null)
            {
                int length = meshReferenceCtrlFrame.vp.Length;
                Vector3[] vp = new Vector3[length];
                Array.Copy(meshReferenceCtrlFrame.vp, vp, length);
                Vector3[] nm = new Vector3[length];
                Array.Copy(meshReferenceCtrlFrame.nm, nm, length);
                Vector2[] uv = new Vector2[length];
                Array.Copy(meshReferenceCtrlFrame.uv, uv, length);
                Color[] cl = new Color[length];
                Vector2[] uv2 = new Vector2[length];

                for (int i = 0; i < vp.Length; ++i)
                {
                    // Thickness correction (X), edge width correction (Y) and span-based offset (Z)
                    if (vp[i].z < 0f) vp[i] = new Vector3(vp[i].x * ctrlThicknessDeviationTip, vp[i].y, vp[i].z + 0.5f - (float)Length / 2f);
                    else vp[i] = new Vector3(vp[i].x * ctrlThicknessDeviationRoot, vp[i].y, vp[i].z - 0.5f + (float)Length / 2f);

                    // Left/right sides
                    if (nm[i] == new Vector3(0f, 0f, 1f) || nm[i] == new Vector3(0f, 0f, -1f))
                    {
                        // Filtering out trailing edge cross sections
                        if (uv[i].y > 0.185f)
                        {
                            // Filtering out root neighbours
                            if (vp[i].y < -0.01f)
                            {
                                if (vp[i].z < 0f)
                                {
                                    vp[i] = new Vector3(vp[i].x, -(float)TipWidth, vp[i].z);
                                    uv[i] = new Vector2((float)TipWidth, uv[i].y);
                                }
                                else
                                {
                                    vp[i] = new Vector3(vp[i].x, -(float)RootWidth, vp[i].z);
                                    uv[i] = new Vector2((float)RootWidth, uv[i].y);
                                }
                            }
                        }
                    }

                    // Root (only needs UV adjustment)
                    else if (nm[i] == new Vector3(0f, 1f, 0f))
                    {
                        if (vp[i].z < 0f) uv[i] = new Vector2((float)Length, uv[i].y);
                    }

                    // Trailing edge
                    else
                    {
                        // Filtering out root neighbours
                        if (vp[i].y < -0.1f)
                        {
                            if (vp[i].z < 0f) vp[i] = new Vector3(vp[i].x, vp[i].y + 0.5f - (float)TipWidth, vp[i].z);
                            else vp[i] = new Vector3(vp[i].x, vp[i].y + 0.5f - (float)RootWidth, vp[i].z);
                        }
                    }

                    // Offset-based distortion
                    if (vp[i].z < 0f)
                    {
                        vp[i] = new Vector3(vp[i].x, vp[i].y, vp[i].z + vp[i].y * ctrlOffsetTipClamped);
                        if (nm[i] != new Vector3(0f, 0f, 1f) && nm[i] != new Vector3(0f, 0f, -1f)) uv[i] = new Vector2(uv[i].x - (vp[i].y * ctrlOffsetTipClamped) / 4f, uv[i].y);
                    }
                    else
                    {
                        vp[i] = new Vector3(vp[i].x, vp[i].y, vp[i].z + vp[i].y * ctrlOffsetRootClamped);
                        if (nm[i] != new Vector3(0f, 0f, 1f) && nm[i] != new Vector3(0f, 0f, -1f)) uv[i] = new Vector2(uv[i].x - (vp[i].y * ctrlOffsetRootClamped) / 4f, uv[i].y);
                    }

                    // Just blanks
                    cl[i] = new Color(0f, 0f, 0f, 0f);
                    uv2[i] = Vector2.zero;
                }

                meshFilterWingSection.mesh.vertices = vp;
                meshFilterWingSection.mesh.uv = uv;
                meshFilterWingSection.mesh.uv2 = uv2;
                meshFilterWingSection.mesh.colors = cl;
                meshFilterWingSection.mesh.RecalculateBounds();

                MeshCollider meshCollider = meshFilterWingSection.gameObject.GetComponent<MeshCollider>();
                if (meshCollider == null)
                    meshCollider = meshFilterWingSection.gameObject.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = null;
                meshCollider.sharedMesh = meshFilterWingSection.mesh;
                meshCollider.convex = true;
            }

            // Next, time for edge types
            // Before modifying geometry, we have to show the correct objects for the current selection
            // As UI only works with floats, we have to cast selections into ints too

            int ctrlEdgeTypeInt = Mathf.RoundToInt(TrailingEdgeType - 1);
            for (int i = 0; i < meshTypeCountEdgeWing; ++i)
            {
                if (i != ctrlEdgeTypeInt)
                    meshFiltersWingEdgeTrailing[i].gameObject.SetActive(false);
                else
                    meshFiltersWingEdgeTrailing[i].gameObject.SetActive(true);
            }

            // Now we can modify geometry
            // Copy-pasted frame deformation sequence at the moment, to be pruned later

            if (meshFiltersWingEdgeTrailing[ctrlEdgeTypeInt] != null)
            {
                MeshReference meshReference = meshReferencesCtrlEdge[ctrlEdgeTypeInt];
                int length = meshReference.vp.Length;
                Vector3[] vp = new Vector3[length];
                Array.Copy(meshReference.vp, vp, length);
                Vector3[] nm = new Vector3[length];
                Array.Copy(meshReference.nm, nm, length);
                Vector2[] uv = new Vector2[length];
                Array.Copy(meshReference.uv, uv, length);
                Color[] cl = new Color[length];
                Vector2[] uv2 = new Vector2[length];

                for (int i = 0; i < vp.Length; ++i)
                {
                    // Thickness correction (X), edge width correction (Y) and span-based offset (Z)
                    if (vp[i].z < 0f) vp[i] = new Vector3(vp[i].x * ctrlThicknessDeviationTip, ((vp[i].y + 0.5f) * ctrlEdgeWidthDeviationTip) - 0.5f, vp[i].z + 0.5f - (float)Length / 2f);
                    else vp[i] = new Vector3(vp[i].x * ctrlThicknessDeviationRoot, ((vp[i].y + 0.5f) * ctrlEdgeWidthDeviationRoot) - 0.5f, vp[i].z - 0.5f + (float)Length / 2f);

                    // Left/right sides
                    if (nm[i] == new Vector3(0f, 0f, 1f) || nm[i] == new Vector3(0f, 0f, -1f))
                    {
                        if (vp[i].z < 0f) vp[i] = new Vector3(vp[i].x, vp[i].y + 0.5f - (float)TipWidth, vp[i].z);
                        else vp[i] = new Vector3(vp[i].x, vp[i].y + 0.5f - (float)RootWidth, vp[i].z);
                    }

                    // Trailing edge
                    else
                    {
                        // Filtering out root neighbours
                        if (vp[i].y < -0.1f)
                        {
                            if (vp[i].z < 0f) vp[i] = new Vector3(vp[i].x, vp[i].y + 0.5f - (float)TipWidth, vp[i].z);
                            else vp[i] = new Vector3(vp[i].x, vp[i].y + 0.5f - (float)RootWidth, vp[i].z);
                        }
                    }

                    // Offset-based distortion
                    if (vp[i].z < 0f)
                    {
                        vp[i] = new Vector3(vp[i].x, vp[i].y, vp[i].z + vp[i].y * ctrlOffsetTipClamped);
                        if (nm[i] != new Vector3(0f, 0f, 1f) && nm[i] != new Vector3(0f, 0f, -1f)) uv[i] = new Vector2(uv[i].x - (vp[i].y * ctrlOffsetTipClamped) / 4f, uv[i].y);
                    }
                    else
                    {
                        vp[i] = new Vector3(vp[i].x, vp[i].y, vp[i].z + vp[i].y * ctrlOffsetRootClamped);
                        if (nm[i] != new Vector3(0f, 0f, 1f) && nm[i] != new Vector3(0f, 0f, -1f)) uv[i] = new Vector2(uv[i].x - (vp[i].y * ctrlOffsetRootClamped) / 4f, uv[i].y);
                    }

                    // Trailing edge (UV adjustment, has to be the last as it's based on cumulative vertex positions)
                    if (nm[i] != new Vector3(0f, 1f, 0f) && nm[i] != new Vector3(0f, 0f, 1f) && nm[i] != new Vector3(0f, 0f, -1f) && uv[i].y < 0.3f)
                    {
                        if (vp[i].z < 0f) uv[i] = new Vector2(vp[i].z, uv[i].y);
                        else uv[i] = new Vector2(vp[i].z, uv[i].y);

                        // Color has to be applied there to avoid blanking out cross sections
                        cl[i] = TrailColour;
                        uv2[i] = GetVertexUV2(SurfTrailMat);
                    }
                }

                meshFiltersWingEdgeTrailing[ctrlEdgeTypeInt].mesh.vertices = vp;
                meshFiltersWingEdgeTrailing[ctrlEdgeTypeInt].mesh.uv = uv;
                meshFiltersWingEdgeTrailing[ctrlEdgeTypeInt].mesh.uv2 = uv2;
                meshFiltersWingEdgeTrailing[ctrlEdgeTypeInt].mesh.colors = cl;
                meshFiltersWingEdgeTrailing[ctrlEdgeTypeInt].mesh.RecalculateBounds();
            }

            // Finally, simple top/bottom surface changes

            if (meshFilterWingSurface != null)
            {
                int length = meshReferenceCtrlSurface.vp.Length;
                Vector3[] vp = new Vector3[length];
                Array.Copy(meshReferenceCtrlSurface.vp, vp, length);
                Vector2[] uv = new Vector2[length];
                Array.Copy(meshReferenceCtrlSurface.uv, uv, length);
                Color[] cl = new Color[length];
                Vector2[] uv2 = new Vector2[length];

                for (int i = 0; i < vp.Length; ++i)
                {
                    // Span-based shift
                    if (vp[i].z < 0f)
                    {
                        vp[i] = new Vector3(vp[i].x, vp[i].y, vp[i].z + 0.5f - (float)Length / 2f);
                        uv[i] = new Vector2(0f, uv[i].y);
                    }
                    else
                    {
                        vp[i] = new Vector3(vp[i].x, vp[i].y, vp[i].z - 0.5f + (float)Length / 2f);
                        uv[i] = new Vector2((float)Length / 4f, uv[i].y);
                    }

                    // Width-based shift
                    if (vp[i].y < -0.1f)
                    {
                        if (vp[i].z < 0f)
                        {
                            vp[i] = new Vector3(vp[i].x, vp[i].y + 0.5f - (float)TipWidth, vp[i].z);
                            uv[i] = new Vector2(uv[i].x, (float)TipWidth / 4f);
                        }
                        else
                        {
                            vp[i] = new Vector3(vp[i].x, vp[i].y + 0.5f - (float)RootWidth, vp[i].z);
                            uv[i] = new Vector2(uv[i].x, (float)RootWidth / 4f);
                        }
                    }
                    else uv[i] = new Vector2(uv[i].x, 0f);

                    // Offsets & thickness
                    if (vp[i].z < 0f)
                    {
                        vp[i] = new Vector3(vp[i].x * ctrlThicknessDeviationTip, vp[i].y, vp[i].z + vp[i].y * ctrlOffsetTipClamped);
                        uv[i] = new Vector2(uv[i].x + (vp[i].y * ctrlOffsetTipClamped) / 4f, uv[i].y);
                    }
                    else
                    {
                        vp[i] = new Vector3(vp[i].x * ctrlThicknessDeviationRoot, vp[i].y, vp[i].z + vp[i].y * ctrlOffsetRootClamped);
                        uv[i] = new Vector2(uv[i].x + (vp[i].y * ctrlOffsetRootClamped) / 4f, uv[i].y);
                    }

                    // Colors
                    if (vp[i].x > 0f)
                    {
                        cl[i] = TopColour;
                        uv2[i] = GetVertexUV2(SurfTopMat);
                    }
                    else
                    {
                        cl[i] = BottomColour;
                        uv2[i] = GetVertexUV2(SurfBottomMat);
                    }
                }
                meshFilterWingSurface.mesh.vertices = vp;
                meshFilterWingSurface.mesh.uv = uv;
                meshFilterWingSurface.mesh.uv2 = uv2;
                meshFilterWingSurface.mesh.colors = cl;
                meshFilterWingSurface.mesh.RecalculateBounds();
            }
            if (updateAerodynamics)
                CalculateAerodynamicValues();
        }

        #endregion

        #region Mesh
        public override int meshTypeCountEdgeWing
        {
            get { return 3; }
        }

        public static MeshReference meshReferenceCtrlFrame;
        public static MeshReference meshReferenceCtrlSurface;
        public static List<MeshReference> meshReferencesCtrlEdge = new List<MeshReference>();

        public override void SetupMeshFilters()
        {
            meshFilterWingSection = CheckMeshFilter(meshFilterWingSection, "frame");
            meshFilterWingSurface = CheckMeshFilter(meshFilterWingSurface, "surface");
            for (int i = 0; i < meshTypeCountEdgeWing; ++i)
            {
                MeshFilter meshFilterCtrlEdge = CheckMeshFilter("edge_type" + i);
                meshFiltersWingEdgeTrailing.Add(meshFilterCtrlEdge);
            }
        }

        public override void SetupMeshReferences()
        {
            if (meshReferenceCtrlFrame == null || meshReferenceCtrlFrame.vp.Length == 0
                || meshReferenceCtrlSurface != null || meshReferenceCtrlSurface.vp.Length > 0
                || meshReferencesCtrlEdge[meshTypeCountEdgeWing - 1] != null || meshReferencesCtrlEdge[meshTypeCountEdgeWing - 1].vp.Length > 0)
            {
                meshReferenceCtrlFrame = FillMeshRefererence(meshFilterWingSection);
                meshReferenceCtrlSurface = FillMeshRefererence(meshFilterWingSurface);
                for (int i = 0; i < meshTypeCountEdgeWing; ++i)
                {
                    MeshReference meshReferenceCtrlEdge = FillMeshRefererence(meshFiltersWingEdgeTrailing[i]);
                    meshReferencesCtrlEdge.Add(meshReferenceCtrlEdge);
                }

            }
        }
        #endregion

        #region Materials
        public override void UpdateMaterials()
        {
            if (materialLayeredSurface == null || materialLayeredEdge == null)
                SetMaterialReferences();
            if (materialLayeredSurface != null)
            {
                SetMaterial(meshFilterWingSurface, materialLayeredSurface);
                SetMaterial(meshFilterWingSection, materialLayeredEdge);
                for (int i = 0; i < meshTypeCountEdgeWing; ++i)
                {
                    SetMaterial(meshFiltersWingEdgeTrailing[i], materialLayeredEdge);
                }
            }
        }

        // todo: this is all duplicate except the set textures call (which may be also a duplicate through *ctrlSurface vs *WingSurface
        public override void SetMaterialReferences()
        {
            if (materialLayeredSurface == null)
                materialLayeredSurface = new Material(StaticWingGlobals.B9WingShader);
            if (materialLayeredEdge == null)
                materialLayeredEdge = new Material(StaticWingGlobals.B9WingShader);

            SetTextures(meshFilterWingSurface, meshFilterWingSection);

            if (materialLayeredSurfaceTextureMain != null && materialLayeredSurfaceTextureMask != null)
            {
                materialLayeredSurface.SetTexture("_MainTex", materialLayeredSurfaceTextureMain);
                materialLayeredSurface.SetTexture("_Emissive", materialLayeredSurfaceTextureMask);
                materialLayeredSurface.SetFloat("_Shininess", materialPropertyShininess);
                materialLayeredSurface.SetColor("_SpecColor", materialPropertySpecular);
            }

            if (materialLayeredEdgeTextureMain != null && materialLayeredEdgeTextureMask != null)
            {
                materialLayeredEdge.SetTexture("_MainTex", materialLayeredEdgeTextureMain);
                materialLayeredEdge.SetTexture("_Emissive", materialLayeredEdgeTextureMask);
                materialLayeredEdge.SetFloat("_Shininess", materialPropertyShininess);
                materialLayeredEdge.SetColor("_SpecColor", materialPropertySpecular);
            }
        }

        #endregion

        #region fuel
        public override bool CanBeFueled
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region Aero

        // Aerodynamics value calculation
        // More or less lifted from pWings, so credit goes to DYJ and Taverius
        public override void CalculateAerodynamicValues()
        {
            double sharedWidthTipSum = TipWidth + TipTrailingEdge;
            double sharedWidthRootSum = RootWidth + RootTrailingEdge;

            double ctrlOffsetRootLimit = (Length / 2f) / (RootWidth + RootTrailingEdge);
            double ctrlOffsetTipLimit = (Length / 2f) / (TipWidth + TipTrailingEdge);

            double ctrlOffsetRootClamped = Utils.Clamp(RootOffset, -ctrlOffsetRootLimit, ctrlOffsetRootLimit);
            double ctrlOffsetTipClamped = Utils.Clamp(TipOffset, -ctrlOffsetTipLimit, ctrlOffsetTipLimit);

            // Base four values
            double taperRatio = (Length + sharedWidthTipSum * ctrlOffsetTipClamped - sharedWidthRootSum * ctrlOffsetRootClamped) / Length;
            double MAC = (double)(sharedWidthTipSum + sharedWidthRootSum) / 2.0;
            double midChordSweep = Math.Atan((double)Math.Abs(sharedWidthRootSum - sharedWidthTipSum) / Length) * Utils.Rad2Deg;

            // Derived values

            double surfaceArea = MAC * Length;
            double aspectRatio = 2.0 * Length / MAC;

            ArSweepScale = Math.Pow(aspectRatio / Math.Cos(Utils.Deg2Rad * midChordSweep), 2.0f) + 4.0f;
            ArSweepScale = 2.0f + Math.Sqrt(ArSweepScale);
            ArSweepScale = (2.0f * Math.PI) / ArSweepScale * aspectRatio;

            wingMass = Utils.Clamp(massFudgeNumber * surfaceArea * ((ArSweepScale * 2.0) / (3.0 + ArSweepScale)) * ((1.0 + taperRatio) / 2), 0.01, double.MaxValue);
            Cd = dragBaseValue / ArSweepScale * dragMultiplier;
            Cl = liftFudgeNumber * surfaceArea * ArSweepScale;
            GatherChildrenCl();
            connectionForce = Math.Round(Utils.Clamp(Math.Sqrt(Cl + ChildrenCl) * (double)connectionFactor, (double)connectionMinimum, double.MaxValue));


            // Shared parameters
            updateCost();
            part.CoMOffset = new Vector3(0f, -(float)(sharedWidthRootSum + sharedWidthTipSum) / 4f, 0f);

            part.breakingForce = Mathf.Round((float)connectionForce);
            part.breakingTorque = Mathf.Round((float)connectionForce);

            // Stock-only values
            if (!StaticWingGlobals.FARactive)
            {
                float stockLiftCoefficient = (float)surfaceArea / 3.52f;
                ModuleControlSurface mCtrlSrf = part.Modules.GetModule<ModuleControlSurface>();
                mCtrlSrf.deflectionLiftCoeff = (float)Math.Round(stockLiftCoefficient, 2);
                mCtrlSrf.ctrlSurfaceArea = ctrlFraction;
                part.mass = stockLiftCoefficient * (1 + mCtrlSrf.ctrlSurfaceArea) * 0.1f;
            }
            else
                setFARModuleParams(midChordSweep, taperRatio, midChordOffsetFromOrigin);

            StartCoroutine(updateAeroDelayed());
        }

        public override void setFARModuleParams(double midChordSweep, double taperRatio, Vector3 midChordOffset)
        {
            base.setFARModuleParams(midChordSweep, taperRatio, midChordOffset);
            if (aeroFARFieldInfoControlSurfaceFraction != null)
            {
                aeroFARFieldInfoControlSurfaceFraction.SetValue(aeroFARModuleReference, ctrlFraction);
                aeroFARMethodInfoUsed.Invoke(aeroFARModuleReference, null);
            }
        }


        #endregion

        #region stock interfacing
        public override float updateCost()
        {
            return (float)Math.Round(wingMass * (1f + ArSweepScale / 4f) * (Base_ProceduralWing.costDensity * (1f - ctrlFraction) + costDensity * ctrlFraction), 1);
        }

        #endregion

        #region UI stuff
        public override string WindowTitle
        {
            get
            {
                return "Control surface";
            }
        }

        public override void ShowEditorUI()
        {
            WindowManager.GetWindow(this);
            WindowManager.Window.wing = this;

            WindowManager.Window.FindPropertyGroup("Base").UpdatePropertyValues(length, rootWidth, tipWidth, tipOffset, rootThickness, tipThickness, rootOffset);
            WindowManager.Window.FindPropertyGroup("Edge (trailing)").UpdatePropertyValues(trailingEdgeType, rootTrailingEdge, tipTrailingEdge);
            WindowManager.Window.FindPropertyGroup("Surface (top)").UpdatePropertyValues(surfTopMat, surfTopOpacity, surfTopHue, surfTopSat, surfTopBright);
            WindowManager.Window.FindPropertyGroup("Surface (bottom)").UpdatePropertyValues(surfBottomMat, surfBottomOpacity, surfBottomHue, surfBottomSat, surfBottomBright);
            WindowManager.Window.FindPropertyGroup("Surface (trailing edge)").UpdatePropertyValues(surfTrailMat, surfTrailOpacity, surfTrailHue, surfTrailSat, surfTrailBright);

            WindowManager.Window.Visible = true;
        }

        public override UI.EditorWindow CreateWindow()
        {
            UI.EditorWindow window = new EditorWindow();
            window.WindowTitle = WindowTitle;
            window.wing = this;
            
            PropertyGroup basegroup = window.AddPropertyGroup("Base", UIUtility.ColorHSBToRGB(uiColorSliderBase));
            basegroup.AddProperty(new WingProperty(length), x => window.wing.Length = x);
            basegroup.AddProperty(new WingProperty(rootWidth), x => window.wing.RootWidth = x);
            basegroup.AddProperty(new WingProperty(tipWidth), x => window.wing.TipWidth = x);
            basegroup.AddProperty(new WingProperty(rootOffset), x => ((B9_ProceduralControl)window.wing).RootOffset = x);
            basegroup.AddProperty(new WingProperty(tipOffset), x => window.wing.TipOffset = x);
            basegroup.AddProperty(new WingProperty(rootThickness), x => window.wing.RootThickness = x);
            basegroup.AddProperty(new WingProperty(tipThickness), x => window.wing.TipThickness = x);

            UI.PropertyGroup trailGroup = window.AddPropertyGroup("Edge (trailing)", UIUtility.ColorHSBToRGB(uiColorSliderEdgeT));
            trailGroup.AddProperty(new WingProperty(trailingEdgeType), x => ((B9_ProceduralWing)window.wing).TrailingEdgeType = (int)x,
                                        new string[] { "Rounded", "Biconvex", "Triangular" });
            trailGroup.AddProperty(new WingProperty(rootTrailingEdge), x => ((B9_ProceduralWing)window.wing).RootTrailingEdge = x);
            trailGroup.AddProperty(new WingProperty(tipTrailingEdge), x => ((B9_ProceduralWing)window.wing).TipTrailingEdge = x);

            UI.PropertyGroup surfTGroup = window.AddPropertyGroup("Surface (top)", UIUtility.ColorHSBToRGB(uiColorSliderColorsST));
            surfTGroup.AddProperty(new WingProperty(surfTopMat), x => ((B9_ProceduralWing)window.wing).SurfTopMat = (int)x,
                                        new string[] { "Uniform", "Standard", "Reinforced", "LRSI", "HRSI" });
            surfTGroup.AddProperty(new WingProperty(surfTopOpacity), x => ((B9_ProceduralWing)window.wing).SurfTopOpacity = x);
            surfTGroup.AddProperty(new WingProperty(surfTopHue), x => ((B9_ProceduralWing)window.wing).SurfTopHue = x);
            surfTGroup.AddProperty(new WingProperty(surfTopSat), x => ((B9_ProceduralWing)window.wing).SurfTopSat = x);
            surfTGroup.AddProperty(new WingProperty(surfTopBright), x => ((B9_ProceduralWing)window.wing).SurfTopBright = x);

            UI.PropertyGroup surfBGroup = window.AddPropertyGroup("Surface (bottom)", UIUtility.ColorHSBToRGB(uiColorSliderColorsSB));
            surfBGroup.AddProperty(new WingProperty(surfBottomMat), x => ((B9_ProceduralWing)window.wing).SurfBottomMat = (int)x,
                                        new string[] { "Uniform", "Standard", "Reinforced", "LRSI", "HRSI" });
            surfBGroup.AddProperty(new WingProperty(surfBottomOpacity), x => ((B9_ProceduralWing)window.wing).SurfBottomOpacity = x);
            surfBGroup.AddProperty(new WingProperty(surfBottomHue), x => ((B9_ProceduralWing)window.wing).SurfBottomHue = x);
            surfBGroup.AddProperty(new WingProperty(surfBottomSat), x => ((B9_ProceduralWing)window.wing).SurfBottomSat = x);
            surfBGroup.AddProperty(new WingProperty(surfBottomBright), x => ((B9_ProceduralWing)window.wing).SurfBottomBright = x);

            UI.PropertyGroup surfRGroup = window.AddPropertyGroup("Surface (trailing edge)", UIUtility.ColorHSBToRGB(uiColorSliderColorsET));
            surfRGroup.AddProperty(new WingProperty(surfTrailMat), x => ((B9_ProceduralWing)window.wing).SurfTrailMat = (int)x,
                                        new string[] { "Uniform", "Standard", "Reinforced", "LRSI", "HRSI" });
            surfRGroup.AddProperty(new WingProperty(surfTrailOpacity), x => ((B9_ProceduralWing)window.wing).SurfTrailOpacity = x);
            surfRGroup.AddProperty(new WingProperty(surfTrailHue), x => ((B9_ProceduralWing)window.wing).SurfTrailHue = x);
            surfRGroup.AddProperty(new WingProperty(surfTrailSat), x => ((B9_ProceduralWing)window.wing).SurfTrailSat = x);
            surfRGroup.AddProperty(new WingProperty(surfTrailBright), x => ((B9_ProceduralWing)window.wing).SurfTrailBright = x);

            return window;
        }

        #endregion
    }
}
