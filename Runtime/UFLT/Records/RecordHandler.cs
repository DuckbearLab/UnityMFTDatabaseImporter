using System.Collections.Generic;
using UFLT.DataTypes.Enums;

namespace UFLT.Records
{
	/// <summary>
	/// The record handler keeps track of what records are handled by a record, if a record does not
	/// handle it then it can determine if the record should throw back for its parent to handle the record.
	/// This approach is taken from blight - http://sourceforge.net/projects/blight
	/// </summary>
	public class RecordHandler
	{
		#region Properties

		public delegate bool HandleRecordDelegate();

		/// <summary>
		/// Dictionary of opcodes to handler methods.
		/// </summary>
		public Dictionary<Opcodes, HandleRecordDelegate> Handler
		{
			get;
			set;
		}

		/// <summary>
		/// Send all opcodes not handled to the parent node?
		/// </summary>
		public bool ThrowBackUnhandled
		{
			get;
			set;
		}

		/// <summary>
		/// If throw back unhandled is false then throwback will only occur if the record is in this set.
		/// </summary>
		public HashSet<Opcodes> ThrowBacks
		{
			get;
			set;
		}

		/// <summary>
		/// Opcodes that indicate its time to return control to parent.
		/// </summary>
		public static readonly Opcodes[] ThrowBackOpcodes = new Opcodes[] {
						Opcodes.Group,
						Opcodes.LevelOfDetail,
						Opcodes.Object,
						Opcodes.PopLevel,
						Opcodes.Switch,
						Opcodes.DegreeOfFreedom,
						Opcodes.Sound,
						Opcodes.ClipRegion,
						Opcodes.ExternalReference,
                        Opcodes.Face
					};

		/// <summary>
		/// Do not report opcodes.
		/// </summary>
		public static readonly Opcodes[] DoNotReportOpcodes = new Opcodes[] {
						Opcodes.RotateAboutEdge,
						Opcodes.Translate,
						Opcodes.Scale,
						Opcodes.RotateAboutPoint,
						Opcodes.RotateAndOrScaleToPoint,
						Opcodes.Put,
						Opcodes.GeneralMatrix,
						Opcodes.EyepointAndTrackplanePalette,
						Opcodes.LongID,
						Opcodes.TextureMappingPalette,
						Opcodes.Extension,
						Opcodes.LightSource,
						Opcodes.LightSourcePalette,
						Opcodes.LineStylePalette,
						Opcodes.Comment,
						( Opcodes )103,
						( Opcodes )104,
						( Opcodes )117,
						( Opcodes )118,
						( Opcodes )120,
						( Opcodes )121,
						( Opcodes )124,
						( Opcodes )125
					};

		#endregion Properties

		/// <summary>
		/// Create a new record handler.
		/// </summary>
		public RecordHandler()
		{
			Handler = new Dictionary<Opcodes, HandleRecordDelegate>();
			ThrowBacks = new HashSet<Opcodes>();
		}

		/// <summary>
		/// Attempts to handle an opcode. Returns true if successful.
		/// </summary>
		/// <param name="opcode"></param>
		/// <returns></returns>
		public bool Handle(Opcodes opcode)
		{
			return Handler[opcode]();
		}

		/// <summary>
		/// Checks if the opcode is handled by this handler.
		/// </summary>
		/// <param name="opcode"></param>
		/// <returns></returns>
		public bool Handles(Opcodes opcode)
		{
			return Handler.ContainsKey(opcode);
		}
	}
}