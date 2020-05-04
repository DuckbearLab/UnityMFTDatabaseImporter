using UnityEngine;
using System.Collections;
using System.IO;
using System.Net;
using System;

namespace UFLT.Streams
{
	/// <summary>
	/// BinaryReader that supports reading big-endian data.
	/// </summary>
	public class BinaryReaderBigEndian : BinaryReader
	{
		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="input"></param>
		public BinaryReaderBigEndian(Stream input) :
			base(input)
		{
		}

		/// <summary>
		/// Reads data and swaps endian before returning.
		/// </summary>
		/// <returns></returns>
		public override float ReadSingle()
		{
			// Get raw data
			byte[] bytes = base.ReadBytes(4);

			// Endian swap
			Array.Reverse(bytes);

			return BitConverter.ToSingle(bytes, 0);
		}

		/// <summary>
		/// Reads data and swaps endian before returning.
		/// </summary>
		/// <returns></returns>
		public override double ReadDouble()
		{
			// Get raw data
			byte[] bytes = base.ReadBytes(8);

			// Endian swap
			Array.Reverse(bytes);

			return BitConverter.ToDouble(bytes, 0);
		}

		/// <summary>
		/// Reads data and swaps endian before returning.
		/// </summary>
		/// <returns></returns>
		public override short ReadInt16()
		{
			return IPAddress.HostToNetworkOrder(base.ReadInt16());
		}

		/// <summary>
		/// Reads data and swaps endian before returning.
		/// </summary>
		/// <returns></returns>
		public override int ReadInt32()
		{
			return IPAddress.HostToNetworkOrder(base.ReadInt32());
		}

		/// <summary>
		/// Reads data and swaps endian before returning.
		/// </summary>
		/// <returns></returns>
		public override long ReadInt64()
		{
			return IPAddress.HostToNetworkOrder(base.ReadInt64());
		}

		/// <summary>
		/// Reads data and swaps endian before returning.
		/// </summary>
		/// <returns></returns>
		public override ushort ReadUInt16()
		{
			// Get raw data
			byte[] bytes = base.ReadBytes(2);

			// Endian swap
			Array.Reverse(bytes);

			return BitConverter.ToUInt16(bytes, 0);
		}

		/// <summary>
		/// Reads data and swaps endian before returning.
		/// </summary>
		/// <returns></returns>
		public override uint ReadUInt32()
		{
			// Get raw data
			byte[] bytes = base.ReadBytes(4);

			// Endian swap
			Array.Reverse(bytes);

			return BitConverter.ToUInt32(bytes, 0);
		}

		/// <summary>
		/// Reads data and swaps endian before returning.
		/// </summary>
		/// <returns></returns>
		public override ulong ReadUInt64()
		{
			// Get raw data
			byte[] bytes = base.ReadBytes(8);

			// Endian swap
			Array.Reverse(bytes);

			return BitConverter.ToUInt64(bytes, 0);
		}
	}
}