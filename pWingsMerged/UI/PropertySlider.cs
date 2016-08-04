using System;
using UnityEngine;
using UnityEngine.UI;

namespace ProceduralWings.UI
{
    /// <summary>
    /// A single property value. Has two images to use for creating a slider, a text field for the property title, and an input field for direct user entry of values
    /// </summary>
    class PropertySlider
    {
        /// <summary>
        /// used by property group to associate the go in the tree for rendering
        /// </summary>
        public GameObject propertyInstance;

        public double Max
        {
            get
            {
                return inputSlider.maxValue;
            }
            set
            {
                inputSlider.maxValue = (float)value;
            }
        }
        public double Min
        {
            get
            {
                return inputSlider.minValue;
            }
            set
            {
                inputSlider.minValue = (float)value;
            }
        }
        public bool AsInt
        {
            get
            {
                return inputSlider.wholeNumbers;
            }
            set
            {
                inputSlider.wholeNumbers = value;
            }
        }
        public double Value
        {
            get
            {
                return inputSlider.value;
            }
            set
            {
                inputSlider.value = (float)value;
            }
        }

        Slider inputSlider;

        /// <summary>
        /// Description of property for user
        /// </summary>
        Text propertyLabel;

        /// <summary>
        /// Place where user can directly enter values if required
        /// </summary>
        InputField input;

        int numDecPlaces;

        /// <summary>
        /// the function to call when the value of this property changes
        /// </summary>
        public Action<float> onValueChanged;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prefab"></param>
        public PropertySlider(string name, Color foreColour, float min, float max, float value, int numDec, Action<float> onChange)
        {
            propertyInstance = UnityEngine.Object.Instantiate(StaticWingGlobals.UI_PropertyPrefab);

            inputSlider = propertyInstance.GetChild("InputSlider").GetComponent<Slider>();
            inputSlider.minValue = min;
            inputSlider.maxValue = max;
            inputSlider.value = value;
            inputSlider.fillRect.GetComponent<Image>().color = foreColour;
            numDecPlaces = numDec;

            propertyLabel = inputSlider.gameObject.GetChild("PropertyLabel").GetComponent<Text>();
            propertyLabel.text = name;
            input = inputSlider.gameObject.GetChild("UserInput").GetComponent<InputField>();
            input.enabled = false; // for now it can just behave like a text object.

            inputSlider.onValueChanged.AddListener(SliderValueChanged);
            Value = 1;

            onValueChanged += onChange;

            AsInt = numDec <= 0; // 0 dec places => integer values only
        }

        void SliderValueChanged(float value)
        {
            if (!AsInt)
            {
                float nvalue = (float)Math.Round(value, numDecPlaces);
                Value = nvalue;
                if (value != nvalue)
                {
                    return;
                }
            }
            input.text = value.ToString($"F{numDecPlaces}");
            onValueChanged?.Invoke(value);
        }
    }
}
