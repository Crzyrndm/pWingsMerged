using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ProceduralWings
{
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class StaticWingGlobals : MonoBehaviour
    {
        public static List<WingTankConfiguration> wingTankConfigurations = new List<WingTankConfiguration>();

        public static Shader B9WingShader;

        public static GameObject UI_WindowPrefab;

        public static GameObject UI_PropertyGroupPrefab;

        public static GameObject UI_PropertyPrefab;

        public static GameObject UI_FuelPanel;

        public void Start()
        {
            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("ProceduralWingFuelSetups"))
            {
                ConfigNode[] fuelNodes = node.GetNodes("FuelSet");
                for (int i = 0; i < fuelNodes.Length; ++i)
                    wingTankConfigurations.Add(new WingTankConfiguration(fuelNodes[i]));
            }
            StartCoroutine(LoadBundleAssets());
        }

        public IEnumerator LoadBundleAssets()
        {
            Debug.Log("[B9PW] Aquiring bundle data");
            using (WWW www = new WWW("file://" + KSPUtil.ApplicationRootPath + Path.DirectorySeparatorChar + "GameData"
                                                                            + Path.DirectorySeparatorChar + "B9_Aerospace_ProceduralWings" + Path.DirectorySeparatorChar + "wingshader.ksp"))
            {
                yield return www;
                Debug.Log($"[B9PW] {www.error}");
                AssetBundle shaderBundle = www.assetBundle;
                Shader[] shaders = shaderBundle.LoadAllAssets<Shader>();
                for (int i = 0; i < shaders.Length; ++i)
                {
                    Debug.Log($"[B9PW] {shaders[i].name}");
                    switch (shaders[i].name)
                    {
                        case "KSP/Specular Layered":
                            B9WingShader = shaders[i] as Shader;
                            Debug.Log($"[B9PW] Wing shader \"{shaders[i].name}\" loaded. Supported? {B9WingShader.isSupported}");
                            break;
                    }
                }

                GameObject[] objects = shaderBundle.LoadAllAssets<GameObject>();
                Debug.Log(objects);
                Debug.Log(objects.Length);
                for (int i = 0; i < objects.Length; ++i)
                {
                    Debug.Log($"[B9PW] {objects[i].name}");
                    switch (objects[i].name)
                    {
                        case "FuelPanelPrefab":
                            UI_FuelPanel = objects[i] as GameObject;
                            Debug.Log($"[B9PW] Prefab \"{objects[i].name}\" loaded");
                            break;
                        case "MainEditorPanel":
                            UI_WindowPrefab = objects[i] as GameObject;
                            Debug.Log($"[B9PW] Prefab \"{objects[i].name}\" loaded");
                            break;
                        case "PropertyGroup":
                            UI_PropertyGroupPrefab = objects[i] as GameObject;
                            Debug.Log($"[B9PW] Prefab \"{objects[i].name}\" loaded");
                            break;
                        case "PropertySelector":
                            UI_PropertyPrefab = objects[i] as GameObject;
                            Debug.Log($"[B9PW] Prefab \"{objects[i].name}\" loaded");
                            break;
                    }
                }

                yield return null; // unknown how neccesary this is
                yield return null; // unknown how neccesary this is

                Debug.Log("[B9PW] unloading bundle");
                shaderBundle.Unload(false); // unload the raw asset bundle
            }
        }
    }
}