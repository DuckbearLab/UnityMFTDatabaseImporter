using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UFLT.DataTypes.Enums;

namespace UFLT.Records
{
	/// <summary>
	/// Double precision vertex records are stored in a vertex palette for the entire database. Vertices 
	/// shared by one or more geometric entities are written only one time in the vertex palette. This 
	/// reduces the overall size of the OpenFlight file by writing only “unique” vertices. Vertex palette 
	/// records are referenced by faces and light points via vertex list and morph vertex list records
	/// </summary>
	public class VertexPalette : Record
	{
		#region Properties

		// Next offset value to be parsed.
		private int _Offset;

		/// <summary>
		/// Length of this record plus all vertices.
		/// </summary>
		public int LengthPlusVertexPalette
		{
			get;
			set;
		}

		/// <summary>
		/// Vertex records with offset as dictionary key.
		/// </summary>
		public Dictionary<int, VertexWithColor> Vertices
		{
			get;
			set;
		}

		#endregion Properties

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="parent"></param>
		public VertexPalette(Record parent) :
			base(parent, parent.Header)
		{
			RootHandler.Handler[Opcodes.VertexWithColor] = HandleVertexColor;
			RootHandler.Handler[Opcodes.VertexwithColorAndNormal] = HandleVertexColorNormal;
			RootHandler.Handler[Opcodes.VertexWithColorAndUV] = HandleVertexColorUV;
			RootHandler.Handler[Opcodes.VertexWithColorNormalAndUV] = HandleVertexColorNormalUV;

			RootHandler.ThrowBackUnhandled = true;

			Vertices = new Dictionary<int, VertexWithColor>();
		}

		/// <summary>
		/// Parses binary stream.
		/// </summary>
		public override void Parse()
		{
			LengthPlusVertexPalette = Header.Stream.Reader.ReadInt32();
			_Offset = 8;

			// Parse vertices
			base.Parse();
		}

		#region Record Handlers

		/// <summary>
		/// Handle vertex with color.
		/// </summary>
		/// <returns></returns>
		private bool HandleVertexColor()
		{
			VertexWithColor v = new VertexWithColor();
			v.Parse(Header);
			Vertices[_Offset] = v;
			_Offset += Header.Stream.Length;
			return true;
		}

		/// <summary>
		/// Handle vertex with color.
		/// </summary>
		/// <returns></returns>
		private bool HandleVertexColorNormal()
		{
			VertexWithColorNormal v = new VertexWithColorNormal();
			v.Parse(Header);
			Vertices[_Offset] = v;
			_Offset += Header.Stream.Length;
			return true;
		}

		/// <summary>
		/// Handle vertex with color.
		/// </summary>
		/// <returns></returns>
		private bool HandleVertexColorUV()
		{
			VertexWithColorUV v = new VertexWithColorUV();
			v.Parse(Header);
			Vertices[_Offset] = v;
			_Offset += Header.Stream.Length;
			return true;
		}

		/// <summary>
		/// Handle vertex with color.
		/// </summary>
		/// <returns></returns>
		private bool HandleVertexColorNormalUV()
		{
			VertexWithColorNormalUV v = new VertexWithColorNormalUV();
			v.Parse(Header);
			Vertices[_Offset] = v;
			_Offset += Header.Stream.Length;
			return true;
		}

		#endregion Record Handlers
	}
}