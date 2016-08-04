using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace ProceduralWings.UI
{
    /// <summary>
    /// Window is an instance of the MainEditorPanelPrefab. It has a header block for title and generic info, a set of property groups, and finally the fuel switch options
    /// </summary>
    class EditorWindow
    {
        static EditorWindow UI_Window;
        public static EditorWindow Instance
        {
            get
            {
                if (UI_Window == null)
                {
                    UI_Window = new EditorWindow();
                }
                return UI_Window;
            }
        }

        bool visible;
        public bool Visible
        {
            get
            {
                return visible;
            }
            set
            {
                visible = value;
                if (!visible)
                {
                    currentWing = null;
                }
                canvas.enabled = visible;
            }
        }


        public static ProceduralWing currentWing;

        /// <summary>
        /// window position
        /// </summary>
        RectTransform windowPosition;

        /// <summary>
        /// the canvas
        /// </summary>
        Canvas canvas;

        /// <summary>
        /// The window backing all other elements
        /// </summary>
        GameObject mainPanel;

        /// <summary>
        /// The green header label that displays the type of the selected panel
        /// </summary>
        Text wingType;

        /// <summary>
        /// display for last modified property label
        /// </summary>
        Text lastModifiedProperty;

        /// <summary>
        /// and a tooltip with a bit of explanation about that property
        /// </summary>
        Text lastModifiedPropertyTooltip;

        /// <summary>
        /// it hides the window...
        /// </summary>
        Button closeButton;

        Dictionary<string, PropertyGroup> propertyGroupList = new Dictionary<string, PropertyGroup>();

        /// <summary>
        /// Constructor
        /// </summary>
        EditorWindow()
        {
            // get references to all useful components
            canvas = UnityEngine.Object.Instantiate(StaticWingGlobals.UI_WindowPrefab.GetComponent<Canvas>());
            
            mainPanel = canvas.gameObject.GetChild("MainPanel");
            windowPosition = mainPanel.GetComponent<RectTransform>();
            GameObject headerPanel = mainPanel.GetChild("HeaderPanel");
            wingType = headerPanel.GetChild("WingType").GetComponent<Text>();
            lastModifiedProperty = headerPanel.GetChild("LastModifiedProperty").GetComponent<Text>();
            lastModifiedPropertyTooltip = headerPanel.GetChild("LastModifiedPropertyToolTip").GetComponent<Text>();
            closeButton = headerPanel.GetChild("CloseButton").GetComponent<Button>();
            
            // window position drag event
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.Drag;
            entry.callback = new EventTrigger.TriggerEvent();
            entry.callback.AddListener((x) => windowDrag(x));
            mainPanel.GetComponent<EventTrigger>().triggers.Add(entry);

            // close button click hides the window
            closeButton.onClick.AddListener(closeWindow);
        }

        void AddFuelPanel()
        {

        }

        #region Event callbacks

        public void windowDrag(UnityEngine.EventSystems.BaseEventData data)
        {
            windowPosition.position += new Vector3(((PointerEventData)data).delta.x, ((PointerEventData)data).delta.y);
        }

        public void closeWindow()
        {
            canvas.enabled = false;
        }

        #endregion

        public void UpdateLastModifiedProperty(string PropertyLabel, string PropertyTooltip)
        {
            lastModifiedProperty.text = PropertyLabel;
            lastModifiedPropertyTooltip.text = PropertyTooltip;
        }

        public PropertyGroup AddPropertyGroup(string name, Color groupColour)
        {
            PropertyGroup newGroup = FindPropertyGroup(name);
            if (newGroup == null)
            {
                newGroup = new PropertyGroup(name, groupColour);
                propertyGroupList.Add(name, newGroup);
                newGroup.groupInstance.transform.SetParent(mainPanel.transform, false);
            }
            return newGroup;
        }

        public PropertyGroup FindPropertyGroup(string groupName)
        {
            PropertyGroup testGroup;
            if (propertyGroupList.TryGetValue(groupName, out testGroup))
                return testGroup;
            return null;
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

        #endregion
    }
}
