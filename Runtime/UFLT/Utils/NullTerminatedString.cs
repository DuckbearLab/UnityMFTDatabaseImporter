using System.Text;

namespace UFLT.Utils
{
	public class NullTerminatedString
	{
		/// <summary>
		/// Reads a bytes array of null terminated ASCII characters.
		/// </summary>		
		/// <param name='bytes'>Null terminated ASCII bytes</param>
		public static string GetAsString(byte[] bytes)
		{
			int offset = bytes.Length;
			for (int i = 0; i < bytes.Length; ++i)
			{
				if (bytes[i] == 0)
				{
					// Found the terminator
					offset = i;
					break;
				}
			}

			return Encoding.ASCII.GetString(bytes, 0, offset);
		}
	}
}