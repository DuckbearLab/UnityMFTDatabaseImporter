using UnityEngine;
using System.Collections.Generic;
using UFLT.Records;
using UFLT.DataTypes.Enums;
using System.IO;
using UFLT.Textures;
using System.Collections;
using System.Reflection;

namespace UFLT.Utils
{
	/// <summary>
	/// Class for storing materials & textures that are created based on multiple records from the OpenFlight file/s.
	/// We cannot rely on the Material palette to provide our materials, it does not take into account the texture or lighting used.
	/// Advanced materials such as reflective, cube maps etc require additional data from extended materials.
	/// All of these different records/fields can impact the type of material/shader we need, this class brings it all together.    
	/// Helps reduce the number of materials & textures used by re-using materials/textures over multiple databases.    
	/// </summary>
	public class MaterialBank
	{
		#region Properties

		/// <summary>
		/// Current materials.		
		/// </summary>		
		public List<IntermediateMaterial> Materials
		{
			get;
			set;
		}

		// Known textures, key is <b>absolute</b> file path.
		private Dictionary<string, Texture2D> _Textures = new Dictionary<string, Texture2D>();

		#endregion Properties

		/// <summary>
		/// Constructor
		/// </summary>
		public MaterialBank()
		{
			Materials = new List<IntermediateMaterial>();
		}

		/// <summary>
		/// Finds the or create material. Thread safe.
		/// </summary>
		/// <returns>
		/// The found or created material, never returns null.
		/// </returns>
		/// <param name='f'>The face to find a material for.</param>
		public IntermediateMaterial FindOrCreateMaterial(Face f)
		{
			// TODO: A faster lookup data structure, currently using a linear search.

			// Fetch palettes
			// Fetch palettes
			MaterialPalette mp = null;
			if (f.MaterialIndex != -1)
			{
				if (!f.Header.MaterialPalettes.TryGetValue(f.MaterialIndex, out mp))
					f.Log.WriteError("FindOrCreateMaterial:Could not find material palette: " + f.MaterialIndex);
			}

			TexturePalette mainTex = null;
			if (f.TexturePattern != -1)
			{
				if (!f.Header.TexturePalettes.TryGetValue(f.TexturePattern, out mainTex))
					f.Log.WriteError("FindOrCreateMaterial:Could not find texture palette: " + f.TexturePattern);
			}

			TexturePalette detailTex = null;
			if (f.DetailTexturePattern != -1)
			{
				if (!f.Header.TexturePalettes.TryGetValue(f.DetailTexturePattern, out detailTex))
					f.Log.WriteError("FindOrCreateMaterial:Could not find detail texture palette: " + f.DetailTexturePattern);
			}

			lock (this)
			{
				foreach (IntermediateMaterial current in Materials)
				{
					if (current.Equals(mp, mainTex, detailTex, f.Transparency, f.LightMode))
					{
						// We found a matching material
						return current;
					}
				}

				// Create a new material
				IntermediateMaterial im = new IntermediateMaterial(this, mp, mainTex, detailTex, f.Transparency, f.LightMode);
				Materials.Add(im);
				return im;
			}
		}

		/// <summary>
		/// Loads all material textures using a coroutine.
		/// </summary>
		public IEnumerator LoadTextures()
		{
			foreach (IntermediateMaterial im in Materials)
			{
				string path = FileFinder.Instance.Find(im.MainTexture.FileName);
				if (path != string.Empty)
				{
					// Have we already loaded this texture?					
					if (_Textures.ContainsKey(path))
					{
						// Dont need to load it.
						break;
					}

					string ext = Path.GetExtension(path);
					if (ext == ".rgb" ||
						ext == ".rgba" ||
						ext == ".bw" ||
						ext == ".int" ||
						ext == ".inta" ||
						ext == ".sgi")
					{
						TextureSGI sgi = new TextureSGI(path);
						Texture2D tex = sgi.Texture;
						if (tex != null)
						{
							_Textures[path] = tex;
							tex.name = Path.GetFileNameWithoutExtension(path);
							yield return tex;
						}
					}
					else
					{
						WWW www = new WWW("file://" + path);
						yield return www;

						if (www.error == null && www.texture != null)
						{
							_Textures[path] = www.texture;
                            // DuckbearLab: FIX!
							//_Textures[path].hideFlags = HideFlags.DontSave;
							www.texture.name = Path.GetFileNameWithoutExtension(path);

							yield return www.texture;
						}
						else
						{
							Debug.LogError(www.error);
						}
					}
				}
			}
		}

		/// <summary>
		/// Finds the texture if it has already been loaded else loads
		/// the new texture and records it for future re-use.
		/// </summary> Texture will be loaded in the same thread.
		/// <returns>
		/// Found texture or null if it can not be found/loaded.
		/// </returns>
		/// <param name='tp'>Tp.</param>
		public Texture2D FindOrCreateTexture(TexturePalette tp)
		{
			// Find the texture 
			// TODO: Make the path absolute?	
			string path = FileFinder.Instance.Find(tp.FileName);
			if (path != string.Empty)
			{
				// Have we already loaded this texture?
				Texture2D tex = null;
				// TODO: Maybe hash the paths for faster lookup?
				if (_Textures.TryGetValue(path, out tex))
				{
					// We found it!
					return tex;
				}

				string ext = Path.GetExtension(path);
				if (ext == ".rgb" ||
					ext == ".rgba" ||
					ext == ".bw" ||
					ext == ".int" ||
					ext == ".inta" ||
					ext == ".sgi")
				{
					TextureSGI sgi = new TextureSGI(path);
					tex = sgi.Texture;
					if (tex != null)
					{
                        // DuckbearLab: FIX!
						//_Textures[path].hideFlags = HideFlags.DontSave;
                        // DuckbearLab: FIX!
						//tex.name = Path.GetFileNameWithoutExtension(path);
                        tex.name = Path.GetFullPath(path);

						_Textures[path] = tex;

						return tex;
					}
				}
				else
				{
					WWW www = new WWW("file://" + path);
					while (!www.isDone)
					{
					}

					if (string.IsNullOrEmpty(www.error) && www.texture != null)
					{
						
                        // DuckbearLab: FIX!
						//www.texture.hideFlags = HideFlags.DontSave;
                        // DuckbearLab: FIX!
						//www.texture.name = Path.GetFileNameWithoutExtension(path);
                        //www.texture.name = Path.GetFullPath(path);

                        var resultTexture = www.texture;
                        resultTexture.name = Path.GetFullPath(path);

                        _Textures[path] = resultTexture;

						// TODO: You are here!!!!
						/*FieldInfo fi = www.texture.GetType().GetField("m_Name", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.IgnoreCase);
						if (fi != null) Debug.Log("FOund it ");*/
						//myFieldInfo.SetValue( www.texture, "TEST" );					
						//Debug.Log( myFieldInfo.GetValue( www.texture ) );


						return resultTexture;
					}
					else
					{
						Debug.LogError(www.error);
					}
				}
			}
			return null;
		}
	}
}