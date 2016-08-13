using System.Collections.Generic;
using UnityEngine;

namespace ProceduralWings.UI
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    class WindowManager : MonoBehaviour
    {
        static Dictionary<string, EditorWindow> wingWindows = new Dictionary<string, EditorWindow>();

        public static EditorWindow Window;
        public static EditorWindow GetWindow(Base_ProceduralWing forWing)
        {
            EditorWindow window;
            if (!wingWindows.TryGetValue(forWing.ClassName, out window))
            {
                window = forWing.CreateWindow();
                wingWindows.Add(forWing.ClassName, window);
            }
            if (window != Window)
            {
                if (Window != null)
                    Window.closeWindow();
                Window = window;
            }
            return Window;
        }
    }
}
