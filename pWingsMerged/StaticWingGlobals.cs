using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using KSP;

namespace ProceduralWings
{
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class StaticWingGlobals : MonoBehaviour
    {
        public static List<Fuel.WingTankConfiguration> wingTankConfigurations = new List<Fuel.WingTankConfiguration>();

        public static Shader B9WingShader;

        public static GameObject UI_WindowPrefab;
        public static GameObject UI_PropertyGroupPrefab;
        public static GameObject UI_PropertyPrefab;
        public static GameObject UI_PropertyValArrayPrefab;
        public static GameObject UI_FuelPanel;

        public static Rect uiRectWindowEditor = new Rect();

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
            ConfigNode node = ConfigNode.Load(KSPUtil.ApplicationRootPath + Path.DirectorySeparatorChar + "GameData" + Path.DirectorySeparatorChar + "PWingsPlugin" + Path.DirectorySeparatorChar + "PluginData" + Path.DirectorySeparatorChar + "settings.cfg");
            node = node.GetNode("PWingsSettings");
            if (node == null)
                return;

            if (node.HasValue("keyTranslation"))
                Base_ProceduralWing.keyTranslation = (KeyCode)Enum.Parse(typeof(KeyCode), node.GetValue("keyTranslation"), true);

            if (node.HasValue("keyTipScale"))
                Base_ProceduralWing.keyTipScale = (KeyCode)Enum.Parse(typeof(KeyCode), node.GetValue("keyTipScale"), true);

            if (node.HasValue("keyRootScale"))
                Base_ProceduralWing.keyRootScale = (KeyCode)Enum.Parse(typeof(KeyCode), node.GetValue("keyRootScale"), true);

            if (node.HasValue("moveSpeed"))
                float.TryParse(node.GetValue("moveSpeed"), out Base_ProceduralWing.moveSpeed);

            if (node.HasValue("scaleSpeed"))
                float.TryParse(node.GetValue("scaleSpeed"), out Base_ProceduralWing.scaleSpeed);

            if (!node.TryGetValue(nameof(uiRectWindowEditor), ref uiRectWindowEditor))
                uiRectWindowEditor = Utility.UIUtility.SetToScreenCenter(new Rect());

            Base_ProceduralWing.loadedConfig = true;
        }

        public static void SaveConfigs()
        {
            ConfigNode node = new ConfigNode();
            ConfigNode settings = node.AddNode("PWingsSettings");
            settings.AddValue("keyTranslation", Base_ProceduralWing.keyTranslation.ToString());
            settings.AddValue("keyTipScale", Base_ProceduralWing.keyTipScale.ToString());
            settings.AddValue("keyRootScale", Base_ProceduralWing.keyRootScale.ToString());
            settings.AddValue("moveSpeed", Base_ProceduralWing.moveSpeed);
            settings.AddValue("scaleSpeed", Base_ProceduralWing.scaleSpeed);
            settings.AddValue(nameof(uiRectWindowEditor), uiRectWindowEditor);

            node.Save(KSPUtil.ApplicationRootPath + Path.DirectorySeparatorChar + "GameData" + Path.DirectorySeparatorChar + "PWingsPlugin" + Path.DirectorySeparatorChar + "PluginData" + Path.DirectorySeparatorChar + "settings.cfg");
        }
    }
}