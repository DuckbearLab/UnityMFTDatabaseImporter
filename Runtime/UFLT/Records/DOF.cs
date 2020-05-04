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
	/// The Degree Of Freedom node specifies a local coordinate system and the range allowed 
	/// for translation, rotation, and scale with respect to that coordinate system.
	/// I.E, a pivot point such as a Tank Turret or gun pivot.
	/// </summary>
	public class DOF : InterRecord
	{
		#region Properties

		/// <summary>
		/// Origin of DOF's local coordinate system.(x, y, z).
		/// </summary>
		public double[] Origin
		{
			get;
			set;
		}

		/// <summary>
		/// Point on x axis of DOF's local coordinate system (x, y, z).
		/// </summary>
		public double[] PointOnXAxis
		{
			get;
			set;
		}

		/// <summary>
		/// Point in xy plane of DOF's local coordinate system (x, y, z)
		/// </summary>
		public double[] PointInXYPlane
		{
			get;
			set;
		}

		/// <summary>
		/// Min, Max, Current & Increment of x with respect to local coordinate system.
		/// </summary>
		public double[] MinMaxCurrentIncrementX
		{
			get;
			set;
		}

		/// <summary>
		/// Min, Max, Current & Increment of y with respect to local coordinate system.
		/// </summary>
		public double[] MinMaxCurrentIncrementY
		{
			get;
			set;
		}

		/// <summary>
		/// Min, Max, Current & Increment of z with respect to local coordinate system.
		/// </summary>
		public double[] MinMaxCurrentIncrementZ
		{
			get;
			set;
		}

		/// <summary>
		/// Min, Max, Current & Increment of pitch 
		/// </summary>
		public double[] MinMaxCurrentIncrementPitch
		{
			get;
			set;
		}

		/// <summary>
		/// Min, Max, Current & Increment of roll 
		/// </summary>
		public double[] MinMaxCurrentIncrementRoll
		{
			get;
			set;
		}

		/// <summary>
		/// Min, Max, Current & Increment of yaw 
		/// </summary>
		public double[] MinMaxCurrentIncrementYaw
		{
			get;
			set;
		}

		/// <summary>
		/// Min, Max, Current & Increment of scale z
		/// </summary>
		public double[] MinMaxCurrentIncrementScaleZ
		{
			get;
			set;
		}

		/// <summary>
		/// Min, Max, Current & Increment of scale y.
		/// </summary>
		public double[] MinMaxCurrentIncrementScaleY
		{
			get;
			set;
		}

		/// <summary>
		/// Min, Max, Current & Increment of scale x.
		/// </summary>
		public double[] MinMaxCurrentIncrementScaleX
		{
			get;
			set;
		}

		/// <summary>
		/// Flags (bits, from left to right)
		/// 0 = x translation is limited
		/// 1 = y translation is limited
		/// 2 = z translation is limited
		/// 3 = Pitch rotation is limited
		/// 4 = Roll rotation is limited
		/// 5 = Yaw rotation is limited
		/// 6 = x scale is limited
		/// 7 = y scale is limited
		/// 8 = z scale is limited
		/// 9 = Reserved
		/// 10 = Reserved
		/// 11-31 = Spare		
		/// </summary>		
		public int Flags
		{
			get;
			set;
		}

		/// <summary>
		/// If true then the X translation should be limited using the relevant Min,Max values.
		/// </summary>		
		public bool FlagsXTranslationLimited
		{
			get
			{
				return (Flags & -2147483648) != 0 ? true : false;
			}
		}

		/// <summary>
		/// If true then the Y translation should be limited using the relevant Min,Max values.
		/// </summary>				
		public bool FlagsYTranslationLimited
		{
			get
			{
				return (Flags & 0x40000000) != 0 ? true : false;
			}
		}

		/// <summary>
		/// If true then the Z translation should be limited using the relevant Min,Max values.
		/// </summary>				
		public bool FlagsZTranslationLimited
		{
			get
			{
				return (Flags & 0x20000000) != 0 ? true : false;
			}
		}

		/// <summary>
		/// If true then the pitch should be limited using the relevant Min,Max values.
		/// </summary>				
		public bool FlagsPitchLimited
		{
			get
			{
				return (Flags & 0x10000000) != 0 ? true : false;
			}
		}

		/// <summary>
		/// If true then the roll should be limited using the relevant Min,Max values.
		/// </summary>					
		public bool FlagsRollLimited
		{
			get
			{
				return (Flags & 0x8000000) != 0 ? true : false;
			}
		}

		/// <summary>
		/// If true then the yaw should be limited using the relevant Min,Max values.
		/// </summary>			
		public bool FlagsYawLimited
		{
			get
			{
				return (Flags & 0x4000000) != 0 ? true : false;
			}
		}

		/// <summary>
		/// If true then the X scale should be limited using the relevant Min,Max values.
		/// </summary>					
		public bool FlagsScaleXLimited
		{
			get
			{
				return (Flags & 0x2000000) != 0 ? true : false;
			}
		}

		/// <summary>
		/// If true then the Y scale should be limited using the relevant Min,Max values.
		/// </summary>				
		public bool FlagsScaleYLimited
		{
			get
			{
				return (Flags & 0x1000000) != 0 ? true : false;
			}
		}

		/// <summary>
		/// If true then the Z scale should be limited using the relevant Min,Max values.
		/// </summary>						
		public bool FlagsScaleZLimited
		{
			get
			{
				return (Flags & 0x800000) != 0 ? true : false;
			}
		}

		#endregion Properties

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="parent"></param>
		public DOF(Record parent) :
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
			ChildHandler.Handler[Opcodes.Switch] = HandleSwitch;
			ChildHandler.Handler[Opcodes.Sound] = HandleUnhandled;
			ChildHandler.Handler[Opcodes.ClipRegion] = HandleUnhandled;

			ChildHandler.Handler[Opcodes.LevelOfDetail] = HandleLevelOfDetail;
			ChildHandler.Handler[Opcodes.ExternalReference] = HandleExternalReference;
		}

		/// <summary>
		/// Parses binary stream.
		/// </summary>
		public override void Parse()
		{
			ID = NullTerminatedString.GetAsString(Header.Stream.Reader.ReadBytes(8));
			/* Skip reserved bytes*/
			Header.Stream.Reader.BaseStream.Seek(4, SeekOrigin.Current);
			Origin = new double[] { Header.Stream.Reader.ReadDouble(), Header.Stream.Reader.ReadDouble(), Header.Stream.Reader.ReadDouble() };
			PointOnXAxis = new double[] { Header.Stream.Reader.ReadDouble(), Header.Stream.Reader.ReadDouble(), Header.Stream.Reader.ReadDouble() };
			PointInXYPlane = new double[] { Header.Stream.Reader.ReadDouble(), Header.Stream.Reader.ReadDouble(), Header.Stream.Reader.ReadDouble() };
			MinMaxCurrentIncrementZ = new double[] { Header.Stream.Reader.ReadDouble(), Header.Stream.Reader.ReadDouble(), Header.Stream.Reader.ReadDouble(), Header.Stream.Reader.ReadDouble() };
			MinMaxCurrentIncrementY = new double[] { Header.Stream.Reader.ReadDouble(), Header.Stream.Reader.ReadDouble(), Header.Stream.Reader.ReadDouble(), Header.Stream.Reader.ReadDouble() };
			MinMaxCurrentIncrementX = new double[] { Header.Stream.Reader.ReadDouble(), Header.Stream.Reader.ReadDouble(), Header.Stream.Reader.ReadDouble(), Header.Stream.Reader.ReadDouble() };
			MinMaxCurrentIncrementPitch = new double[] { Header.Stream.Reader.ReadDouble(), Header.Stream.Reader.ReadDouble(), Header.Stream.Reader.ReadDouble(), Header.Stream.Reader.ReadDouble() };
			MinMaxCurrentIncrementRoll = new double[] { Header.Stream.Reader.ReadDouble(), Header.Stream.Reader.ReadDouble(), Header.Stream.Reader.ReadDouble(), Header.Stream.Reader.ReadDouble() };
			MinMaxCurrentIncrementYaw = new double[] { Header.Stream.Reader.ReadDouble(), Header.Stream.Reader.ReadDouble(), Header.Stream.Reader.ReadDouble(), Header.Stream.Reader.ReadDouble() };
			MinMaxCurrentIncrementScaleZ = new double[] { Header.Stream.Reader.ReadDouble(), Header.Stream.Reader.ReadDouble(), Header.Stream.Reader.ReadDouble(), Header.Stream.Reader.ReadDouble() };
			MinMaxCurrentIncrementScaleY = new double[] { Header.Stream.Reader.ReadDouble(), Header.Stream.Reader.ReadDouble(), Header.Stream.Reader.ReadDouble(), Header.Stream.Reader.ReadDouble() };
			MinMaxCurrentIncrementScaleX = new double[] { Header.Stream.Reader.ReadDouble(), Header.Stream.Reader.ReadDouble(), Header.Stream.Reader.ReadDouble(), Header.Stream.Reader.ReadDouble() };
			Flags = Header.Stream.Reader.ReadInt32();

			// Apply the dof origin to our GO position so we can pivot using the GO transform as you would expect to.
            // DuckbearLab: FIX! Put this in comment?!?
			//Position = new Vector3((float)Origin[0], (float)Origin[1], (float)Origin[2]);

			// TODO: Can DOF have a matrix? This would mess the origin up if they can, maybe apply in the import function?

			// Parse children
			base.Parse();
		}

		/// <summary>
		/// Converts the record/s into a Unity GameObject structure with meshes, materials etc and imports into the scene. 		
		/// </summary>
		public override void ImportIntoScene()
		{
			base.ImportIntoScene();
			var dofComp = UnityGameObject.AddComponent<DegreeOfFreedom>();
			dofComp.OnDOFNode(this);
		}
	}
}