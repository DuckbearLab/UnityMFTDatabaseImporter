using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DatabaseImporter.Importers;
using System.IO;
using UFLT.MonoBehaviours;
using System.Linq;
using UFLT.Textures;
using System;

namespace DatabaseImporter.Editor
{

    public class DatabaseImporter : EditorWindow
    {

        private static string TerrainFolder = @"C:\DB\Terrain";

        [MenuItem("Database/1 - Import Terrain")]
        private static void ShowImportTerrainWizard()
        {
            EditorWindow.GetWindow<DatabaseImporter>().ShowPopup();
        }

        void OnGUI()
        {
            TerrainFolder = EditorGUILayout.TextField("TerrainFolder", TerrainFolder);
            if(GUILayout.Button("Import!"))
            {
                ImportTerrain();
                EditorWindow.GetWindow<DatabaseImporter>().Close();
            }
        }

        private static void ImportTerrain()
        {
            foreach(var gameObj in GameObject.FindObjectsOfType<GameObject>())
                GameObject.DestroyImmediate(gameObj);

            List<MftData> additionalsMfts = new List<MftData>();

            MftData terrainMft = LoadMft("ds100"); // Terrain
            MftData vtMft = LoadMft("ds200"); // VT
            TryAddMft(additionalsMfts, "ds101"); // Buildings
            TryAddMft(additionalsMfts, "ds102"); // Trees
            TryAddMft(additionalsMfts, "ds103"); // Bushes
            TryAddMft(additionalsMfts, "ds104"); // Walls
            TryAddMft(additionalsMfts, "ds107"); // Scene Adds

            var terrainImporter = new Importers.TerrainImporter(
                terrainMft,
                vtMft,
                additionalsMfts.ToArray()
            );
            terrainImporter.ParentTransform = new GameObject("terrain").transform;
            terrainImporter.MaterialToUse = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.duckbearlab.mftdabaseimporter/Editor/MftDatabaseImporter/TerrainMaterial.mat");
            terrainImporter.LoadedTerrainPart += HandleLoadedTerrainPart;
            terrainImporter.LoadedTerrain += HandleLoadedTerrain;
            terrainImporter.PrepareForLoad();
            terrainImporter.LoadAll();
        }

        private static void TryAddMft(List<MftData> additionalsMfts, string mftType)
        {
            try
            {
                additionalsMfts.Add(LoadMft(mftType));
            }
            catch (Exception e)
            {
                Debug.Log("Can't load mft " + mftType);
            }
        }

        private static MftData LoadMft(string mftType)
        {
            return MftData.FromFile(FindMft(mftType));
        }

        private static string FindMft(string type)
        {
            var arr = Directory.GetDirectories(TerrainFolder);
            var dir = Directory.GetDirectories(TerrainFolder).First((f) =>
            {
                return f.Contains(type);
            });
            var mft = Directory.GetFiles(dir).First((f) => (!f.Contains("vsb") && f.EndsWith(".mft")));
            return mft;
        }

