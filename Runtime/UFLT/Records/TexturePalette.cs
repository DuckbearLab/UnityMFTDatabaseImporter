using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UFLT.Utils;

namespace UFLT.Records
{
	/// <summary>
	/// Record for each texture pattern referenced in the database. A texture
	/// palette is made up of 256 patterns. The pattern index for the first
	/// palette is 0 - 255, for the second palette 256 - 511, etc. 
	/// The x and y palette locations are used to store offset locations in 
	/// the palette for display
	/// </summary>
	public class TexturePalette
	{
		#region Properties

		/// <summary>
		/// Texture file path.
		/// </summary>
		public string FileName
		{
			get;
			set;
		}

		/// <summary>
		/// Index, position of texture in list.
		/// </summary>
		public int Index
		{
			get;
			set;
		}

		/// <summary>
		/// Offset location in the palette (x,y).
		/// </summary>
		public int[] Location
		{
			get;
			set;
		}

		/// <summary>
		/// Materials that use this texture. Key is material id, -1 for default material.
		/// </summary>		
		private Dictionary<int, Material> Materials
		{
			get;
			set;
		}

		#endregion Properties

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="parent"></param>
		public TexturePalette()
		{
		}

		/// <summary>
		/// Parses binary stream.
		/// </summary>
		/// <param name="db">Database that this vertex is part of.</param>
		public void Parse(Database db)
		{
			FileName = NullTerminatedString.GetAsString(db.Stream.Reader.ReadBytes(200));
			Index = db.Stream.Reader.ReadInt32();
			Location = new int[] { db.Stream.Reader.ReadInt32(), db.Stream.Reader.ReadInt32() };
		}

		/// <summary>
		/// Determines whether the specified <see cref="TexturePalette"/> is equal to the current <see cref="UFLT.Records.TexturePalette"/>.
		/// </summary>
		/// <param name='other'>
		/// The <see cref="TexturePalette"/> to compare with the current <see cref="UFLT.Records.TexturePalette"/>.
		/// </param>
		/// <returns>
		/// <c>true</c> if the specified <see cref="TexturePalette"/> is equal to the current
		/// <see cref="UFLT.Records.TexturePalette"/>; otherwise, <c>false</c>.
		/// </returns>
		public bool Equals(TexturePalette other)
		{
			// TODO: Filename could be relative for the current db, e.g if we are supporting multiple db using the same file. Need a way to check if this is so. Maybe convert to absolute addresses?

			if (!FileName.Equals(other.FileName)) return false;

			return true;
		}
	}
}