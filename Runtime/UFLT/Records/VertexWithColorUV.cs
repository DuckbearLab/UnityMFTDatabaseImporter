using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;

namespace UFLT.Records
{
	/// <summary>
	/// A vertex with color & UV data.
	/// </summary>
	public class VertexWithColorUV : VertexWithColor
	{
		#region Properties

		/// <summary>
		/// x,y uv texture coordinates.
		/// </summary>
		public Vector2 UV
		{
			get;
			set;
		}

		#endregion Properties

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="parent"></param>
		public VertexWithColorUV()
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
            double x = db.Stream.Reader.ReadDouble(), z = db.Stream.Reader.ReadDouble(), y = db.Stream.Reader.ReadDouble();
            Coordinate = new double[3] { x, y, z };
            UV = new Vector2(db.Stream.Reader.ReadSingle(), db.Stream.Reader.ReadSingle());

			Color32 c = new Color32();
			c.a = db.Stream.Reader.ReadByte();
			c.b = db.Stream.Reader.ReadByte();
			c.g = db.Stream.Reader.ReadByte();
			c.r = db.Stream.Reader.ReadByte();
			PackedColor = c;

			VertexColorIndex = db.Stream.Reader.ReadUInt32();
			// Last 4 bytes are reserved
		}
	}
}