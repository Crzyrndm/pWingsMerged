using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ProceduralWings
{
    class OnGUI_Window
    {
        bool uiWindowActive;
        public static bool uiEditMode = false;
        public static bool uiAdjustWindow = true;
        public static bool uiEditModeTimeout = false;
        private float uiEditModeTimeoutDuration = 0.25f;
        private float uiEditModeTimer = 0f;

        public static bool sharedFieldGroupBaseStatic = true;
        public static bool sharedFieldGroupEdgeLeadingStatic = false;
        public float sharedBaseLengthCached = 4f;
        public static Vector4 sharedBaseLengthDefaults = new Vector4(4f, 1f, 4f, 1f);


        public float sharedBaseWidthRootCached = 4f;
        public static Vector4 sharedBaseWidthRootDefaults = new Vector4(4f, 0.5f, 4f, 0.5f);


        public float sharedBaseWidthTipCached = 4f;
        public static Vector4 sharedBaseWidthTipDefaults = new Vector4(4f, 0.5f, 4f, 0.5f);


        public float sharedBaseOffsetRootCached = 0f;
        public static Vector4 sharedBaseOffsetRootDefaults = new Vector4(0f, 0f, 0f, 0f);


        public float sharedBaseOffsetTipCached = 0f;
        public static Vector4 sharedBaseOffsetTipDefaults = new Vector4(0f, 0f, 0f, 0f);


        public float sharedBaseThicknessRootCached = 0.24f;
        public static Vector4 sharedBaseThicknessRootDefaults = new Vector4(0.24f, 0.24f, 0.24f, 0.24f);


        public float sharedBaseThicknessTipCached = 0.24f;
        public static Vector4 sharedBaseThicknessTipDefaults = new Vector4(0.24f, 0.24f, 0.24f, 0.24f);


        private static string[] sharedFieldGroupEdgeLeadingArray = new string[] { "sharedEdgeTypeLeading", "sharedEdgeWidthLeadingRoot", "sharedEdgeWidthLeadingTip" };


        public float sharedEdgeTypeLeadingCached = 2f;
        public static Vector4 sharedEdgeTypeLeadingDefaults = new Vector4(2f, 1f, 2f, 1f);


        public float sharedEdgeWidthLeadingRootCached = 0.24f;
        public static Vector4 sharedEdgeWidthLeadingRootDefaults = new Vector4(0.24f, 0.24f, 0.24f, 0.24f);

        public float sharedEdgeWidthLeadingTipCached = 0.24f;
        public static Vector4 sharedEdgeWidthLeadingTipDefaults = new Vector4(0.24f, 0.24f, 0.24f, 0.24f);

        public static bool sharedFieldGroupEdgeTrailingStatic = false;

        private static string[] sharedFieldGroupEdgeTrailingArray = new string[] { "sharedEdgeTypeTrailing", "sharedEdgeWidthTrailingRoot", "sharedEdgeWidthTrailingTip" };

        public float sharedEdgeTypeTrailingCached = 3f;
        public static Vector4 sharedEdgeTypeTrailingDefaults = new Vector4(3f, 2f, 3f, 2f);

        public float sharedEdgeWidthTrailingRootCached = 0.48f;
        public static Vector4 sharedEdgeWidthTrailingRootDefaults = new Vector4(0.48f, 0.48f, 0.48f, 0.48f);

        public float sharedEdgeWidthTrailingTipCached = 0.48f;
        public static Vector4 sharedEdgeWidthTrailingTipDefaults = new Vector4(0.48f, 0.48f, 0.48f, 0.48f);
        public static bool sharedFieldGroupColorSTStatic = false;

        private static string[] sharedFieldGroupColorSTArray = new string[] { "sharedMaterialST", "sharedColorSTOpacity", "sharedColorSTHue", "sharedColorSTSaturation", "sharedColorSTBrightness" };

       

        public float sharedMaterialSTCached = 1f;
        public static Vector4 sharedMaterialSTDefaults = new Vector4(1f, 1f, 1f, 1f);


        public float sharedColorSTOpacityCached = 0f;
        public static Vector4 sharedColorSTOpacityDefaults = new Vector4(0f, 0f, 0f, 0f);


        public float sharedColorSTHueCached = 0.10f;
        public static Vector4 sharedColorSTHueDefaults = new Vector4(0.1f, 0.1f, 0.1f, 0.1f);


        public float sharedColorSTSaturationCached = 0.75f;
        public static Vector4 sharedColorSTSaturationDefaults = new Vector4(0.75f, 0.75f, 0.75f, 0.75f);

        public float sharedColorSTBrightnessCached = 0.6f;
        public static Vector4 sharedColorSTBrightnessDefaults = new Vector4(0.6f, 0.6f, 0.6f, 0.6f);
        public static bool sharedFieldGroupColorSBStatic = false;

        private static string[] sharedFieldGroupColorSBArray = new string[] { "sharedMaterialSB", "sharedColorSBOpacity", "sharedColorSBHue", "sharedColorSBSaturation", "sharedColorSBBrightness" };


        public float sharedMaterialSBCached = 4f;
        public static Vector4 sharedMaterialSBDefaults = new Vector4(4f, 4f, 4f, 4f);


        public float sharedColorSBOpacityCached = 0f;
        public static Vector4 sharedColorSBOpacityDefaults = new Vector4(0f, 0f, 0f, 0f);


        public float sharedColorSBHueCached = 0.10f;
        public static Vector4 sharedColorSBHueDefaults = new Vector4(0.1f, 0.1f, 0.1f, 0.1f);


        public float sharedColorSBSaturationCached = 0.75f;
        public static Vector4 sharedColorSBSaturationDefaults = new Vector4(0.75f, 0.75f, 0.75f, 0.75f);


        public float sharedColorSBBrightnessCached = 0.6f;
        public static Vector4 sharedColorSBBrightnessDefaults = new Vector4(0.6f, 0.6f, 0.6f, 0.6f);

        public static bool sharedFieldGroupColorETStatic = false;

        private static string[] sharedFieldGroupColorETArray = new string[] { "sharedMaterialET", "sharedColorETOpacity", "sharedColorETHue", "sharedColorETSaturation", "sharedColorETBrightness" };

        
        public static Vector4 sharedMaterialETDefaults = new Vector4(4f, 4f, 4f, 4f);

        

        public float sharedColorETOpacityCached = 0f;
        public static Vector4 sharedColorETOpacityDefaults = new Vector4(0f, 0f, 0f, 0f);

        

        public float sharedColorETHueCached = 0.10f;
        public static Vector4 sharedColorETHueDefaults = new Vector4(0.1f, 0.1f, 0.1f, 0.1f);

        

        public float sharedColorETSaturationCached = 0.75f;
        public static Vector4 sharedColorETSaturationDefaults = new Vector4(0.75f, 0.75f, 0.75f, 0.75f);

        
        public float sharedColorETBrightnessCached = 0.6f;
        public static Vector4 sharedColorETBrightnessDefaults = new Vector4(0.6f, 0.6f, 0.6f, 0.6f);
        
        public static bool sharedFieldGroupColorELStatic = false;

        private static string[] sharedFieldGroupColorELArray = new string[] { "sharedMaterialEL", "sharedColorELOpacity", "sharedColorELHue", "sharedColorELSaturation", "sharedColorELBrightness" };

        
        public float sharedMaterialELCached = 4f;
        public static Vector4 sharedMaterialELDefaults = new Vector4(4f, 4f, 4f, 4f);

        
        public float sharedColorELOpacityCached = 0f;
        public static Vector4 sharedColorELOpacityDefaults = new Vector4(0f, 0f, 0f, 0f);

        
        public float sharedColorELHueCached = 0.10f;
        public static Vector4 sharedColorELHueDefaults = new Vector4(0.1f, 0.1f, 0.1f, 0.1f);

        
        public float sharedColorELSaturationCached = 0.75f;
        public static Vector4 sharedColorELSaturationDefaults = new Vector4(0.75f, 0.75f, 0.75f, 0.75f);

        public float sharedColorELBrightnessCached = 0.6f;
        public static Vector4 sharedColorELBrightnessDefaults = new Vector4(0.6f, 0.6f, 0.6f, 0.6f);


        Base_ProceduralWing wing;

        bool isCtrlSrf
        {
            get
            {
                return wing.IsCtrlSrf;
            }
        }
        public void Show(Base_ProceduralWing wing)
        {
            Debug.Log("show");
            uiWindowActive = true;
            this.wing = wing;
        }

        public void GUI(Base_ProceduralWing w)
        {
            if (wing == null)
                return;
            if (w.GetInstanceID() == wing.GetInstanceID())
            {
                if (!UIUtility.uiStyleConfigured)
                    UIUtility.ConfigureStyles();

                UIUtility.uiRectWindowEditor = GUILayout.Window(w.GetInstanceID(), UIUtility.uiRectWindowEditor, OnWindow, w.WindowTitle, UIUtility.uiStyleWindow, GUILayout.Height(uiAdjustWindow ? 0 : UIUtility.uiRectWindowEditor.height));
                uiAdjustWindow = false;

                // Thanks to ferram4
                // Following section lock the editor, preventing window clickthrough
                if (UIUtility.uiRectWindowEditor.Contains(UIUtility.GetMousePos()))
                {
                    EditorLogic.fetch.Lock(false, false, false, "WingProceduralWindow");
                    //if (EditorTooltip.Instance != null)
                    //    EditorTooltip.Instance.HideToolTip ();
                }
                else
                    EditorLogic.fetch.Unlock("WingProceduralWindow");
            }
        }

        private static Vector4 sharedBaseLengthLimits = new Vector4(0.125f, 16f, 0.04f, 8f);
        private static Vector2 sharedBaseThicknessLimits = new Vector2(0.04f, 1f);
        private static Vector4 sharedBaseWidthRootLimits = new Vector4(0.125f, 16f, 0.04f, 1.6f);
        private static Vector4 sharedBaseWidthTipLimits = new Vector4(0.0001f, 16f, 0.04f, 1.6f);
        private static Vector4 sharedBaseOffsetLimits = new Vector4(-8f, 8f, -2f, 2f);
        private static Vector4 sharedEdgeTypeLimits = new Vector4(1f, 4f, 1f, 3f);
        private static Vector4 sharedEdgeWidthLimits = new Vector4(0f, 1f, 0f, 1f);
        private static Vector2 sharedMaterialLimits = new Vector2(0f, 4f);
        private static Vector2 sharedColorLimits = new Vector2(0f, 1f);

        private static float sharedIncrementColor = 0.01f;
        private static float sharedIncrementColorLarge = 0.10f;
        private static float sharedIncrementMain = 0.125f;
        private static float sharedIncrementSmall = 0.04f;
        private static float sharedIncrementInt = 1f;

        public static Vector4 uiColorSliderBase = new Vector4(0.25f, 0.5f, 0.4f, 1f);
        public static Vector4 uiColorSliderEdgeL = new Vector4(0.20f, 0.5f, 0.4f, 1f);
        public static Vector4 uiColorSliderEdgeT = new Vector4(0.15f, 0.5f, 0.4f, 1f);
        public static Vector4 uiColorSliderColorsST = new Vector4(0.10f, 0.5f, 0.4f, 1f);
        public static Vector4 uiColorSliderColorsSB = new Vector4(0.05f, 0.5f, 0.4f, 1f);
        public static Vector4 uiColorSliderColorsET = new Vector4(0.00f, 0.5f, 0.4f, 1f);
        public static Vector4 uiColorSliderColorsEL = new Vector4(0.95f, 0.5f, 0.4f, 1f);

        private void OnWindow(int window)
        {
            if (uiWindowActive)
            {
                bool returnEarly = false;
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                if (uiLastFieldName.Length > 0) GUILayout.Label("Last: " + uiLastFieldName, UIUtility.uiStyleLabelMedium);
                else GUILayout.Label("Property editor", UIUtility.uiStyleLabelMedium);
                if (uiLastFieldTooltip.Length > 0) GUILayout.Label(uiLastFieldTooltip + "\n_________________________", UIUtility.uiStyleLabelHint, GUILayout.MaxHeight(44f), GUILayout.MinHeight(44f)); // 58f for four lines
                GUILayout.EndVertical();
                if (GUILayout.Button("Close", UIUtility.uiStyleButton, GUILayout.MaxWidth(50f)))
                {
                    EditorLogic.fetch.Unlock("WingProceduralWindow");
                    uiWindowActive = false;
                    //stockButton.SetFalse(false);
                    returnEarly = true;
                }
                GUILayout.EndHorizontal();
                if (returnEarly)
                    return;

                DrawFieldGroupHeader(ref sharedFieldGroupBaseStatic, "Base");
                if (sharedFieldGroupBaseStatic)
                {
                    wing.Scale = DrawField(wing.Scale, GetIncrementFromType(sharedIncrementMain, sharedIncrementSmall), GetIncrementFromType(1f, 0.24f), GetLimitsFromType(sharedBaseLengthLimits), "Scale", uiColorSliderBase, -1, 0);
                    wing.Length = DrawField(wing.Length, GetIncrementFromType(sharedIncrementMain, sharedIncrementSmall), GetIncrementFromType(1f, 0.24f), GetLimitsFromType(sharedBaseLengthLimits), "Length", uiColorSliderBase, 0, 0);
                    wing.RootWidth = DrawField(wing.RootWidth, GetIncrementFromType(sharedIncrementMain, sharedIncrementSmall), GetIncrementFromType(1f, 0.24f), GetLimitsFromType(sharedBaseWidthRootLimits), "Width (root)", uiColorSliderBase, 1, 0);
                    wing.TipWidth = DrawField(wing.TipWidth, GetIncrementFromType(sharedIncrementMain, sharedIncrementSmall), GetIncrementFromType(1f, 0.24f), GetLimitsFromType(sharedBaseWidthTipLimits), "Width (tip)", uiColorSliderBase, 2, 0);
                    //if (isCtrlSrf)
                    //DrawField(sharedBaseOffsetRoot, GetIncrementFromType(sharedIncrementMain, sharedIncrementSmall), 1f, GetLimitsFromType(sharedBaseOffsetLimits), "Offset (root)", uiColorSliderBase, 3, 0);
                    wing.TipOffset = DrawField(wing.TipOffset, GetIncrementFromType(sharedIncrementMain, sharedIncrementSmall), 1f, GetLimitsFromType(sharedBaseOffsetLimits), "Offset (tip)", uiColorSliderBase, 4, 0);
                    wing.RootThickness = DrawField(wing.RootThickness, sharedIncrementSmall, sharedIncrementSmall, sharedBaseThicknessLimits, "Thickness (root)", uiColorSliderBase, 5, 0);
                    wing.TipThickness = DrawField(wing.TipThickness, sharedIncrementSmall, sharedIncrementSmall, sharedBaseThicknessLimits, "Thickness (tip)", uiColorSliderBase, 6, 0);
                }

                B9PWing.B9_ProceduralWing w = wing as B9PWing.B9_ProceduralWing;
                if (w != null)
                {
                    if (!isCtrlSrf)
                    {
                        DrawFieldGroupHeader(ref sharedFieldGroupEdgeLeadingStatic, "Edge (leading)");
                        if (sharedFieldGroupEdgeLeadingStatic)
                        {
                            w.LeadingEdgeType = (int)DrawField(w.LeadingEdgeType, sharedIncrementInt, sharedIncrementInt, GetLimitsFromType(sharedEdgeTypeLimits), "Shape", uiColorSliderEdgeL, 7, 2, false);
                            w.RootLeadingEdge = DrawField(w.RootLeadingEdge, sharedIncrementSmall, sharedIncrementSmall, GetLimitsFromType(sharedEdgeWidthLimits), "Width (root)", uiColorSliderEdgeL, 8, 0);
                            w.TipLeadingEdge = DrawField(w.TipLeadingEdge, sharedIncrementSmall, sharedIncrementSmall, GetLimitsFromType(sharedEdgeWidthLimits), "Width (tip)", uiColorSliderEdgeL, 9, 0);
                        }
                    }

                    DrawFieldGroupHeader(ref sharedFieldGroupEdgeTrailingStatic, "Edge (trailing)");
                    if (sharedFieldGroupEdgeTrailingStatic)
                    {
                        w.TrailingEdgeType = (int)DrawField(w.TrailingEdgeType, sharedIncrementInt, sharedIncrementInt, GetLimitsFromType(sharedEdgeTypeLimits), "Shape", uiColorSliderEdgeT, 10, isCtrlSrf ? 3 : 2, false);
                        w.RootTrailingEdge = DrawField(w.RootTrailingEdge, sharedIncrementSmall, sharedIncrementSmall, GetLimitsFromType(sharedEdgeWidthLimits), "Width (root)", uiColorSliderEdgeT, 11, 0);
                        w.TipTrailingEdge = DrawField(w.TipTrailingEdge, sharedIncrementSmall, sharedIncrementSmall, GetLimitsFromType(sharedEdgeWidthLimits), "Width (tip)", uiColorSliderEdgeT, 12, 0);
                    }

                    DrawFieldGroupHeader(ref sharedFieldGroupColorSTStatic, "Surface (top)");
                    if (sharedFieldGroupColorSTStatic)
                    {
                        w.SurfTopMat = (int)DrawField(w.SurfTopMat, sharedIncrementInt, sharedIncrementInt, sharedMaterialLimits, "Material", uiColorSliderColorsST, 13, 1, false);
                        w.SurfTopOpacity = DrawField(w.SurfTopOpacity, sharedIncrementColor, sharedIncrementColorLarge, sharedColorLimits, "Opacity", uiColorSliderColorsST, 14, 0);
                        w.SurfTopHue = DrawField(w.SurfTopHue, sharedIncrementColor, sharedIncrementColorLarge, sharedColorLimits, "Hue", uiColorSliderColorsST, 15, 0);
                        w.SurfTopSat = DrawField(w.SurfTopSat, sharedIncrementColor, sharedIncrementColorLarge, sharedColorLimits, "Saturation", uiColorSliderColorsST, 16, 0);
                        w.SurfTopBright = DrawField(w.SurfTopBright, sharedIncrementColor, sharedIncrementColorLarge, sharedColorLimits, "Brightness", uiColorSliderColorsST, 17, 0);
                    }

                    DrawFieldGroupHeader(ref sharedFieldGroupColorSBStatic, "Surface (bottom)");
                    if (sharedFieldGroupColorSBStatic)
                    {
                        w.SurfBottomMat = (int)DrawField(w.SurfBottomMat, sharedIncrementInt, sharedIncrementInt, sharedMaterialLimits, "Material", uiColorSliderColorsSB, 13, 1, false);
                        w.SurfBottomOpacity = DrawField(w.SurfBottomOpacity, sharedIncrementColor, sharedIncrementColorLarge, sharedColorLimits, "Opacity", uiColorSliderColorsSB, 14, 0);
                        w.SurfBottomHue = DrawField(w.SurfBottomHue, sharedIncrementColor, sharedIncrementColorLarge, sharedColorLimits, "Hue", uiColorSliderColorsSB, 15, 0);
                        w.SurfBottomSat = DrawField(w.SurfBottomSat, sharedIncrementColor, sharedIncrementColorLarge, sharedColorLimits, "Saturation", uiColorSliderColorsSB, 16, 0);
                        w.SurfBottomBright = DrawField(w.SurfBottomBright, sharedIncrementColor, sharedIncrementColorLarge, sharedColorLimits, "Brightness", uiColorSliderColorsSB, 17, 0);
                    }

                    DrawFieldGroupHeader(ref sharedFieldGroupColorETStatic, "Surface (trailing edge)");
                    if (sharedFieldGroupColorETStatic)
                    {
                        w.SurfTrailMat = (int)DrawField(w.SurfTrailMat, sharedIncrementInt, sharedIncrementInt, sharedMaterialLimits, "Material", uiColorSliderColorsET, 13, 1, false);
                        w.SurfTrailOpacity = DrawField(w.SurfTrailOpacity, sharedIncrementColor, sharedIncrementColorLarge, sharedColorLimits, "Opacity", uiColorSliderColorsET, 14, 0);
                        w.SurfTrailHue = DrawField(w.SurfTrailHue, sharedIncrementColor, sharedIncrementColorLarge, sharedColorLimits, "Hue", uiColorSliderColorsET, 15, 0);
                        w.SurfTrailSat = DrawField(w.SurfTrailSat, sharedIncrementColor, sharedIncrementColorLarge, sharedColorLimits, "Saturation", uiColorSliderColorsET, 16, 0);
                        w.SurfTrailBright = DrawField(w.SurfTrailBright, sharedIncrementColor, sharedIncrementColorLarge, sharedColorLimits, "Brightness", uiColorSliderColorsET, 17, 0);
                    }

                    if (!isCtrlSrf)
                    {
                        DrawFieldGroupHeader(ref sharedFieldGroupColorELStatic, "Surface (leading edge)");
                        if (sharedFieldGroupColorELStatic)
                        {
                            w.SurfLeadMat = (int)DrawField(w.SurfLeadMat, sharedIncrementInt, sharedIncrementInt, sharedMaterialLimits, "Material", uiColorSliderColorsEL, 13, 1, false);
                            w.SurfLeadOpacity = DrawField(w.SurfLeadOpacity, sharedIncrementColor, sharedIncrementColorLarge, sharedColorLimits, "Opacity", uiColorSliderColorsEL, 14, 0);
                            w.SurfLeadHue = DrawField(w.SurfLeadHue, sharedIncrementColor, sharedIncrementColorLarge, sharedColorLimits, "Hue", uiColorSliderColorsEL, 15, 0);
                            w.SurfLeadSat = DrawField(w.SurfLeadSat, sharedIncrementColor, sharedIncrementColorLarge, sharedColorLimits, "Saturation", uiColorSliderColorsEL, 16, 0);
                            w.SurfLeadBright = DrawField(w.SurfLeadBright, sharedIncrementColor, sharedIncrementColorLarge, sharedColorLimits, "Brightness", uiColorSliderColorsEL, 17, 0);
                        }
                    }
                }

                GUILayout.Label("_________________________\n\nPress J to exit edit mode\nOptions below allow you to change default values", UIUtility.uiStyleLabelHint);
                if (wing.CanBeFueled && w.useStockFuel)
                {
                    if (GUILayout.Button(FuelGUIGetConfigDesc() + " | Next tank setup", UIUtility.uiStyleButton))
                    {
                        w.NextConfiguration();
                    }
                }
                //if (inheritancePossibleOnShape || inheritancePossibleOnMaterials)
                //{
                //    GUILayout.Label("_________________________\n\nOptions options allow you to match the part properties to it's parent", UIUtility.uiStyleLabelHint);
                //    GUILayout.BeginHorizontal();
                //    if (inheritancePossibleOnShape)
                //    {
                //        if (GUILayout.Button("Shape", UIUtility.uiStyleButton))
                //            InheritParentValues(0);
                //        if (GUILayout.Button("Base", UIUtility.uiStyleButton))
                //            InheritParentValues(1);
                //        if (GUILayout.Button("Edges", UIUtility.uiStyleButton))
                //            InheritParentValues(2);
                //    }
                //    if (inheritancePossibleOnMaterials)
                //    {
                //        if (GUILayout.Button("Color", UIUtility.uiStyleButton)) InheritParentValues(3);
                //    }
                //    GUILayout.EndHorizontal();
                //}
            }
            else
            {
                if (uiEditModeTimeout)
                    GUILayout.Label("Exiting edit mode...\n", UIUtility.uiStyleLabelMedium);
                else
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Press J while pointing at a\nprocedural part to edit it", UIUtility.uiStyleLabelHint);
                    if (GUILayout.Button("Close", UIUtility.uiStyleButton, GUILayout.MaxWidth(50f)))
                    {
                        uiWindowActive = false;
                        //stockButton.SetFalse(false);
                        uiAdjustWindow = true;
                        EditorLogic.fetch.Unlock("WingProceduralWindow");
                    }
                    GUILayout.EndHorizontal();
                }
            }
            UnityEngine.GUI.DragWindow();
        }

        /// <summary>
        /// returns a string containing an abreviation of the current fuels and the number of units of each. eg LFO (360/420)
        /// </summary>
        private string FuelGUIGetConfigDesc()
        {
            if (wing.fuelSelectedTankSetup == -1 || StaticWingGlobals.wingTankConfigurations.Count == 0)
                return "Invalid";
            else
            {
                string units = StaticWingGlobals.wingTankConfigurations[wing.fuelSelectedTankSetup].ConfigurationName + " (";
                if (StaticWingGlobals.wingTankConfigurations[wing.fuelSelectedTankSetup].resources.Count != 0)
                {
                    foreach (var kvp in StaticWingGlobals.wingTankConfigurations[wing.fuelSelectedTankSetup].resources)
                    {
                        units += " " + (1000 * kvp.Value.fraction * wing.fuelVolume).ToString("G3") + " /";
                    }
                    units = units.Substring(0, units.Length - 1);
                }
                return units + ")";
            }
        }

        private int GetFieldMode()
        {
            if (!isCtrlSrf) return 1;
            else return 2;
        }

        private float SetupFieldValue(float value, Vector2 limits, float defaultValue)
        {
            return defaultValue;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="field">the value to draw</param>
        /// <param name="increment">mouse drag increment</param>
        /// <param name="incrementLarge">button increment</param>
        /// <param name="limits">min/max value</param>
        /// <param name="name">the field name to display</param>
        /// <param name="hsbColor">field colour</param>
        /// <param name="fieldID">tooltip stuff</param>
        /// <param name="fieldType">tooltip stuff</param>
        /// <param name="allowFine">Whether right click drag behaves as fine control or not</param>
        private double DrawField(double field, float increment, float incrementLarge, Vector2 limits, string name, Vector4 hsbColor, int fieldID, int fieldType, bool allowFine = true)
        {
            field = UIUtility.FieldSlider((float)field, increment, incrementLarge, limits, name, out bool changed, ColorHSBToRGB(hsbColor), fieldType, allowFine);
            if (changed)
            {
                uiLastFieldName = name;
                uiLastFieldTooltip = UpdateTooltipText(fieldID);
            }
            return field;
        }

        private void DrawFieldGroupHeader(ref bool fieldGroupBoolStatic, string header)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(header, UIUtility.uiStyleLabelHint))
            {
                fieldGroupBoolStatic = !fieldGroupBoolStatic;
                uiAdjustWindow = true;
            }
            if (fieldGroupBoolStatic) GUILayout.Label("|", UIUtility.uiStyleLabelHint, GUILayout.MaxWidth(15f));
            else GUILayout.Label("+", UIUtility.uiStyleLabelHint, GUILayout.MaxWidth(15f));
            GUILayout.EndHorizontal();
        }

        private static string uiLastFieldName = "";
        private static string uiLastFieldTooltip = "Additional info on edited \nproperties is displayed here";

        private string UpdateTooltipText(int fieldID)
        {
            // Base descriptions
            if (fieldID == 0) // sharedBaseLength))
            {
                if (!isCtrlSrf) return "Lateral measurement of the wing, \nalso referred to as semispan";
                else return "Lateral measurement of the control \nsurface at it's root";
            }
            else if (fieldID == 1) // sharedBaseWidthRoot))
            {
                if (!isCtrlSrf) return "Longitudinal measurement of the wing \nat the root cross section";
                else return "Longitudinal measurement of \nthe root chord";
            }
            else if (fieldID == 2) // sharedBaseWidthTip))
            {
                if (!isCtrlSrf) return "Longitudinal measurement of the wing \nat the tip cross section";
                else return "Longitudinal measurement of \nthe tip chord";
            }
            else if (fieldID == 3) // sharedBaseOffsetRoot))
            {
                if (!isCtrlSrf) return "This property shouldn't be accessible \non a wing";
                else return "Offset of the trailing edge \nroot corner on the lateral axis";
            }
            else if (fieldID == 4) // sharedBaseOffsetTip))
            {
                if (!isCtrlSrf) return "Distance between midpoints of the cross \nsections on the longitudinal axis";
                else return "Offset of the trailing edge \ntip corner on the lateral axis";
            }
            else if (fieldID == 5) // sharedBaseThicknessRoot))
            {
                if (!isCtrlSrf) return "Thickness at the root cross section \nUsually kept proportional to edge width";
                else return "Thickness at the root cross section \nUsually kept proportional to edge width";
            }
            else if (fieldID == 6) // sharedBaseThicknessTip))
            {
                if (!isCtrlSrf) return "Thickness at the tip cross section \nUsually kept proportional to edge width";
                else return "Thickness at the tip cross section \nUsually kept proportional to edge width";
            }

            // Edge descriptions
            else if (fieldID == 7) // sharedEdgeTypeTrailing))
            {
                if (!isCtrlSrf) return "Shape of the trailing edge cross \nsection (round/biconvex/sharp)";
                else return "Shape of the trailing edge cross \nsection (round/biconvex/sharp)";
            }
            else if (fieldID == 8) // sharedEdgeWidthTrailingRoot))
            {
                if (!isCtrlSrf) return "Longitudinal measurement of the trailing \nedge cross section at wing root";
                else return "Longitudinal measurement of the trailing \nedge cross section at with root";
            }
            else if (fieldID == 9) // sharedEdgeWidthTrailingTip))
            {
                if (!isCtrlSrf) return "Longitudinal measurement of the trailing \nedge cross section at wing tip";
                else return "Longitudinal measurement of the trailing \nedge cross section at with tip";
            }
            else if (fieldID == 10) // sharedEdgeTypeLeading))
            {
                if (!isCtrlSrf) return "Shape of the leading edge cross \nsection (round/biconvex/sharp)";
                else return "Shape of the leading edge cross \nsection (round/biconvex/sharp)";
            }
            else if (fieldID == 11) // sharedEdgeWidthLeadingRoot))
            {
                if (!isCtrlSrf) return "Longitudinal measurement of the leading \nedge cross section at wing root";
                else return "Longitudinal measurement of the leading \nedge cross section at wing root";
            }
            else if (fieldID == 12) // sharedEdgeWidthLeadingTip))
            {
                if (!isCtrlSrf) return "Longitudinal measurement of the leading \nedge cross section at with tip";
                else return "Longitudinal measurement of the leading \nedge cross section at with tip";
            }

            // Surface descriptions
            else if (fieldID == 13)
            {
                if (!isCtrlSrf) return "Surface material (uniform fill, plating, \nLRSI/HRSI tiles and so on)";
                else return "Surface material (uniform fill, plating, \nLRSI/HRSI tiles and so on)";
            }
            else if (fieldID == 14)
            {
                if (!isCtrlSrf) return "Fairly self-explanatory, controls the paint \nopacity: no paint at 0, full coverage at 1";
                else return "Fairly self-explanatory, controls the paint \nopacity: no paint at 0, full coverage at 1";
            }
            else if (fieldID == 15)
            {
                if (!isCtrlSrf) return "Controls the paint hue (HSB axis): \nvalues from zero to one make full circle";
                else return "Controls the paint hue (HSB axis): \nvalues from zero to one make full circle";
            }
            else if (fieldID == 16)
            {
                if (!isCtrlSrf) return "Controls the paint saturation (HSB axis): \ncolorless at 0, full color at 1";
                else return "Controls the paint saturation (HSB axis): \ncolorless at 0, full color at 1";
            }
            else if (fieldID == 17)
            {
                if (!isCtrlSrf) return "Controls the paint brightness (HSB axis): black at 0, white at 1, primary at 0.5";
                else return "Controls the paint brightness (HSB axis): black at 0, white at 1, primary at 0.5";
            }

            // This should not really happen
            else return "Unknown field\n";
        }

        private Color ColorHSBToRGB(Vector4 hsbColor)
        {
            float r = hsbColor.z;
            float g = hsbColor.z;
            float b = hsbColor.z;
            if (hsbColor.y != 0)
            {
                float max = hsbColor.z;
                float dif = hsbColor.z * hsbColor.y;
                float min = hsbColor.z - dif;
                float h = hsbColor.x * 360f;
                if (h < 60f)
                {
                    r = max;
                    g = h * dif / 60f + min;
                    b = min;
                }
                else if (h < 120f)
                {
                    r = -(h - 120f) * dif / 60f + min;
                    g = max;
                    b = min;
                }
                else if (h < 180f)
                {
                    r = min;
                    g = max;
                    b = (h - 120f) * dif / 60f + min;
                }
                else if (h < 240f)
                {
                    r = min;
                    g = -(h - 240f) * dif / 60f + min;
                    b = max;
                }
                else if (h < 300f)
                {
                    r = (h - 240f) * dif / 60f + min;
                    g = min;
                    b = max;
                }
                else if (h <= 360f)
                {
                    r = max;
                    g = min;
                    b = -(h - 360f) * dif / 60 + min;
                }
                else
                {
                    r = 0;
                    g = 0;
                    b = 0;
                }
            }
            return new Color(Mathf.Clamp01(r), Mathf.Clamp01(g), Mathf.Clamp01(b), hsbColor.w);
        }


        private float GetIncrementFromType(float incrementWing, float incrementCtrl)
        {
            if (!isCtrlSrf)
                return incrementWing;
            else
                return incrementCtrl;
        }

        private Vector2 GetLimitsFromType(Vector4 set)
        {
            if (!isCtrlSrf)
                return new Vector2(set.x, set.y);
            else
                return new Vector2(set.z, set.w);
        }

        private float GetDefault(Vector4 set)
        {
            if (!isCtrlSrf) return set.x;
            else return set.y;
        }
    }
}
