using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using UFLT.DataTypes.Enums;
using UFLT.Utils;

namespace UFLT.Records
{
	/// <summary>
	/// Objects are low-level grouping nodes that contain attributes pertaining 
	/// to the state of it child geometry. Only face and light point nodes may 
	/// be the children of object nodes.
	/// </summary>
	public class Object : InterRecord
	{
		#region Properties

		/// <summary>
		/// Flags (bits, from left to right)
		///   0 = Don't display in daylight
		///   1 = Don't display at dusk
		///   2 = Don't display at night
		///   3 = Don't illuminate
		///   4 = Flat shaded
		///   5 = Group's shadow object
		///   6 = Preserve at runtime
		///   7-31 = Spare
		/// </summary>
		public int Flags
		{
			get;
			set;
		}

		/// <summary>
		/// Flags value
		/// </summary>
		public bool FlagsDontDisplayInDaylight
		{
			get
			{
				return (Flags & -2147483648) != 0 ? true : false;
			}
		}

		/// <summary>
		/// Flags value
		/// </summary>
		public bool FlagsDontDisplayInDusk
		{
			get
			{
				return (Flags & 0x40000000) != 0 ? true : false;
			}
		}

		/// <summary>
		/// Flags value
		/// </summary>
		public bool FlagsDontDisplayAtNight
		{
			get
			{
				return (Flags & 0x20000000) != 0 ? true : false;
			}
		}

		/// <summary>
		/// Flags value
		/// When set the object is self illuminating.
		/// </summary>
		public bool FlagsDontIlluminate
		{
			get
			{
				return (Flags & 0x10000000) != 0 ? true : false;
			}
		}

		/// <summary>
		/// Flags value
		/// Lighting calculations should produce a faceted appearance to the object’s geometry.
		/// Geometric normals should be constrained to face normals.
		/// </summary>
		public bool FlagsFlatShaded
		{
			get
			{
				return (Flags & 0x8000000) != 0 ? true : false;
			}
		}

		/// <summary>
		/// Flags value
		/// Indicates the object represents the shadow of the rest of the group. When used 
		/// as part of a moving model (e.g.,an aircraft), the application can apply appropriate 
		/// distortions, creating a realistic shadow on the terrain or runway.
		/// </summary>
		public bool FlagsGroupsShadowObject
		{
			get
			{
				return (Flags & 0x4000000) != 0 ? true : false;
			}
		}

		/// <summary>
		/// Flags value
		/// </summary>
		public bool FlagsPreserveAtRuntime
		{
			get
			{
				return (Flags & 0x2000000) != 0 ? true : false;
			}
		}

		/// <summary>
		/// Relative priority specifies a fixed ordering of the object relative to its sibling nodes. Ordering is 
		/// from left (lesser values) to right (higher values). Nodes of equal priority may be arbitrarily ordered.
		/// All nodes have an implicit (default) value of zero.
		/// </summary>
		public int RelativePriority
		{
			get;
			set;
		}

		/// <summary>
		/// Transparency of child objects, the value should be modulated with the transparency of 
		/// the geometry and material alpha calculation.
		/// 0 = Opaque
		/// 65535 = Totally clear
		/// </summary>
		public ushort Transparency
		{
			get;
			set;
		}

		/// <summary>
		/// Special effect ID 1 - application defined.
		/// </summary>
		public short SpecialEffectID1
		{
			get;
			set;
		}

		/// <summary>
		/// Special effect ID 2 - application defined.
		/// </summary>
		public short SpecialEffectID2
		{
			get;
			set;
		}

		#endregion Properties

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="parent"></param>
		public Object(Record parent) :
			base(parent, parent.Header)
		{
			RootHandler.Handler[Opcodes.PushLevel] = HandlePush;
			RootHandler.Handler[Opcodes.LongID] = HandleLongID;
			RootHandler.Handler[Opcodes.Comment] = HandleComment;
			RootHandler.Handler[Opcodes.Matrix] = HandleMatrix;

			RootHandler.ThrowBacks.UnionWith(RecordHandler.ThrowBackOpcodes);

			ChildHandler.Handler[Opcodes.Face] = HandleFace;
			// TODO: index light point
			// TODO: inline light point
			ChildHandler.Handler[Opcodes.PushLevel] = HandlePush;
			ChildHandler.Handler[Opcodes.PopLevel] = HandlePop;
		}

		/// <summary>
		/// Parses binary stream.
		/// </summary>
		public override void Parse()
		{
			ID = NullTerminatedString.GetAsString(Header.Stream.Reader.ReadBytes(8));
			Flags = Header.Stream.Reader.ReadInt32();
			RelativePriority = Header.Stream.Reader.ReadInt16();
			Transparency = Header.Stream.Reader.ReadUInt16();
			SpecialEffectID1 = Header.Stream.Reader.ReadInt16();
			SpecialEffectID2 = Header.Stream.Reader.ReadInt16();
			// Ignore last 2 reserved bytes.    

			// Parse children
			base.Parse();
		}
	}
}