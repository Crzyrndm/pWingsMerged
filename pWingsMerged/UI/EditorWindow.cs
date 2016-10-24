using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProceduralWings.UI
{
    /// <summary>
    /// Window is an instance of the MainEditorPanelPrefab. It has a header block for title and generic info, a set of property groups, and finally the fuel switch options
    /// </summary>
    public class EditorWindow
    {
        public bool Visible
        {
            get
            {
                return canvas.enabled;
            }
            set
            {
                canvas.enabled = value;
                if (!canvas.enabled)
                {
                    wing = null;
                }
                else
                {
                    windowPosition.position = StaticWingGlobals.uiRectWindowEditor.position;
                }
            }
        }

        public Base_ProceduralWing wing;

        /// <summary>
        /// window position
        /// </summary>
        private RectTransform windowPosition;

        /// <summary>
        /// the canvas
        /// </summary>
        private Canvas canvas;

        /// <summary>
        /// The window backing all other elements
        /// </summary>
        public GameObject mainPanel;

        /// <summary>
        /// The green header label that displays the type of the selected panel
        /// </summary>
        private Text wingType;

        /// <summary>
        /// display for last modified property label
        /// </summary>
        private Text lastModifiedProperty;

        /// <summary>
        /// and a tooltip with a bit of explanation about that property
        /// </summary>
        private Text lastModifiedPropertyTooltip;

        /// <summary>
        /// it hides the window...
        /// </summary>
        private Button closeButton;

        /// <summary>
        /// a list of the propertyGroups that this window is using
        /// </summary>
        private List<PropertyGroup> propertyGroupList = new List<PropertyGroup>();

        // can do lookups by property ID into this when the edited property changes
        private Dictionary<string, PropertySlider> propertiesDict = new Dictionary<string, PropertySlider>();

        // store the last edited property for quick reference when continuously updating
        private PropertySlider lastEditedPropertyRef;

        private GameObject go;

        /// <summary>
        /// Constructor
        /// </summary>
        public EditorWindow()
        {
            // get references to all useful components
            go = Object.Instantiate(StaticWingGlobals.UI_WindowPrefab);
            go.transform.SetParent(KSP.UI.UIMasterController.Instance.appCanvas.transform, false);
            canvas = go.GetComponent<Canvas>();

            mainPanel = canvas.gameObject.GetChild("MainPanel");
            windowPosition = mainPanel.GetComponent<RectTransform>();
            GameObject headerPanel = mainPanel.GetChild("HeaderPanel");
            wingType = headerPanel.GetChild("WingType").GetComponent<Text>();
            lastModifiedProperty = headerPanel.GetChild("LastModifiedProperty").GetComponent<Text>();
            lastModifiedPropertyTooltip = headerPanel.GetChild("LastModifiedPropertyToolTip").GetComponent<Text>();
            closeButton = headerPanel.GetChild("CloseButton").GetComponent<Button>();

            // window position drag event
            EventTrigger.Entry dragEntry = new EventTrigger.Entry();
            dragEntry.eventID = EventTriggerType.Drag;
            dragEntry.callback = new EventTrigger.TriggerEvent();
            dragEntry.callback.AddListener((x) => windowDrag(x));
            mainPanel.GetComponent<EventTrigger>().triggers.Add(dragEntry);

            // close button click hides the window
            closeButton.onClick.AddListener(closeWindow);
            Visible = false;
        }

        public void AddFuelPanel()
        {
            GameObject fuelPanel = UnityEngine.Object.Instantiate(StaticWingGlobals.UI_FuelPanel);
            fuelPanel.transform.SetParent(mainPanel.transform, false); // parented onto the window

            Dropdown drop = fuelPanel.GetChild("Dropdown").GetComponent<Dropdown>();
            for (int i = 0; i < StaticWingGlobals.wingTankConfigurations.Count; ++i)
            {
                drop.options.Add(new Dropdown.OptionData(StaticWingGlobals.wingTankConfigurations[i].ConfigurationName));
            }
            drop.RefreshShownValue();
            drop.onValueChanged.AddListener(fuelSelectedChanged);
        }

        #region Event callbacks

        public void windowDrag(UnityEngine.EventSystems.BaseEventData data)
        {
            windowPosition.position += new Vector3(((PointerEventData)data).delta.x, ((PointerEventData)data).delta.y);
            StaticWingGlobals.uiRectWindowEditor.position = windowPosition.position;
        }

        public void closeWindow()
        {
            canvas.enabled = false;
        }

        public void fuelSelectedChanged(int index)
        {
            wing.fuelSelectedTankSetup = index;
            wing.FuelTankTypeChanged();
        }

        #endregion Event callbacks

        public PropertyGroup AddPropertyGroup(string name, Color groupColour)
        {
            PropertyGroup newGroup = FindPropertyGroup(name);
            if (newGroup == null)
            {
                newGroup = new PropertyGroup(name, groupColour, this);
                propertyGroupList.Add(newGroup);
                newGroup.groupInstance.transform.SetParent(mainPanel.transform, false);
            }
            return newGroup;
        }

        public PropertyGroup FindPropertyGroup(string groupName)
        {
            return propertyGroupList.Find(g => g.Name == groupName);
        }

        public void ResetGroups()
        {
            foreach (PropertyGroup group in propertyGroupList)
            {
                group.groupInstance.SetActive(false);
            }
        }

        public void UpdateProperty(WingProperty wp)
        {
            if (lastEditedPropertyRef.propertyRef.ID != wp.ID)
            {
                PropertySlider slider;
                if (propertiesDict.TryGetValue(wp.ID, out slider))
                {
                    lastEditedPropertyRef = slider;
                    SetLastModifiedProperty(wp);
                }
            }
            lastEditedPropertyRef.Refresh(wp);
        }

        public void SetLastModifiedProperty(WingProperty wp)
        {
            if (wp.ID == "scale")
                return;

            lastModifiedPropertyTooltip.text = wp.tooltip;
            lastModifiedProperty.text = wp.name;
        }

        public void GroupAddProperty(PropertySlider slider)
        {
            if (slider?.propertyRef?.ID != null && !propertiesDict.ContainsKey(slider.propertyRef.ID))
            {
                propertiesDict.Add(slider.propertyRef.ID, slider);
            }
        }

        #region Properties

        public string WindowTitle
        {
            get
            {
                return wingType.text;
            }
            set
            {
                wingType.text = value;
            }
        }

        #endregion Properties
    }
}