using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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

        Color groupColour;

        /// <summary>
        /// OnClick toggles the visibility of the propertiesList
        /// </summary>
        Button groupButton;

        /// <summary>
        /// Toggle enabled to toggle visibility of properties
        /// </summary>
        GameObject propertiesListGroup;

        /// <summary>
        /// true if the properties in the list are visible
        /// </summary>
        bool propertiesVisible;

        /// <summary>
        /// List of all properties added to this group
        /// </summary>
        Dictionary<string, PropertySlider> properties = new Dictionary<string, PropertySlider>();

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="prefab"></param>
        public PropertyGroup(string name, Color GroupColour)
        {
            groupInstance = UnityEngine.Object.Instantiate(StaticWingGlobals.UI_PropertyGroupPrefab); // create a copy

            groupButton = groupInstance.GetChild("GroupButton").GetComponent<Button>();
            groupButton.onClick.AddListener(GroupButtonClicked);
            groupButton.GetComponentInChildren<Text>().text = name;

            propertiesListGroup = groupInstance.GetChild("PropertiesList");
            propertiesListGroup.SetActive(false);

            groupColour = GroupColour;
        }

        /// <summary>
        /// header button clicked callback
        /// </summary>
        public void GroupButtonClicked()
        {
            propertiesVisible = !propertiesVisible;
            propertiesListGroup.SetActive(propertiesVisible);
        }

        public PropertySlider AddProperty(WingProperty propertyRef, Action<float> onChanged)
        {
            PropertySlider newProp;
            if (!properties.TryGetValue(propertyRef.name, out newProp)) // prevent adding duplicate properties for some reason
            {
                newProp = new PropertySlider(propertyRef, groupColour, onChanged);
                properties.Add(propertyRef.name, newProp);
                newProp.propertyInstance.transform.SetParent(propertiesListGroup.transform, false);
            }
            return newProp;
        }

        public void UpdatePropertyValues(params WingProperty[] props)
        {
            for (int i = props.Length - 1; i >= 0; --i)
            {
                PropertySlider prop;
                if (properties.TryGetValue(props[i].name, out prop))
                {
                    prop.propertyRef.Update(props[i]);
                }
            }
        }

        public void UpdateGroupColour(Color c)
        {
            groupColour = c;
            foreach (var slide in properties)
            {
                slide.Value.UpdateColour(c);
            }
        }

        public string Name
        {
            get
            {
                return groupButton.GetComponent<Text>().text;
            }
        }
    }
}
