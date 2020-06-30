using UnityEngine;
using System.Collections.Generic;
using UFLT.DataTypes.Enums;
using System.IO;
using System.Text;
using UFLT.Utils;


namespace UFLT.Records
{
	/// <summary>
	/// The base class for all OpenFlight records that have a id/name field.
	/// </summary>
	public class InterRecord : Record
	{
		#region Properties

		/// <summary>
		/// The Unity object for this record.
		/// </summary>
		public GameObject UnityGameObject
		{
			get;
			set;
		}

		#region Mesh Params

		/// <summary>
		/// The vertices used in our mesh.
		/// </summary>
		public List<VertexWithColor> Vertices
		{
			get;
			set;
		}

		/// <summary>
		/// Position of vertices if this record contains a mesh
		/// </summary>
		public List<Vector3> VertexPositions
		{
			get;
			set;
		}

		/// <summary>
		/// Mesh vertex normals
		/// </summary>
		public List<Vector3> Normals
		{
			get;
			set;
		}

		/// <summary>
		/// Mesh vertex Uvs
		/// </summary>
		public List<Vector2> UVS
		{
			get;
			set;
		}

		/// <summary>
		/// Materials paired with their triangles.
		/// </summary>
		public List<KeyValuePair<IntermediateMaterial, List<int>>> SubMeshes
		{
			get;
			set;
		}

		#endregion Mesh Params

		/// <summary>
		/// Object position
		/// </summary>
		public Vector3 Position
		{
			get;
			set;
		}

		/// <summary>
		/// Object local scale
		/// </summary>
		public Vector3 Scale
		{
			get;
			set;
		}

		/// <summary>
		/// Object Rotation.
		/// </summary>
		public Quaternion Rotation
		{
			get;
			set;
		}

		#endregion Properties

