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
    class WingProcedural : PartModule, IDeprecatedWingModule
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

        public void UpgradeModule(Base_ProceduralWing newModule)
        {
            Debug.Log($"Upgrading B9_PWing");
            B9_ProceduralWing newModule_B9 = (B9_ProceduralWing)newModule;
            // assign all the variables
            newModule_B9.Length = sharedBaseLength;
            newModule_B9.TipOffset = sharedBaseOffsetTip;
            newModule_B9.RootWidth = sharedBaseWidthRoot;
            newModule_B9.TipWidth = sharedBaseWidthTip;
            newModule_B9.RootThickness = sharedBaseThicknessRoot;
            newModule_B9.TipThickness = sharedBaseThicknessTip;

            if (!(newModule_B9 is B9_ProceduralControl))
            {
                newModule_B9.LeadingEdgeType = (int)sharedEdgeTypeLeading;
                newModule_B9.RootLeadingEdge = sharedEdgeWidthLeadingRoot;
                newModule_B9.TipLeadingEdge = sharedEdgeWidthLeadingTip;
            }

            newModule_B9.TrailingEdgeType = (int)sharedEdgeTypeTrailing;
            newModule_B9.RootTrailingEdge = sharedEdgeWidthTrailingRoot;
            newModule_B9.RootTrailingEdge = sharedEdgeWidthTrailingTip;

            newModule_B9.SurfTopMat = (int)sharedMaterialST;
            newModule_B9.SurfTopOpacity = sharedColorSTOpacity;
            newModule_B9.SurfTopHue = sharedColorSTHue;
            newModule_B9.SurfTopSat = sharedColorSTSaturation;
            newModule_B9.SurfTopBright = sharedColorSTBrightness;


            newModule_B9.SurfBottomMat = (int)sharedMaterialSB;
            newModule_B9.SurfBottomOpacity = sharedColorSBOpacity;
            newModule_B9.SurfBottomHue = sharedColorSBHue;
            newModule_B9.SurfBottomSat = sharedColorSBSaturation;
            newModule_B9.SurfBottomBright = sharedColorSBBrightness;

            if (!(newModule_B9 is B9_ProceduralControl))
            {
                newModule_B9.SurfLeadMat = (int)sharedMaterialEL;
                newModule_B9.SurfLeadOpacity = sharedColorELOpacity;
                newModule_B9.SurfLeadHue = sharedColorELHue;
                newModule_B9.SurfLeadSat = sharedColorELSaturation;
                newModule_B9.SurfLeadBright = sharedColorELBrightness;
            }

            newModule_B9.SurfTrailMat = (int)sharedMaterialET;
            newModule_B9.SurfTrailOpacity = sharedColorETOpacity;
            newModule_B9.SurfTrailHue = sharedColorETHue;
            newModule_B9.SurfTrailSat = sharedColorETSaturation;
            newModule_B9.SurfTrailBright = sharedColorETBrightness;

            newModule_B9.fuelSelectedTankSetup = fuelSelectedTankSetup;

            if (newModule_B9 is B9_ProceduralControl)
            {
                ((B9_ProceduralControl)newModule_B9).RootOffset = sharedBaseOffsetRoot;
            }
        }
    }
}
