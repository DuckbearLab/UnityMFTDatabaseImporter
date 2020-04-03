using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using UFLT.Utils;

namespace UFLT.Records
{
	/// <summary>
	/// Contains all colors indexed by face and vertex nodes in the database. 
	/// The record must follow the header record and precede the first push.
	/// The color record is divided into two sections: one for color entries and one for color names.
	/// All color entries are in 32-bit packed format (a, b, g, r). 
	/// Each color consists of red, green, and blue components of 8 bits each, plus 8 bits reserved 
	/// for alpha (future). Currently alpha is always 0xff (fully opaque). 
	/// The color entry section consists of 1024 ramped colors of 128 intensities each.
	/// The color name section may or may not be included.    
	/// </summary>
	public class ColorPalette : Record
	{
		#region Properties

		/// <summary>
		/// Holds a color and color name.
		/// </summary>
		public struct PaletteColor
		{
			public Color32 Color;
			public string Name;
		}

		/// <summary>
		/// 1024 colors.
		/// </summary>
		public PaletteColor[] Colors
		{
			get;
			set;
		}

		#endregion Properties

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="parent"></param>
		public ColorPalette(Record parent) :
			base(parent, parent.Header)
		{
		}

		/// <summary>
		/// Parses binary stream.
		/// </summary>
		public override void Parse()
		{
			// Parse colors, 1024 in total
			Colors = new PaletteColor[1024];

			for (int i = 0; i < 1024; ++i)
			{
				//Colors[i] = new PaletteColor();
				Colors[i].Color.a = Header.Stream.Reader.ReadByte();
				Colors[i].Color.b = Header.Stream.Reader.ReadByte();
				Colors[i].Color.g = Header.Stream.Reader.ReadByte();
				Colors[i].Color.r = Header.Stream.Reader.ReadByte();
			}

			// Do we have color names?
			if (Length > 4228)
			{
				int numColNames = Header.Stream.Reader.ReadInt32();
				for (int i = 0; i < numColNames; ++i)
				{
					short len = Header.Stream.Reader.ReadInt16();
					Header.Stream.Reader.BaseStream.Seek(2, SeekOrigin.Current); // Skip reserved
					short index = Header.Stream.Reader.ReadInt16();
					Header.Stream.Reader.BaseStream.Seek(2, SeekOrigin.Current); // Skip reserved
					if (len > 8 && index >= 0 && index < Colors.Length)
						Colors[index].Name = NullTerminatedString.GetAsString(Header.Stream.Reader.ReadBytes(len - 8));
				}
			}
		}
	}
}
