using UnityEngine;
using System.Collections.Generic;

namespace UFLT.Utils
{
	/// <summary>
	/// Triangulates a face, taken from wiki:
	/// http://wiki.unity3d.com/index.php?title=Triangulator
	/// </summary>
	public class Triangulator
	{
		private List<Vector2> m_points = new List<Vector2>();

		/// <summary>
		/// Inits the triangulator.
		/// </summary>
		/// <param name='points'>Points</param>
		/// <param name='normal'>Face normal</param>
		public void initTriangulator(List<Vector3> points, Vector3 normal)
		{
			var quad = Quaternion.FromToRotation(normal, Vector3.forward);

			foreach (var v in points)
				m_points.Add(quad * v);
		}

		/// <summary>
		/// Inits the triangulator.
		/// </summary>
		/// <param name='points'>Points</param>
		/// <param name='normal'>Face normal</param>
		public void initTriangulator(List<Vector2> points, Vector3 normal)
		{
			m_points = new List<Vector2>(points);
		}

		/// <summary>
		/// Generates triangles from the points, returns indexes.
		/// </summary>
		/// <param name='points'>Points</param>
		/// <param name='normal'>Face normal</param>
		public int[] Triangulate(int offset)
		{
			var indices = new List<int>();

			var n = m_points.Count;
			if (n < 3)
				return indices.ToArray();

			var V = new int[n];
			if (Area() > 0)
			{
				for (var v = 0; v < n; v++)
					V[v] = v;
			}
			else
			{
				for (var v = 0; v < n; v++)
					V[v] = (n - 1) - v;
			}

			var nv = n;
			var count = 2 * nv;
			var m = 0;
			for (var v = nv - 1; nv > 2;)
			{
				if ((count--) <= 0)
					return indices.ToArray();

				var u = v;
				if (nv <= u)
					u = 0;
				v = u + 1;
				if (nv <= v)
					v = 0;
				var w = v + 1;
				if (nv <= w)
					w = 0;

				if (Snip(u, v, w, nv, V))
				{
					int a;
					int b;
					int c;
					int s;
					int t;
					a = V[u];
					b = V[v];
					c = V[w];
					indices.Add(offset + a);
					indices.Add(offset + b);
					indices.Add(offset + c);
					m++;
					s = v;
					for (t = v + 1; t < nv; t++)
					{
						V[s] = V[t];
						s++;
					}
					nv--;
					count = 2 * nv;
				}
			}

			return indices.ToArray();
		}

		private float Area()
		{
			int n = m_points.Count;
			float A = 0.0f;
			int q = 0;
			for (var p = n - 1; q < n; p = q++)
			{
				var pval = m_points[p];
				var qval = m_points[q];
				A += pval.x * qval.y - qval.x * pval.y;
			}
			return (A * 0.5f);
		}

		private bool Snip(int u, int v, int w, int n, int[] V)
		{
			int p;
			var A = m_points[V[u]];
			var B = m_points[V[v]];
			var C = m_points[V[w]];

			if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
				return false;
			for (p = 0; p < n; p++)
			{
				if ((p == u) || (p == v) || (p == w))
					continue;
				var P = m_points[V[p]];
				if (InsideTriangle(A, B, C, P))
					return false;
			}
			return true;
		}

		private bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
		{
			float ax;
			float ay;
			float bx;
			float by;
			float cx;
			float cy;
			float apx;
			float apy;
			float bpx;
			float bpy;
			float cpx;
			float cpy;
			float cCROSSap;
			float bCROSScp;
			float aCROSSbp;

			ax = C.x - B.x; ay = C.y - B.y;
			bx = A.x - C.x; by = A.y - C.y;
			cx = B.x - A.x; cy = B.y - A.y;
			apx = P.x - A.x; apy = P.y - A.y;
			bpx = P.x - B.x; bpy = P.y - B.y;
			cpx = P.x - C.x; cpy = P.y - C.y;

			aCROSSbp = ax * bpy - ay * bpx;
			cCROSSap = cx * apy - cy * apx;
			bCROSScp = bx * cpy - by * cpx;

			return ((aCROSSbp > 0.0) && (bCROSScp > 0.0) && (cCROSSap > 0.0));
		}
	}
}