using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ProceduralWings
{
    using Utility;

    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class StaticWingGlobals : MonoBehaviour
    {
        // version for any save upgrading
        public static readonly int version = 1;

        // fuel configuration
        public static List<Fuel.WingTankConfiguration> wingTankConfigurations = new List<Fuel.WingTankConfiguration>();

        // prefabs from bundle
        public static Shader B9WingShader;

        public static GameObject UI_WindowPrefab;
        public static GameObject UI_PropertyGroupPrefab;
        public static GameObject UI_PropertyPrefab;
        public static GameObject UI_PropertyValArrayPrefab;
        public static GameObject UI_FuelPanel;
        public static GameObject UI_ButtonPrefab;

        // User settings
        public static Rect uiRectWindowEditor = new Rect();

        public static KeyCode keyTranslation;
        public static KeyCode keyTipScale;
        public static KeyCode keyRootScale;
        public static float moveSpeed;
        public static float scaleSpeed;

        public static KeyCode uiKeyCodeEdit = KeyCode.J;

        public static bool assembliesChecked;
        public static bool FARactive;
        public static bool RFactive;
        public static bool MFTactive;
        public static bool originalPWingPlugins;

        public void Start()
        {
            // checks for presence of FAR, MFT, RF, or original PWings plugins
            for (int i = AssemblyLoader.loadedAssemblies.Count - 1; i >= 0; --i)
            {
                AssemblyLoader.LoadedAssembly test = AssemblyLoader.loadedAssemblies[i];
                if (test.assembly.GetName().Name.Equals("FerramAerospaceResearch", StringComparison.InvariantCultureIgnoreCase))
                    FARactive = true;
                else if (test.assembly.GetName().Name.Equals("RealFuels", StringComparison.InvariantCultureIgnoreCase))
                    RFactive = true;
                else if (test.assembly.GetName().Name.Equals("modularFuelTanks", StringComparison.InvariantCultureIgnoreCase))
                    MFTactive = true;
                else if (test.assembly.GetName().Name.Equals("B9_Aerospace_WingStuff", StringComparison.InvariantCultureIgnoreCase) // B9 PWings
                    || test.assembly.GetName().Name.Equals("pWings", StringComparison.InvariantCultureIgnoreCase)) // original pwings
                {
                    originalPWingPlugins = true;
                    Log("Error : conflicting procedural wings plugin detected. PWings Plugin supercedes and conflicts with the original and B9 pwings plugins");
                }
            }

            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("ProceduralWingFuelSetups"))
            {
                ConfigNode[] fuelNodes = node.GetNodes("FuelSet");
                for (int i = 0; i < fuelNodes.Length; ++i)
                    wingTankConfigurations.Add(new Fuel.WingTankConfiguration(fuelNodes[i]));
            }
            StartCoroutine(LoadBundleAssets());
        }

        public IEnumerator LoadBundleAssets()
        {
            AssetBundle bundle = AssetBundle.LoadFromFile(KSPUtil.ApplicationRootPath + "GameData" + Path.DirectorySeparatorChar + "PWingsPlugin" + Path.DirectorySeparatorChar + "wingshader");
            if (bundle != null)
            {
                Shader[] shaders = bundle.LoadAllAssets<Shader>();
                for (int i = 0; i < shaders.Length; ++i)
                {
                    switch (shaders[i].name)
                    {
                        case "KSP/Specular Layered":
                            B9WingShader = shaders[i] as Shader;
                            Log($"Wing shader \"{shaders[i].name}\" loaded. Supported? {B9WingShader.isSupported}");
                            break;
                    }
                }

                GameObject[] objects = bundle.LoadAllAssets<GameObject>();
                foreach (GameObject go in objects)
                {
                    switch (go.name)
                    {
                        case "FuelPanelPrefab":
                            UI_FuelPanel = go as GameObject;
                            Log($"Prefab \"{go.name}\" loaded");
                            break;

                        case "MainEditorPanel":
                            UI_WindowPrefab = go as GameObject;
                            Log($"Prefab \"{go.name}\" loaded");
                            break;

                        case "PropertyGroup":
                            UI_PropertyGroupPrefab = go as GameObject;
                            Log($"Prefab \"{go.name}\" loaded");
                            break;

                        case "PropertySelector":
                            UI_PropertyPrefab = go as GameObject;
                            Log($"Prefab \"{go.name}\" loaded");
                            break;

                        case "PropertySelector_ValArray":
                            UI_PropertyValArrayPrefab = go as GameObject;
                            Log($"Prefab \"{go.name}\" loaded");
                            break;

                        case "ButtonPrefab":
                            UI_ButtonPrefab = go as GameObject;
                            Log($"Prefab \"{go.name}\" loaded");
                            break;
                    }
                }
                yield return new WaitForSeconds(1.0f);
                Log("unloading bundle");
                bundle.Unload(false); // unload the raw asset bundle
            }
        }

        public static void LoadConfigs()
        {
            ConfigNode node = ConfigNode.Load(KSPUtil.ApplicationRootPath + Path.DirectorySeparatorChar + "GameData" + Path.DirectorySeparatorChar
                                                + "PWingsPlugin" + Path.DirectorySeparatorChar + "PluginData" + Path.DirectorySeparatorChar + "settings.cfg")?.GetNode("PWingsSettings");

            if (node == null || !node.TryGetValue(nameof(keyTranslation), ref keyTranslation))
                keyTranslation = KeyCode.G;
            if (node == null || !node.TryGetValue(nameof(keyTipScale), ref keyTipScale))
                keyTipScale = KeyCode.T;
            if (node == null || !node.TryGetValue(nameof(keyRootScale), ref keyRootScale))
                keyRootScale = KeyCode.B;
            if (node == null || !node.TryGetValue(nameof(moveSpeed), ref moveSpeed))
                moveSpeed = 5;
            if (node == null || !node.TryGetValue(nameof(scaleSpeed), ref scaleSpeed))
                scaleSpeed = 0.25f;
            if (node == null || !node.TryGetValue(nameof(uiRectWindowEditor), ref uiRectWindowEditor))
                uiRectWindowEditor = UIUtility.SetToScreenCenter(new Rect());
            if (node == null || !node.TryGetValue(nameof(uiKeyCodeEdit), ref uiKeyCodeEdit))
                uiKeyCodeEdit = KeyCode.J;
        }

        public static void SaveConfigs()
        {
            ConfigNode node = new ConfigNode();
            ConfigNode settings = node.AddNode("PWingsSettings");
            settings.AddValue(nameof(keyTranslation), keyTranslation.ToString());
            settings.AddValue(nameof(keyTipScale), keyTipScale.ToString());
            settings.AddValue(nameof(keyRootScale), keyRootScale.ToString());
            settings.AddValue(nameof(moveSpeed), moveSpeed);
            settings.AddValue(nameof(scaleSpeed), scaleSpeed);
            settings.AddValue(nameof(uiRectWindowEditor), uiRectWindowEditor);
            settings.AddValue(nameof(uiKeyCodeEdit), uiKeyCodeEdit.ToString());

            node.Save(KSPUtil.ApplicationRootPath + Path.DirectorySeparatorChar + "GameData" + Path.DirectorySeparatorChar + "PWingsPlugin" + Path.DirectorySeparatorChar + "PluginData" + Path.DirectorySeparatorChar + "settings.cfg");
        }

        public void Log(object formatted)
        {
            Debug.Log($"[PW Plugin] " + formatted);
        }
    }
}