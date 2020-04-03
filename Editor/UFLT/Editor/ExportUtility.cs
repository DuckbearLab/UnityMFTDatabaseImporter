using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

namespace UFLT.Editor
{
	public class ExportUtility
	{
		public static string MakePathRelative(string abs)
		{
			return "Assets" + abs.Replace(Application.dataPath, "");
		}

		/// <summary>
		/// Sometimes an object wont have a name or it will be invalid so we need to give it one, uses Guid to create a name that wont clash.
		/// </summary>
		/// <param name="file">Filename or empty</param>
		/// <param name="extension">Extension not including the dot.</param>
		/// <returns></returns>
		public static string GetSafeFileName(string file, string extension)
		{
			if (string.IsNullOrEmpty(file) || file.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
				file = System.Guid.NewGuid().ToString();
			return file + "." + extension;
		}

		/// <summary>
		/// Saves the texture as a png, imports the new texture and return the imported texture.
		/// </summary>
		/// <param name="t"></param>
		/// <param name="dir"></param>
		/// <returns></returns>
		public static Texture SaveTextureToDisc(Texture t, string dir)
		{
			Texture2D tex2D = t as Texture2D;
			if (tex2D)
			{
				string name = GetSafeFileName(t.name, "png");
				string path = Path.Combine(dir, name);
				string outFileRelative = MakePathRelative(path);
				if (!File.Exists(path)) // Does the file already exist?
				{
					byte[] bytes = tex2D.EncodeToPNG();
					File.WriteAllBytes(path, bytes);
					AssetDatabase.ImportAsset(outFileRelative);
				}

				Object o = AssetDatabase.LoadAssetAtPath(outFileRelative, typeof(Texture));
				if (o != null)
				{
					//Object.DestroyImmediate(t); // Dont destroy it, if its a shared texture we lose connection in the other materials.
					return o as Texture;
				}
			}

			return t;
		}
	}
}