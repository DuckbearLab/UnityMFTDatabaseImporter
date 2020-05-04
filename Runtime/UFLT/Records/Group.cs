using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using UFLT.DataTypes.Enums;
using UFLT.Utils;

namespace UFLT.Records
{
	/// <summary>
	/// Groups are the most generic hierarchical node present in the database tree.
	/// Attributes within the group record provide bounding volumes that encompass the group�s 
	/// children and real-time control flags. A group can represent an animation sequence 
	/// in which case each immediate child of the group represents one frame of the sequence.
	/// </summary>
	public class Group : InterRecord
	{
		#region Properties

		/// <summary>
		/// Relative priority specifies a fixed ordering of the Group relative to its sibling nodes. Ordering is 
		/// from left (lesser values) to right (higher values). Nodes of equal priority may be arbitrarily ordered.
		/// All nodes have an implicit (default) value of zero.
		/// </summary>
		public int RelativePriority
		{
			get;
			set;
		}

		/// <summary>
		/// Flags (bits, from left to right)
		///   0 = Reserved
		///   1 = Forward animation
		///   2 = Swing animation
		///   3 = Bounding box follows
		///   4 = Freeze bounding box
		///   5 = Default parent
		///   6 = Backward animation
		///   7 = Preserve at runtime
		///   8-31 = Spare
		/// </summary>
		public int Flags
		{
			get;
			set;
		}

		/// <summary>
		/// Flags value
		/// </summary>
		public bool FlagsForwardAnimation
		{
			get
			{
				return (Flags & 0x40000000) != 0 ? true : false;
			}
		}

		/// <summary>
		/// Flags value
		/// </summary>
		public bool FlagsSwingAnimation
		{
			get
			{
				return (Flags & 0x20000000) != 0 ? true : false;
			}
		}

		/// <summary>
		/// Flags value
		/// </summary>
		public bool FlagsBoundingBoxFollows
		{
			get
			{
				return (Flags & 0x10000000) != 0 ? true : false;
			}
		}

		/// <summary>
		/// Flags value
		/// </summary>
		public bool FlagsFreezeBoundingBox
		{
			get
			{
				return (Flags & 0x8000000) != 0 ? true : false;
			}
		}

		/// <summary>
		/// Flags value
		/// </summary>
		public bool FlagsDefaultParent
		{
			get
			{
				return (Flags & 0x4000000) != 0 ? true : false;
			}
		}

		/// <summary>
		/// Flags value
		/// </summary>
		public bool FlagsBackwardAnimation
		{
			get
			{
				return (Flags & 0x2000000) != 0 ? true : false;
			}
		}

		/// <summary>
		/// Flags value        
		/// </summary>
		public bool FlagsPreserveAtRuntime
		{
			get
			{
				return (Flags & 0x1000000) != 0 ? true : false;
			}
		}

		/// <summary>
		/// Special effect ID 1 - application defined.
		/// Can be used to enhance the meaning of existing attributes, 
		/// such as the animation flags, or extend the interpretation of the
		/// group node. Normally, the value of these attributes is zero.
		/// </summary>
		public short SpecialEffectID1
		{
			get;
			set;
		}

		/// <summary>
		/// Special effect ID 2 - application defined.
		/// Can be used to enhance the meaning of existing attributes, 
		/// such as the animation flags, or extend the interpretation of the
		/// group node. Normally, the value of these attributes is zero.
		/// </summary>
		public short SpecialEffectID2
		{
			get;
			set;
		}

		/// <summary>
		/// Can be used to assist real-time culling and load balancing mechanisms, 
		/// by defining the visual significance of this group with respect to other
		/// groups in the database. Default is 0.
		/// </summary>
		public short Significance
		{
			get;
			set;
		}

		/// <summary>
		/// Layer the group belongs to in the modelling environment it was created in. 
		/// </summary>
		public sbyte LayerCode
		{
			get;
			set;
		}

		/// <summary>
		/// The number of times an animation loop repeats within the sequence.
		/// A loop count of 0 indicates that the loop is to repeat forever.
		/// </summary>
		public int LoopCount
		{
			get;
			set;
		}

		/// <summary>
		/// The duration of one loop within an animation sequence, measured in seconds.
		/// </summary>
		public float LoopDuration
		{
			get;
			set;
		}

		/// <summary>
		/// For finite animation sequences (those with positive, non-zero loop count values),
		/// the duration that the last frame of the last loop is extended after the sequence 
		/// has finished, measured in seconds. A last frame duration of 0 indicates that the
		/// last frame is not displayed any longer after the sequence finishes. 
		/// </summary>
		public float LastFrameDuration
		{
			get;
			set;
		}

		#endregion Properties

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="parent"></param>
		public Group(Record parent) :
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
			ChildHandler.Handler[Opcodes.ExternalReference] = HandleExternalReference;
			ChildHandler.Handler[Opcodes.Group] = HandleGroup;
			ChildHandler.Handler[Opcodes.PushLevel] = HandlePush;
			ChildHandler.Handler[Opcodes.PopLevel] = HandlePop;
			ChildHandler.Handler[Opcodes.Object] = HandleObject;
			ChildHandler.Handler[Opcodes.Switch] = HandleSwitch;
			ChildHandler.Handler[Opcodes.Sound] = HandleUnhandled;
			ChildHandler.Handler[Opcodes.ClipRegion] = HandleUnhandled;
			ChildHandler.Handler[Opcodes.DegreeOfFreedom] = HandleDOF;

			ChildHandler.Handler[Opcodes.LevelOfDetail] = HandleLevelOfDetail;
		}

		/// <summary>
		/// Parses binary stream.
		/// </summary>
		public override void Parse()
		{
			ID = NullTerminatedString.GetAsString(Header.Stream.Reader.ReadBytes(8));
			RelativePriority = Header.Stream.Reader.ReadInt16();
			Header.Stream.Reader.BaseStream.Seek(2, SeekOrigin.Current); // Skip reserved            
			Flags = Header.Stream.Reader.ReadInt32();
			SpecialEffectID1 = Header.Stream.Reader.ReadInt16();
			SpecialEffectID2 = Header.Stream.Reader.ReadInt16();
			Significance = Header.Stream.Reader.ReadInt16();
			LayerCode = Header.Stream.Reader.ReadSByte();
			Header.Stream.Reader.BaseStream.Seek(5, SeekOrigin.Current); // Skip reserved            
			LoopCount = Header.Stream.Reader.ReadInt32();
			LoopDuration = Header.Stream.Reader.ReadSingle();
			LastFrameDuration = Header.Stream.Reader.ReadSingle();

			// Parse children
			base.Parse();
		}
	}
}