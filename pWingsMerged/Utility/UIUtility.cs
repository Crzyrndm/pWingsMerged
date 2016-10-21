using UnityEngine;

namespace ProceduralWings.Utility
{
    public static class UIUtility
    {
        public static Rect ClampToScreen(Rect window)
        {
            window.x = Mathf.Clamp(window.x, -window.width + 20, Screen.width - 20);
            window.y = Mathf.Clamp(window.y, -window.height + 20, Screen.height - 20);

            return window;
        }

        public static Rect SetToScreenCenter(this Rect r)
        {
            if (r.width > 0 && r.height > 0)
            {
                r.x = Screen.width / 2f - r.width / 2f;
                r.y = Screen.height / 2f - r.height / 2f;
            }
            return r;
        }

        public static Rect SetToScreenCenterAlways(this Rect r)
        {
            r.x = Screen.width / 2f - r.width / 2f;
            r.y = Screen.height / 2f - r.height / 2f;
            return r;
        }

        public static Vector3 GetMousePos()
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.y = Screen.height - mousePos.y;
            return mousePos;
        }

        public static Vector3 GetMouseWindowPos(Rect windowRect)
        {
            Vector3 mousepos = GetMousePos();
            mousepos.x -= windowRect.x;
            mousepos.y -= windowRect.y;
            return mousepos;
        }
    }
}