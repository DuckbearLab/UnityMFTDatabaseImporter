using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UFLT.DataTypes.Enums;
using System.Linq;
using UFLT.Utils;

namespace UFLT.Records
{
	/// <summary>
	/// A face contains attributes describing the visual state of its child vertices.
	/// Only vertex and morph vertex nodes may be children of faces
	/// </summary>
	public class Face : Record
	{
		#region Properties

		/// <summary>
		/// IR Color Code
		/// </summary>
		public int IRColorCode
		{
			get;
			set;
		}

		/// <summary>
		/// Relative priority specifies a fixed ordering of the Face relative to its sibling nodes. Ordering is 
		/// from left (lesser values) to right (higher values). Nodes of equal priority may be arbitrarily ordered.
		/// All nodes have an implicit (default) value of zero.
		/// </summary>
		public int RelativePriority
		{
			get;
			set;
		}

		/// <summary>
		/// Face draw type.
		/// </summary>
		public DrawType DrawType
		{
			get;
			set;
		}

		/// <summary>
		/// If TRUE, draw textured face white.
		/// </summary>
		public bool TexWhite
		{
			get;
			set;
		}

		/// <summary>
		/// Color name index.
		/// </summary>
		public ushort ColorNameIndex
		{
			get;
			set;
		}

		/// <summary>
		/// Color name index or -1 if none.
		/// </summary>
		public ushort AlternativeColorNameIndex
		{
			get;
			set;
		}

		/// <summary>
		/// Template (Billboard)
		/// </summary>
		public TemplateBillboard TemplateBillboard
		{
			get;
			set;
		}

		/// <summary>
		/// Detail texture pattern or -1 if none.
		/// </summary>
		public short DetailTexturePattern
		{
			get;
			set;
		}

		/// <summary>
		/// Texture pattern or -1 if none.
		/// </summary>
		public short TexturePattern
		{
			get;
			set;
		}

		/// <summary>
		/// Material index or -1 if none.
		/// </summary>
		public short MaterialIndex
		{
			get;
			set;
		}

		/// <summary>
		/// Surface material code (for DFAD)
		/// </summary>
		public short SurfaceMaterialCode
		{
			get;
			set;
		}

		/// <summary>
		/// Feature ID(for DFAD)
		/// </summary>
		public short FeatureID
		{
			get;
			set;
		}

		/// <summary>
		/// IR material code.
		/// </summary>
		public int IRMaterialCode
		{
			get;
			set;
		}

		/// <summary>
		/// Transparency
		/// 0 = Opaque
		/// 65535 = Totally clear
		/// </summary>
		public ushort Transparency
		{
			get;
			set;
		}

		/// <summary>
		/// Level Of Detail Generation Control.
		/// </summary>
		public byte LODGenerationControl
		{
			get;
			set;
		}

		/// <summary>
		/// Line style index.
		/// </summary>
		public byte LineStyleIndex
		{
			get;
			set;
		}

		/// <summary>
		/// Flags (bits, from left to right)
		///  0 = Terrain
		///  1 = No color
		///  2 = No alternate color
		///  3 = Packed color
		///  4 = Terrain culture cutout (footprint)
		///  5 = Hidden, not drawn
		///  6 = Roofline
		///  7-31 = Spare
		/// </summary>
		public int Flags
		{
			get;
			set;
		}

		/// <summary>
		/// Flags value
		/// Is the face part of the terrain?
		/// </summary>
		public bool FlagsTerrain
		{
			get
			{
				return (Flags & -2147483648) != 0 ? true : false;
			}
		}

		/// <summary>
		/// Flags value
		/// </summary>
		public bool FlagsNoColor
		{
			get
			{
				return (Flags & 0x40000000) != 0 ? true : false;
			}
		}

		/// <summary>
		/// Flags value
		/// </summary>
		public bool FlagsNoAlternateColor
		{
			get
			{
				return (Flags & 0x20000000) != 0 ? true : false;
			}
		}

		/// <summary>
		/// Flags value        
		/// </summary>
		public bool FlagsPackedColor
		{
			get
			{
				return (Flags & 0x10000000) != 0 ? true : false;
			}
		}

