using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class TerrainImportSettings : ScriptableObject {

    [System.Serializable]
    public struct TreeNameGameObj
    {
        public string TreeName;
        public GameObject GameObj;
    }

    public TreeNameGameObj[] Trees;

#if UNITY_EDITOR
    public static TerrainImportSettings Instance
    {
        get
        {
            string settingsPath = "Assets/MftDatabaseImporter/Settings.asset";

            var settingsAsset = AssetDatabase.LoadAssetAtPath<TerrainImportSettings>(settingsPath);
            if (!settingsAsset)
            {
                if (!Directory.Exists(Path.GetDirectoryName(settingsPath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(settingsPath));

                AssetDatabase.CreateAsset(TerrainImportSettings.CreateInstance<TerrainImportSettings>(), settingsPath);
                settingsAsset = AssetDatabase.LoadAssetAtPath<TerrainImportSettings>(settingsPath);
            }

            return settingsAsset;
        }
    }
#endif
}
