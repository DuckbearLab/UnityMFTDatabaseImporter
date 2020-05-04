using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;

namespace UFLT.Records
{
	/// <summary>
	/// A vertex with color & normal data.
	/// </summary>
	public class VertexWithColorNormal : VertexWithColor
	{
		#region Properties

		/// <summary>
		/// x,y,z normal of vertex.
		/// </summary>
		public Vector3 Normal
		{
			get;
			set;
		}

		#endregion Properties

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="parent"></param>
		public VertexWithColorNormal()
		{
		}

		/// <summary>
		/// Parses binary stream.
		/// </summary>
		public override void Parse(Database db)
		{
			ColorNameIndex = db.Stream.Reader.ReadUInt16();
			Flags = db.Stream.Reader.ReadInt16();
            // DuckbearLab: FIX! Inverted X
			Coordinate = new double[3] { -db.Stream.Reader.ReadDouble(), db.Stream.Reader.ReadDouble(), db.Stream.Reader.ReadDouble() };
			Normal = new Vector3(-db.Stream.Reader.ReadSingle(), db.Stream.Reader.ReadSingle(), db.Stream.Reader.ReadSingle());

			Color32 c = new Color32();
			c.a = db.Stream.Reader.ReadByte();
			c.b = db.Stream.Reader.ReadByte();
			c.g = db.Stream.Reader.ReadByte();
			c.r = db.Stream.Reader.ReadByte();
			PackedColor = c;

			VertexColorIndex = db.Stream.Reader.ReadUInt32();
		}
	}
}