using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UFLT.Records;

namespace UFLT.Utils
{
	/// <summary>
	/// Stores all file paths used in an openflight in order to help locate files that are not where they are supposed to be, e.g textures.
	/// Some file references are absolute so as soon as the file is moved they are invalid, this helps find those files.
	/// </summary>
	public class FileFinder
	{
		#region Properties

		#region Private

		/// <summary>
		/// All known file paths to be checked.
		/// </summary>
		private static List<string> _Paths
		{
			get;
			set;
		}

		private static FileFinder _Instance;

		#endregion

		/// <summary>
		/// Singleton instance
		/// </summary>        
		public static FileFinder Instance
		{
			get
			{
				if (_Instance == null)
				{
					_Instance = new FileFinder();
				}

				return _Instance;
			}
		}

		#endregion Properties

		/// <summary>
		/// Ctor
		/// </summary>
		private FileFinder()
		{
			_Paths = new List<string>();
		}

		/// <summary>
		/// Adds a path to the file finder, this path will now be checked when searching for missing files.
		/// </summary>
		/// <param name="fileName"></param>
		public void AddPath(string fileName)
		{
			string dir = Path.GetDirectoryName(fileName);
			if (!_Paths.Contains(dir)) // Dont add duplicates            
				_Paths.Add(dir);
		}

		/// <summary>
		/// Clears all known paths.
		/// </summary>
		public void ClearPaths()
		{
			_Paths.Clear();
		}

		/// <summary>
		/// Checks if the file exists, if not searches for a file with the same name using all known paths.
		/// Returns the path if found else returns an empty string.
		/// </summary>
		/// <param name="full_path"></param>
		/// <returns></returns>
		public string Find(string fullPath)
		{
			// TODO: Always return an absolute path, we can then check if a texture is already loaded without
			// worrying about relative paths which may be different but point to the same file.            							

			// Is the path absolute?			
			if (Path.IsPathRooted(fullPath))
			{
				if (File.Exists(fullPath))
				{
					// Make sure the path is cleaned up, no ./, ../ etc
					string clean = Path.GetFullPath(fullPath);
					AddPath(clean);
					return clean;
				}
			}
			else
			{
				// Try the relative path against our list of paths.
				foreach (string currentPath in _Paths)
				{
					string combPath = Path.Combine(currentPath, fullPath);
					if (File.Exists(combPath))
					{
						// Make sure the path is cleaned up, no ./, ../ etc
						string clean = Path.GetFullPath(combPath);
						AddPath(clean);
						return clean;
					}
				}
			}

			// Search previous directories that have worked.
			foreach (string currentPath in _Paths)
			{
				string file = Path.GetFileName(fullPath);
				string combPath = Path.Combine(currentPath, file);
				if (File.Exists(combPath))
				{
					// Make sure the path is cleaned up, no ./, ../ etc
					string clean = Path.GetFullPath(combPath);
					AddPath(clean);
					return clean;
				}
			}

            // DuckbearLab: FIX! Stop being annoying about vt_sub missing
            if (fullPath != "vt_sub.rgb")
			    Debug.LogWarning("Could not find file: " + fullPath);

			return string.Empty;
		}
	}
}