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
	public class Switch : InterRecord
	{
		#region Properties

		/// <summary>
		/// Origin of DOF's local coordinate system.(x, y, z).
		/// </summary>
		public int Index
		{
			get;
			set;
		}

		/// <summary>
		/// Point on x axis of DOF's local coordinate system (x, y, z).
		/// </summary>
		public int[] Masks
		{
			get;
			set;
		}

		#endregion Properties

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="parent"></param>
		public Switch(Record parent) :
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
			Index = Header.Stream.Reader.ReadInt32();
			int numMasks = Header.Stream.Reader.ReadInt32();
			/* Skip #words per mask, we'll assume its 1 32bit word per mask for now*/
			Header.Stream.Reader.BaseStream.Seek(4, SeekOrigin.Current);
			Masks = new int[numMasks];
			for (int i = 0; i < numMasks; i++)
			{
				Masks[i] = Header.Stream.Reader.ReadInt32();
			}

			// Parse children
			base.Parse();
		}

		/// <summary>
		/// Converts the record/s into a Unity GameObject structure with meshes, materials etc and imports into the scene. 
		/// Adds a Component to the DOF GameObject if one is assigned in the Settings.
		/// </summary>
		public override void ImportIntoScene()
		{
			base.ImportIntoScene();
			var switchComp = UnityGameObject.AddComponent<SwitchNode>();
			switchComp.OnSwitchNode(this);
		}
	}
}