using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralWings.B9PWing
{
    using UI;
    using Utility;

    internal class B9_ProceduralControl : B9_ProceduralWing
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
            get { return rootOffset.Value * Scale; }
            set
            {
                rootOffset.Value = value / Scale;
                StartCoroutine(UpdateSymmetricGeometry());
            }
        }

        #endregion physical dimensions

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

        #endregion entry points

        #region setting up

        public override void SetupProperties()
        {
            base.SetupProperties();
            if (rootOffset != null)
                return;
            if (part.symmetryCounterparts.Count == 0 || part.symmetryCounterparts[0].Modules.GetModule<B9_ProceduralControl>().rootOffset == null)
            {
                rootOffset = new WingProperty("Offset (root)", nameof(rootOffset), 0, 2, -2, 2, "Offset of the trailing edge \nroot corner on the lateral axis");
                tipOffset.min = -2;
                tipOffset.max = 2;
                length.Value = 1;
                tipWidth.Value = 0.5;
                rootWidth.Value = 0.5;
            }
            else
            {
                B9_ProceduralControl wp = part.symmetryCounterparts[0].Modules.GetModule<B9_ProceduralControl>();  // all properties for symmetry will be the same object. Yay for no need to update values :D
                rootOffset = wp.rootOffset;
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

        #endregion setting up

        #region Geometry

        public override void UpdateGeometry()
        {
            UpdateGeometry(true);
        }

        public override void UpdateGeometry(bool updateAerodynamics)
        {
            float ctrlOffsetRootClamped = (float)Utils.Clamp(RootOffset, rootOffset.min, rootOffset.max);
            float ctrlOffsetTipClamped = (float)Utils.Clamp(TipOffset, Math.Max(tipOffset.min, ctrlOffsetRootClamped - Length), tipOffset.max);

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
                    else if (nm[i] == new Vector3(0f, 1f, 0f)) // Root (only needs UV adjustment)
                    {
                        if (vp[i].z < 0f) uv[i] = new Vector2((float)Length, uv[i].y);
                    }
                    else // Trailing edge
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

        public override void SetStockModuleParams()
        {
            base.SetStockModuleParams();
            wingMass *= 1 + ctrlFraction; // 100% control surfaces are twice as heavy as normal wings
        }

        #endregion Geometry

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

        #endregion Mesh

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

        #endregion Materials

        #region fuel

        public override bool CanBeFueled
        {
            get
            {
                return false;
            }
        }

        #endregion fuel

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

        #endregion Aero

        #region stock interfacing

        public override float updateCost()
        {
            return (float)Math.Round(wingMass * (1f + ArSweepScale / 4f) * (Base_ProceduralWing.costDensity * (1f - ctrlFraction) + costDensity * ctrlFraction), 1);
        }

        #endregion stock interfacing

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

            PropertyGroup basegroup = window.AddPropertyGroup("Base", uiColorSliderBase);
            basegroup.AddProperty(new WingProperty(scale), x => window.wing.Scale = x);
            basegroup.AddProperty(new WingProperty(length), x => window.wing.Length = x, true);
            basegroup.AddProperty(new WingProperty(rootWidth), x => window.wing.RootWidth = x, true);
            basegroup.AddProperty(new WingProperty(tipWidth), x => window.wing.TipWidth = x, true);
            basegroup.AddProperty(new WingProperty(rootOffset), x => ((B9_ProceduralControl)window.wing).RootOffset = x, true);
            basegroup.AddProperty(new WingProperty(tipOffset), x => window.wing.TipOffset = x, true);
            basegroup.AddProperty(new WingProperty(rootThickness), x => window.wing.RootThickness = x, true);
            basegroup.AddProperty(new WingProperty(tipThickness), x => window.wing.TipThickness = x, true);

            UI.PropertyGroup trailGroup = window.AddPropertyGroup("Edge (trailing)", uiColorSliderEdgeT);
            trailGroup.AddProperty(new WingProperty(trailingEdgeType), x => ((B9_ProceduralWing)window.wing).TrailingEdgeType = (int)x,
                                        new string[] { "Rounded", "Biconvex", "Triangular" });
            trailGroup.AddProperty(new WingProperty(rootTrailingEdge), x => ((B9_ProceduralWing)window.wing).RootTrailingEdge = x, true);
            trailGroup.AddProperty(new WingProperty(tipTrailingEdge), x => ((B9_ProceduralWing)window.wing).TipTrailingEdge = x, true);

            UI.PropertyGroup surfTGroup = window.AddPropertyGroup("Surface (top)", uiColorSliderColorsST);
            surfTGroup.AddProperty(new WingProperty(surfTopMat), x => ((B9_ProceduralWing)window.wing).SurfTopMat = (int)x,
                                        new string[] { "Uniform", "Standard", "Reinforced", "LRSI", "HRSI" });
            surfTGroup.AddProperty(new WingProperty(surfTopOpacity), x => ((B9_ProceduralWing)window.wing).SurfTopOpacity = x);
            surfTGroup.AddProperty(new WingProperty(surfTopHue), x => ((B9_ProceduralWing)window.wing).SurfTopHue = x);
            surfTGroup.AddProperty(new WingProperty(surfTopSat), x => ((B9_ProceduralWing)window.wing).SurfTopSat = x);
            surfTGroup.AddProperty(new WingProperty(surfTopBright), x => ((B9_ProceduralWing)window.wing).SurfTopBright = x);

            UI.PropertyGroup surfBGroup = window.AddPropertyGroup("Surface (bottom)", uiColorSliderColorsSB);
            surfBGroup.AddProperty(new WingProperty(surfBottomMat), x => ((B9_ProceduralWing)window.wing).SurfBottomMat = (int)x,
                                        new string[] { "Uniform", "Standard", "Reinforced", "LRSI", "HRSI" });
            surfBGroup.AddProperty(new WingProperty(surfBottomOpacity), x => ((B9_ProceduralWing)window.wing).SurfBottomOpacity = x);
            surfBGroup.AddProperty(new WingProperty(surfBottomHue), x => ((B9_ProceduralWing)window.wing).SurfBottomHue = x);
            surfBGroup.AddProperty(new WingProperty(surfBottomSat), x => ((B9_ProceduralWing)window.wing).SurfBottomSat = x);
            surfBGroup.AddProperty(new WingProperty(surfBottomBright), x => ((B9_ProceduralWing)window.wing).SurfBottomBright = x);

            UI.PropertyGroup surfRGroup = window.AddPropertyGroup("Surface (trailing edge)", uiColorSliderColorsET);
            surfRGroup.AddProperty(new WingProperty(surfTrailMat), x => ((B9_ProceduralWing)window.wing).SurfTrailMat = (int)x,
                                        new string[] { "Uniform", "Standard", "Reinforced", "LRSI", "HRSI" });
            surfRGroup.AddProperty(new WingProperty(surfTrailOpacity), x => ((B9_ProceduralWing)window.wing).SurfTrailOpacity = x);
            surfRGroup.AddProperty(new WingProperty(surfTrailHue), x => ((B9_ProceduralWing)window.wing).SurfTrailHue = x);
            surfRGroup.AddProperty(new WingProperty(surfTrailSat), x => ((B9_ProceduralWing)window.wing).SurfTrailSat = x);
            surfRGroup.AddProperty(new WingProperty(surfTrailBright), x => ((B9_ProceduralWing)window.wing).SurfTrailBright = x);

            return window;
        }

        #endregion UI stuff
    }
}