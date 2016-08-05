using UnityEngine;

namespace ProceduralWings.Utility
{
    public static class UIUtility
    {
        public static Rect ClampToScreen (Rect window)
        {
            window.x = Mathf.Clamp (window.x, -window.width + 20, Screen.width - 20);
            window.y = Mathf.Clamp (window.y, -window.height + 20, Screen.height - 20);

            return window;
        }

        public static Rect SetToScreenCenter (this Rect r)
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

        public static Vector3 GetMousePos ()
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

        public static string GetValueTranslation (double value, int type)
        {
            switch (type)
            { 
                case 1:
                {
                    type = (int)value;
                    switch (type)
                    {
                        case 0:
                            return "Uniform";
                        case 1:
                            return "Standard";
                        case 2:
                            return "Reinforced";
                        case 3:
                            return "LRSI";
                        case 4:
                            return "HRSI";
                        default:
                            return "Unknown material";
                    }
                }
                case 2:
                {
                    type = (int)value;
                    switch (type)
                    {
                        case 1:
                            return "Standard";
                        case 2:
                            return "Rounded";
                        case 3:
                            return "Biconvex";
                        case 4:
                            return "Triangular";
                        default:
                            return "Unknown";
                    }
                }
                case 3:
                {
                    type = (int)value;
                    switch (type)
                    {
                        case 1:
                            return "Rounded";
                        case 2:
                            return "Biconvex";
                        case 3:
                            return "Triangular";
                        default:
                            return "Unknown";
                    }
                }
                default:
                    return value.ToString ("0.000");
            }
        }

        public static Color ColorHSBToRGB(Vector4 hsbColor)
        {
            float r = hsbColor.z;
            float g = hsbColor.z;
            float b = hsbColor.z;
            if (hsbColor.y != 0)
            {
                float max = hsbColor.z;
                float dif = hsbColor.z * hsbColor.y;
                float min = hsbColor.z - dif;
                float h = hsbColor.x * 360f;
                if (h < 60f)
                {
                    r = max;
                    g = h * dif / 60f + min;
                    b = min;
                }
                else if (h < 120f)
                {
                    r = -(h - 120f) * dif / 60f + min;
                    g = max;
                    b = min;
                }
                else if (h < 180f)
                {
                    r = min;
                    g = max;
                    b = (h - 120f) * dif / 60f + min;
                }
                else if (h < 240f)
                {
                    r = min;
                    g = -(h - 240f) * dif / 60f + min;
                    b = max;
                }
                else if (h < 300f)
                {
                    r = (h - 240f) * dif / 60f + min;
                    g = min;
                    b = max;
                }
                else if (h <= 360f)
                {
                    r = max;
                    g = min;
                    b = -(h - 360f) * dif / 60 + min;
                }
                else
                {
                    r = 0;
                    g = 0;
                    b = 0;
                }
            }
            return new Color(Mathf.Clamp01(r), Mathf.Clamp01(g), Mathf.Clamp01(b), hsbColor.w);
        }
    }
}
