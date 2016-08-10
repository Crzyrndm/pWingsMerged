using System;
using System.Linq;
using UnityEngine;

namespace ProceduralWings.B9
{
    using System.Collections.Generic;
    using Utility;
    class B9_ProceduralControl : B9_ProceduralWing
    {
        protected WingProperty rootOffset;
        public double RootOffset
        {
            get { return rootOffset.value; }
            set
            {
                rootOffset.value = value;
                UpdateSymmetricGeometry();
            }

        }
        public MeshFilter meshFilterCtrlFrame;
        public MeshFilter meshFilterCtrlSurface;
        public List<MeshFilter> meshFiltersCtrlEdge = new List<MeshFilter>();

        public static MeshReference meshReferenceCtrlFrame;
        public static MeshReference meshReferenceCtrlSurface;
        public static List<MeshReference> meshReferencesCtrlEdge = new List<MeshReference>();

        public static int meshTypeCountEdgeCtrl = 3;

        public static string[] sharedFieldGroupBaseArrayCtrl = new string[] { "sharedBaseOffsetRoot" };

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

        public override void SetupProperties()
        {
            base.SetupProperties();
            rootOffset = new WingProperty("Offset (root)", nameof(rootOffset), 0, 2, -1, 1);
        }

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


        #region Geometry
        public override void UpdateGeometry(bool updateAerodynamics)
        {
            float ctrlOffsetRootClamped = (float)Utils.Clamp(RootOffset, rootOffset.min, rootOffset.max); // Mathf.Clamp (sharedBaseOffsetRoot, sharedBaseOffsetLimits.z, ctrlOffsetRootLimit - 0.075f);
            float ctrlOffsetTipClamped = (float)Utils.Clamp(TipOffset, Math.Max(rootOffset.min, ctrlOffsetRootClamped - Length), rootOffset.max); // Mathf.Clamp (sharedBaseOffsetTip, -ctrlOffsetTipLimit + 0.075f, sharedBaseOffsetLimits.w);

            float ctrlThicknessDeviationRoot = (float)RootThickness / 0.24f;
            float ctrlThicknessDeviationTip = (float)TipThickness / 0.24f;

            float ctrlEdgeWidthDeviationRoot = (float)RootTrailingEdge / 0.24f;
            float ctrlEdgeWidthDeviationTip = (float)TipTrailingEdge / 0.24f;

            if (meshFilterCtrlFrame != null)
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

                meshFilterCtrlFrame.mesh.vertices = vp;
                meshFilterCtrlFrame.mesh.uv = uv;
                meshFilterCtrlFrame.mesh.uv2 = uv2;
                meshFilterCtrlFrame.mesh.colors = cl;
                meshFilterCtrlFrame.mesh.RecalculateBounds();

                MeshCollider meshCollider = meshFilterCtrlFrame.gameObject.GetComponent<MeshCollider>();
                if (meshCollider == null)
                    meshCollider = meshFilterCtrlFrame.gameObject.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = null;
                meshCollider.sharedMesh = meshFilterCtrlFrame.mesh;
                meshCollider.convex = true;
            }

            // Next, time for edge types
            // Before modifying geometry, we have to show the correct objects for the current selection
            // As UI only works with floats, we have to cast selections into ints too

            int ctrlEdgeTypeInt = Mathf.RoundToInt(TrailingEdgeType - 1);
            for (int i = 0; i < meshTypeCountEdgeCtrl; ++i)
            {
                if (i != ctrlEdgeTypeInt)
                    meshFiltersCtrlEdge[i].gameObject.SetActive(false);
                else
                    meshFiltersCtrlEdge[i].gameObject.SetActive(true);
            }

            // Now we can modify geometry
            // Copy-pasted frame deformation sequence at the moment, to be pruned later

