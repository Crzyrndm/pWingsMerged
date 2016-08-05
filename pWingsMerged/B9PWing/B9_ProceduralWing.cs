using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ProceduralWings.B9
{
    using Utility;
    public class B9_ProceduralWing : Base_ProceduralWing
    {
        #region Unity stuff and Callbacks/events

        public override void Update()
        {
            base.Update();
        }
        // Attachment handling
        public void UpdateOnEditorAttach()
        {
            UpdateGeometry(true);
        }

        public void UpdateOnEditorDetach()
        {
            if (this.part.parent != null)
            {
                B9_ProceduralWing parentModule = this.part.parent.Modules.OfType<B9_ProceduralWing>().FirstOrDefault();
                if (parentModule != null)
                {
                    parentModule.FuelUpdateVolume();
                    parentModule.CalculateAerodynamicValues();
                }
            }
        }


        public override void SetupGeometryAndAppearance()
        {
            SetupMeshFilters();
            SetupMeshReferences();
            UpdateMaterials();
        }
        #endregion

        #region Mesh properties

        [System.Serializable]
        public class MeshReference
        {
            public Vector3[] vp;
            public Vector3[] nm;
            public Vector2[] uv;
        }

        public MeshFilter meshFilterWingSection;
        public MeshFilter meshFilterWingSurface;
        public List<MeshFilter> meshFiltersWingEdgeTrailing = new List<MeshFilter>();
        public List<MeshFilter> meshFiltersWingEdgeLeading = new List<MeshFilter>();

        public MeshFilter meshFilterCtrlFrame;
        public MeshFilter meshFilterCtrlSurface;
        public List<MeshFilter> meshFiltersCtrlEdge = new List<MeshFilter>();

        public static MeshReference meshReferenceWingSection;
        public static MeshReference meshReferenceWingSurface;
        public static List<MeshReference> meshReferencesWingEdge = new List<MeshReference>();

        public static MeshReference meshReferenceCtrlFrame;
        public static MeshReference meshReferenceCtrlSurface;
        public static List<MeshReference> meshReferencesCtrlEdge = new List<MeshReference>();

        public static int meshTypeCountEdgeWing = 4;
        public static int meshTypeCountEdgeCtrl = 3;
        #endregion

        #region Shared properties / Limits and increments
        public virtual Vector2d GetLimitsFromType(Vector4d set)
        {
            return new Vector2d(set.x, set.y);
        }

        public virtual float GetIncrementFromType(float incrementWing, float incrementCtrl)
        {
            return incrementWing;
        }

        public static Vector4d sharedBaseLengthLimits = new Vector4d(0.125, 16, 0.04, 8);
        public static Vector2d sharedBaseThicknessLimits = new Vector2d(0.04, 1);
        public static Vector4d sharedBaseWidthRootLimits = new Vector4d(0.125, 16, 0.04, 1.6);
        public static Vector4d sharedBaseWidthTipLimits = new Vector4d(0.0001, 16, 0.04, 1.6);
        public static Vector4d sharedBaseOffsetLimits = new Vector4d(-8, 8, -2, 2);
        public static Vector4d sharedEdgeTypeLimits = new Vector4d(1, 4, 1, 3);
        public static Vector4d sharedEdgeWidthLimits = new Vector4d(0, 1, 0, 1);
        public static Vector2d sharedMaterialLimits = new Vector2d(0, 4);
        public static Vector2d sharedColorLimits = new Vector2d(0, 1);

        public static float sharedIncrementColor = 0.01f;
        public static float sharedIncrementColorLarge = 0.10f;
        public static float sharedIncrementMain = 0.125f;
        public static float sharedIncrementSmall = 0.04f;
        public static float sharedIncrementInt = 1f;
        #endregion

        #region Shared properties / Base

        public static string[] sharedFieldGroupBaseArray = new string[] { "sharedBaseLength", "sharedBaseWidthRoot", "sharedBaseWidthTip", "sharedBaseThicknessRoot", "sharedBaseThicknessTip", "sharedBaseOffsetTip" };
        public static string[] sharedFieldGroupBaseArrayCtrl = new string[] { "sharedBaseOffsetRoot" };

        public float sharedBaseLength = 4f;
        public static Vector4 sharedBaseLengthDefaults = new Vector4(4f, 1f, 4f, 1f);

        public float sharedBaseWidthRoot = 4f;
        public static Vector4 sharedBaseWidthRootDefaults = new Vector4(4f, 0.5f, 4f, 0.5f);

        public float sharedBaseWidthTip = 4f;
        public static Vector4 sharedBaseWidthTipDefaults = new Vector4(4f, 0.5f, 4f, 0.5f);

        public float sharedBaseOffsetRoot = 0f;
        public static Vector4 sharedBaseOffsetRootDefaults = new Vector4(0f, 0f, 0f, 0f);

        public float sharedBaseOffsetTip = 0f;
        public static Vector4 sharedBaseOffsetTipDefaults = new Vector4(0f, 0f, 0f, 0f);

        public float sharedBaseThicknessRoot = 0.24f;
        public static Vector4 sharedBaseThicknessRootDefaults = new Vector4(0.24f, 0.24f, 0.24f, 0.24f);

        public float sharedBaseThicknessTip = 0.24f;
        public static Vector4 sharedBaseThicknessTipDefaults = new Vector4(0.24f, 0.24f, 0.24f, 0.24f);

        #endregion

        #region Shared properties / Edge / Leading

        public static string[] sharedFieldGroupEdgeLeadingArray = new string[] { "sharedEdgeTypeLeading", "sharedEdgeWidthLeadingRoot", "sharedEdgeWidthLeadingTip" };

        public float sharedEdgeTypeLeading = 2f;
        public static Vector4 sharedEdgeTypeLeadingDefaults = new Vector4(2f, 1f, 2f, 1f);

        public float sharedEdgeWidthLeadingRoot = 0.24f;
        public static Vector4 sharedEdgeWidthLeadingRootDefaults = new Vector4(0.24f, 0.24f, 0.24f, 0.24f);

        public float sharedEdgeWidthLeadingTip = 0.24f;
        public static Vector4 sharedEdgeWidthLeadingTipDefaults = new Vector4(0.24f, 0.24f, 0.24f, 0.24f);

        #endregion

        #region Shared properties / Edge / Trailing

        public static string[] sharedFieldGroupEdgeTrailingArray = new string[] { "sharedEdgeTypeTrailing", "sharedEdgeWidthTrailingRoot", "sharedEdgeWidthTrailingTip" };

        public float sharedEdgeTypeTrailing = 3f;
        public static Vector4 sharedEdgeTypeTrailingDefaults = new Vector4(3f, 2f, 3f, 2f);

        public float sharedEdgeWidthTrailingRoot = 0.48f;
        public static Vector4 sharedEdgeWidthTrailingRootDefaults = new Vector4(0.48f, 0.48f, 0.48f, 0.48f);

        public float sharedEdgeWidthTrailingTip = 0.48f;
        public static Vector4 sharedEdgeWidthTrailingTipDefaults = new Vector4(0.48f, 0.48f, 0.48f, 0.48f);

        #endregion

        #region Shared properties / Surface / Top
        public static string[] sharedFieldGroupColorSTArray = new string[] { "sharedMaterialST", "sharedColorSTOpacity", "sharedColorSTHue", "sharedColorSTSaturation", "sharedColorSTBrightness" };

        public float sharedMaterialST = 1f;
        public static Vector4 sharedMaterialSTDefaults = new Vector4(1f, 1f, 1f, 1f);

        public float sharedColorSTOpacity = 0f;
        public static Vector4 sharedColorSTOpacityDefaults = new Vector4(0f, 0f, 0f, 0f);

        public float sharedColorSTHue = 0.10f;
        public static Vector4 sharedColorSTHueDefaults = new Vector4(0.1f, 0.1f, 0.1f, 0.1f);

        public float sharedColorSTSaturation = 0.75f;
        public static Vector4 sharedColorSTSaturationDefaults = new Vector4(0.75f, 0.75f, 0.75f, 0.75f);

        public float sharedColorSTBrightness = 0.6f;
        public static Vector4 sharedColorSTBrightnessDefaults = new Vector4(0.6f, 0.6f, 0.6f, 0.6f);

        #endregion

        #region Shared properties / Surface / bottom

        public static string[] sharedFieldGroupColorSBArray = new string[] { "sharedMaterialSB", "sharedColorSBOpacity", "sharedColorSBHue", "sharedColorSBSaturation", "sharedColorSBBrightness" };

        public float sharedMaterialSB = 4f;
        public static Vector4 sharedMaterialSBDefaults = new Vector4(4f, 4f, 4f, 4f);

        public float sharedColorSBOpacity = 0f;
        public static Vector4 sharedColorSBOpacityDefaults = new Vector4(0f, 0f, 0f, 0f);

        public float sharedColorSBHue = 0.10f;
        public static Vector4 sharedColorSBHueDefaults = new Vector4(0.1f, 0.1f, 0.1f, 0.1f);

        public float sharedColorSBSaturation = 0.75f;
        public static Vector4 sharedColorSBSaturationDefaults = new Vector4(0.75f, 0.75f, 0.75f, 0.75f);

        public float sharedColorSBBrightness = 0.6f;
        public static Vector4 sharedColorSBBrightnessDefaults = new Vector4(0.6f, 0.6f, 0.6f, 0.6f);
        #endregion

        #region Shared properties / Surface / trailing edge

        public static string[] sharedFieldGroupColorETArray = new string[] { "sharedMaterialET", "sharedColorETOpacity", "sharedColorETHue", "sharedColorETSaturation", "sharedColorETBrightness" };

        public float sharedMaterialET = 4f;
        public static Vector4 sharedMaterialETDefaults = new Vector4(4f, 4f, 4f, 4f);

        public float sharedColorETOpacity = 0f;
        public static Vector4 sharedColorETOpacityDefaults = new Vector4(0f, 0f, 0f, 0f);

        public float sharedColorETHue = 0.10f;
        public static Vector4 sharedColorETHueDefaults = new Vector4(0.1f, 0.1f, 0.1f, 0.1f);

        public float sharedColorETSaturation = 0.75f;
        public static Vector4 sharedColorETSaturationDefaults = new Vector4(0.75f, 0.75f, 0.75f, 0.75f);

        public float sharedColorETBrightness = 0.6f;
        public static Vector4 sharedColorETBrightnessDefaults = new Vector4(0.6f, 0.6f, 0.6f, 0.6f);

        #endregion

        #region Shared properties / Surface / leading edge
        public static bool sharedFieldGroupColorELStatic = false;
        public static string[] sharedFieldGroupColorELArray = new string[] { "sharedMaterialEL", "sharedColorELOpacity", "sharedColorELHue", "sharedColorELSaturation", "sharedColorELBrightness" };

        public float sharedMaterialEL = 4f;
        public static Vector4 sharedMaterialELDefaults = new Vector4(4f, 4f, 4f, 4f);

        public float sharedColorELOpacity = 0f;
        public static Vector4 sharedColorELOpacityDefaults = new Vector4(0f, 0f, 0f, 0f);

        public float sharedColorELHue = 0.10f;
        public static Vector4 sharedColorELHueDefaults = new Vector4(0.1f, 0.1f, 0.1f, 0.1f);

        public float sharedColorELSaturation = 0.75f;
        public static Vector4 sharedColorELSaturationDefaults = new Vector4(0.75f, 0.75f, 0.75f, 0.75f);

        public float sharedColorELBrightness = 0.6f;
        public static Vector4 sharedColorELBrightnessDefaults = new Vector4(0.6f, 0.6f, 0.6f, 0.6f);

        #endregion

        #region Inheritance
        public bool inheritancePossibleOnShape = false;
        public bool inheritancePossibleOnMaterials = false;
        public virtual void InheritanceStatusUpdate()
        {
            if (this.part.parent == null)
                return;

            Base_ProceduralWing parentModule = part.parent.Modules.OfType<Base_ProceduralWing>().FirstOrDefault();
            if (parentModule != null)
            {
                inheritancePossibleOnMaterials = true;
                inheritancePossibleOnShape = true;
            }
        }

        public virtual void InheritParentValues(int mode)
        {
            if (this.part.parent == null)
                return;

            Base_ProceduralWing parentModule = part.parent.Modules.OfType<Base_ProceduralWing>().FirstOrDefault();
            if (parentModule == null)
                return;

            switch (mode)
            {
                case 0:
                    inheritShape(parentModule);
                    break;
                case 1:
                    inheritBase(parentModule);
                    break;
                case 2:
                    inheritEdges(parentModule);
                    break;
                case 3:
                    inheritColours(parentModule);
                    break;
            }
        }

        public override void inheritShape(Base_ProceduralWing parent)
        {
            base.inheritShape(parent);

            if (tipWidth < sharedBaseWidthTipLimits.x)
                length *= (rootWidth - sharedBaseWidthTipLimits.x) / (sharedBaseWidthRoot - sharedBaseWidthTip);
            else if (tipWidth > sharedBaseWidthTipLimits.y)
                length *= sharedBaseWidthTipLimits.y / tipWidth;

            if (tipOffset > sharedBaseOffsetLimits.y)
                length *= sharedBaseOffsetLimits.y / tipOffset;
            else if (tipOffset < sharedBaseOffsetLimits.x)
                length *= sharedBaseOffsetLimits.x / tipOffset;

            length = Utils.Clamp(length, sharedBaseLengthLimits.x, sharedBaseLengthLimits.y);
            tipWidth = Utils.Clamp(tipWidth, sharedBaseWidthTipLimits.x, sharedBaseWidthTipLimits.y);
            tipOffset = Utils.Clamp(tipOffset, sharedBaseOffsetLimits.x, sharedBaseOffsetLimits.y);
            tipThickness = Utils.Clamp(rootThickness + length / parent.length * (parent.tipThickness - parent.rootThickness), sharedBaseThicknessLimits.x, sharedBaseThicknessLimits.y);

            if (Input.GetMouseButtonUp(0))
                inheritEdges(parent);
        }

        public override void inheritBase(Base_ProceduralWing parent)
        {
            base.inheritBase(parent);

            B9_ProceduralWing wing = parent as B9_ProceduralWing;
            if (wing == null)
                return;
            sharedEdgeTypeLeading = wing.sharedEdgeTypeLeading;
            sharedEdgeWidthLeadingRoot = wing.sharedEdgeWidthLeadingTip;

            sharedEdgeTypeTrailing = wing.sharedEdgeTypeTrailing;
            sharedEdgeWidthTrailingRoot = wing.sharedEdgeWidthTrailingTip;
        }

        public virtual void inheritEdges(Base_ProceduralWing parent)
        {
            B9_ProceduralWing wing = parent as B9_ProceduralWing;
            if (wing == null)
                return;

            sharedEdgeTypeLeading = wing.sharedEdgeTypeLeading;
            sharedEdgeWidthLeadingRoot = wing.sharedEdgeWidthLeadingTip;
            sharedEdgeWidthLeadingTip = (float)Utils.Clamp(sharedEdgeWidthLeadingRoot + ((wing.sharedEdgeWidthLeadingTip - wing.sharedEdgeWidthLeadingRoot) / wing.sharedBaseLength) * sharedBaseLength, sharedEdgeWidthLimits.x, sharedEdgeWidthLimits.y);

            sharedEdgeTypeTrailing = wing.sharedEdgeTypeTrailing;
            sharedEdgeWidthTrailingRoot = wing.sharedEdgeWidthTrailingTip;
            sharedEdgeWidthTrailingTip = (float)Utils.Clamp(sharedEdgeWidthTrailingRoot + ((wing.sharedEdgeWidthTrailingTip - wing.sharedEdgeWidthTrailingRoot) / wing.sharedBaseLength) * sharedBaseLength, sharedEdgeWidthLimits.x, sharedEdgeWidthLimits.y);
        }

        public virtual void inheritColours(Base_ProceduralWing parent)
        {
            B9_ProceduralWing wing = parent as B9_ProceduralWing;
            if (wing == null)
                return;

            sharedMaterialST = wing.sharedMaterialST;
            sharedColorSTOpacity = wing.sharedColorSTOpacity;
            sharedColorSTHue = wing.sharedColorSTHue;
            sharedColorSTSaturation = wing.sharedColorSTSaturation;
            sharedColorSTBrightness = wing.sharedColorSTBrightness;

            sharedMaterialSB = wing.sharedMaterialSB;
            sharedColorSBOpacity = wing.sharedColorSBOpacity;
            sharedColorSBHue = wing.sharedColorSBHue;
            sharedColorSBSaturation = wing.sharedColorSBSaturation;
            sharedColorSBBrightness = wing.sharedColorSBBrightness;

            sharedMaterialET = wing.sharedMaterialET;
            sharedColorETOpacity = wing.sharedColorETOpacity;
            sharedColorETHue = wing.sharedColorETHue;
            sharedColorETSaturation = wing.sharedColorETSaturation;
            sharedColorETBrightness = wing.sharedColorETBrightness;

            sharedMaterialEL = wing.sharedMaterialEL;
            sharedColorELOpacity = wing.sharedColorELOpacity;
            sharedColorELHue = wing.sharedColorELHue;
            sharedColorELSaturation = wing.sharedColorELSaturation;
            sharedColorELBrightness = wing.sharedColorELBrightness;
        }

        #endregion

        #region Geometry
        public override void UpdateGeometry()
        {
            UpdateGeometry(true);
        }

        public virtual void UpdateGeometry(bool updateAerodynamics)
        {
            float wingThicknessDeviationRoot = sharedBaseThicknessRoot / 0.24f;
            float wingThicknessDeviationTip = sharedBaseThicknessTip / 0.24f;
            float wingWidthTipBasedOffsetTrailing = sharedBaseWidthTip / 2f + sharedBaseOffsetTip;
            float wingWidthTipBasedOffsetLeading = -sharedBaseWidthTip / 2f + sharedBaseOffsetTip;
            float wingWidthRootBasedOffset = sharedBaseWidthRoot / 2f;

            // First, wing cross section
            // No need to filter vertices by normals

            if (meshFilterWingSection != null)
            {
                int length = meshReferenceWingSection.vp.Length;
                Vector3[] vp = new Vector3[length];
                Array.Copy(meshReferenceWingSection.vp, vp, length);
                Vector2[] uv = new Vector2[length];
                Array.Copy(meshReferenceWingSection.uv, uv, length);

                for (int i = 0; i < length; ++i)
                {
                    // Root/tip filtering followed by leading/trailing filtering
                    if (vp[i].x < -0.05f)
                    {
                        if (vp[i].z < 0f)
                        {
                            vp[i] = new Vector3(-sharedBaseLength, vp[i].y * wingThicknessDeviationTip, wingWidthTipBasedOffsetLeading);
                            uv[i] = new Vector2(sharedBaseWidthTip, uv[i].y);
                        }
                        else
                        {
                            vp[i] = new Vector3(-sharedBaseLength, vp[i].y * wingThicknessDeviationTip, wingWidthTipBasedOffsetTrailing);
                            uv[i] = new Vector2(0f, uv[i].y);
                        }
                    }
                    else
                    {
                        if (vp[i].z < 0f)
                        {
                            vp[i] = new Vector3(vp[i].x, vp[i].y * wingThicknessDeviationRoot, -wingWidthRootBasedOffset);
                            uv[i] = new Vector2(sharedBaseWidthRoot, uv[i].y);
                        }
                        else
                        {
                            vp[i] = new Vector3(vp[i].x, vp[i].y * wingThicknessDeviationRoot, wingWidthRootBasedOffset);
                            uv[i] = new Vector2(0f, uv[i].y);
                        }
                    }
                }

                meshFilterWingSection.mesh.vertices = vp;
                meshFilterWingSection.mesh.uv = uv;
                meshFilterWingSection.mesh.RecalculateBounds();

                MeshCollider meshCollider = meshFilterWingSection.gameObject.GetComponent<MeshCollider>();
                if (meshCollider == null)
                    meshCollider = meshFilterWingSection.gameObject.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = null;
                meshCollider.sharedMesh = meshFilterWingSection.mesh;
                meshCollider.convex = true;
            }

            // Second, wing surfaces
            // Again, no need for filtering by normals

            if (meshFilterWingSurface != null)
            {
                meshFilterWingSurface.transform.localPosition = Vector3.zero;
                meshFilterWingSurface.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

                int length = meshReferenceWingSurface.vp.Length;
                Vector3[] vp = new Vector3[length];
                Array.Copy(meshReferenceWingSurface.vp, vp, length);
                Vector2[] uv = new Vector2[length];
                Array.Copy(meshReferenceWingSurface.uv, uv, length);
                Color[] cl = new Color[length];
                Vector2[] uv2 = new Vector2[length];

                for (int i = 0; i < length; ++i)
                {
                    // Root/tip filtering followed by leading/trailing filtering
                    if (vp[i].x < -0.05f)
                    {
                        if (vp[i].z < 0f)
                        {
                            vp[i] = new Vector3(-sharedBaseLength, vp[i].y * wingThicknessDeviationTip, wingWidthTipBasedOffsetLeading);
                            uv[i] = new Vector2(sharedBaseLength / 4f, 1f - 0.5f + sharedBaseWidthTip / 8f - sharedBaseOffsetTip / 4f);
                        }
                        else
                        {
                            vp[i] = new Vector3(-sharedBaseLength, vp[i].y * wingThicknessDeviationTip, wingWidthTipBasedOffsetTrailing);
                            uv[i] = new Vector2(sharedBaseLength / 4f, 0f + 0.5f - sharedBaseWidthTip / 8f - sharedBaseOffsetTip / 4f);
                        }
                    }
                    else
                    {
                        if (vp[i].z < 0f)
                        {
                            vp[i] = new Vector3(vp[i].x, vp[i].y * wingThicknessDeviationRoot, -wingWidthRootBasedOffset);
                            uv[i] = new Vector2(0.0f, 1f - 0.5f + sharedBaseWidthRoot / 8f);
                        }
                        else
                        {
                            vp[i] = new Vector3(vp[i].x, vp[i].y * wingThicknessDeviationRoot, wingWidthRootBasedOffset);
                            uv[i] = new Vector2(0f, 0f + 0.5f - sharedBaseWidthRoot / 8f);
                        }
                    }

                    // Top/bottom filtering
                    if (vp[i].y > 0f)
                    {
                        cl[i] = GetVertexColor(0);
                        uv2[i] = GetVertexUV2(sharedMaterialST);
                    }
                    else
                    {
                        cl[i] = GetVertexColor(1);
                        uv2[i] = GetVertexUV2(sharedMaterialSB);
                    }
                }

                meshFilterWingSurface.mesh.vertices = vp;
                meshFilterWingSurface.mesh.uv = uv;
                meshFilterWingSurface.mesh.uv2 = uv2;
                meshFilterWingSurface.mesh.colors = cl;
                meshFilterWingSurface.mesh.RecalculateBounds();
            }

            // Next, time for leading and trailing edges
            // Before modifying geometry, we have to show the correct objects for the current selection
            // As UI only works with floats, we have to cast selections into ints too

            int wingEdgeTypeTrailingInt = Mathf.RoundToInt(sharedEdgeTypeTrailing - 1);
            int wingEdgeTypeLeadingInt = Mathf.RoundToInt(sharedEdgeTypeLeading - 1);

            for (int i = 0; i < meshTypeCountEdgeWing; ++i)
            {
                if (i != wingEdgeTypeTrailingInt)
                    meshFiltersWingEdgeTrailing[i].gameObject.SetActive(false);
                else
                    meshFiltersWingEdgeTrailing[i].gameObject.SetActive(true);

                if (i != wingEdgeTypeLeadingInt)
                    meshFiltersWingEdgeLeading[i].gameObject.SetActive(false);
                else
                    meshFiltersWingEdgeLeading[i].gameObject.SetActive(true);
            }

            // Next we calculate some values reused for all edge geometry

            float wingEdgeWidthLeadingRootDeviation = sharedEdgeWidthLeadingRoot / 0.24f;
            float wingEdgeWidthLeadingTipDeviation = sharedEdgeWidthLeadingTip / 0.24f;

            float wingEdgeWidthTrailingRootDeviation = sharedEdgeWidthTrailingRoot / 0.24f;
            float wingEdgeWidthTrailingTipDeviation = sharedEdgeWidthTrailingTip / 0.24f;

            // Next, we fetch appropriate mesh reference and mesh filter for the edges and modify the meshes
            // Geometry is split into groups through simple vertex normal filtering 

            if (meshFiltersWingEdgeTrailing[wingEdgeTypeTrailingInt] != null)
            {
                MeshReference meshReference = meshReferencesWingEdge[wingEdgeTypeTrailingInt];
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
                    if (vp[i].x < -0.1f)
                    {
                        vp[i] = new Vector3(-sharedBaseLength, vp[i].y * wingThicknessDeviationTip, vp[i].z * wingEdgeWidthTrailingTipDeviation + sharedBaseWidthTip / 2f + sharedBaseOffsetTip); // Tip edge
                        if (nm[i].x == 0f) uv[i] = new Vector2(sharedBaseLength, uv[i].y);
                    }
                    else
                        vp[i] = new Vector3(0f, vp[i].y * wingThicknessDeviationRoot, vp[i].z * wingEdgeWidthTrailingRootDeviation + sharedBaseWidthRoot / 2f); // Root edge
                    if (nm[i].x == 0f && sharedEdgeTypeTrailing != 1)
                    {
                        cl[i] = GetVertexColor(2);
                        uv2[i] = GetVertexUV2(sharedMaterialET);
                    }
                }

                meshFiltersWingEdgeTrailing[wingEdgeTypeTrailingInt].mesh.vertices = vp;
                meshFiltersWingEdgeTrailing[wingEdgeTypeTrailingInt].mesh.uv = uv;
                meshFiltersWingEdgeTrailing[wingEdgeTypeTrailingInt].mesh.uv2 = uv2;
                meshFiltersWingEdgeTrailing[wingEdgeTypeTrailingInt].mesh.colors = cl;
                meshFiltersWingEdgeTrailing[wingEdgeTypeTrailingInt].mesh.RecalculateBounds();
            }
            if (meshFiltersWingEdgeLeading[wingEdgeTypeLeadingInt] != null)
            {
                MeshReference meshReference = meshReferencesWingEdge[wingEdgeTypeLeadingInt];
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
                    if (vp[i].x < -0.1f)
                    {
                        vp[i] = new Vector3(-sharedBaseLength, vp[i].y * wingThicknessDeviationTip, vp[i].z * wingEdgeWidthLeadingTipDeviation + sharedBaseWidthTip / 2f - sharedBaseOffsetTip); // Tip edge
                        if (nm[i].x == 0f)
                            uv[i] = new Vector2(sharedBaseLength, uv[i].y);
                    }
                    else
                        vp[i] = new Vector3(0f, vp[i].y * wingThicknessDeviationRoot, vp[i].z * wingEdgeWidthLeadingRootDeviation + sharedBaseWidthRoot / 2f); // Root edge
                    if (nm[i].x == 0f && sharedEdgeTypeLeading != 1)
                    {
                        cl[i] = GetVertexColor(3);
                        uv2[i] = GetVertexUV2(sharedMaterialEL);
                    }
                }

                meshFiltersWingEdgeLeading[wingEdgeTypeLeadingInt].mesh.vertices = vp;
                meshFiltersWingEdgeLeading[wingEdgeTypeLeadingInt].mesh.uv = uv;
                meshFiltersWingEdgeLeading[wingEdgeTypeLeadingInt].mesh.uv2 = uv2;
                meshFiltersWingEdgeLeading[wingEdgeTypeLeadingInt].mesh.colors = cl;
                meshFiltersWingEdgeLeading[wingEdgeTypeLeadingInt].mesh.RecalculateBounds();
            }
            if (updateAerodynamics)
                CalculateAerodynamicValues();
        }

        public override void UpdateCounterparts()
        {
            B9_ProceduralWing clone;
            for (int i = part.symmetryCounterparts.Count - 1; i >=0; --i)
            {
                clone = null;
                for (int j = part.symmetryCounterparts[i].Modules.Count - 1; i >=0; --j)
                {
                    if (part.symmetryCounterparts[i].Modules[j] is B9_ProceduralWing)
                        clone = (B9_ProceduralWing)part.symmetryCounterparts[i].Modules[j];
                }
                if (clone == null)
                {
                    throw new NullReferenceException("symmetry counterpart should never not have the same modules !!!");
                }

                clone.sharedBaseLength = sharedBaseLength;
                clone.sharedBaseWidthRoot = sharedBaseWidthRoot;
                clone.sharedBaseWidthTip = sharedBaseWidthTip;
                clone.sharedBaseThicknessRoot = sharedBaseThicknessRoot;
                clone.sharedBaseThicknessTip = sharedBaseThicknessTip;
                clone.sharedBaseOffsetRoot = sharedBaseOffsetRoot;
                clone.sharedBaseOffsetTip = sharedBaseOffsetTip;

                clone.sharedEdgeTypeLeading = sharedEdgeTypeLeading;
                clone.sharedEdgeWidthLeadingRoot = sharedEdgeWidthLeadingRoot;
                clone.sharedEdgeWidthLeadingTip = sharedEdgeWidthLeadingTip;

                clone.sharedEdgeTypeTrailing = sharedEdgeTypeTrailing;
                clone.sharedEdgeWidthTrailingRoot = sharedEdgeWidthTrailingRoot;
                clone.sharedEdgeWidthTrailingTip = sharedEdgeWidthTrailingTip;

                clone.sharedMaterialST = sharedMaterialST;
                clone.sharedMaterialSB = sharedMaterialSB;
                clone.sharedMaterialET = sharedMaterialET;
                clone.sharedMaterialEL = sharedMaterialEL;

                clone.sharedColorSTBrightness = sharedColorSTBrightness;
                clone.sharedColorSBBrightness = sharedColorSBBrightness;
                clone.sharedColorETBrightness = sharedColorETBrightness;
                clone.sharedColorELBrightness = sharedColorELBrightness;

                clone.sharedColorSTOpacity = sharedColorSTOpacity;
                clone.sharedColorSBOpacity = sharedColorSBOpacity;
                clone.sharedColorETOpacity = sharedColorETOpacity;
                clone.sharedColorELOpacity = sharedColorELOpacity;

                clone.sharedColorSTHue = sharedColorSTHue;
                clone.sharedColorSBHue = sharedColorSBHue;
                clone.sharedColorETHue = sharedColorETHue;
                clone.sharedColorELHue = sharedColorELHue;

                clone.sharedColorSTSaturation = sharedColorSTSaturation;
                clone.sharedColorSBSaturation = sharedColorSBSaturation;
                clone.sharedColorETSaturation = sharedColorETSaturation;
                clone.sharedColorELSaturation = sharedColorELSaturation;

                clone.UpdateGeometry();
            }
        }

        // Edge geometry
        public Vector3[] GetReferenceVertices(MeshFilter source)
        {
            return source?.mesh?.vertices;
        }

        #endregion

        #region Mesh Setup and Checking
        public virtual void SetupMeshFilters()
        {
            meshFilterWingSurface = CheckMeshFilter(meshFilterWingSurface, "surface");
            meshFilterWingSection = CheckMeshFilter(meshFilterWingSection, "section");
            for (int i = 0; i < meshTypeCountEdgeWing; ++i)
            {
                MeshFilter meshFilterWingEdgeTrailing = CheckMeshFilter("edge_trailing_type" + i);
                meshFiltersWingEdgeTrailing.Add(meshFilterWingEdgeTrailing);

                MeshFilter meshFilterWingEdgeLeading = CheckMeshFilter("edge_leading_type" + i);
                meshFiltersWingEdgeLeading.Add(meshFilterWingEdgeLeading);
            }
        }

        public virtual void SetupMeshReferences()
        {
            if (meshReferenceWingSection == null || meshReferenceWingSection.vp.Length == 0
                || meshReferenceWingSurface == null || meshReferenceWingSurface.vp.Length == 0
                || meshReferencesWingEdge[meshTypeCountEdgeWing - 1] == null || meshReferencesWingEdge[meshTypeCountEdgeWing - 1].vp.Length == 0)
            {
                meshReferenceWingSection = FillMeshRefererence(meshFilterWingSection);
                meshReferenceWingSurface = FillMeshRefererence(meshFilterWingSurface);
                for (int i = 0; i < meshTypeCountEdgeWing; ++i)
                {
                    MeshReference meshReferenceWingEdge = FillMeshRefererence(meshFiltersWingEdgeTrailing[i]);
                    meshReferencesWingEdge.Add(meshReferenceWingEdge);
                }
            }
        }

        // Reference fetching
        public virtual MeshFilter CheckMeshFilter(string name) { return CheckMeshFilter(null, name, false); }
        public virtual MeshFilter CheckMeshFilter(MeshFilter reference, string name) { return CheckMeshFilter(reference, name, false); }
        public virtual MeshFilter CheckMeshFilter(MeshFilter reference, string name, bool disable)
        {
            if (reference == null)
            {
                Transform parent = part.transform.GetChild(0).GetChild(0).GetChild(0).Find(name);
                if (parent != null)
                {
                    parent.localPosition = Vector3.zero;
                    reference = parent.gameObject.GetComponent<MeshFilter>();
                    if (disable)
                        parent.gameObject.SetActive(false);
                }
            }
            return reference;
        }

        public virtual Transform CheckTransform(string name)
        {
            return part.transform.GetChild(0).GetChild(0).GetChild(0).Find(name);
        }

        public virtual MeshReference FillMeshRefererence(MeshFilter source)
        {
            if (source != null)
            {
                MeshReference reference = new MeshReference();
                int length = source.mesh.vertices.Length;
                reference.vp = new Vector3[length];
                Array.Copy(source.mesh.vertices, reference.vp, length);
                reference.nm = new Vector3[length];
                Array.Copy(source.mesh.normals, reference.nm, length);
                reference.uv = new Vector2[length];
                Array.Copy(source.mesh.uv, reference.uv, length);
                return reference;
            }
            return null;
        }
        #endregion

        #region Materials
        public static Material materialLayeredSurface;
        public static Texture materialLayeredSurfaceTextureMain;
        public static Texture materialLayeredSurfaceTextureMask;

        public static Material materialLayeredEdge;
        public static Texture materialLayeredEdgeTextureMain;
        public static Texture materialLayeredEdgeTextureMask;

        public float materialPropertyShininess = 0.4f;
        public Color materialPropertySpecular = new Color(0.62109375f, 0.62109375f, 0.62109375f, 1.0f);

        public virtual void UpdateMaterials()
        {
            if (materialLayeredSurface == null || materialLayeredEdge == null)
                SetMaterialReferences();

            SetMaterial(meshFilterWingSurface, materialLayeredSurface);
            for (int i = 0; i < meshTypeCountEdgeWing; ++i)
            {
                SetMaterial(meshFiltersWingEdgeTrailing[i], materialLayeredEdge);
                SetMaterial(meshFiltersWingEdgeLeading[i], materialLayeredEdge);
            }
        }

        public virtual void SetMaterialReferences()
        {
            if (materialLayeredSurface == null)
                materialLayeredSurface = new Material(StaticWingGlobals.B9WingShader);
            if (materialLayeredEdge == null)
                materialLayeredEdge = new Material(StaticWingGlobals.B9WingShader);

            SetTextures(meshFilterWingSurface, meshFiltersWingEdgeTrailing[0]);

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

        public virtual void SetMaterial(MeshFilter target, Material material)
        {
            if (target != null)
            {
                Renderer r = target.gameObject.GetComponent<Renderer>();
                if (r != null)
                    r.sharedMaterial = material;
            }
        }

        public virtual void SetTextures(MeshFilter sourceSurface, MeshFilter sourceEdge)
        {
            if (sourceSurface != null)
            {
                Renderer r = sourceSurface.gameObject.GetComponent<Renderer>();
                if (r != null)
                {
                    materialLayeredSurfaceTextureMain = r.sharedMaterial.GetTexture("_MainTex");
                    materialLayeredSurfaceTextureMask = r.sharedMaterial.GetTexture("_Emissive");
                }
            }
            if (sourceEdge != null)
            {
                Renderer r = sourceEdge.gameObject.GetComponent<Renderer>();
                if (r != null)
                {
                    materialLayeredEdgeTextureMain = r.sharedMaterial.GetTexture("_MainTex");
                    materialLayeredEdgeTextureMask = r.sharedMaterial.GetTexture("_Emissive");
                }
            }
        }

        public static Vector2 GetVertexUV2(float selectedLayer)
        {
            if (selectedLayer == 0)
                return new Vector2(0f, 1f);
            else
                return new Vector2((selectedLayer - 1f) / 3f, 0f);
        }

        #endregion

        #region Aero

        // Delayed aero value setup
        // Must be run after all geometry setups, otherwise FAR checks will be done before surrounding parts take shape, producing incorrect results
        public IEnumerator SetupReorderedForFlight()
        {
            // First we need to determine whether the vessel this part is attached to is included into the status list
            // If it's included, we need to fetch it's index in that list

            bool vesselListInclusive = false;
            int vesselID = vessel.GetInstanceID();
            int vesselStatusIndex = 0;
            int vesselListCount = vesselList.Count;
            for (int i = 0; i < vesselListCount; ++i)
            {
                if (vesselList[i].vessel.GetInstanceID() == vesselID)
                {
                    vesselListInclusive = true;
                    vesselStatusIndex = i;
                }
            }

            // If it was not included, we add it to the list
            // Correct index is then fairly obvious

            if (!vesselListInclusive)
            {
                vesselList.Add(new VesselStatus(vessel, false));
                vesselStatusIndex = vesselList.Count - 1;
            }

            // Using the index for the status list we obtained, we check whether it was updated yet
            // So that only one part can run the following part

            if (!vesselList[vesselStatusIndex].isUpdated)
            {
                vesselList[vesselStatusIndex].isUpdated = true;
                List<B9_ProceduralWing> moduleList = new List<B9_ProceduralWing>();

                // First we get a list of all relevant parts in the vessel
                // Found modules are added to a list

                int vesselPartsCount = vessel.parts.Count;
                for (int i = 0; i < vesselPartsCount; ++i)
                {
                    if (vessel.parts[i].Modules.Contains("WingProcedural"))
                        moduleList.Add((B9_ProceduralWing)vessel.parts[i].Modules["WingProcedural"]);
                }

                // After that we make two separate runs through that list
                // First one setting up all geometry and second one setting up aerodynamic values
                int moduleListCount = moduleList.Count;
                for (int i = 0; i < moduleListCount; ++i)
                {
                    moduleList[i].Setup();
                }

                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();

                for (int i = 0; i < moduleListCount; ++i)
                {
                    moduleList[i].CalculateAerodynamicValues();
                }
            }
        }


        public Vector3d aeroStatRootMidChordOffsetFromOrigin;

        public PartModule aeroFARModuleReference;
        public Type aeroFARModuleType;
        public MethodInfo aeroFARMethodInfoUsed;

        public FieldInfo aeroFARFieldInfoSemispan;
        public FieldInfo aeroFARFieldInfoSemispan_Actual; // to handle tweakscale, FARs wings have semispan (unscaled) and semispan_actual (tweakscaled). Need to set both (actual is the important one, and tweakscale isn't needed here, so only _actual actually needs to be set, but it would be silly to not set it)
        public FieldInfo aeroFARFieldInfoMAC;
        public FieldInfo aeroFARFieldInfoMAC_Actual; //  to handle tweakscale, FARs wings have MAC (unscaled) and MAC_actual (tweakscaled). Need to set both (actual is the important one, and tweakscale isn't needed here, so only _actual actually needs to be set, but it would be silly to not set it)
        public FieldInfo aeroFARFieldInfoMidChordSweep;
        public FieldInfo aeroFARFieldInfoTaperRatio;
        public FieldInfo aeroFARFieldInfoControlSurfaceFraction;
        public FieldInfo aeroFARFieldInfoRootChordOffset;

        public override void CalculateAerodynamicValues()
        {
            CheckAssemblies();

            float sharedWidthTipSum = sharedBaseWidthTip;
            float sharedWidthRootSum = sharedBaseWidthRoot;

            double offset = 0;
            if (sharedEdgeTypeLeading != 1)
            {
                sharedWidthTipSum += sharedEdgeWidthLeadingTip;
                sharedWidthRootSum += sharedEdgeWidthLeadingRoot;
                offset += 0.2 * (sharedEdgeWidthLeadingRoot + sharedEdgeWidthLeadingTip);
            }
            if (sharedEdgeTypeTrailing != 1)
            {
                sharedWidthTipSum += sharedEdgeWidthTrailingTip;
                sharedWidthRootSum += sharedEdgeWidthTrailingRoot;
                offset -= 0.25 * (sharedEdgeWidthTrailingRoot + sharedEdgeWidthTrailingTip);
            }
            aeroStatRootMidChordOffsetFromOrigin = offset * Vector3d.up;

            float ctrlOffsetRootLimit = (sharedBaseLength / 2f) / (sharedBaseWidthRoot + sharedEdgeWidthTrailingRoot);
            float ctrlOffsetTipLimit = (sharedBaseLength / 2f) / (sharedBaseWidthTip + sharedEdgeWidthTrailingTip);

            float ctrlOffsetRootClamped = Mathf.Clamp(sharedBaseOffsetRoot, -ctrlOffsetRootLimit, ctrlOffsetRootLimit);
            float ctrlOffsetTipClamped = Mathf.Clamp(sharedBaseOffsetTip, -ctrlOffsetTipLimit, ctrlOffsetTipLimit);

            // Base four values
            length = (double)sharedBaseLength;
            taperRatio = (double)sharedWidthTipSum / (double)sharedWidthRootSum;
            MAC = (double)(sharedWidthTipSum + sharedWidthRootSum) / 2.0;
            midChordSweep = Math.Atan((double)sharedBaseOffsetTip / (double)sharedBaseLength) * Utils.Rad2Deg;

            // Derived values

            surfaceArea = MAC * length;
            aspectRatio = 2.0f * length / MAC;

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
            part.CoMOffset = new Vector3(sharedBaseLength / 2f, -sharedBaseOffsetTip / 2f, 0f);

            part.breakingForce = Mathf.Round((float)connectionForce);
            part.breakingTorque = Mathf.Round((float)connectionForce);

            // Stock-only values
            if (!FARactive)
            {
                SetStockModuleParams();
            }
            else
                setFARModuleParams();
            
            StartCoroutine(updateAeroDelayed());
        }

        #endregion

        #region Alternative UI/input
        public static Vector4 uiColorSliderBase = new Vector4(0.25f, 0.5f, 0.4f, 1f);
        public static Vector4 uiColorSliderEdgeL = new Vector4(0.20f, 0.5f, 0.4f, 1f);
        public static Vector4 uiColorSliderEdgeT = new Vector4(0.15f, 0.5f, 0.4f, 1f);
        public static Vector4 uiColorSliderColorsST = new Vector4(0.10f, 0.5f, 0.4f, 1f);
        public static Vector4 uiColorSliderColorsSB = new Vector4(0.05f, 0.5f, 0.4f, 1f);
        public static Vector4 uiColorSliderColorsET = new Vector4(0.00f, 0.5f, 0.4f, 1f);
        public static Vector4 uiColorSliderColorsEL = new Vector4(0.95f, 0.5f, 0.4f, 1f);

        public override void scaleTip(Vector3 diff)
        {
            if (!Input.GetKey(keyTipScale))
            {
                state = 0;
                return;
            }
            sharedBaseWidthTip += diff.x * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.right, -part.transform.up) + diff.y * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.up, -part.transform.up);
            sharedBaseWidthTip = (float)Utils.Clamp(sharedBaseWidthTip, GetLimitsFromType(sharedBaseWidthTipLimits).x, GetLimitsFromType(sharedBaseWidthTipLimits).y);
            sharedBaseThicknessTip += diff.x * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.right, -part.transform.forward) + diff.y * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.up, part.transform.forward * (part.isMirrored ? 1 : -1));
            sharedBaseThicknessTip = (float)Utils.Clamp(sharedBaseThicknessTip, sharedBaseThicknessLimits.x, sharedBaseThicknessLimits.y);
        }

        public override void scaleRoot(Vector3 diff)
        {
            if (part.parent.Modules.OfType<Base_ProceduralWing>().Any())
                return;
            if (!Input.GetKey(keyRootScale))
            {
                state = 0;
                return;
            }

            sharedBaseWidthRoot += diff.x * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.right, -part.transform.up) + diff.y * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.up, -part.transform.up);
            sharedBaseWidthRoot = (float)Utils.Clamp(sharedBaseWidthRoot, GetLimitsFromType(sharedBaseWidthRootLimits).x, GetLimitsFromType(sharedBaseWidthRootLimits).y);
            sharedBaseThicknessRoot += diff.x * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.right, -part.transform.forward) + diff.y * Vector3.Dot(EditorCamera.Instance.GetComponentCached<Camera>(ref editorCam).transform.up, part.transform.forward * (part.isMirrored ? 1 : -1));
            sharedBaseThicknessRoot = (float)Utils.Clamp(sharedBaseThicknessRoot, sharedBaseThicknessLimits.x, sharedBaseThicknessLimits.y);
        }

        public virtual Color GetVertexColor(int side)
        {
            if (side == 0)
                return UIUtility.ColorHSBToRGB(new Vector4(sharedColorSTHue, sharedColorSTSaturation, sharedColorSTBrightness, sharedColorSTOpacity));
            else if (side == 1)
                return UIUtility.ColorHSBToRGB(new Vector4(sharedColorSBHue, sharedColorSBSaturation, sharedColorSBBrightness, sharedColorSBOpacity));
            else if (side == 2)
                return UIUtility.ColorHSBToRGB(new Vector4(sharedColorETHue, sharedColorETSaturation, sharedColorETBrightness, sharedColorETOpacity));
            else
                return UIUtility.ColorHSBToRGB(new Vector4(sharedColorELHue, sharedColorELSaturation, sharedColorELBrightness, sharedColorELOpacity));
        }
        #endregion

        public static double incrementMain = 0.125, incrementSmall = 0.04;
        public static Vector2d uiLengthLimit = new Vector2d(0.125, 16);
        public static Vector2d uiRootLimit = new Vector2d(0.125, 16);
        public static Vector2d uiTipLimit = new Vector2d(0.0000001, 16);
        public static Vector2d uiOffsetLimit = new Vector2d(-8, 8);
        public static Vector2d uiThicknessLimit = new Vector2d(0.04, 1);
        public static Vector4 baseColour = new Vector4(0.25f, 0.5f, 0.4f, 1f);

        public override Vector3 tipPos
        {
            get { return new Vector3(-sharedBaseOffsetTip, 0, sharedBaseLength); }
            set
            {
                sharedBaseLength = value.z;
                sharedBaseOffsetTip = -value.x;
                UpdateGeometry(true);
            }
        }
        public override double tipWidth
        {
            get { return sharedBaseWidthTip; }
            set
            {
                sharedBaseWidthTip = (float)value;
                UpdateGeometry(true);
            }
        }
        public override double tipThickness
        {
            get { return sharedBaseThicknessTip; }
            set
            {
                sharedBaseThicknessTip = (float)value;
                UpdateGeometry(true);
            }
        }
        public override double rootWidth
        {
            get { return sharedBaseWidthRoot; }
            set
            {
                sharedBaseWidthRoot = (float)value;
                UpdateGeometry(true);
            }
        }
        public override double rootThickness
        {
            get { return sharedBaseThicknessRoot; }
            set
            {
                sharedBaseThicknessRoot = (float)value;
                UpdateGeometry(true);
            }
        }
        public override double minSpan
        {
            get { return sharedBaseLengthLimits.x; }
        }
        public override double tipOffset
        {
            get { return sharedBaseOffsetTip; }
            set
            {
                sharedBaseOffsetTip = (float)value;
                UpdateGeometry(true);
            }
        }
        public override double Length
        {
            get { return sharedBaseLength; }
            set
            {
                sharedBaseLength = (float)value;
                UpdateGeometry(true);
            }
        }
        public override double Scale
        {
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