		/// <summary>
		/// Constructor
		/// </summary>
		public InterRecord()
		{
			Scale = Vector3.one;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="header"></param>
		public InterRecord(Record parent, Database header) :
			base(parent, header)
		{
			Scale = Vector3.one;
		}

		/// <summary>
		/// Sets up record for importing into scene, if its a mesh creates the mesh structures.
		/// </summary>
		public override void PrepareForImport()
		{
			// Do we have any faces?
			if (Children.Find(o => o is Face) != null)
			{
				Vertices = new List<VertexWithColor>();
				SubMeshes = new List<KeyValuePair<IntermediateMaterial, List<int>>>();

				base.PrepareForImport();

				// Do we have any verts, we may have just processed hidden faces.
				if (Vertices.Count > 0)
				{
					// TODO: Remove doubles. Check for duplicate verts and merge if possible. dont forget to change triangle indexes.

					// Now setup for mesh
					VertexPositions = new List<Vector3>(Vertices.Count);
					Normals = new List<Vector3>(Vertices.Count);
					UVS = new List<Vector2>(Vertices.Count);

					foreach (VertexWithColor vwc in Vertices)
					{
						VertexPositions.Add(new Vector3((float)vwc.Coordinate[0], (float)vwc.Coordinate[1], (float)vwc.Coordinate[2]));

						// Normals
						if (vwc is VertexWithColorNormal)
						{
							Normals.Add((vwc as VertexWithColorNormal).Normal);
						}
						else
						{
							Normals.Add(Vector3.zero);
						}

						// Uvs
						if (vwc is VertexWithColorNormalUV)
						{
							UVS.Add((vwc as VertexWithColorNormalUV).UV);
						}
						else if (vwc is VertexWithColorUV)
						{
							UVS.Add((vwc as VertexWithColorUV).UV);
						}
						else
						{
							UVS.Add(Vector2.zero);
						}
					}
				}
			}
			else
			{
				base.PrepareForImport();
			}
		}

		/// <summary>
		/// Converts the record/s into a Unity GameObject structure with meshes,
		/// materials etc and imports into the scene.
		/// </summary>
		public override void ImportIntoScene()
		{
			// Create an empty gameobject
			UnityGameObject = new GameObject(ID);
            // DuckbearLab: FIX!
			//UnityGameObject.transform.localScale = Vector3.one;

			// Apply transformations
			UnityGameObject.transform.localPosition = Position;
			UnityGameObject.transform.localRotation = Rotation;
            if(Scale != Vector3.one)
			    UnityGameObject.transform.localScale = Scale;

			// Assign parent
			if (Parent != null && Parent is InterRecord)
			{
                // DuckbearLab: FIX!
                UnityGameObject.transform.SetParent((Parent as InterRecord).UnityGameObject.transform, false);
				//UnityGameObject.transform.parent = (Parent as InterRecord).UnityGameObject.transform;
			}

            // Add Comment
            if (!string.IsNullOrEmpty(Comment))
                UnityGameObject.AddComponent<UFLT.MonoBehaviours.Comment>().Value = Comment;

			// Processes children
			base.ImportIntoScene();

			// Create mesh
			if (Vertices != null && Vertices.Count > 0)
			{
				Mesh m = new Mesh();
				m.name = ID;
				m.vertices = VertexPositions.ToArray();
				m.normals = Normals.ToArray();
				m.uv = UVS.ToArray();

                // DuckbearLab: Fix for very large meshes
                if(VertexPositions.Count >= ushort.MaxValue)
                    m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

				MeshRenderer mr = UnityGameObject.AddComponent<MeshRenderer>();
				Material[] mats = new Material[SubMeshes.Count];
				MeshFilter mf = UnityGameObject.AddComponent<MeshFilter>();

				// Set submeshes
				m.subMeshCount = SubMeshes.Count;
				for (int i = 0; i < SubMeshes.Count; i++)
				{
					mats[i] = SubMeshes[i].Key.UnityMaterial;
					m.SetTriangles(SubMeshes[i].Value.ToArray(), i);
				}


                // DuckbearLab: OPTIMIZE!
                //mr.materials = mats;
				;
                {
                    bool equal = true;
                    foreach (var material in mats)
                        if (material != mats[0])
                            equal = false;

                    if (equal && SubMeshes.Count > 1)
                    {
                        CombineInstance[] combine = new CombineInstance[SubMeshes.Count];
                        for (int i = 0; i < combine.Length; i++)
                        {
                            combine[i].mesh = m;
                            combine[i].subMeshIndex = i;
                        }
                        var newMesh = new Mesh();
                        newMesh.name = ID;
                        newMesh.CombineMeshes(combine, true, false);
                        m = newMesh;

                        Material[] newMats = new Material[1];
                        newMats[0] = mats[0];
                        mr.materials = newMats;
                    }
                    else
                    {
                        mr.materials = mats;
                    }
                }

#if UNITY_EDITOR
                UnityEditor.MeshUtility.Optimize(m);
#endif
				mf.mesh = m;
			}
		}

		/// <summary>
		/// Returns the submesh for this face based on material info.
		/// </summary>        
		/// <param name='f'>The face to find a submesh for.</param>
		public KeyValuePair<IntermediateMaterial, List<int>> FindOrCreateSubMesh(Face f)
		{
			ExternalReference externalRef = null;
			if (Header.Parent != null)
				externalRef = Header.Parent as ExternalReference;

			// Fetch palettes
			MaterialPalette mp = null;
			if (f.MaterialIndex != -1)
			{
				if (externalRef != null)
					externalRef.Header.MaterialPalettes.TryGetValue(f.MaterialIndex, out mp);

				if (mp == null)
					Header.MaterialPalettes.TryGetValue(f.MaterialIndex, out mp);

				if (mp == null)
					Log.WriteError("Could not find material palette: " + f.MaterialIndex);
			}

			TexturePalette mainTex = null;
			if (f.TexturePattern != -1)
			{
				if (externalRef != null)
					externalRef.Header.TexturePalettes.TryGetValue(f.TexturePattern, out mainTex);

				if (mainTex == null)
					Header.TexturePalettes.TryGetValue(f.TexturePattern, out mainTex);

				if (mainTex == null)
					Log.WriteError("Could not find texture pattern: " + f.TexturePattern);
			}

			TexturePalette detailTex = null;
			if (f.DetailTexturePattern != -1)
			{
				if (externalRef != null)
					externalRef.Header.TexturePalettes.TryGetValue(f.DetailTexturePattern, out detailTex);

				if (mainTex == null)
					Header.TexturePalettes.TryGetValue(f.DetailTexturePattern, out detailTex);

				if (mainTex == null)
					Log.WriteError("Could not find detail texture pattern: " + f.DetailTexturePattern);
			}

			// Check locally
			foreach (KeyValuePair<IntermediateMaterial, List<int>> mesh in SubMeshes)
			{
				if (mesh.Key.Equals(mp, mainTex, detailTex, f.Transparency, f.LightMode))
				{
					return mesh;
				}
			}

			// Create a new submesh
			IntermediateMaterial im = Header.MaterialBank.FindOrCreateMaterial(f);
			KeyValuePair<IntermediateMaterial, List<int>> newMesh = new KeyValuePair<IntermediateMaterial, List<int>>(im, new List<int>());
			SubMeshes.Add(newMesh);
			return newMesh;
		}

		#region Record Handlers

		/// <summary>
		/// Handle matrix records.
		/// Reads a 4x4 matrix of floats, row major order and converts them 
		/// into position, rotation and scale.
		/// </summary>
		/// <returns></returns>
		protected bool HandleMatrix()
		{
			Matrix4x4 m = new Matrix4x4();
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 4; ++j)
				{
                    // DuckbearLab: FIX
					m[j, i] = Header.Stream.Reader.ReadSingle();
				}
			}

