using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ProceduralWings.UI
{
    /// <summary>
    /// an integer property slider which selects the display value from a list of values
    /// </summary>
    public class PropertySlider_ValueArray<T> : PropertySlider
    {
        T[] displayValues;
        public PropertySlider_ValueArray(T[] values, WingProperty propRef, Color foreColour, Action<float> onChange) : base(propRef, foreColour, onChange)
        {
            displayValues = values;
            AsInt = true;
        }

        protected override void SliderValueChanged(float value)
        {
            base.SliderValueChanged(value);

            input.text = displayValues[(int)(value - Min)].ToString();
        }
    }
}
