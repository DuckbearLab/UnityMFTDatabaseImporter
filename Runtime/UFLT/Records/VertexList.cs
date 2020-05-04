using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UFLT.Records
{
	/// <summary>
	/// A vertex list record is the primary record of a vertex node. 
	/// Each record references one or more vertices in the vertex palette.
	/// A vertex node is a leaf node in the database and therefore cannot have any children.
	/// </summary>
	public class VertexList : Record
	{
		#region Properties

		/// <summary>
		/// A list of offset values used to reference vertices in the VertexPalette.
		/// </summary>
		public List<int> Offsets
		{
			get;
			set;
		}

		#endregion Properties

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="parent"></param>
		public VertexList(Record parent) :
			base(parent, parent.Header)
		{
		}

		/// <summary>
		/// Parses binary stream.
		/// </summary>
		public override void Parse()
		{
			Offsets = new List<int>();

			int numVerts = (Length - 4) / 4;
			for (int i = 0; i < numVerts; ++i)
			{
				int v = Header.Stream.Reader.ReadInt32();

				// Error check, make sure the offset is in the palette.
				if (Header.VertexPalette.Vertices.ContainsKey(v))
				{
					Offsets.Add(v);
				}
				else
				{
					Log.WriteWarning(string.Format("Unable to find vertex for byte offset {0} in the palette, this vertex will be ignored.", v));
				}
			}
		}
	}
}