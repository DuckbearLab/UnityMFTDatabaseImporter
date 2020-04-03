using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TerrainImportSettings : ScriptableObject {

    [System.Serializable]
    public struct TreeNameGameObj
    {
        public string TreeName;
        public GameObject GameObj;
    }

    public TreeNameGameObj[] Trees;

    public static TerrainImportSettings Instance
    {
        get
        {
            string settingPath = "Packages/com.duckbearlab.mftdabaseimporter/Editor/MftDatabaseImporter/Settings.asset";

            var settingsAsset = AssetDatabase.LoadAssetAtPath<TerrainImportSettings>(settingPath);
            if (!settingsAsset)
            {
                AssetDatabase.CreateAsset(TerrainImportSettings.CreateInstance<TerrainImportSettings>(), "Packages/com.duckbearlab.mftdabaseimporter/Editor/MftDatabaseImporter/Settings.asset");
                settingsAsset = AssetDatabase.LoadAssetAtPath<TerrainImportSettings>(settingPath);
            }

            return settingsAsset;
        }
    }
}