            if (meshFiltersCtrlEdge[ctrlEdgeTypeInt] != null)
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
                        cl[i] = GetVertexColor(2);
                        uv2[i] = GetVertexUV2(TrailingEdgeType);
                    }
                }

                meshFiltersCtrlEdge[ctrlEdgeTypeInt].mesh.vertices = vp;
                meshFiltersCtrlEdge[ctrlEdgeTypeInt].mesh.uv = uv;
                meshFiltersCtrlEdge[ctrlEdgeTypeInt].mesh.uv2 = uv2;
                meshFiltersCtrlEdge[ctrlEdgeTypeInt].mesh.colors = cl;
                meshFiltersCtrlEdge[ctrlEdgeTypeInt].mesh.RecalculateBounds();
            }

            // Finally, simple top/bottom surface changes

            if (meshFilterCtrlSurface != null)
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
                        cl[i] = GetVertexColor(0);
                        uv2[i] = GetVertexUV2(SurfTopMat);
                    }
                    else
                    {
                        cl[i] = GetVertexColor(1);
                        uv2[i] = GetVertexUV2(SurfBottomMat);
                    }
                }
                meshFilterCtrlSurface.mesh.vertices = vp;
                meshFilterCtrlSurface.mesh.uv = uv;
                meshFilterCtrlSurface.mesh.uv2 = uv2;
                meshFilterCtrlSurface.mesh.colors = cl;
                meshFilterCtrlSurface.mesh.RecalculateBounds();
            }
            if (updateAerodynamics)
                CalculateAerodynamicValues();
        }

        #endregion

        #region Mesh Setup and Checking
        public override void SetupMeshFilters()
        {
            meshFilterCtrlFrame = CheckMeshFilter(meshFilterCtrlFrame, "frame");
            meshFilterCtrlSurface = CheckMeshFilter(meshFilterCtrlSurface, "surface");
            for (int i = 0; i < meshTypeCountEdgeCtrl; ++i)
            {
                MeshFilter meshFilterCtrlEdge = CheckMeshFilter("edge_type" + i);
                meshFiltersCtrlEdge.Add(meshFilterCtrlEdge);
            }
        }

        public override void SetupMeshReferences()
        {
            if (meshReferenceCtrlFrame == null || meshReferenceCtrlFrame.vp.Length == 0
                || meshReferenceCtrlSurface != null || meshReferenceCtrlSurface.vp.Length > 0
                || meshReferencesCtrlEdge[meshTypeCountEdgeCtrl - 1] != null || meshReferencesCtrlEdge[meshTypeCountEdgeCtrl - 1].vp.Length > 0)
            {
                meshReferenceCtrlFrame = FillMeshRefererence(meshFilterCtrlFrame);
                meshReferenceCtrlSurface = FillMeshRefererence(meshFilterCtrlSurface);
                for (int i = 0; i < meshTypeCountEdgeCtrl; ++i)
                {
                    MeshReference meshReferenceCtrlEdge = FillMeshRefererence(meshFiltersCtrlEdge[i]);
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
                SetMaterial(meshFilterCtrlSurface, materialLayeredSurface);
                SetMaterial(meshFilterCtrlFrame, materialLayeredEdge);
                for (int i = 0; i < meshTypeCountEdgeCtrl; ++i)
                {
                    SetMaterial(meshFiltersCtrlEdge[i], materialLayeredEdge);
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

            SetTextures(meshFilterCtrlSurface, meshFilterCtrlFrame);

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

        #region Aero

        // Aerodynamics value calculation
        // More or less lifted from pWings, so credit goes to DYJ and Taverius
        public override void CalculateAerodynamicValues()
        {
            CheckAssemblies();

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
            if (!FARactive)
            {
                float stockLiftCoefficient = (float)surfaceArea / 3.52f;
                ModuleControlSurface mCtrlSrf = part.Modules.OfType<ModuleControlSurface>().FirstOrDefault();
                mCtrlSrf.deflectionLiftCoeff = (float)Math.Round(stockLiftCoefficient, 2);
                mCtrlSrf.ctrlSurfaceArea = ctrlFraction;
                part.mass = stockLiftCoefficient * (1 + mCtrlSrf.ctrlSurfaceArea) * 0.1f;
            }
            else
            {
                if (aeroFARModuleReference == null)
                {
                    if (part.Modules.Contains(FarModuleName))
                        aeroFARModuleReference = part.Modules[FarModuleName];
                    else if (part.Modules.Contains("FARWingAerodynamicModel"))
                        aeroFARModuleReference = part.Modules["FARWingAerodynamicModel"];
                }
                if (aeroFARModuleReference != null)
                {
                    if (aeroFARModuleType == null)
                        aeroFARModuleType = aeroFARModuleReference.GetType();
                    if (aeroFARModuleType != null)
                    {
                        if (aeroFARFieldInfoSemispan == null)
                            aeroFARFieldInfoSemispan = aeroFARModuleType.GetField("b_2");
                        if (aeroFARFieldInfoSemispan_Actual == null)
                            aeroFARFieldInfoSemispan_Actual = aeroFARModuleType.GetField("b_2_actual");
                        if (aeroFARFieldInfoMAC == null)
                            aeroFARFieldInfoMAC = aeroFARModuleType.GetField("MAC");
                        if (aeroFARFieldInfoMAC_Actual == null)
                            aeroFARFieldInfoMAC_Actual = aeroFARModuleType.GetField("MAC_actual");
                        if (aeroFARFieldInfoMidChordSweep == null)
                            aeroFARFieldInfoMidChordSweep = aeroFARModuleType.GetField("MidChordSweep");
                        if (aeroFARFieldInfoTaperRatio == null)
                            aeroFARFieldInfoTaperRatio = aeroFARModuleType.GetField("TaperRatio");
                        if (aeroFARFieldInfoControlSurfaceFraction == null)
                            aeroFARFieldInfoControlSurfaceFraction = aeroFARModuleType.GetField("ctrlSurfFrac");
                        if (aeroFARFieldInfoRootChordOffset == null)
                            aeroFARFieldInfoRootChordOffset = aeroFARModuleType.GetField("rootMidChordOffsetFromOrig");

                        if (aeroFARMethodInfoUsed == null)
                        {
                            aeroFARMethodInfoUsed = aeroFARModuleType.GetMethod("StartInitialization");
                        }
                        if (aeroFARMethodInfoUsed != null)
                        {
                            aeroFARFieldInfoSemispan.SetValue(aeroFARModuleReference, length);
                            aeroFARFieldInfoSemispan_Actual.SetValue(aeroFARModuleReference, length);
                            aeroFARFieldInfoMAC.SetValue(aeroFARModuleReference, MAC);
                            aeroFARFieldInfoMAC_Actual.SetValue(aeroFARModuleReference, MAC);
                            aeroFARFieldInfoMidChordSweep.SetValue(aeroFARModuleReference, midChordSweep);
                            aeroFARFieldInfoTaperRatio.SetValue(aeroFARModuleReference, taperRatio);
                            aeroFARFieldInfoControlSurfaceFraction.SetValue(aeroFARModuleReference, ctrlFraction);
                            aeroFARFieldInfoRootChordOffset.SetValue(aeroFARModuleReference, (Vector3)aeroStatRootMidChordOffsetFromOrigin);

                            aeroFARMethodInfoUsed.Invoke(aeroFARModuleReference, null);
                        }
                    }
                }
            }

            StartCoroutine(updateAeroDelayed());
        }


        #endregion

        #region Alternative UI/input


        public override string WindowTitle
        {
            get
            {
                if (ctrlFraction >= 1)
                    return "All-moving control surface";
                else
                    return "Control surface";
            }
        }

        public override void ShowEditorUI()
        {
            base.ShowEditorUI();

            window.FindPropertyGroup("Base").UpdatePropertyValues(rootOffset);
            window.FindPropertyGroup("Edge (leading)").UpdatePropertyValues(leadingEdgeType, rootLeadingEdge, tipLeadingEdge);
            window.FindPropertyGroup("Edge (trailing)").UpdatePropertyValues(trailingEdgeType, rootTrailingEdge, tipTrailingEdge);
            window.FindPropertyGroup("Surface (top)").UpdatePropertyValues(surfTopMat, surfTopOpacity, surfTopHue, surfTopSat, surfTopBright);
            window.FindPropertyGroup("Surface (bottom)").UpdatePropertyValues(surfBottomMat, surfBottomOpacity, surfBottomHue, surfBottomSat, surfBottomBright);
            window.FindPropertyGroup("Surface (leading edge)").UpdatePropertyValues(surfLeadMat, surfLeadOpacity, surfLeadHue, surfLeadSat, surfLeadBright);
            window.FindPropertyGroup("Surface (trailing edge)").UpdatePropertyValues(surfTrailMat, surfTrailOpacity, surfTrailHue, surfTrailSat, surfTrailBright);
        }

        public override void SetupWindowGroups()
        {
            base.SetupWindowGroups();
            if (window.FindPropertyGroup("Base")?.AddProperty(rootOffset, x => ((B9_ProceduralControl)window.wing).RootOffset = x) != null)
            { }

        }

        #endregion
    }
}
