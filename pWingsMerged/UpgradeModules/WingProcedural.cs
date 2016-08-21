using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ProceduralWings.UpgradeModules
{
    using B9PWing;

    /// <summary>
    /// A stub class that takes the saved values from old saves and recreates the new module as appropriate to upgrade vessels
    /// </summary>
    class WingProcedural : PartModule
    {
        [KSPField(isPersistant = true)]
        public float sharedBaseLength = 4f;

        [KSPField(isPersistant = true)]
        public float sharedBaseWidthRoot = 4f;

        [KSPField(isPersistant = true)]
        public float sharedBaseWidthTip = 4f;

        [KSPField(isPersistant = true)]
        public float sharedBaseOffsetRoot = 0f;

        [KSPField(isPersistant = true)]
        public float sharedBaseOffsetTip = 0f;

        [KSPField(isPersistant = true)]
        public float sharedBaseThicknessRoot = 0.24f;

        [KSPField(isPersistant = true)]
        public float sharedBaseThicknessTip = 0.24f;

        [KSPField(isPersistant = true)]
        public float sharedEdgeTypeLeading = 2f;

        [KSPField(isPersistant = true)]
        public float sharedEdgeWidthLeadingRoot = 0.24f;

        [KSPField(isPersistant = true)]
        public float sharedEdgeWidthLeadingTip = 0.24f;

        [KSPField(isPersistant = true)]
        public float sharedEdgeTypeTrailing = 3f;

        [KSPField(isPersistant = true)]
        public float sharedEdgeWidthTrailingRoot = 0.48f;

        [KSPField(isPersistant = true)]
        public float sharedEdgeWidthTrailingTip = 0.48f;

        [KSPField(isPersistant = true)]
        public float sharedMaterialST = 1f;

        [KSPField(isPersistant = true)]
        public float sharedColorSTOpacity = 0f;

        [KSPField(isPersistant = true)]
        public float sharedColorSTHue = 0.10f;

        [KSPField(isPersistant = true)]
        public float sharedColorSTSaturation = 0.75f;

        [KSPField(isPersistant = true)]
        public float sharedColorSTBrightness = 0.6f;

        [KSPField(isPersistant = true)]
        public float sharedMaterialSB = 4f;

        [KSPField(isPersistant = true)]
        public float sharedColorSBOpacity = 0f;

        [KSPField(isPersistant = true)]
        public float sharedColorSBHue = 0.10f;

        [KSPField(isPersistant = true)]
        public float sharedColorSBSaturation = 0.75f;

        [KSPField(isPersistant = true)]
        public float sharedColorSBBrightness = 0.6f;

        [KSPField(isPersistant = true)]
        public float sharedMaterialET = 4f;

        [KSPField(isPersistant = true)]
        public float sharedColorETOpacity = 0f;

        [KSPField(isPersistant = true)]
        public float sharedColorETHue = 0.10f;

        [KSPField(isPersistant = true)]
        public float sharedColorETSaturation = 0.75f;

        [KSPField(isPersistant = true)]
        public float sharedColorETBrightness = 0.6f;

        [KSPField(isPersistant = true)]
        public float sharedMaterialEL = 4f;

        [KSPField(isPersistant = true)]
        public float sharedColorELOpacity = 0f;

        [KSPField(isPersistant = true)]
        public float sharedColorELHue = 0.10f;

        [KSPField(isPersistant = true)]
        public float sharedColorELSaturation = 0.75f;

        [KSPField(isPersistant = true)]
        public float sharedColorELBrightness = 0.6f;

        [KSPField(isPersistant = true)]
        public int fuelSelectedTankSetup = 0;

        public void Start()
        {
            B9_ProceduralWing wing = part.Modules.GetModule<B9_ProceduralWing>();
            // assign all the variables
            wing.Length = sharedBaseLength;
            wing.TipOffset = sharedBaseOffsetTip;
            wing.RootWidth = sharedBaseWidthRoot;
            wing.TipWidth = sharedBaseWidthTip;
            wing.RootThickness = sharedBaseThicknessRoot;
            wing.TipThickness = sharedBaseThicknessTip;

            wing.LeadingEdgeType = (int)sharedEdgeTypeLeading;
            wing.RootLeadingEdge = sharedEdgeWidthLeadingRoot;
            wing.TipLeadingEdge = sharedEdgeWidthLeadingTip;

            wing.TrailingEdgeType = (int)sharedEdgeTypeTrailing;
            wing.RootTrailingEdge = sharedEdgeWidthTrailingRoot;
            wing.RootLeadingEdge = sharedEdgeWidthTrailingTip;

            wing.SurfTopMat = (int)sharedMaterialST;
            wing.SurfTopOpacity = sharedColorSTOpacity;
            wing.SurfTopHue = sharedColorSTHue;
            wing.SurfTopSat = sharedColorSTSaturation;
            wing.SurfTopBright = sharedColorSTBrightness;


            wing.SurfBottomMat = (int)sharedMaterialSB;
            wing.SurfBottomOpacity = sharedColorSBOpacity;
            wing.SurfBottomHue = sharedColorSBHue;
            wing.SurfBottomSat = sharedColorSBSaturation;
            wing.SurfBottomBright = sharedColorSBBrightness;

            wing.SurfLeadMat = (int)sharedMaterialEL;
            wing.SurfLeadOpacity = sharedColorELOpacity;
            wing.SurfLeadHue = sharedColorELHue;
            wing.SurfLeadSat = sharedColorELSaturation;
            wing.SurfLeadBright = sharedColorELBrightness;

            wing.SurfTrailMat = (int)sharedMaterialET;
            wing.SurfTrailOpacity = sharedColorETOpacity;
            wing.SurfTrailHue = sharedColorETHue;
            wing.SurfTrailSat = sharedColorETSaturation;
            wing.SurfTrailBright = sharedColorETBrightness;

            if (wing is B9_ProceduralControl)
            {
                ((B9_ProceduralControl)wing).RootOffset = sharedBaseOffsetRoot;
            }

            wing.fuelSelectedTankSetup = fuelSelectedTankSetup;
        }
    }
}
