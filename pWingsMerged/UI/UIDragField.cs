using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ProceduralWings.UI
{
    using Utility;
    /// <summary>
    /// draws a labelled horizontal value selection bar that is draggable to different values
    /// Also provides indication that the value has been altered
    /// </summary>
    public class UIDragField
    {
        bool bChanged;
        // unimplemented. Was used for the material selection, should have its own class
        //int fieldType;
        bool allowFine;
        string tooltipText;

        public string label;
        public Color fieldColour;
        public Vector2d limits; // x = low value, y = high value
        public Vector2d increments; // x = main increment, y = minor increment

        public UIDragField(string Label, string Tooltip, Vector2d Limits, Vector2d Increments, double DefaultValue = 0, bool AllowFine = true)
        {
            label = Label;
            tooltipText = Tooltip;
            limits = Limits;
            increments = Increments;
            allowFine = AllowFine;
        }

        public bool ValueChanged
        {
            get
            {
                bool tmp = bChanged;
                if (tmp)
                    UpdateTooltipText();
                bChanged = false;
                return tmp;
            }
        }

        /// <summary>
        /// This should really be cleaned up at some point
        /// </summary>
        /// <param name="value"></param>
        /// <param name="increment"></param>
        /// <param name="incrementLarge"></param>
        /// <param name="limits"></param>
        /// <param name="label"></param>
        /// <param name="changed"></param>
        /// <param name="backgroundColor"></param>
        /// <param name="valueType"></param>
        /// <param name="allowFine"></param>
        /// <returns></returns>
        public double FieldSlider(ref double value)
        {
            GUILayout.BeginHorizontal();
            double range = limits.y - limits.x;
            double value01 = (value - limits.x) / range; // rescaling value to be 0-100% of range for convenience
            double increment01 = increments.y / range;
            double valueOld = value01;
            float buttonWidth = 12, spaceWidth = 3;

            GUILayout.Label("", ProceduralWingManager.uiStyleLabelHint);
            Rect rectLast = GUILayoutUtility.GetLastRect();
            Rect rectSlider = new Rect(rectLast.xMin + buttonWidth + spaceWidth, rectLast.yMin, rectLast.width - 2 * (buttonWidth + spaceWidth), rectLast.height);
            Rect rectSliderValue = new Rect(rectSlider.xMin, rectSlider.yMin, rectSlider.width * (float)value01, rectSlider.height - 3f);
            Rect rectButtonL = new Rect(rectLast.xMin, rectLast.yMin, buttonWidth, rectLast.height);
            Rect rectButtonR = new Rect(rectLast.xMin + rectLast.width - buttonWidth, rectLast.yMin, buttonWidth, rectLast.height);
            Rect rectLabelValue = new Rect(rectSlider.xMin + rectSlider.width * 0.75f, rectSlider.yMin, rectSlider.width * 0.25f, rectSlider.height);

            if (GUI.Button(rectButtonL, "", ProceduralWingManager.uiStyleButton))
            {
                if (Input.GetMouseButtonUp(0) || !allowFine)
                    value01 -= increments.x / range;
                else if (Input.GetMouseButtonUp(1) && allowFine)
                    value01 -= increment01;
            }
            if (GUI.Button(rectButtonR, "", ProceduralWingManager.uiStyleButton))
            {
                if (Input.GetMouseButtonUp(0) || !allowFine)
                    value01 += increments.x / range;
                else if (Input.GetMouseButtonUp(1) && allowFine)
                    value01 += increment01;
            }

            if (rectLast.Contains(Event.current.mousePosition) && (Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseDown) // right click drag doesn't work properly without the event check
                    && Event.current.type != EventType.MouseUp) // drag event covers this, but don't want it to
            {
                value01 = GUI.HorizontalSlider(rectSlider, (float)value01, 0f, 1f, ProceduralWingManager.uiStyleSlider, ProceduralWingManager.uiStyleSliderThumb);

                if (valueOld != value01)
                {
                    if (Input.GetMouseButton(0) || !allowFine) // normal control
                    {
                        double excess = value01 / increment01;
                        value01 -= (excess - Math.Round(excess)) * increment01;
                    }
                    else if (Input.GetMouseButton(1) && allowFine) // fine control
                    {
                        double excess = valueOld / increment01;
                        value01 = (valueOld - (excess - Math.Round(excess)) * increment01) + Math.Min(value01 - 0.5, 0.4999) * increment01;
                    }
                }
            }
            else
                GUI.HorizontalSlider(rectSlider, (float)value01, 0f, 1f, ProceduralWingManager.uiStyleSlider, ProceduralWingManager.uiStyleSliderThumb);

            bChanged = valueOld != value; // value old is in 01 range
            value = limits.x + range * Utils.Clamp(value01, 0, 1);

            GUI.DrawTexture(rectSliderValue, fieldColour.GetTexture2D());
            GUI.Label(rectSlider, "  " + label, ProceduralWingManager.uiStyleLabelHint);
            //GUI.Label(rectLabelValue, UIUtility.GetValueTranslation(value, fieldType), ProceduralWingManager.uiStyleLabelHint);

            GUILayout.EndHorizontal();
            return value;
        }

        public void UpdateTooltipText()
        {
            if (bChanged)
            {
                ProceduralWing.uiLastFieldName = label;
                ProceduralWing.uiLastFieldTooltip = tooltipText;
            }
        }
    }
}
