using System;
using UnityEngine;
using UnityEngine.UI;

namespace ProceduralWings.UI
{
    /// <summary>
    /// A single property value. Has two images to use for creating a slider, a text field for the property title, and an input field for direct user entry of values
    /// </summary>
    public class PropertySlider
    {
        /// <summary>
        /// used by property group to associate the go in the tree for rendering
        /// </summary>
        public GameObject propertyInstance;

        public WingProperty propertyRef;

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

        public virtual string Text
        {
            get
            {
                return input.text;
            }
        }

        protected Slider inputSlider;

        /// <summary>
        /// Description of property for user
        /// </summary>
        protected Text propertyLabel;

        /// <summary>
        /// Place where user can directly enter values if required
        /// </summary>
        protected InputField input;

        /// <summary>
        /// the function to call when the value of this property changes
        /// </summary>
        public Action<float> onValueChanged;

        /// <summary>
        ///
        /// </summary>
        /// <param name="prefab"></param>
        public PropertySlider(WingProperty propRef, Color foreColour, Action<float> onChange)
        {
            propertyInstance = UnityEngine.Object.Instantiate(StaticWingGlobals.UI_PropertyPrefab);

            inputSlider = propertyInstance.GetChild("InputSlider").GetComponent<Slider>();
            inputSlider.fillRect.GetComponent<Image>().color = foreColour;

            propertyLabel = inputSlider.gameObject.GetChild("PropertyLabel").GetComponent<Text>();
            propertyLabel.text = propRef.name;
            input = inputSlider.gameObject.GetChild("UserInput").GetComponent<InputField>();

            Refresh(propRef);

            input.onEndEdit.AddListener(TextValueChanged);
            inputSlider.onValueChanged.AddListener(SliderValueChanged);
            onValueChanged += onChange;
        }

        // for derived classes that roll their own constructor
        protected PropertySlider()
        { }

        protected virtual void TextValueChanged(string text)
        {
            float f;
            if (float.TryParse(text, out f))
            {
                inputSlider.value = f;
            }
        }

        protected virtual void SliderValueChanged(float value)
        {
            if (!AsInt)
            {
                float nvalue = (float)Math.Round(value, propertyRef.decPlaces);
                if (value != nvalue)
                {
                    Value = nvalue;
                    return;
                }
            }
            SetText(value);
            if (!refreshing)
            {
                onValueChanged?.Invoke(value);
            }
        }

        public virtual void SetText(double d)
        {
            input.text = d.ToString($"F{propertyRef.decPlaces}");
        }

        public void UpdateColour(Color c)
        {
            inputSlider.fillRect.GetComponent<Image>().color = c;
        }

        private bool refreshing;
        public void Refresh(WingProperty p)
        {
            if (p == null)
                return;
            refreshing = true;
            propertyRef = p;
            AsInt = p.decPlaces == 0; // 0 dec places => integer values only
            Min = p.min;
            Max = p.max;
            Value = p.Value;
            SetText(Value);
            refreshing = false;
        }
    }
}