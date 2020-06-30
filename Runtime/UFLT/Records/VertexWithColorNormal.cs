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
            double x = db.Stream.Reader.ReadDouble(), z = db.Stream.Reader.ReadDouble(), y = db.Stream.Reader.ReadDouble();
            Coordinate = new double[3] { x, y, z };
            float xN = db.Stream.Reader.ReadSingle(), zN = db.Stream.Reader.ReadSingle(), yN = db.Stream.Reader.ReadSingle();
            Normal = new Vector3(xN, yN, zN);

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