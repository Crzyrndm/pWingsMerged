using System.Collections.Generic;
using UnityEngine;

namespace ProceduralWings.UI
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    class WindowManager : MonoBehaviour
    {
        static Dictionary<int, EditorWindow> wingWindows = new Dictionary<int, EditorWindow>();

        public static EditorWindow Window;
        public static EditorWindow GetWindow(Base_ProceduralWing forWing)
        {
            if (Window != null)
                Window.closeWindow();
            if (!wingWindows.TryGetValue(forWing.ClassID, out Window))
            {
                Window = forWing.CreateWindow();
                wingWindows.Add(forWing.ClassID, Window);
            }
            
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
