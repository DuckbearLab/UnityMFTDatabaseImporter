using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;

namespace UFLT.Records
{
	/// <summary>
	/// A vertex with color data.
	/// </summary>
	public class VertexWithColor
	{
		#region Properties

		/// <summary>
		/// Color name index.
		/// </summary>
		public ushort ColorNameIndex
		{
			get;
			set;
		}

		/// <summary>
		/// Flags (bits, from left to right)
		///   0 = Start hard edge
		///   1 = Normal frozen
		///   2 = No color
		///   3 = Packed color
		///   4-15 = Spare
		/// </summary>
		public short Flags
		{
			get;
			set;
		}

		/// <summary>
		/// Flags value
		/// </summary>
		public bool FlagsStartHardEdge
		{
			get
			{
				return (Flags & -0x8000) != 0 ? true : false;
			}
		}

		/// <summary>
		/// Flags value
		/// </summary>
		public bool FlagsNormalFrozen
		{
			get
			{
				return (Flags & 0x4000) != 0 ? true : false;
			}
		}

		/// <summary>
		/// Flags value
		/// </summary>
		public bool FlagsNoColor
		{
			get
			{
				return (Flags & 0x2000) != 0 ? true : false;
			}
		}

		/// <summary>
		/// Flags value
		/// </summary>
		public bool FlagsPackedColor
		{
			get
			{
				return (Flags & 0x1000) != 0 ? true : false;
			}
		}

		/// <summary>
		/// x,y,z position of vertex.
		/// </summary>
		public double[] Coordinate
		{
			get;
			set;
		}

		/// <summary>
		/// Packed color - always specified when the vertex has color.
		/// </summary>
		public Color32 PackedColor
		{
			get;
			set;
		}

		/// <summary>
		/// valid only if vertex has color and Packed color flag is not set.
		/// </summary>
		public uint VertexColorIndex
		{
			get;
			set;
		}

		#endregion Properties

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="parent"></param>
		public VertexWithColor()
		{
		}

		/// <summary>
		/// Parses binary stream.
		/// </summary>
		/// <param name="db">Database that this vertex is part of.</param>
		public virtual void Parse(Database db)
		{
			ColorNameIndex = db.Stream.Reader.ReadUInt16();
			Flags = db.Stream.Reader.ReadInt16();
            // DuckbearLab: FIX! Inverted X
            double x = db.Stream.Reader.ReadDouble(), z = db.Stream.Reader.ReadDouble(), y = db.Stream.Reader.ReadDouble();
            Coordinate = new double[3] { x, y, z };

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