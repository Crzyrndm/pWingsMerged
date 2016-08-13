using System;
using UnityEngine;
using UnityEngine.UI;

namespace ProceduralWings.UI
{
    /// <summary>
    /// an integer property slider which selects the display value from a list of values
    /// </summary>
    public class PropertySlider_ValueArray<T> : PropertySlider
    {
        T[] displayValues;

        Text text;
        public override string Text
        {
            get
            {
                return text.text;
            }
        }

        public override void SetText(double d)
        {
            text.text = displayValues[(int)d - (int)Min].ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prefab"></param>
        public PropertySlider_ValueArray(T[] values, WingProperty propRef, Color foreColour, Action<float> onChange)
        {
            propertyInstance = UnityEngine.Object.Instantiate(StaticWingGlobals.UI_PropertyValArrayPrefab);
            
            inputSlider = propertyInstance.GetChild("InputSlider").GetComponent<Slider>();
            inputSlider.fillRect.GetComponent<Image>().color = foreColour;
            
            propertyLabel = inputSlider.gameObject.GetChild("PropertyLabel").GetComponent<Text>();
            propertyLabel.text = propRef.name;
            input = null;
            text = inputSlider.gameObject.GetChild("Text").GetComponent<Text>();
            
            displayValues = values;
            AsInt = true;
            
            Refresh(propRef);
            
            inputSlider.onValueChanged.AddListener(SliderValueChanged);
            onValueChanged += onChange;
        }
    }
}
