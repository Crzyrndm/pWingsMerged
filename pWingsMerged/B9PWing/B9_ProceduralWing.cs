using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralWings.B9PWing
{
    using UI;
    using Utility;
    public class B9_ProceduralWing : Base_ProceduralWing
    {
        #region physical dimensions
        public bool isMirrored;

        public override double MAC
        {
            get
            {
                return Length * (TipWidth + TipLeadingEdge + TipTrailingEdge + RootWidth + RootLeadingEdge + RootTrailingEdge) / 2;
            }
        }

        #region Shared properties / Edge / Leading

        protected WingProperty leadingEdgeType;
        public int LeadingEdgeType
        {
            get { return (int)leadingEdgeType.Value; }
            set
            {
                leadingEdgeType.Value = value;
                UpdateSymmetricGeometry();
            }
        }

        protected WingProperty rootLeadingEdge;
        public double RootLeadingEdge
        {
            get { return LeadingEdgeType != 0 ? rootLeadingEdge.Value : 0; }
            set
            {
                rootLeadingEdge.Value = value;
                UpdateSymmetricGeometry();
            }
        }

        protected WingProperty tipLeadingEdge;
        public double TipLeadingEdge
        {
            get { return LeadingEdgeType != 0 ? tipLeadingEdge.Value : 0; }
            set
            {
                tipLeadingEdge.Value = value;
                UpdateSymmetricGeometry();
            }
        }
        #endregion

        #region Shared properties / Edge / Trailing

        protected WingProperty trailingEdgeType;
        public int TrailingEdgeType
        {
            get { return (int)trailingEdgeType.Value; }
            set
            {
                trailingEdgeType.Value = value;
                UpdateSymmetricGeometry();
            }
        }

        protected WingProperty rootTrailingEdge;
        public double RootTrailingEdge
        {
            get { return TrailingEdgeType != 0 ? rootTrailingEdge.Value : 0; }
            set
            {
                rootTrailingEdge.Value = value;
                UpdateSymmetricGeometry();
            }
        }

        protected WingProperty tipTrailingEdge;
        public double TipTrailingEdge
        {
            get { return TrailingEdgeType != 0 ? tipTrailingEdge.Value : 0; }
            set
            {
                tipTrailingEdge.Value = value;
                UpdateSymmetricGeometry();
            }
        }

        #endregion

        #region Shared properties / Surface / Top

        protected WingProperty surfTopMat;
        public int SurfTopMat
        {
            get { return (int)surfTopMat.Value; }
            set
            {
                surfTopMat.Value = value;
                UpdateSymmetricAppearance();
            }
        }

        protected WingProperty surfTopOpacity;
        public double SurfTopOpacity
        {
            get { return surfTopOpacity.Value; }
            set
            {
                surfTopOpacity.Value = value;
                UpdateSymmetricAppearance();
            }
        }

        protected WingProperty surfTopHue;
        public double SurfTopHue
        {
            get { return surfTopHue.Value; }
            set
            {
                surfTopHue.Value = value;
                UpdateSymmetricAppearance();
            }
        }

        protected WingProperty surfTopSat;
        public double SurfTopSat
        {
            get { return surfTopSat.Value; }
            set
            {
                surfTopSat.Value = value;
                UpdateSymmetricAppearance();
            }
        }

        protected WingProperty surfTopBright;
        public double SurfTopBright
        {
            get { return surfTopBright.Value; }
            set
            {
                surfTopBright.Value = value;
                UpdateSymmetricAppearance();
            }
        }

        #endregion

        #region Shared properties / Surface / bottom
        protected WingProperty surfBottomMat;
        public int SurfBottomMat
        {
            get { return (int)surfBottomMat.Value; }
            set
            {
                surfBottomMat.Value = value;
                UpdateSymmetricAppearance();
            }
        }

        protected WingProperty surfBottomOpacity;
        public double SurfBottomOpacity
        {
            get { return surfBottomOpacity.Value; }
            set
            {
                surfBottomOpacity.Value = value;
                UpdateSymmetricAppearance();
            }
        }

        protected WingProperty surfBottomHue;
        public double SurfBottomHue
        {
            get { return surfBottomHue.Value; }
            set
            {
                surfBottomHue.Value = value;
                UpdateSymmetricAppearance();
            }
        }

        protected WingProperty surfBottomSat;
        public double SurfBottomSat
        {
            get { return surfBottomSat.Value; }
            set
            {
                surfBottomSat.Value = value;
            }
        }

        protected WingProperty surfBottomBright;
        public double SurfBottomBright
        {
            get { return surfBottomBright.Value; }
            set
            {
                surfBottomBright.Value = value;
                UpdateSymmetricAppearance();
            }
        }
        #endregion

        #region Shared properties / Surface / trailing edge

        protected WingProperty surfTrailMat;
        public int SurfTrailMat
        {
            get { return (int)surfTrailMat.Value; }
            set
            {
                surfTrailMat.Value = value;
                UpdateSymmetricAppearance();
            }
        }

        protected WingProperty surfTrailOpacity;
        public double SurfTrailOpacity
        {
            get { return surfTrailOpacity.Value; }
            set
            {
                surfTrailOpacity.Value = value;
                UpdateSymmetricAppearance();
            }
        }

        protected WingProperty surfTrailHue;
        public double SurfTrailHue
        {
            get { return surfTrailHue.Value; }
            set
            {
                surfTrailHue.Value = value;
                UpdateSymmetricAppearance();
            }
        }

        protected WingProperty surfTrailSat;
        public double SurfTrailSat
        {
            get { return surfTrailSat.Value; }
            set
            {
                surfTrailSat.Value = value;
                UpdateSymmetricAppearance();
            }
        }

        protected WingProperty surfTrailBright;
        public double SurfTrailBright
        {
            get { return surfTrailBright.Value; }
            set
            {
                surfTrailBright.Value = value;
                UpdateSymmetricAppearance();
            }
        }

        #endregion

        #region Shared properties / Surface / leading edge
        protected WingProperty surfLeadMat;
        public int SurfLeadMat
        {
            get { return (int)surfLeadMat.Value; }
            set
            {
                surfLeadMat.Value = value;
                UpdateSymmetricAppearance();
            }
        }

        protected WingProperty surfLeadOpacity;
        public double SurfLeadOpacity
        {
            get { return surfLeadOpacity.Value; }
            set
            {
                surfLeadOpacity.Value = value;
                UpdateSymmetricAppearance();
            }
        }

        protected WingProperty surfLeadHue;
        public double SurfLeadHue
        {
            get { return surfLeadHue.Value; }
            set
            {
                surfLeadHue.Value = value;
                UpdateSymmetricAppearance();
            }
        }

        protected WingProperty surfLeadSat;
        public double SurfLeadSat
        {
            get { return surfLeadSat.Value; }
            set
            {
                surfLeadSat.Value = value;
                UpdateSymmetricAppearance();
            }
        }

        protected WingProperty surfLeadBright;
        public double SurfLeadBright
        {
            get { return surfLeadBright.Value; }
            set
            {
                surfLeadBright.Value = value;
                UpdateSymmetricAppearance();
            }
        }

        #endregion

        #endregion

        #region Unity stuff and Callbacks/events

        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
            node.AddValue("mirrorTexturing", isMirrored);

            try
            {
                leadingEdgeType.Save(node);
                rootLeadingEdge.Save(node);
                tipLeadingEdge.Save(node);
                trailingEdgeType.Save(node);
                rootTrailingEdge.Save(node);
                tipTrailingEdge.Save(node);
                surfTopMat.Save(node);
                surfTopOpacity.Save(node);
                surfTopHue.Save(node);
                surfTopSat.Save(node);
                surfTopBright.Save(node);
                surfBottomMat.Save(node);
                surfBottomOpacity.Save(node);
                surfBottomHue.Save(node);
                surfBottomSat.Save(node);
                surfBottomBright.Save(node);
                surfLeadMat.Save(node);
                surfLeadOpacity.Save(node);
                surfLeadHue.Save(node);
                surfLeadSat.Save(node);
                surfLeadBright.Save(node);
                surfTrailMat.Save(node);
                surfTrailOpacity.Save(node);
                surfTrailHue.Save(node);
                surfTrailSat.Save(node);
                surfTrailBright.Save(node);
            }
            catch
            {
                Log("failed to save properties");
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            node.TryGetValue("mirrorTexturing", ref isMirrored);
        }

        #endregion

        #region Setting up

        public override void SetupProperties()
        {
            if (length != null)
                return;
            if (part.symmetryCounterparts.Count == 0 || part.symmetryCounterparts[0].Modules.GetModule<B9_ProceduralWing>().length == null)
            {
                length = new WingProperty("Length", nameof(length), 4, 2, 0.05, 16, "Lateral measurement of the wing, \nalso referred to as semispan");
                tipOffset = new WingProperty("Offset (tip)", nameof(tipOffset), 0, 2, -8, 8, "Distance between midpoints of the cross \nsections on the longitudinal axis");
                rootWidth = new WingProperty("Width (root)", nameof(rootWidth), 4, 2, 0.05, 16, "Longitudinal measurement of the wing \nat the root cross section");
                tipWidth = new WingProperty("Width (tip)", nameof(tipWidth), 4, 2, 0.05, 16, "Longitudinal measurement of the wing \nat the tip cross section");
                rootThickness = new WingProperty("Thickness (root)", nameof(rootThickness), 0.24, 2, 0.01, 1, "Thickness at the root cross section \nUsually kept proportional to edge width");
                tipThickness = new WingProperty("Thickness (tip)", nameof(tipThickness), 0.24, 2, 0.01, 1, "Thickness at the tip cross section \nUsually kept proportional to edge width");

                leadingEdgeType = new WingProperty("Shape", nameof(leadingEdgeType), 2, 0, 1, 4, "Shape of the leading edge cross \nsection (round/biconvex/sharp)");
                rootLeadingEdge = new WingProperty("Width (root)", nameof(rootLeadingEdge), 0.24, 2, 0.01, 1.0, "Longitudinal measurement of the leading \nedge cross section at wing root");
                tipLeadingEdge = new WingProperty("Width (tip)", nameof(tipLeadingEdge), 0.24, 2, 0.01, 1.0, "Longitudinal measurement of the leading \nedge cross section at with tip");

                trailingEdgeType = new WingProperty("Shape", nameof(trailingEdgeType), 3, 0, 1, 4, "Shape of the trailing edge cross \nsection (round/biconvex/sharp)");
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

                surfLeadMat = new WingProperty("Material", nameof(surfLeadMat), 4, 0, 0, 4, "Surface material (uniform fill, plating, \nLRSI/HRSI tiles and so on)");
                surfLeadOpacity = new WingProperty("Opacity", nameof(surfLeadOpacity), 0, 2, 0, 1, "Fairly self-explanatory, controls the paint \nopacity: no paint at 0, full coverage at 1");
                surfLeadHue = new WingProperty("Hue", nameof(surfLeadHue), 0.1, 2, 0, 1, "Controls the paint hue (HSB axis): \nvalues from zero to one make full circle");
                surfLeadSat = new WingProperty("Saturation", nameof(surfLeadSat), 0.75, 2, 0, 1, "Controls the paint saturation (HSB axis): \ncolorless at 0, full color at 1");
                surfLeadBright = new WingProperty("Brightness", nameof(surfLeadBright), 0.6, 2, 0, 1, "Controls the paint brightness (HSB axis): black at 0, white at 1, primary at 0.5");

                surfTrailMat = new WingProperty("Material", nameof(surfTrailMat), 4, 0, 0, 4, "Surface material (uniform fill, plating, \nLRSI/HRSI tiles and so on)");
                surfTrailOpacity = new WingProperty("Opacity", nameof(surfTrailOpacity), 0, 2, 0, 1, "Fairly self-explanatory, controls the paint \nopacity: no paint at 0, full coverage at 1");
                surfTrailHue = new WingProperty("Hue", nameof(surfTrailHue), 0.1, 2, 0, 1, "Controls the paint hue (HSB axis): \nvalues from zero to one make full circle");
                surfTrailSat = new WingProperty("Saturation", nameof(surfTrailSat), 0.75, 2, 0, 1, "Controls the paint saturation (HSB axis): \ncolorless at 0, full color at 1");
                surfTrailBright = new WingProperty("Brightness", nameof(surfTrailBright), 0.6, 2, 0, 1, "Controls the paint brightness (HSB axis): black at 0, white at 1, primary at 0.5");
            }
            else
            {
                B9_ProceduralWing pw = part.symmetryCounterparts[0].Modules.GetModule<B9_ProceduralWing>(); // all properties for symmetry will be the same object. Yay for no need to update values :D

                length = pw.length;
                tipOffset = pw.tipOffset;
                rootWidth = pw.rootWidth;
                tipWidth = pw.tipWidth;
                rootThickness = pw.rootThickness;
                tipThickness = pw.tipThickness;

                leadingEdgeType = pw.leadingEdgeType;
                rootLeadingEdge = pw.rootLeadingEdge;
                tipLeadingEdge = pw.tipLeadingEdge;

                trailingEdgeType = pw.trailingEdgeType;
                rootTrailingEdge = pw.rootTrailingEdge;
                tipTrailingEdge = pw.tipTrailingEdge;

                surfTopMat = pw.surfTopMat;
                surfTopOpacity = pw.surfTopOpacity;
                surfTopHue = pw.surfTopHue;
                surfTopSat = pw.surfTopSat;
                surfTopBright = pw.surfTopBright;

                surfBottomMat = pw.surfBottomMat;
                surfBottomOpacity = pw.surfBottomOpacity;
                surfBottomHue = pw.surfBottomHue;
                surfBottomSat = pw.surfBottomSat;
                surfBottomBright = pw.surfBottomBright;

                surfLeadMat = pw.surfLeadMat;
                surfLeadOpacity = pw.surfLeadOpacity;
                surfLeadHue = pw.surfLeadHue;
                surfLeadSat = pw.surfLeadSat;
                surfLeadBright = pw.surfLeadBright;

                surfTrailMat = pw.surfTrailMat;
                surfTrailOpacity = pw.surfTrailOpacity;
                surfTrailHue = pw.surfTrailHue;
                surfTrailSat = pw.surfTrailSat;
                surfTrailBright = pw.surfTrailBright;
            }
        }

        public override void SetupGeometryAndAppearance()
        {
            SetupMeshFilters();
            SetupMeshReferences();
            UpdateMaterials();
        }

        public override void LoadWingProperty(ConfigNode n)
        {
            switch (n.GetValue("ID"))
            {
                case nameof(leadingEdgeType):
                    leadingEdgeType.Load(n);
                    break;
                case nameof(rootLeadingEdge):
                    rootLeadingEdge.Load(n);
                    break;
                case nameof(tipLeadingEdge):
                    tipLeadingEdge.Load(n);
                    break;
                case nameof(trailingEdgeType):
                    trailingEdgeType.Load(n);
                    break;
                case nameof(rootTrailingEdge):
                    rootTrailingEdge.Load(n);
                    break;
                case nameof(tipTrailingEdge):
                    tipTrailingEdge.Load(n);
                    break;
                case nameof(surfTopMat):
                    surfTopMat.Load(n);
                    break;
                case nameof(surfTopOpacity):
                    surfTopOpacity.Load(n);
                    break;
                case nameof(surfTopHue):
                    surfTopHue.Load(n);
                    break;
                case nameof(surfTopSat):
                    surfTopSat.Load(n);
                    break;
                case nameof(surfTopBright):
                    surfTopBright.Load(n);
                    break;
                case nameof(surfBottomMat):
                    surfBottomMat.Load(n);
                    break;
                case nameof(surfBottomOpacity):
                    surfBottomOpacity.Load(n);
                    break;
                case nameof(surfBottomHue):
                    surfBottomHue.Load(n);
                    break;
                case nameof(surfBottomSat):
                    surfBottomSat.Load(n);
                    break;
                case nameof(surfBottomBright):
                    surfBottomBright.Load(n);
                    break;
                case nameof(surfLeadMat):
                    surfLeadMat.Load(n);
                    break;
                case nameof(surfLeadOpacity):
                    surfLeadOpacity.Load(n);
                    break;
                case nameof(surfLeadHue):
                    surfLeadHue.Load(n);
                    break;
                case nameof(surfLeadSat):
                    surfLeadSat.Load(n);
                    break;
                case nameof(surfLeadBright):
                    surfLeadBright.Load(n);
                    break;
                case nameof(surfTrailMat):
                    surfTrailMat.Load(n);
                    break;
                case nameof(surfTrailOpacity):
                    surfTrailOpacity.Load(n);
                    break;
                case nameof(surfTrailHue):
                    surfTrailHue.Load(n);
                    break;
                case nameof(surfTrailSat):
                    surfTrailSat.Load(n);
                    break;
                case nameof(surfTrailBright):
                    surfTrailBright.Load(n);
                    break;
                default:
                    base.LoadWingProperty(n);
                    break;
            }
        }

        #endregion

        #region Inheritance
        public bool inheritancePossibleOnShape = false;
        public bool inheritancePossibleOnMaterials = false;
        public virtual void InheritanceStatusUpdate()
        {
            if (this.part.parent == null)
                return;

            Base_ProceduralWing parentModule = part.parent.Modules.GetModule<Base_ProceduralWing>();
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

            Base_ProceduralWing parentModule = part.parent.Modules.GetModule<Base_ProceduralWing>();
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

            if (TipWidth < tipWidth.min)
                Length *= (RootWidth - tipWidth.min) / (RootWidth - TipWidth);
            else if (TipWidth > tipWidth.max)
                Length *= tipWidth.max / TipWidth;

            if (TipOffset > tipOffset.max)
                Length *= tipOffset.max / TipOffset;
            else if (TipOffset < tipOffset.min)
                Length *= tipOffset.min / TipOffset;

            Length = Utils.Clamp(Length, length.min, length.max);
            TipWidth = Utils.Clamp(TipWidth, tipWidth.min, tipWidth.max);
            TipOffset = Utils.Clamp(TipOffset, tipOffset.min, tipOffset.max);
            TipThickness = Utils.Clamp(RootThickness + Length / parent.Length * (parent.TipThickness - parent.RootThickness), tipThickness.min, tipThickness.max);
        }

        public override void inheritBase(Base_ProceduralWing parent)
        {
            base.inheritBase(parent);

            B9_ProceduralWing wing = parent as B9_ProceduralWing;
            if (wing == null)
                return;
            LeadingEdgeType = wing.LeadingEdgeType;
            RootLeadingEdge = wing.TipLeadingEdge;

            TrailingEdgeType = wing.TrailingEdgeType;
            RootTrailingEdge = wing.TipTrailingEdge;
        }

        public virtual void inheritEdges(Base_ProceduralWing parent)
        {
            B9_ProceduralWing wing = parent as B9_ProceduralWing;
            if (wing == null)
                return;

            LeadingEdgeType = wing.LeadingEdgeType;
            RootLeadingEdge = wing.TipLeadingEdge;
            TipLeadingEdge = Utils.Clamp(RootLeadingEdge + ((wing.TipLeadingEdge - wing.RootLeadingEdge) / wing.Length) * Length, tipLeadingEdge.min, tipLeadingEdge.max);

            TrailingEdgeType = wing.TrailingEdgeType;
            RootTrailingEdge = wing.TipTrailingEdge;
            TipTrailingEdge = Utils.Clamp(RootTrailingEdge + ((wing.TipTrailingEdge - wing.RootTrailingEdge) / wing.Length) * Length, tipTrailingEdge.min, tipTrailingEdge.max);
        }

        public virtual void inheritColours(Base_ProceduralWing parent)
        {
            B9_ProceduralWing wing = parent as B9_ProceduralWing;
            if (wing == null)
                return;

            SurfTopMat = wing.SurfTopMat;
            SurfTopOpacity = wing.SurfTopOpacity;
            SurfTopHue = wing.SurfTopHue;
            SurfTopSat = wing.SurfTopSat;
            SurfTopBright = wing.SurfTopBright;

            SurfBottomMat = wing.SurfBottomMat;
            SurfBottomOpacity = wing.SurfBottomOpacity;
            SurfBottomHue = wing.SurfBottomHue;
            SurfBottomSat = wing.SurfBottomSat;
            SurfBottomBright = wing.SurfBottomBright;

            SurfTrailMat = wing.SurfTrailMat;
            SurfTrailOpacity = wing.SurfTrailOpacity;
            SurfTrailHue = wing.SurfTrailHue;
            SurfTrailSat = wing.SurfTrailSat;
            SurfTrailBright = wing.SurfTrailBright;

            SurfLeadMat = wing.SurfLeadMat;
            SurfLeadOpacity = wing.SurfLeadOpacity;
            SurfLeadHue = wing.SurfLeadHue;
            SurfLeadSat = wing.SurfLeadSat;
            SurfLeadBright = wing.SurfLeadBright;
        }

        #endregion

        #region Geometry

        // Attachment handling
        public override void OnAttach()
        {
            base.OnAttach();
            isMirrored = Vector3.Dot(EditorLogic.SortedShipList[0].transform.forward, part.transform.forward) < 0;
        }

        public void UpdateOnEditorDetach()
        {
            if (this.part.parent != null)
            {
                B9_ProceduralWing parentModule = this.part.parent.Modules.GetModule<B9_ProceduralWing>();
                if (parentModule != null)
                {
                    parentModule.FuelUpdateVolume();
                    parentModule.CalculateAerodynamicValues();
                }
            }
        }

        public virtual void UpdateSymmetricAppearance()
        {
            UpdateGeometry(false);
            for (int i = part.symmetryCounterparts.Count - 1; i >= 0; --i)
            {
                part.symmetryCounterparts[i].Modules.GetModule<B9_ProceduralWing>().UpdateGeometry(false);
            }
        }

        public override void UpdateGeometry()
        {
            UpdateGeometry(true);
        }

        public virtual void UpdateGeometry(bool updateAerodynamics)
        {
            isMirrored = Vector3.Dot(HighLogic.LoadedSceneIsEditor ? EditorLogic.SortedShipList[0].transform.forward : vessel.rootPart.transform.forward, part.transform.forward) < 0;
            float wingThicknessDeviationRoot = (float)RootThickness / 0.24f;
            float wingThicknessDeviationTip = (float)TipThickness / 0.24f;
            float wingWidthTipBasedOffsetTrailing = (float)TipWidth / 2f + (float)TipOffset;
            float wingWidthTipBasedOffsetLeading = -(float)TipWidth / 2f + (float)TipOffset;
            float wingWidthRootBasedOffset = (float)RootWidth / 2f;

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
                            vp[i] = new Vector3(-(float)Length, vp[i].y * wingThicknessDeviationTip, wingWidthTipBasedOffsetLeading);
                            uv[i] = new Vector2((float)TipWidth, uv[i].y);
                        }
                        else
                        {
                            vp[i] = new Vector3(-(float)Length, vp[i].y * wingThicknessDeviationTip, wingWidthTipBasedOffsetTrailing);
                            uv[i] = new Vector2(0f, uv[i].y);
                        }
                    }
                    else
                    {
                        if (vp[i].z < 0f)
                        {
                            vp[i] = new Vector3(vp[i].x, vp[i].y * wingThicknessDeviationRoot, -wingWidthRootBasedOffset);
                            uv[i] = new Vector2((float)RootWidth, uv[i].y);
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
                            vp[i] = new Vector3(-(float)Length, vp[i].y * wingThicknessDeviationTip, wingWidthTipBasedOffsetLeading);
                            uv[i] = new Vector2((float)Length / 4f, 1f - 0.5f + (float)TipWidth / 8f - (float)TipOffset / 4f);
                        }
                        else
                        {
                            vp[i] = new Vector3(-(float)Length, vp[i].y * wingThicknessDeviationTip, wingWidthTipBasedOffsetTrailing);
                            uv[i] = new Vector2((float)Length / 4f, 0f + 0.5f - (float)TipWidth / 8f - (float)TipOffset / 4f);
                        }
                    }
                    else
                    {
                        if (vp[i].z < 0f)
                        {
                            vp[i] = new Vector3(vp[i].x, vp[i].y * wingThicknessDeviationRoot, -wingWidthRootBasedOffset);
                            uv[i] = new Vector2(0.0f, 1f - 0.5f + (float)RootWidth / 8f);
                        }
                        else
                        {
                            vp[i] = new Vector3(vp[i].x, vp[i].y * wingThicknessDeviationRoot, wingWidthRootBasedOffset);
                            uv[i] = new Vector2(0f, 0f + 0.5f - (float)RootWidth / 8f);
                        }
                    }

                    // Top/bottom filtering
                    if ((vp[i].y > 0f) ^ isMirrored)
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

            // Next, time for leading and trailing edges
            // Before modifying geometry, we have to show the correct objects for the current selection
            // As UI only works with floats, we have to cast selections into ints too

            int wingEdgeTypeTrailingInt = Mathf.RoundToInt(TrailingEdgeType - 1);
            int wingEdgeTypeLeadingInt = Mathf.RoundToInt(LeadingEdgeType - 1);

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

            float wingEdgeWidthLeadingRootDeviation = (float)RootLeadingEdge / 0.24f;
            float wingEdgeWidthLeadingTipDeviation = (float)TipLeadingEdge / 0.24f;

            float wingEdgeWidthTrailingRootDeviation = (float)RootTrailingEdge / 0.24f;
            float wingEdgeWidthTrailingTipDeviation = (float)TipTrailingEdge / 0.24f;

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
                        vp[i] = new Vector3(-(float)Length, vp[i].y * wingThicknessDeviationTip, vp[i].z * wingEdgeWidthTrailingTipDeviation + (float)TipWidth / 2f + (float)TipOffset); // Tip edge
                        if (nm[i].x == 0f) uv[i] = new Vector2((float)Length, uv[i].y);
                    }
                    else
                        vp[i] = new Vector3(0f, vp[i].y * wingThicknessDeviationRoot, vp[i].z * wingEdgeWidthTrailingRootDeviation + (float)RootWidth / 2f); // Root edge
                    if (nm[i].x == 0f)
                    {
                        cl[i] = TrailColour;
                        uv2[i] = GetVertexUV2(SurfTrailMat);
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
                        vp[i] = new Vector3(-(float)Length, vp[i].y * wingThicknessDeviationTip, vp[i].z * wingEdgeWidthLeadingTipDeviation + (float)TipWidth / 2f - (float)TipOffset); // Tip edge
                        if (nm[i].x == 0f)
                            uv[i] = new Vector2((float)Length, uv[i].y);
                    }
                    else
                        vp[i] = new Vector3(0f, vp[i].y * wingThicknessDeviationRoot, vp[i].z * wingEdgeWidthLeadingRootDeviation + (float)RootWidth / 2f); // Root edge
                    if (nm[i].x == 0f)
                    {
                        cl[i] = LeadColour;
                        uv2[i] = GetVertexUV2(SurfLeadMat);
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

        // Edge geometry
        public Vector3[] GetReferenceVertices(MeshFilter source)
        {
            return source?.mesh?.vertices;
        }

        #endregion

        #region Fuel
        public override void FuelUpdateVolume()
        {
            if (!HighLogic.LoadedSceneIsEditor)
                return;
            if (!CanBeFueled)
            {
                fuelVolume = 0;
                return;
            }

            fuelVolume = 0.7 * Length * (RootWidth + TipWidth) * (RootThickness + TipThickness) / 4; // MAC includes edges
            Fuel.WingTankConfiguration wtc = StaticWingGlobals.wingTankConfigurations[fuelSelectedTankSetup];
            for (int i = part.Resources.Count - 1; i >= 0; --i)
            {
                PartResource res = part.Resources[i];
                double fillPct = res.maxAmount > 0 ? res.amount / res.maxAmount : 1.0;


                res.maxAmount = 1000 * wtc.resources[res.resourceName].fraction * fuelVolume / wtc.resources[res.resourceName].resource.volume;
                res.amount = res.maxAmount * fillPct;
            }
            part.Resources.UpdateList();
        }
        #endregion

        #region Mesh
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

        public static MeshReference meshReferenceWingSection;
        public static MeshReference meshReferenceWingSurface;
        public static List<MeshReference> meshReferencesWingEdge = new List<MeshReference>();

        public virtual int meshTypeCountEdgeWing { get { return 4; } }

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
        
        public override void CalculateAerodynamicValues()
        {
            CheckAssemblies();

            double sharedWidthTipSum = TipWidth;
            double sharedWidthRootSum = RootWidth;

            double offset = 0;
            if (LeadingEdgeType != 1)
            {
                sharedWidthTipSum += TipLeadingEdge;
                sharedWidthRootSum += RootLeadingEdge;
                offset += 0.2 * (RootLeadingEdge + TipLeadingEdge);
            }
            if (TrailingEdgeType != 1)
            {
                sharedWidthTipSum += TipTrailingEdge;
                sharedWidthRootSum += RootTrailingEdge;
                offset -= 0.25 * (TipTrailingEdge + RootTrailingEdge);
            }
            Vector3 midChordOffset = offset * Vector3d.up;

            double ctrlOffsetRootLimit = (Length / 2f) / (RootWidth + RootTrailingEdge);
            double ctrlOffsetTipLimit = (Length / 2f) / (TipWidth + TipTrailingEdge);

            double ctrlOffsetRootClamped = Utils.Clamp(RootWidth, -ctrlOffsetRootLimit, ctrlOffsetRootLimit);
            double ctrlOffsetTipClamped = Utils.Clamp(TipWidth, -ctrlOffsetTipLimit, ctrlOffsetTipLimit);

            // Base four values
            double taperRatio = sharedWidthTipSum / sharedWidthRootSum;
            double midChordSweep = Math.Atan(TipWidth / Length) * Utils.Rad2Deg;

            // Derived values

            double aspectRatio = 2.0f * Length / MAC;

            ArSweepScale = Math.Pow(aspectRatio / Math.Cos(Utils.Deg2Rad * midChordSweep), 2.0f) + 4.0f;
            ArSweepScale = 2.0f + Math.Sqrt(ArSweepScale);
            ArSweepScale = (2.0f * Math.PI) / ArSweepScale * aspectRatio;

            wingMass = Utils.Clamp(massFudgeNumber * MAC * Length * ((ArSweepScale * 2.0) / (3.0 + ArSweepScale)) * ((1.0 + taperRatio) / 2), 0.01, double.MaxValue);
            Cd = dragBaseValue / ArSweepScale * dragMultiplier;
            Cl = liftFudgeNumber * MAC * Length * ArSweepScale;
            GatherChildrenCl();
            connectionForce = Math.Round(Utils.Clamp(Math.Sqrt(Cl + ChildrenCl) * connectionFactor, connectionMinimum, double.MaxValue));

            // Shared parameters

            updateCost();

            part.breakingForce = Mathf.Round((float)connectionForce);
            part.breakingTorque = Mathf.Round((float)connectionForce);

            // Stock-only values
            if (!FARactive)
            {
                SetStockModuleParams();
            }
            else
            {
                setFARModuleParams(midChordSweep, taperRatio, midChordOffset);
            }
            
            StartCoroutine(updateAeroDelayed());
        }
        #endregion

        #region UI Stuff

        public static float sharedIncrementColor = 0.01f;
        public static float sharedIncrementColorLarge = 0.10f;
        public static float sharedIncrementMain = 0.125f;
        public static float sharedIncrementSmall = 0.04f;
        public static float sharedIncrementInt = 1f;

        public static Vector4 sharedBaseLengthDefaults = new Vector4(4f, 1f, 4f, 1f);
        public static Vector4 sharedBaseWidthRootDefaults = new Vector4(4f, 0.5f, 4f, 0.5f);
        public static Vector4 sharedBaseWidthTipDefaults = new Vector4(4f, 0.5f, 4f, 0.5f);
        public static Vector4 sharedBaseOffsetRootDefaults = new Vector4(0f, 0f, 0f, 0f);
        public static Vector4 sharedBaseOffsetTipDefaults = new Vector4(0f, 0f, 0f, 0f);
        public static Vector4 sharedBaseThicknessRootDefaults = new Vector4(0.24f, 0.24f, 0.24f, 0.24f);
        public static Vector4 sharedBaseThicknessTipDefaults = new Vector4(0.24f, 0.24f, 0.24f, 0.24f);


        public static Vector4 uiColorSliderEdgeL = new Vector4(0.20f, 0.5f, 0.4f, 1f);
        public static Vector4 uiColorSliderEdgeT = new Vector4(0.15f, 0.5f, 0.4f, 1f);
        public static Vector4 uiColorSliderColorsST = new Vector4(0.10f, 0.5f, 0.4f, 1f);
        public static Vector4 uiColorSliderColorsSB = new Vector4(0.05f, 0.5f, 0.4f, 1f);
        public static Vector4 uiColorSliderColorsET = new Vector4(0.00f, 0.5f, 0.4f, 1f);
        public static Vector4 uiColorSliderColorsEL = new Vector4(0.95f, 0.5f, 0.4f, 1f);

        public static double incrementMain = 0.125, incrementSmall = 0.04;
        public static Vector2d uiLengthLimit = new Vector2d(0.125, 16);
        public static Vector2d uiRootLimit = new Vector2d(0.125, 16);
        public static Vector2d uiTipLimit = new Vector2d(0.0000001, 16);
        public static Vector2d uiOffsetLimit = new Vector2d(-8, 8);
        public static Vector2d uiThicknessLimit = new Vector2d(0.04, 1);
        public static Vector4 baseColour = new Vector4(0.25f, 0.5f, 0.4f, 1f);

        public virtual Color TopColour
        {
            get
            {
                return UIUtility.ColorHSBToRGB(new Vector4((float)SurfTopHue, (float)SurfTopSat, (float)SurfTopBright, (float)SurfTopOpacity));
            }
        }

        public virtual Color BottomColour
        {
            get
            {
                return UIUtility.ColorHSBToRGB(new Vector4((float)SurfBottomHue, (float)SurfBottomSat, (float)SurfBottomBright, (float)SurfBottomOpacity));
            }
        }

        public virtual Color TrailColour
        {
            get
            {
                return UIUtility.ColorHSBToRGB(new Vector4((float)SurfTrailHue, (float)SurfTrailSat, (float)SurfTrailBright, (float)SurfTrailOpacity));
            }
        }

        public virtual Color LeadColour
        {
            get
            {
                return UIUtility.ColorHSBToRGB(new Vector4((float)SurfLeadHue, (float)SurfLeadSat, (float)SurfLeadBright, (float)SurfLeadOpacity));
            }
        }

        public override void ShowEditorUI()
        {
            base.ShowEditorUI();

            WindowManager.Window.FindPropertyGroup("Edge (leading)").UpdatePropertyValues(leadingEdgeType, rootLeadingEdge, tipLeadingEdge);
            WindowManager.Window.FindPropertyGroup("Edge (trailing)").UpdatePropertyValues(trailingEdgeType, rootTrailingEdge, tipTrailingEdge);
            WindowManager.Window.FindPropertyGroup("Surface (top)").UpdatePropertyValues(surfTopMat, surfTopOpacity, surfTopHue, surfTopSat, surfTopBright);
            WindowManager.Window.FindPropertyGroup("Surface (bottom)").UpdatePropertyValues(surfBottomMat, surfBottomOpacity, surfBottomHue, surfBottomSat, surfBottomBright);
            WindowManager.Window.FindPropertyGroup("Surface (leading edge)").UpdatePropertyValues(surfLeadMat, surfLeadOpacity, surfLeadHue, surfLeadSat, surfLeadBright);
            WindowManager.Window.FindPropertyGroup("Surface (trailing edge)").UpdatePropertyValues(surfTrailMat, surfTrailOpacity, surfTrailHue, surfTrailSat, surfTrailBright);
        }

        public override UI.EditorWindow CreateWindow()
        {
            EditorWindow window = new EditorWindow();
            window.WindowTitle = WindowTitle;
            window.wing = this;

            PropertyGroup basegroup = window.AddPropertyGroup("Base", UIUtility.ColorHSBToRGB(uiColorSliderBase));
            basegroup.AddProperty(new WingProperty(length), x => window.wing.Length = x);
            basegroup.AddProperty(new WingProperty(rootWidth), x => window.wing.RootWidth = x);
            basegroup.AddProperty(new WingProperty(tipWidth), x => window.wing.TipWidth = x);
            basegroup.AddProperty(new WingProperty(tipOffset), x => window.wing.TipOffset = x);
            basegroup.AddProperty(new WingProperty(rootThickness), x => window.wing.RootThickness = x);
            basegroup.AddProperty(new WingProperty(tipThickness), x => window.wing.TipThickness = x);

            UI.PropertyGroup leadgroup = window.AddPropertyGroup("Edge (leading)", UIUtility.ColorHSBToRGB(uiColorSliderEdgeL));
            leadgroup.AddProperty(new WingProperty(leadingEdgeType), x => ((B9_ProceduralWing)window.wing).LeadingEdgeType = (int)x,
                                        new string[] { "No Edge", "Rounded", "Biconvex", "Triangular" });
            leadgroup.AddProperty(new WingProperty(rootLeadingEdge), x => ((B9_ProceduralWing)window.wing).RootLeadingEdge = x);
            leadgroup.AddProperty(new WingProperty(tipLeadingEdge), x => ((B9_ProceduralWing)window.wing).TipLeadingEdge = x);

            UI.PropertyGroup trailGroup = window.AddPropertyGroup("Edge (trailing)", UIUtility.ColorHSBToRGB(uiColorSliderEdgeT));
            trailGroup.AddProperty(new WingProperty(trailingEdgeType), x => ((B9_ProceduralWing)window.wing).TrailingEdgeType = (int)x,
                                        new string[] { "No Edge", "Rounded", "Biconvex", "Triangular" });
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

            UI.PropertyGroup surfLGroup = window.AddPropertyGroup("Surface (leading edge)", UIUtility.ColorHSBToRGB(uiColorSliderColorsEL));
            surfLGroup.AddProperty(new WingProperty(surfLeadMat), x => ((B9_ProceduralWing)window.wing).SurfLeadMat = (int)x,
                                        new string[] { "Uniform", "Standard", "Reinforced", "LRSI", "HRSI" });
            surfLGroup.AddProperty(new WingProperty(surfLeadOpacity), x => ((B9_ProceduralWing)window.wing).SurfLeadOpacity = x);
            surfLGroup.AddProperty(new WingProperty(surfLeadHue), x => ((B9_ProceduralWing)window.wing).SurfLeadHue = x);
            surfLGroup.AddProperty(new WingProperty(surfBottomSat), x => ((B9_ProceduralWing)window.wing).SurfLeadSat = x);
            surfLGroup.AddProperty(new WingProperty(surfLeadBright), x => ((B9_ProceduralWing)window.wing).SurfLeadBright = x);

            UI.PropertyGroup surfRGroup = window.AddPropertyGroup("Surface (trailing edge)", UIUtility.ColorHSBToRGB(uiColorSliderColorsET));
            surfRGroup.AddProperty(new WingProperty(surfTrailMat), x => ((B9_ProceduralWing)window.wing).SurfTrailMat = (int)x,
                                        new string[] { "Uniform", "Standard", "Reinforced", "LRSI", "HRSI" });
            surfRGroup.AddProperty(new WingProperty(surfTrailOpacity), x => ((B9_ProceduralWing)window.wing).SurfTrailOpacity = x);
            surfRGroup.AddProperty(new WingProperty(surfTrailHue), x => ((B9_ProceduralWing)window.wing).SurfTrailHue = x);
            surfRGroup.AddProperty(new WingProperty(surfTrailSat), x => ((B9_ProceduralWing)window.wing).SurfTrailSat = x);
            surfRGroup.AddProperty(new WingProperty(surfTrailBright), x => ((B9_ProceduralWing)window.wing).SurfTrailBright = x);

            WindowAddFuel(window);

            return window;
        }

        #endregion
    }
}
