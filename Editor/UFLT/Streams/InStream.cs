
using System.IO;
using UFLT.DataTypes.Enums;
using System;
using UnityEngine;
using UFLT.Records;

namespace UFLT.Streams
{
	/// <summary>
	/// Stream for reading an OpenFlight file.
	/// </summary>
	public class InStream
	{
		#region Properties

		#region Private

		// Current position in file
		private long _CurrentPosition;

		#endregion Private

		/// <summary>
		/// Reader for the file.
		/// </summary>
		public BinaryReader Reader
		{
			get;
			set;
		}

		/// <summary>
		/// Repeat the current record on the next BeginRecord?
		/// </summary>
		public bool Repeat
		{
			get;
			set;
		}

		/// <summary>
		/// The current record being processed
		/// </summary>
		public Opcodes Opcode
		{
			get;
			set;
		}

		/// <summary>
		/// The length of the current record being processed.
		/// </summary>
		public ushort Length
		{
			get;
			set;
		}

		/// <summary>
		/// The current level in the tree.
		/// </summary>
		public int Level
		{
			get;
			set;
		}

		#endregion Properties

		/// <summary>
		/// Creates a new binary stream.
		/// </summary>
		/// <param name="file"></param>
		public InStream(string file)
		{
			Stream s = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			Reader = BitConverter.IsLittleEndian ? new BinaryReaderBigEndian(s) : new BinaryReader(s);
			Repeat = false;
			_CurrentPosition = 0;
		}

		/// <summary>
		/// Attempts to read the next record in the stream. Returns true if successful
		/// or false if no data is left or an error occured.
		/// </summary>
		/// <returns></returns>
		public bool BeginRecord()
		{
			if (Repeat)
			{
				// We need to repeat this record so do nothing this time.
				Repeat = false;
			}
			else
			{
				// Move to next record
				_CurrentPosition += Length;
			}

			if (Reader.BaseStream.Length - _CurrentPosition < 4)
			{
				// TODO: Not enough data left, close the file                
				return false;
			}

			try
			{
				Reader.BaseStream.Seek(_CurrentPosition, SeekOrigin.Begin);

				// Read record header
				Opcode = (Opcodes)Reader.ReadInt16();
				Length = Reader.ReadUInt16();
			}
			catch (Exception e)
			{
				Debug.LogError("Parse Error!\n" + e.ToString());
				return false;
			}

			return true;
		}
	}
}