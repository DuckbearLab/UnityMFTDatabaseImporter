using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using UFLT.DataTypes.Enums;
using UFLT.Utils;

namespace UFLT.Records
{
	/// <summary>
	/// A record that is not fully handled but required in order to access child records.
	/// </summary>
	public class Unhandled : InterRecord
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="parent"></param>
		public Unhandled(Record parent) :
			base(parent, parent.Header)
		{
			RootHandler.Handler[Opcodes.PushLevel] = HandlePush;
			RootHandler.Handler[Opcodes.LongID] = HandleLongID;
			RootHandler.Handler[Opcodes.Comment] = HandleComment;
			RootHandler.Handler[Opcodes.Matrix] = HandleMatrix;

			RootHandler.ThrowBacks.UnionWith(RecordHandler.ThrowBackOpcodes);

			ChildHandler.Handler[Opcodes.Face] = HandleFace;
			ChildHandler.Handler[Opcodes.Group] = HandleGroup;
			ChildHandler.Handler[Opcodes.Object] = HandleObject;
			ChildHandler.Handler[Opcodes.PushLevel] = HandlePush;
			ChildHandler.Handler[Opcodes.PopLevel] = HandlePop;
			ChildHandler.Handler[Opcodes.Switch] = HandleSwitch;
			ChildHandler.Handler[Opcodes.DegreeOfFreedom] = HandleDOF;
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
			base.Parse();
		}
	}
}