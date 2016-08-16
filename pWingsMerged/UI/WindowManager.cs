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
            if (Window != null)
                Window.closeWindow();

            if (!wingWindows.TryGetValue(forWing.ClassName, out Window))
            {
                Window = forWing.CreateWindow();
                wingWindows.Add(forWing.ClassName, Window);
            }
            return Window;
        }
    }
}
