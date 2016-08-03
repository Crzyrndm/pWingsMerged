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

        /// <summary>
        /// the function to call when the value of this property changes
        /// </summary>
        public Action<float> onValueChanged;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prefab"></param>
        public PropertySlider(string name)
        {
            propertyInstance = UnityEngine.Object.Instantiate(StaticWingGlobals.UI_PropertyPrefab);
            propertyLabel = propertyInstance.GetChild("PropertyLabel").GetComponent<Text>();
            propertyLabel.text = name;
            input = propertyInstance.GetChild("User Input").GetComponent<InputField>();
            inputSlider = propertyInstance.GetChild("PropertySlider").GetComponent<Slider>();

            inputSlider.onValueChanged.AddListener(SliderValueChanged);
            Value = 1;
        }

        void SliderValueChanged(float value)
        {
            input.text = value.ToString();
            onValueChanged.Invoke(value);
        }
    }
}
