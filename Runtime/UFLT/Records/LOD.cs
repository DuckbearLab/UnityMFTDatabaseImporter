using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using UFLT.DataTypes.Enums;
using UFLT.Utils;
using UFLT.MonoBehaviours;
using System;

namespace UFLT.Records
{
	/// <summary>
	/// Level Of Detail.
	/// </summary>
	public class LOD : InterRecord
	{
		#region Properties       

		/// <summary>
		/// The distance to switch the model into view.
		/// </summary>
		public double SwitchInDistance
		{
			get;
			set;
		}

		/// <summary>
		/// The distance to switch the model out of view.
		/// </summary>
		public double SwitchOutDistance
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

		/// <summary>
		/// Flags (bits, from left to right)
		///   0 = Use previous slant range
		///   1 = Additive LODs below
		///   2 = Freeze center (don't recalculate)
		///   3-31 = Spare
		/// </summary>
		public int Flags
		{
			get;
			set;
		}

		/// <summary>
		/// Flags value
		/// </summary>
		public bool FlagsUsePreviousSlantRange
		{
			get
			{
				return (Flags & -2147483648) != 0 ? true : false;
			}
		}

		/// <summary>
		/// Flags value
		/// </summary>
		public bool FlagsAdditiveLODsBelow
		{
			get
			{
				return (Flags & 0x40000000) != 0 ? true : false;
			}
		}

		/// <summary>
		/// Flags value
		/// </summary>
		public bool FlagsFreezeCenter
		{
			get
			{
				return (Flags & 0x20000000) != 0 ? true : false;
			}
		}

		/// <summary>
		/// Center of LOD.
		/// </summary>
		public double[] Center
		{
			get;
			set;
		}

		/// <summary>
		/// The range over which real-time smoothing effects should be employed 
		/// while switching from one LOD to another. Smoothing effects include 
		/// geometric morphing and image blending. The smoothing effect is active
		/// between: switch-in distance minus transition range (near), and 
		/// switch-in distance(far).
		/// </summary>
		public double TransitionRange
		{
			get;
			set;
		}

		/// <summary>
		/// Used to calculate switch in and out distances based on viewing 
		/// parameters of your simulation display system
		/// </summary>
		public double SignificantSize
		{
			get;
			set;
		}

		#endregion Properties

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="parent"></param>
		public LOD(Record parent) :
			base(parent, parent.Header)
		{
			RootHandler.Handler[Opcodes.PushLevel] = HandlePush;
			RootHandler.Handler[Opcodes.LongID] = HandleLongID;
			RootHandler.Handler[Opcodes.Comment] = HandleComment;
			RootHandler.Handler[Opcodes.Matrix] = HandleMatrix;

			RootHandler.ThrowBacks.UnionWith(RecordHandler.ThrowBackOpcodes);

			ChildHandler.Handler[Opcodes.Group] = HandleGroup;
			ChildHandler.Handler[Opcodes.Object] = HandleObject;
			ChildHandler.Handler[Opcodes.PushLevel] = HandlePush;
			ChildHandler.Handler[Opcodes.PopLevel] = HandlePop;
			ChildHandler.Handler[Opcodes.DegreeOfFreedom] = HandleDOF;
			ChildHandler.Handler[Opcodes.Switch] = HandleSwitch;
			ChildHandler.Handler[Opcodes.Sound] = HandleUnhandled;
			ChildHandler.Handler[Opcodes.ClipRegion] = HandleUnhandled;
			ChildHandler.Handler[Opcodes.LevelOfDetail] = HandleLevelOfDetail;
			ChildHandler.Handler[Opcodes.ExternalReference] = HandleExternalReference;
			ChildHandler.Handler[Opcodes.Face] = HandleFace;
		}

		/// <summary>
		/// Parses binary stream.
		/// </summary>
		public override void Parse()
		{
			ID = NullTerminatedString.GetAsString(Header.Stream.Reader.ReadBytes(8));
			/* Skip reserved bytes*/
			Header.Stream.Reader.BaseStream.Seek(4, SeekOrigin.Current);
			SwitchInDistance = Header.Stream.Reader.ReadDouble();
			SwitchOutDistance = Header.Stream.Reader.ReadDouble();
			SpecialEffectID1 = Header.Stream.Reader.ReadInt16();
			SpecialEffectID2 = Header.Stream.Reader.ReadInt16();
			Flags = Header.Stream.Reader.ReadInt32();
			Center = new double[] { Header.Stream.Reader.ReadDouble(), Header.Stream.Reader.ReadDouble(), Header.Stream.Reader.ReadDouble() };
			TransitionRange = Header.Stream.Reader.ReadDouble();
			SignificantSize = Header.Stream.Reader.ReadDouble();

			// Parse children
			base.Parse();
		}

		/// <summary>
		/// Converts the record/s into a Unity GameObject structure with meshes, materials etc and imports into the scene. 
		/// Adds a Component to the LOD GameObject if one is assigned in the Settings.
		/// </summary>
		public override void ImportIntoScene()
		{
			base.ImportIntoScene();
			var lodComp = UnityGameObject.AddComponent<LevelOfDetail>();
			lodComp.OnLODNode(this);
		}
	}
}