using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ProceduralWings.UI
{
    /// <summary>
    /// A group of properties with visibility toggled by a button press
    /// </summary>
    public class PropertyGroup
    {
        /// <summary>
        /// used by the window to add groups to the go tree correctly
        /// </summary>
        public GameObject groupInstance;

        /// <summary>
        ///
        /// </summary>
        private Color groupColour;

        /// <summary>
        /// OnClick toggles the visibility of the propertiesList
        /// </summary>
        private Button groupButton;

        /// <summary>
        /// Toggle enabled to toggle visibility of properties
        /// </summary>
        private GameObject propertiesListGroup;

        /// <summary>
        /// the window this group is attached to
        /// </summary>
        private EditorWindow myWindow;

        /// <summary>
        /// true if the properties in the list are visible
        /// </summary>
        private bool propertiesVisible;

        /// <summary>
        /// List of all properties added to this group
        /// </summary>
        private List<PropertySlider> properties = new List<PropertySlider>();

        private string name;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="prefab"></param>
        public PropertyGroup(string name, Color GroupColour, EditorWindow window)
        {
            groupInstance = UnityEngine.Object.Instantiate(StaticWingGlobals.UI_PropertyGroupPrefab); // create a copy

            groupButton = groupInstance.GetChild("GroupButton").GetComponent<Button>();
            groupButton.onClick.AddListener(GroupButtonClicked);
            this.name = name;
            groupButton.GetComponentInChildren<Text>().text = name;

            propertiesListGroup = groupInstance.GetChild("PropertiesList");
            propertiesListGroup.SetActive(false);

            groupColour = GroupColour;

            myWindow = window;
        }

        /// <summary>
        /// header button clicked callback
        /// </summary>
        public void GroupButtonClicked()
        {
            propertiesVisible = !propertiesVisible;
            propertiesListGroup.SetActive(propertiesVisible);
        }

        // need to change how these are created so it's not duplicating code for every option
        public PropertySlider AddProperty(WingProperty propertyRef, Action<float> onChanged, bool geometric = false)
        {
            PropertySlider slider = properties.Find(sl => sl.propertyRef.ID == propertyRef.ID);
            if (slider == null) // prevent adding duplicate properties for some reason
            {
                if (geometric)
                {
                    slider = new PropertySlider_GeometryScaled(propertyRef, groupColour, onChanged);
                }
                else
                {
                    slider = new PropertySlider(propertyRef, groupColour, onChanged);
                }
                slider.propertyInstance.transform.SetParent(propertiesListGroup.transform, false);
                properties.Add(slider);
                myWindow.GroupAddProperty(slider);
            }
            return slider;
        }

        public PropertySlider AddProperty<T>(WingProperty propertyRef, Action<float> onChanged, T[] values)
        {
            PropertySlider slider = properties.Find(sl => sl.propertyRef.ID == propertyRef.ID);
            if (slider == null) // prevent adding duplicate properties for some reason
            {
                slider = new PropertySlider_ValueArray<T>(values, propertyRef, groupColour, onChanged);
                slider.propertyInstance.transform.SetParent(propertiesListGroup.transform, false);
                properties.Add(slider);
                myWindow.GroupAddProperty(slider);
            }
            return slider;
        }

        public void UpdatePropertyValues(params WingProperty[] props)
        {
            groupInstance.SetActive(true);
            for (int i = props.Length - 1; i >= 0; --i)
            {
                PropertySlider prop = properties.Find(sl => sl.propertyRef.ID == props[i].ID);
                if (prop != null)
                {
                    prop.Refresh(props[i]);
                }
            }
        }

        public void UpdateGroupColour(Color c)
        {
            groupColour = c;
            foreach (var slide in properties)
            {
                slide.UpdateColour(c);
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }
    }
}