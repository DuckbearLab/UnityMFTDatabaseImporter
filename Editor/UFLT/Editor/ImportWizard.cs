using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Collections.Generic;

namespace UFLT.Editor
{
	public class ImportWizard : ScriptableWizard
	{
		#region Properties

		// Labels
		GUIContent fltFileLbl = new GUIContent("File(.flt)", "The root openflight database file.");
		GUIContent outputDirLbl = new GUIContent("Output Directory", "Where to save the converted file and its dependencies(Materials/Textures). Must be inside the Unity project");

		// The full file path to the flt file
		string openflightFile = EditorPrefs.GetString("uflt-importWiz-lastFile", "");
		string exportDirectory = Application.dataPath;

		bool loadIntoScene = true;

		// Our import settings.
		public ImportSettings settings = new ImportSettings();

		Vector2 logScroll;
		string log;

		#endregion

		[MenuItem("Assets/Import OpenFlight(.flt)")]
		static void CreateWizard()
		{
			ScriptableWizard.DisplayWizard<ImportWizard>("Import Openflight", "Import");
		}

		#region GUI

		private void OnGUI()
		{
			// Title
			GUILayout.Label("Import & Convert Openflight Settings", EditorStyles.boldLabel);
			GUILayout.Space(20);

			// Settings
			EditorGUILayout.BeginVertical(GUI.skin.box);

			FileSelectionField();
			FileExportDirectoryField();

			GUILayout.Space(10);
			EditorGUILayout.EndVertical();

			if (!string.IsNullOrEmpty(log))
			{
				logScroll = EditorGUILayout.BeginScrollView(logScroll);
				GUILayout.Label(log);
				EditorGUILayout.EndScrollView();
			}
			else if (GUILayout.Button("Start Import"))
				OnWizardCreate();
		}

		private void FileSelectionField()
		{
			// Store the default GUI color so we can revert back.
			Color defaultGUICol = GUI.contentColor;

			EditorGUILayout.BeginHorizontal();
			EditorGUI.BeginChangeCheck();

			EditorGUILayout.LabelField(fltFileLbl, GUILayout.Width(100));
			openflightFile = EditorGUILayout.TextField(openflightFile, GUILayout.ExpandWidth(true));

			// Select file dlg
			if (GUILayout.Button("...", EditorStyles.toolbarButton, GUILayout.Width(50)))
			{
				string selectedFile = EditorUtility.OpenFilePanel("Import OpenFlight", Application.dataPath, "flt");
				if (!string.IsNullOrEmpty(selectedFile))
				{
					openflightFile = selectedFile;
				}
			}
			EditorGUILayout.EndHorizontal();
		}

		private void FileExportDirectoryField()
		{
			// Store the default GUI color so we can revert back.
			Color defaultGUICol = GUI.contentColor;

			EditorGUILayout.BeginHorizontal();
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.LabelField("Output Directory", GUILayout.Width(100));
			exportDirectory = EditorGUILayout.TextField(exportDirectory, GUILayout.ExpandWidth(true));

			// Select dir dlg
			if (GUILayout.Button("...", EditorStyles.toolbarButton, GUILayout.Width(50)))
			{
				string outDir = EditorUtility.SaveFolderPanel("Save Asset", Application.dataPath, "Converted OpenFlight");
				if (!string.IsNullOrEmpty(outDir))
				{
					exportDirectory = outDir;
					AssetDatabase.Refresh();
				}
			}
			EditorGUILayout.EndHorizontal();
		}

		#endregion GUI

		void OnWizardCreate()
		{
			UFLT.Records.Database db = new Records.Database(openflightFile);
			EditorUtility.DisplayProgressBar("Importing", "Parsing flt file", 0);
			db.Parse();
			EditorUtility.DisplayProgressBar("Importing", "Preparing for import", 0.2f);
			db.PrepareForImport();
			EditorUtility.DisplayProgressBar("Importing", "Importing into scene", 0.4f);
			db.ImportIntoScene();
			
			log = "Loading completed.\nDetails:\n" + db.Log.ToString();

			// Make relative
			string fltName = Path.GetFileNameWithoutExtension(openflightFile);
			string outDirRelative = ExportUtility.MakePathRelative(exportDirectory);

			// Create materials directory
			if(!AssetDatabase.IsValidFolder(exportDirectory + "/Materials"))
			{				
				AssetDatabase.CreateFolder(outDirRelative, "Materials"); // Create materials dir	
				AssetDatabase.Refresh(); // Refresh for new directories that may have been created.
			}

			// Create our asset/s														
			EditorUtility.DisplayProgressBar("Importing", "Saving meshes", 0.5f);
			var meshFilters = db.UnityGameObject.GetComponentsInChildren<MeshFilter>();			
			if(meshFilters.Length > 0)
			{
				string meshAssetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(outDirRelative + "/", fltName + "_Meshes.asset"));
				Mesh mainMeshAsset = meshFilters[0].sharedMesh;
				AssetDatabase.CreateAsset(mainMeshAsset, meshAssetPath);
				for (int i = 1; i < meshFilters.Length; ++i)
				{
					AssetDatabase.AddObjectToAsset(meshFilters[i].sharedMesh, mainMeshAsset);
				}
			}

			EditorUtility.DisplayProgressBar("Importing", "Saving materials and textures", 0.6f);
			Dictionary<int, Texture> savedTextures = new Dictionary<int, Texture>();
			Dictionary<int, Material> savedMaterials = new Dictionary<int, Material>();
			var meshRenderers = db.UnityGameObject.GetComponentsInChildren<MeshRenderer>();
			foreach(var renderer in meshRenderers)
			{
				foreach(var mat in renderer.sharedMaterials)
				{
					if (savedMaterials.ContainsKey(mat.GetInstanceID()))
						continue;

					Texture t = mat.mainTexture; // The connection to the texture will be lost when we create the asset so we will need to re-assign it
					string name = ExportUtility.GetSafeFileName(mat.name, "mat");					
					string fileName = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(outDirRelative + "/Materials/", name));
					savedMaterials[mat.GetInstanceID()] = mat;

					try
					{
						AssetDatabase.CreateAsset(mat, fileName);
						if (t != null)
						{
							Texture assetTex = null;
							if (savedTextures.TryGetValue(t.GetInstanceID(), out assetTex))
							{
								mat.mainTexture = assetTex;
							}
							else
							{
								assetTex = ExportUtility.SaveTextureToDisc(t, exportDirectory);
								mat.mainTexture = assetTex;
								savedTextures[t.GetInstanceID()] = assetTex;
							}
						}
					}
					catch(System.Exception e)
					{
						Debug.LogError("Failed on material: " + mat.ToString());
						Debug.LogException(e);
					}
				}				
			}

			EditorUtility.DisplayProgressBar("Importing", "Creating prefab", 0.9f);
			GameObject prefab = PrefabUtility.CreatePrefab(Path.Combine(outDirRelative, fltName + ".prefab").Replace("\\", "/"), db.UnityGameObject);
			EditorUtility.ClearProgressBar();

			// Create a prefab from the asset			
			AssetDatabase.SaveAssets();

			DestroyImmediate(db.UnityGameObject);
			EditorUtility.UnloadUnusedAssetsImmediate(); // cleanup old textures

			if(loadIntoScene)
			{
				PrefabUtility.InstantiatePrefab(prefab);
			}

			EditorPrefs.SetString("uflt-importWiz-lastFile", openflightFile);
		}
	}
}