		/// <summary>
		/// Flags value        
		/// Terrain culture cutout (footprint)
		/// </summary>
		public bool FlagsTerrainCultureCutout
		{
			get
			{
				return (Flags & 0x8000000) != 0 ? true : false;
			}
		}

		/// <summary>
		/// Flags value
		/// Hidden face, not drawn.
		/// </summary>
		public bool FlagsHidden
		{
			get
			{
				return (Flags & 0x4000000) != 0 ? true : false;
			}
		}

		/// <summary>
		/// Flags value
		/// </summary>
		public bool FlagsRoofline
		{
			get
			{
				return (Flags & 0x2000000) != 0 ? true : false;
			}
		}

		/// <summary>
		/// Light mode.
		/// </summary>
		public LightMode LightMode
		{
			get;
			set;
		}

		/// <summary>
		/// Packed color primary - only b, g, r used
		/// </summary>
		public Color32 PackedColorPrimary
		{
			get;
			set;
		}

		/// <summary>
		/// Packed color alternate - only b, g, r used.
		/// </summary>
		public Color32 PackedColorAlternate
		{
			get;
			set;
		}

		/// <summary>
		/// Texture mapping index or -1 if none.
		/// </summary>
		public short TextureMappingIndex
		{
			get;
			set;
		}

		/// <summary>
		/// Primary color index or -1 if none.
		/// </summary>
		public uint PrimaryColorIndex
		{
			get;
			set;
		}

		/// <summary>
		/// Alternate color index or -1 if none.
		/// </summary>
		public uint AlternateColorIndex
		{
			get;
			set;
		}

		/// <summary>
		/// Shader index or -1 if none.
		/// </summary>
		public short ShaderIndex
		{
			get;
			set;
		}

		#endregion Properties

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="parent"></param>
		public Face(Record parent) :
			base(parent, parent.Header)
		{
			RootHandler.Handler[Opcodes.PushLevel] = HandlePush;
			RootHandler.Handler[Opcodes.Comment] = HandleComment;
			RootHandler.Handler[Opcodes.LongID] = HandleLongID;

			RootHandler.ThrowBacks.UnionWith(RecordHandler.ThrowBackOpcodes);

			ChildHandler.Handler[Opcodes.PushLevel] = HandlePush;
			ChildHandler.Handler[Opcodes.PopLevel] = HandlePop;
			ChildHandler.Handler[Opcodes.VertexList] = HandleVertexList;
		}

		/// <summary>
		/// Parses binary stream.
		/// </summary>
		public override void Parse()
		{
			ID = NullTerminatedString.GetAsString(Header.Stream.Reader.ReadBytes(8));
			IRColorCode = Header.Stream.Reader.ReadInt32();
			RelativePriority = Header.Stream.Reader.ReadInt16();
			DrawType = (DrawType)Header.Stream.Reader.ReadSByte();
			TexWhite = Header.Stream.Reader.ReadBoolean();
			ColorNameIndex = Header.Stream.Reader.ReadUInt16();
			AlternateColorIndex = Header.Stream.Reader.ReadUInt16();
			/* Skip reserved bytes*/
			Header.Stream.Reader.BaseStream.Seek(1, SeekOrigin.Current);
			TemplateBillboard = (TemplateBillboard)Header.Stream.Reader.ReadSByte();
			DetailTexturePattern = Header.Stream.Reader.ReadInt16();
			TexturePattern = Header.Stream.Reader.ReadInt16();
			MaterialIndex = Header.Stream.Reader.ReadInt16();
			SurfaceMaterialCode = Header.Stream.Reader.ReadInt16();
			FeatureID = Header.Stream.Reader.ReadInt16();
			IRMaterialCode = Header.Stream.Reader.ReadInt32();
			Transparency = Header.Stream.Reader.ReadUInt16();
			LODGenerationControl = Header.Stream.Reader.ReadByte();
			LineStyleIndex = Header.Stream.Reader.ReadByte();
			Flags = Header.Stream.Reader.ReadInt32();
			LightMode = (LightMode)Header.Stream.Reader.ReadByte();
			/* Skip reserved bytes*/
			Header.Stream.Reader.BaseStream.Seek(7, SeekOrigin.Current);
			Color32 c = new Color32();
			c.a = Header.Stream.Reader.ReadByte();
			c.b = Header.Stream.Reader.ReadByte();
			c.g = Header.Stream.Reader.ReadByte();
			c.r = Header.Stream.Reader.ReadByte();
			PackedColorPrimary = c;
			c.a = Header.Stream.Reader.ReadByte();
			c.b = Header.Stream.Reader.ReadByte();
			c.g = Header.Stream.Reader.ReadByte();
			c.r = Header.Stream.Reader.ReadByte();
			PackedColorAlternate = c;
			TextureMappingIndex = Header.Stream.Reader.ReadInt16();
			/* Skip reserved bytes*/
			Header.Stream.Reader.BaseStream.Seek(2, SeekOrigin.Current);
			PrimaryColorIndex = Header.Stream.Reader.ReadUInt32();
			AlternateColorIndex = Header.Stream.Reader.ReadUInt32();
			/* Skip reserved bytes*/
			Header.Stream.Reader.BaseStream.Seek(2, SeekOrigin.Current);
			ShaderIndex = Header.Stream.Reader.ReadInt16();

			// Parse children
			base.Parse();
		}

