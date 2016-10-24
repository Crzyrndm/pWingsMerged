using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ProceduralWings.UI
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    internal class WindowManager : MonoBehaviour
    {
        private static Dictionary<int, EditorWindow> wingWindows = new Dictionary<int, EditorWindow>();

        public static EditorWindow Window;
        public static EditorWindow GetWindow(Base_ProceduralWing forWing)
        {
            if (Window != null)
                Window.closeWindow();
            if (!wingWindows.TryGetValue(forWing.ClassID, out Window))
            {
                Window = forWing.CreateMainWindow();
                Window.wing = forWing;
                forWing.AddMatchingButtons(Window);
                if (forWing.CanBeFueled)
                {
                    Window.AddFuelPanel();
                }
                wingWindows.Add(forWing.ClassID, Window);
            }
            Window.wing = forWing;
            return Window;
        }

        public void Awake()
        {
            StaticWingGlobals.LoadConfigs();
        }

        public void OnDestroy()
        {
            if (Window != null)
            {
                Window.Visible = false;
            }

            StaticWingGlobals.SaveConfigs();
        }

        public static void AddButtonComponentToUI(GameObject parent, string buttonText, UnityAction onClick)
        {
            GameObject buttonGO = Instantiate(StaticWingGlobals.UI_ButtonPrefab);
            buttonGO.transform.SetParent(parent.transform, false);

            Button button = buttonGO.GetComponent<Button>();
            button.onClick.AddListener(onClick);

            Text tx = button.GetComponentInChildren<Text>();
            tx.text = buttonText;
            tx.resizeTextForBestFit = true;
        }
    }
}