        [MenuItem("Database/2 - Replace LODs")]
        private static void ReplaceLODs()
        {
            foreach (Transform trans in GameObject.Find("terrain").GetComponentsInChildren<Transform>(true))
                trans.gameObject.SetActive(true);

            foreach (var lod in GameObject.FindObjectsOfType<LevelOfDetail>())
            {
                if (lod)
                {
                    foreach (var parentLod in lod.GetComponentsInParent<LevelOfDetail>(true))
                    {
                        lod.SwitchInDistance = Mathf.Min(lod.SwitchInDistance, parentLod.SwitchInDistance);
                        lod.SwitchOutDistance = Mathf.Max(lod.SwitchOutDistance, parentLod.SwitchOutDistance);
                    }

                    if (lod.SwitchInDistance - lod.SwitchOutDistance < Mathf.Epsilon)
                        GameObject.DestroyImmediate(lod.gameObject);
                }
            }

            var lodParents = new Dictionary<Transform, List<float>>();

            foreach (var lod in GameObject.FindObjectsOfType<LevelOfDetail>())
            {
                var lodParent = GetParentLodGroup(lod.transform);
                if(lodParent != null)
                {
                    if (!lodParents.ContainsKey(lodParent))
                        lodParents[lodParent] = new List<float>();
                    if (!lodParents[lodParent].Contains(lod.SwitchInDistance))
                        lodParents[lodParent].Add(lod.SwitchInDistance);
                    if (!lodParents[lodParent].Contains(lod.SwitchOutDistance))
                        lodParents[lodParent].Add(lod.SwitchOutDistance);
                }

                if (lod.SwitchOutDistance > Mathf.Epsilon)
                    foreach (Collider collider in lod.transform.GetComponentsInChildren<Collider>(true))
                        GameObject.DestroyImmediate(collider);
            }

            foreach(var lodParent in lodParents)
            {
                var lodParentTransform = lodParent.Key;
                var lodParentLODs = lodParent.Value;

                if (!lodParentLODs.Contains(0))
                    lodParentLODs.Add(0);

                lodParentLODs.Sort();

                LOD[] LODs = new LOD[lodParentLODs.Count - 1];
                List<Renderer>[] renderers = new List<Renderer>[LODs.Length];
                for (int i = 0; i < LODs.Length; i++)
                    renderers[i] = new List<Renderer>();

                foreach (var renderer in lodParentTransform.GetComponentsInChildren<Renderer>())
                {
                    var levelOfDetail = GetLevelOfDetail(renderer);
                    if (levelOfDetail != null)
                    {
                        for (int i = 0; i < LODs.Length; i++)
                        {
                            if (lodParentLODs[i] >= levelOfDetail.SwitchOutDistance && lodParentLODs[i + 1] <= levelOfDetail.SwitchInDistance)
                            {
                                renderers[i].Add(renderer);
                            }
                        }
                    }
                }
                
                float baseNum = Mathf.Tan(60 * Mathf.Deg2Rad) / 2;

                for(int i = 0; i < LODs.Length; i++)
                {
                    LODs[i] = new LOD(baseNum / lodParentLODs[i + 1], renderers[i].ToArray());
                }


                var lodGroup = lodParentTransform.gameObject.AddComponent<LODGroup>();

                lodGroup.SetLODs(LODs);
                lodGroup.RecalculateBounds();

                for (int i = 0; i < LODs.Length; i++)
                    LODs[i].screenRelativeTransitionHeight *= lodGroup.size;

                lodGroup.SetLODs(LODs);
            }

            foreach (var lod in GameObject.FindObjectsOfType<LevelOfDetail>())
                GameObject.DestroyImmediate(lod);
        }

        private static LevelOfDetail GetLevelOfDetail(Renderer renderer)
        {
            var myLevelOfDetail = renderer.GetComponent<LevelOfDetail>();
            if (myLevelOfDetail != null)
                return myLevelOfDetail;
            return renderer.GetComponentInParent<LevelOfDetail>();
        }

        private static Transform GetParentLodGroup(Transform transform)
        {
            if (transform.name.StartsWith("Ref: ") || transform.parent.name == "terrain")
                return transform;
            else if (transform.parent != null)
                return GetParentLodGroup(transform.parent);
            else
                return null;
            
        }

        [MenuItem("Database/3 - Optimize Terrain")]
        private static void optimizeTerrain()
        {
            var terrainParent = GameObject.Find("terrain");
            if (terrainParent)
            {
                var putCopy = terrainParent.GetComponent<PutCopy>();

                foreach (var copyMarker in GameObject.FindObjectsOfType<PutCopyMarker>())
                {
                    putCopy.Copies.Add(new PutCopy.Details()
                    {
                        ToCopy = copyMarker.ToCopy,
                        Position = copyMarker.transform.position,
                        Rotation = copyMarker.transform.rotation
                    });
                    GameObject.DestroyImmediate(copyMarker.gameObject);
                }

                bool needToOptimizeAgain = true;
                while (needToOptimizeAgain)
                {
                    var transforms = terrainParent.GetComponentsInChildren<Transform>(true);
                    for (int i = 0; i < transforms.Length; i++)
                    {
                        transforms[i].gameObject.SetActive(true);
                    }

                    var lods = terrainParent.GetComponentsInChildren<LevelOfDetail>(true);
                    for (int i = 0; i < lods.Length; i++)
                    {
                        var lod = lods[i];
                        if (lod && lod.gameObject)
                        {
                            if (lod.SwitchOutDistance > Mathf.Epsilon)
                            {
                                //lod.gameObject.SetActive(false);
                                GameObject.DestroyImmediate(lod.gameObject);
                            }
                            else
                            {
                                lod.gameObject.SetActive(true);
                                GameObject.DestroyImmediate(lod);
                            }
                        }
                        /*if (transform.childCount == 0 && transform.GetComponent<MeshRenderer>() == null)
                        {
                            needToOptimizeAgain = true;
                            GameObject.DestroyImmediate(transform.gameObject);
                        }*/
                    }
                    Debug.Log("Done");

                    needToOptimizeAgain = false;
                    var transforms2 = terrainParent.GetComponentsInChildren<Transform>(true);
                    for (int i = 0; i < transforms2.Length; i++)
                    {
                        var transform = transforms2[i];

                        if (!transform)
                            continue;

                        TryReplaceWithTree(transform);

                        if (transform.childCount == 0 && transform.GetComponent<MeshRenderer>() == null && transform.GetComponent<PutTree>() == null && transform.GetComponent<PutCopy>() == null)
                        {
                            needToOptimizeAgain = true;
                            GameObject.DestroyImmediate(transform.gameObject);
                        }
                    }
                    Debug.Log("Need to optimize:" + needToOptimizeAgain);
                }
            }

        }

