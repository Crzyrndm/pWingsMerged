using System;
using UnityEngine;

namespace ProceduralWings.UI
{
    internal class PropertySlider_GeometryScaled : PropertySlider
    {
        public PropertySlider_GeometryScaled(WingProperty propRef, Color foreColour, Action<float> onChange) : base(propRef, foreColour, onChange)
        {
        }

        public override void SetText(double d)
        {
            if (WindowManager.Window?.wing != null)
            {
                base.SetText(d * WindowManager.Window.wing.Scale);
            }
            else
            {
                base.SetText(d);
            }
        }
    }
}