using UnityEngine;
using System;

namespace UFLT.Utils
{
	/// <summary>
	/// Taken from the Unity forum's:
	/// http://forum.unity3d.com/threads/121966-How-to-assign-Matrix4x4-to-Transform
	/// Extension functions for converting a Matrix into position, rotation and scale values.
	/// </summary>    
	internal static class MatrixUtils
	{
		/// <summary>
		/// Converts the matrix into position, rotation, scale and assigns to the transform.
		/// </summary>
		/// <param name="transform"></param>
		/// <param name="matrix"></param>
		public static void FromMatrix4x4(this Transform transform, Matrix4x4 matrix)
		{
			transform.localScale = matrix.GetScale();
			transform.localRotation = matrix.GetRotation();
			transform.localPosition = matrix.GetPosition();
		}

		/// <summary>
		/// Returns the rotation for the matrix.
		/// </summary>
		/// <param name="matrix"></param>
		/// <returns></returns>
		public static Quaternion GetRotation(this Matrix4x4 matrix)
		{
			var qw = Mathf.Sqrt(1f + matrix.m00 + matrix.m11 + matrix.m22) / 2;
			var w = 4 * qw;
			var qx = (matrix.m21 - matrix.m12) / w;
			var qy = (matrix.m02 - matrix.m20) / w;
			var qz = (matrix.m10 - matrix.m01) / w;
			return new Quaternion(qx, qy, qz, qw);
		}

		/// <summary>
		/// Returns the position for the matrix.
		/// </summary>
		/// <param name="matrix"></param>
		/// <returns></returns>
		public static Vector3 GetPosition(this Matrix4x4 matrix)
		{
			var x = matrix.m03;
			var y = matrix.m13;
			var z = matrix.m23;
			return new Vector3(x, y, z);
		}

		/// <summary>
		/// Returns the scale for the matrix.
		/// </summary>
		/// <param name="m"></param>
		/// <returns></returns>
		public static Vector3 GetScale(this Matrix4x4 m)
		{
			var x = Mathf.Sqrt(m.m00 * m.m00 + m.m01 * m.m01 + m.m02 * m.m02);
			var y = Mathf.Sqrt(m.m10 * m.m10 + m.m11 * m.m11 + m.m12 * m.m12);
			var z = Mathf.Sqrt(m.m20 * m.m20 + m.m21 * m.m21 + m.m22 * m.m22);
			return new Vector3(x, y, z);
		}
	}
}