        private static void TryReplaceWithTree(Transform transform)
        {
            if (transform.GetComponent<PutTree>() != null)
                return;

            var trees = TerrainImportSettings.Instance.Trees;
            foreach (var treeNameGameObj in trees)
            {
                if (transform.name == treeNameGameObj.TreeName)
                {
                    foreach (Transform child in transform)
                        if (child != transform)
                            GameObject.DestroyImmediate(child.gameObject);

                    GameObject.Instantiate(treeNameGameObj.GameObj, transform.position, Quaternion.Euler(0, UnityEngine.Random.value * 360f, 0), transform);
                    return;
                }
            }
        }

        [MenuItem("Database/4 - Save Terrain")]
        private static void saveTerrain()
        {
            var terrainParent = GameObject.Find("terrain");
            if(terrainParent)
            {
                var meshFilters = terrainParent.GetComponentsInChildren<MeshFilter>(true);
                string meshAssetPath = AssetDatabase.GenerateUniqueAssetPath("Assets/DEL/terrain_Meshes.asset");
                Dictionary<string, Mesh> savedMeshes = new Dictionary<string, Mesh>();

                Mesh mainMeshAsset = meshFilters[0].sharedMesh;
                //////AssetDatabase.CreateAsset(mainMeshAsset, meshAssetPath);
                savedMeshes[GetRefPath(meshFilters[0].transform)] = mainMeshAsset;
                
                for (int i = 1; i < meshFilters.Length; ++i)
                {
                    var meshName = GetRefPath(meshFilters[i].transform);
                    if (savedMeshes.ContainsKey(meshName))
                    {
                        meshFilters[i].sharedMesh = savedMeshes[meshName];
                    }
                    else
                    {
                        //////AssetDatabase.AddObjectToAsset(meshFilters[i].sharedMesh, mainMeshAsset);
                        savedMeshes[GetRefPath(meshFilters[i].transform)] = meshFilters[i].sharedMesh;
                    }
                }

                string texturesAssetPath = AssetDatabase.GenerateUniqueAssetPath("Assets/DEL/terrain_Textures.asset");
                bool createdTexturesAsset = false;
                Dictionary<string, Texture> savedTextures = new Dictionary<string, Texture>();
                Dictionary<string, Material> savedMaterials = new Dictionary<string, Material>();
                var meshRenderers = terrainParent.GetComponentsInChildren<MeshRenderer>(true);
                int processedCount = 1;
                foreach (var renderer in meshRenderers)
                {
                    EditorUtility.DisplayProgressBar("Saving...", processedCount + "/" + meshRenderers.Length, (float)processedCount / meshRenderers.Length);
                    processedCount++;
                    for (int matI = 0; matI < renderer.sharedMaterials.Length; matI++/*var mat in renderer.sharedMaterials*/)
                    {
                        var mat = renderer.sharedMaterials[matI];
                        if(mat == null)
                        {
                            Debug.LogWarning("NULL MATERIAL??", renderer);
                            continue;
                        }
                        if (savedMaterials.ContainsKey(mat.name))
                        {
                            var prev = renderer.sharedMaterials;
                            prev[matI] = savedMaterials[mat.name];
                            renderer.sharedMaterials = prev;
                            continue;
                        }

                        try
                        {
                            Texture2D t = mat.mainTexture as Texture2D; // The connection to the texture will be lost when we create the asset so we will need to re-assign it
                            /*string name = ExportUtility.GetSafeFileName(mat.name, "mat");
                            string fileName = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(outDirRelative + "/Materials/", name));*/
                            /*if (Path.GetExtension(t.name) == ".dds")
                                mat.mainTextureScale = new Vector2(mat.mainTextureScale.x, -mat.mainTextureScale.y);*/
                            //////AssetDatabase.AddObjectToAsset(mat, mainMeshAsset);
                            savedMaterials[mat.name] = mat;

                            //AssetDatabase.CreateAsset(mat, fileName);
                            if (t != null)
                            {
                                Texture assetTex = null;
                                if (savedTextures.TryGetValue(t.name, out assetTex))
                                {
                                    mat.mainTexture = assetTex;
                                }
                                else
                                {
                                    //assetTex = ExportUtility.SaveTextureToDisc(t, exportDirectory);

                                    string path = "Assets/DEL/" + Path.GetFileName(t.name);
                                    {
                                        /*if (Path.GetExtension(t.name) == ".dds")
                                        {
                                            File.Copy(t.name, path);
                                        }
                                        else
                                        {
                                            path += ".png";
                                            File.WriteAllBytes(path, t.EncodeToPNG());

                                        }
                                        (AssetImporter.GetAtPath(path) as TextureImporter).alphaIsTransparency = true;*/
                                        EditorUtility.CompressTexture(t, TextureFormat.DXT5, TextureCompressionQuality.Normal);
                                        t.hideFlags = HideFlags.None;
                                        if(!createdTexturesAsset)
                                        {
                                            //////AssetDatabase.CreateAsset(t, texturesAssetPath);
                                            createdTexturesAsset = true;
                                        }
                                        else
                                        {
                                            //////AssetDatabase.AddObjectToAsset(t, texturesAssetPath);
                                        }
                                        //AssetDatabase.ImportAsset(path);
                                    }

                                    //Texture o = AssetDatabase.LoadAssetAtPath<Texture>(path);

                                    //AssetDatabase.AddObjectToAsset(assetTex, mainMeshAsset);
                                    //mat.mainTexture = o;
                                    savedTextures[t.name] = t;
                                }
                            }
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError("Failed on material: " + mat.ToString());
                            Debug.LogException(e);
                        }
                    }
                }

                AddTextureSwitches(savedMaterials);

                EditorUtility.ClearProgressBar();
            }
        }

