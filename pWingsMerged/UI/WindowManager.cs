using System.Collections.Generic;
using UnityEngine;

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
                Window = forWing.CreateWindow();
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
            Window = null;
            wingWindows.Clear(); // window canvas gets deleted on scene load so the windows this is holding cant actually be used...

            StaticWingGlobals.SaveConfigs();
        }
    }
}