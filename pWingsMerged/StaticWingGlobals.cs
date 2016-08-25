using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using KSP;

namespace ProceduralWings
{
    using Utility;
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class StaticWingGlobals : MonoBehaviour
    {
        // fuel configuration
        public static List<Fuel.WingTankConfiguration> wingTankConfigurations = new List<Fuel.WingTankConfiguration>();

        // prefabs from bundle
        public static Shader B9WingShader;

        public static GameObject UI_WindowPrefab;
        public static GameObject UI_PropertyGroupPrefab;
        public static GameObject UI_PropertyPrefab;
        public static GameObject UI_PropertyValArrayPrefab;
        public static GameObject UI_FuelPanel;

        // User settings
        public static Rect uiRectWindowEditor = new Rect();
        public static KeyCode keyTranslation;
        public static KeyCode keyTipScale;
        public static KeyCode keyRootScale;
        public static float moveSpeed;
        public static float scaleSpeed;

        public static KeyCode uiKeyCodeEdit = KeyCode.J;

        public void Start()
        {
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
            using (WWW www = new WWW("file://" + KSPUtil.ApplicationRootPath + Path.DirectorySeparatorChar + "GameData"
                    + Path.DirectorySeparatorChar + "PWingsPlugin" + Path.DirectorySeparatorChar + "wingshader.ksp"))
            {
                yield return www;

                AssetBundle bundle = www.assetBundle;
                Shader[] shaders = bundle.LoadAllAssets<Shader>();
                for (int i = 0; i < shaders.Length; ++i)
                {
                    switch (shaders[i].name)
                    {
                        case "KSP/Specular Layered":
                            B9WingShader = shaders[i] as Shader;
                            Base_ProceduralWing.Log($"Wing shader \"{shaders[i].name}\" loaded. Supported? {B9WingShader.isSupported}");
                            break;
                    }
                }

                GameObject[] objects = bundle.LoadAllAssets<GameObject>();
                for (int i = 0; i < objects.Length; ++i)
                {
                    switch (objects[i].name)
                    {
                        case "FuelPanelPrefab":
                            UI_FuelPanel = objects[i] as GameObject;
                            Base_ProceduralWing.Log($"Prefab \"{objects[i].name}\" loaded");
                            break;
                        case "MainEditorPanel":
                            UI_WindowPrefab = objects[i] as GameObject;
                            Base_ProceduralWing.Log($"Prefab \"{objects[i].name}\" loaded");
                            break;
                        case "PropertyGroup":
                            UI_PropertyGroupPrefab = objects[i] as GameObject;
                            Base_ProceduralWing.Log($"Prefab \"{objects[i].name}\" loaded");
                            break;
                        case "PropertySelector":
                            UI_PropertyPrefab = objects[i] as GameObject;
                            Base_ProceduralWing.Log($"Prefab \"{objects[i].name}\" loaded");
                            break;
                        case "PropertySelector_ValArray":
                            UI_PropertyValArrayPrefab = objects[i] as GameObject;
                            Base_ProceduralWing.Log($"Prefab \"{objects[i].name}\" loaded");
                            break;
                    }
                }
                yield return new WaitForSeconds(1.0f);
                Debug.Log("[B9PW] unloading bundle");
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
    }
}