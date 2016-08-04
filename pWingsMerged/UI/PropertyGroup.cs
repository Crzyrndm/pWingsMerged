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
    class PropertyGroup
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
        List<PropertySlider> propertyList = new List<PropertySlider>();

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

        public PropertySlider AddProperty(string name)
        {
            PropertySlider newGroup = new PropertySlider(name);
            propertyList.Add(newGroup);
            newGroup.propertyInstance.transform.SetParent(propertiesListGroup.transform, false);
            return newGroup;
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