		/// <summary>
		/// Prepares the vertices and triangulates the faces ready for creating a mesh.
		/// </summary>
		public override void PrepareForImport()
		{
			// Do we draw this face?
			if (FlagsHidden)
			{
				return;
			}

			if (Parent is InterRecord)
			{
				InterRecord ir = Parent as InterRecord;

				// Find vertex list
				VertexList vl = Children.Find(o => o is VertexList) as VertexList;
				if (vl != null)
				{
					int startIndex = ir.Vertices.Count;

					int[] triangles = null;

					//////////////////////////
					// Triangle
					//////////////////////////                    
					if (vl.Offsets.Count == 3)
					{
                        // DuckbearLab: FIX! Inverted X so inverted order too
                        triangles = new int[] { startIndex + 2, startIndex + 1, startIndex };

						// Extract verts for this triangle
						foreach (int vwcI in vl.Offsets)
						{
							VertexWithColor vwc = Header.VertexPalette.Vertices[vwcI];
							ir.Vertices.Add(vwc);
						}
					}
					//////////////////////////
					// Polygon
					//////////////////////////
					else
					{
						// Extract verts and positions for triangulation of this face
						List<Vector3> positions = new List<Vector3>(vl.Offsets.Count);
						Vector3 faceNormal = Vector3.zero; // Need the normal to convert the positions to 2d for triangulation.
						foreach (int vwcI in vl.Offsets)
						{
							VertexWithColor vwc = Header.VertexPalette.Vertices[vwcI];
							ir.Vertices.Add(vwc);
							positions.Add(new Vector3((float)vwc.Coordinate[0], (float)vwc.Coordinate[1], (float)vwc.Coordinate[2]));

							if (vwc is VertexWithColorNormal)
							{
								faceNormal += (vwc as VertexWithColorNormal).Normal;
							}
						}
						faceNormal.Normalize();

						// Triangulate the face
						Triangulator triangulator = new Triangulator();
						triangulator.initTriangulator(positions, faceNormal);
						triangles = triangulator.Triangulate(0);

						// Apply index offset
						for (int i = 0; i < triangles.Length; ++i)
						{
							triangles[i] += startIndex;
						}
					}

					// We now have our triangles. Lets find the correct submesh to add them to.
					KeyValuePair<IntermediateMaterial, List<int>> submesh = ir.FindOrCreateSubMesh(this);
					submesh.Value.AddRange(triangles);
				}
				else
				{
					Log.WriteWarning(ID + "- Could not find vertex list for face");
				}
			}
			else
			{
				Log.WriteWarning("Face is not a child of a InterRecord, can not create face.");
			}

			base.ImportIntoScene();
		}
	}
}