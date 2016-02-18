using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProceduralWings.UI
{
    public class UIGroup
    {
        public List<UIDragField> fieldsToDraw = new List<UIDragField>();
        public bool Open;
        public string Label;

        public void drawGroup(ref double[] vals)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(Label, ProceduralWingManager.uiStyleLabelHint))
                Open = !Open;
            if (Open)
                GUILayout.Label("|", ProceduralWingManager.uiStyleLabelHint, GUILayout.MaxWidth(15f));
            else
                GUILayout.Label("+", ProceduralWingManager.uiStyleLabelHint, GUILayout.MaxWidth(15f));
            GUILayout.EndHorizontal();

            if (fieldsToDraw.Count != vals.Length)
                throw new Exception("argument count mismatch for group " + Label); // this should never happen, so make sure the programmer knows about it quickly
            for (int i = 0; i < fieldsToDraw.Count; ++i)
                fieldsToDraw[i].FieldSlider(ref vals[i]);
        }
    }
}
