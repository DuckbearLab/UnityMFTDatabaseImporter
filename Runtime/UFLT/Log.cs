using UnityEngine;
using System.Collections;
using System.Text;

namespace UFLT
{
	/// <summary>
	/// Log class. This allows for multiple Debug messages without cluttering the console.
	/// </summary>
	public class Log
	{
		#region Properties

		/// <summary>
		/// Gets or sets the log builder.		
		/// </summary>		
		public StringBuilder LogBuilder
		{
			get;
			set;
		}

		#endregion Properties	

		public Log()
		{
			LogBuilder = new StringBuilder();
		}

		/// <summary>
		/// Writes a line to the log.
		/// </summary>
		public void Write(string line)
		{
			LogBuilder.AppendLine(line);
		}

		/// <summary>
		/// Writes a warning line to the log.
		/// </summary>
		public void WriteWarning(string line)
		{
			LogBuilder.AppendLine("WARNING: " + line);
		}

		/// <summary>
		/// Writes a error line to the log.
		/// </summary>
		public void WriteError(string line)
		{
			LogBuilder.AppendLine("ERROR: " + line);
		}

		/// <summary>
		/// Returns log data as one string.
		/// </summary>        
		public new string ToString()
		{
			return LogBuilder.ToString();
		}
	}
}