        private static string GetRefPath(Transform transform)
        {
            string result = transform.GetSiblingIndex() + "_" + transform.gameObject.name;
            while(transform.parent)
            {
                transform = transform.parent;
                if (transform.gameObject.name.StartsWith("Ref: "))
                {
                    result = transform.gameObject.name + "/" + result;
                    break;
                }
                else
                {
                    result = transform.GetSiblingIndex() + "_" + transform.gameObject.name + "/" + result;
                }
            }
            return result;
        }

        private static void AddTextureSwitches(Dictionary<string, Material> savedMaterials)
        {
            var copies = GameObject.FindObjectOfType<PutCopy>();

            string texturesFolder = TerrainFolder;
            string texturesFile = Path.Combine(texturesFolder, "textures.txt");
            
            if(!File.Exists(texturesFile))
            {
                Debug.Log("textures.txt missing!");
            }

            var str = File.ReadAllText(texturesFile);
            var arr = str.Split(new char[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var a in arr)
            {
                var arr2 = a.Split('\t');

                var group = int.Parse(arr2[0]);

                if (group == 0)
                    continue;

                var texturePath = Path.Combine(texturesFolder, Path.GetFileName(arr2[4]));

                if (!File.Exists(texturePath))
                    continue;

                Material material;

                if(!savedMaterials.TryGetValue(texturePath, out material))
                {
                    material = new Material(savedMaterials.First().Value);
                    material.mainTexture = new TextureSGI(texturePath).Texture;
                    material.name = texturePath;
                    savedMaterials[texturePath] = material;
                }

                copies.TextureSwitches.Add(new PutCopy.TextureSwitch()
                {
                    Group = group,
                    Material = material
                });
            }
        }

        [MenuItem("Database/End - Save Terrain Bundle")]
        private static void saveTerrainBundle()
        {
            BuildPipeline.BuildAssetBundles("Assets/OutputAssetBundles", BuildAssetBundleOptions.UncompressedAssetBundle, BuildTarget.StandaloneWindows);
        }

        [MenuItem("Database/Open Settings")]
        private static void OpenSettings()
        {
            Selection.activeObject = TerrainImportSettings.Instance;
        }

        static void HandleLoadedTerrainPart(int partsLeft, int total)
        {
            if (partsLeft > 0)
            {
                var index = total - partsLeft;
                EditorUtility.DisplayProgressBar("Loading terrain", index.ToString() + " / " + total.ToString(), (float) index / total);
            }
            else
                EditorUtility.ClearProgressBar();
        }

        static void HandleLoadedTerrain()
        {
            EditorUtility.ClearProgressBar();
        }

    }

}