			// Convert to position, rotation & scale
			Position = m.GetPosition();
            // DuckbearLab: FIX! Inverted X
            Position = new Vector3(Position.x, Position.z, Position.y);
			Rotation = m.GetRotation();
            // DuckbearLab: FIX! Inverted X
            Rotation = Quaternion.Euler(Rotation.eulerAngles.x, -Rotation.eulerAngles.z, Rotation.eulerAngles.y);
			Scale = m.GetScale();
			return true;
		}

		/// <summary>
		/// Handles records that are not fully handled.
		/// </summary>
		/// <returns></returns>
		protected bool HandleUnhandled()
		{
			Unhandled uh = new Unhandled(this);
			uh.Parse();
			return true;
		}

		/// <summary>
		/// Handles object records.
		/// </summary>
		/// <returns></returns>
		protected bool HandleObject()
		{
			Object o = new Object(this);
			o.Parse();
			return true;
		}

		/// <summary>
		/// Handles Group records.
		/// </summary>
		/// <returns></returns>
		protected bool HandleGroup()
		{
			Group g = new Group(this);
			g.Parse();
			return true;
		}

		/// <summary>
		/// Handles Switch records.
		/// </summary>
		/// <returns></returns>
		protected bool HandleSwitch()
		{
			Switch s = new Switch(this);
			s.Parse();
			return true;
		}

		/// <summary>
		/// Handles Degree Of Freedom records.
		/// </summary>
		/// <returns></returns>
		protected bool HandleDOF()
		{
			DOF d = new DOF(this);
			d.Parse();
			return true;
		}

		/// <summary>
		/// Handles external reference records.
		/// </summary>
		/// <returns></returns>
		protected bool HandleExternalReference()
		{
			ExternalReference e = new ExternalReference(this);
			e.Parse();
			return true;
		}

		/// <summary>
		/// Handles level of detail records.
		/// </summary>
		/// <returns></returns>
		protected bool HandleLevelOfDetail()
		{
			LOD l = new LOD(this);
			l.Parse();
			return true;
		}

		#endregion Record Handlers
	}
}