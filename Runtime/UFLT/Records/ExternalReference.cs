using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using UFLT.DataTypes.Enums;
using UFLT.Utils;

namespace UFLT.Records
{
	/// <summary>
	/// An externally referenced OpenFlight database file.
	/// </summary>
	public class ExternalReference : InterRecord
	{
		#region Properties

		/// <summary>
		/// Path to the referenced file.
		/// </summary>		
		public string Path
		{
			get;
			set;
		}

		/// <summary>
		/// An absolute path to the file generated if the file can be found.
		/// </summary>		
		public string AbsolutePath
		{
			get;
			set;
		}

		/// <summary>
		/// Flags (bits, from left to right)
		///  0 = Color palette override
		///  1 = Material palette override
		///  2 = Texture and texture mapping palette override
		///  3 = Line style palette override
		///  4 = Sound palette override
		///  5 = Light source palette override
		///  6 = Light point palette override
		///  7 = Shader palette override
		///  8-31 = Spare
		/// </summary>
		public int Flags
		{
			get;
			set;
		}

		/// <summary>
		/// Flags value
		/// Is the color palette overriden by the parent db?
		/// </summary>
		public bool FlagsColorPaletteOverridden
		{
			get
			{
				return (Flags & -2147483648) != 0 ? true : false;
			}
		}

		/// <summary>
		/// Flags value
		/// Is the material palette overriden by the parent db?
		/// </summary>
		public bool FlagsMaterialPaletteOverridden
		{
			get
			{
				return (Flags & 0x40000000) != 0 ? true : false;
			}
		}

		/// <summary>
		/// Flags value
		/// Is the texture palette & texture mapping overridden by the parent db.
		/// </summary>
		public bool FlagsTexturePaletteOverridden
		{
			get
			{
				return (Flags & 0x20000000) != 0 ? true : false;
			}
		}

		/// <summary>
		/// Flags value        
		/// Is the line style palette overriden by the parent db.
		/// </summary>
		public bool FlagsLineStylePaletteOverridden
		{
			get
			{
				return (Flags & 0x10000000) != 0 ? true : false;
			}
		}

		/// <summary>
		/// Flags value        
		/// Is the sound palette overridden by the parent db?
		/// </summary>
		public bool FlagsSoundPaletteOverridden
		{
			get
			{
				return (Flags & 0x8000000) != 0 ? true : false;
			}
		}

		/// <summary>
		/// Flags value
		/// Hidden face, not drawn.
		/// </summary>
		public bool FlagsLightSourcePalette
		{
			get
			{
				return (Flags & 0x4000000) != 0 ? true : false;
			}
		}

		/// <summary>
		/// Flags value
		/// </summary>
		public bool FlagsRoofline
		{
			get
			{
				return (Flags & 0x2000000) != 0 ? true : false;
			}
		}

		/// <summary>
		/// View external reference as bounding box(true) or normal(false).
		/// </summary>		
		public bool ViewAsBoundingBox
		{
			get;
			set;
		}

		/// <summary>
		/// The external reference.
		/// </summary>		
		public Database Reference
		{
			get;
			set;
		}

		#endregion Properties

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="parent"></param>
		public ExternalReference(Record parent) :
			base(parent, parent.Header)
		{
			RootHandler.Handler[Opcodes.Matrix] = HandleMatrix;
            // DuckbearLab: FIX! Handle faces
            ChildHandler.Handler[Opcodes.Face] = HandleFace;
			RootHandler.ThrowBacks.UnionWith(RecordHandler.ThrowBackOpcodes);
		}

		/// <summary>
		/// Parses binary stream.
		/// </summary>
		public override void Parse()
		{
            //return;

			Path = NullTerminatedString.GetAsString(Header.Stream.Reader.ReadBytes(200));
			Header.Stream.Reader.BaseStream.Seek(4, SeekOrigin.Current); // Skip reserved.
			Flags = Header.Stream.Reader.ReadInt32();
			ViewAsBoundingBox = Header.Stream.Reader.ReadInt16() == 1 ? true : false;

			// Find the file
			AbsolutePath = FileFinder.Instance.Find(Path);
			if (AbsolutePath != string.Empty)
			{
				ID = "Ref: " + Path;
                
                if (!Header.ExternalReferencesBank.Contains(Path))
                {
                    Header.ExternalReferencesBank.Add(Path, this);

                    Reference = new Database(AbsolutePath, this, Header.Settings);

                    // Override 
                    //if( FlagsColorPaletteOverridden ) Reference.ColorPalette = Header.ColorPalette;
                    //if( FlagsMaterialPaletteOverridden ) Reference.MaterialPalettes = Header.MaterialPalettes;
                    //if( FlagsTexturePaletteOverridden ) Reference.TexturePalettes = Header.TexturePalettes;

                    // TODO: implement overrides for other records that are not currently implemented.		

                    if (FlagsMaterialPaletteOverridden || FlagsTexturePaletteOverridden)
                    {
                        Reference.MaterialBank = Header.MaterialBank; // Share material bank.	
                    }
                    Reference.ExternalReferencesBank = Header.ExternalReferencesBank;

                    if(!ID.StartsWith("Ref: tree"))
                        Reference.Parse();
                }
			}
			else
			{
				ID = "Broken Ref: " + Path;
				Log.WriteError("Could not find external reference: " + Path);
			}

			base.Parse();
		}

        public override void PrepareForImport()
        {
            if (Header.ExternalReferencesBank.ContainsMe(Path, this))
                base.PrepareForImport();
        }

        public override void ImportIntoScene()
        {
            if (Header.ExternalReferencesBank.ContainsMe(Path, this))
            {
                base.ImportIntoScene();
            }
            else
            {
                //Transform t = Header.ExternalReferencesBank.DEL.transform;

                ////t.SetParent(null);

                //t = new GameObject().transform;
                //t.localPosition = Position;
                //t.localRotation = Rotation;
                //t.localScale = Scale;

                //if (Parent != null && Parent is InterRecord)
                //{
                //    t.SetParent((Parent as InterRecord).UnityGameObject.transform, false);
                //}

                //var a = t.position;
                //var b = t.localPosition;

                //Header.ExternalReferencesBank.AddCopy(Header.ExternalReferencesBank.Get(Path).UnityGameObject,
                //    t.position,
                //    t.rotation,
                //    t.lossyScale);

                /*if (Parent != null && Parent is InterRecord)
                {
                    Transform parent = (Parent as InterRecord).UnityGameObject.transform;

                    Header.ExternalReferencesBank.AddCopy(Header.ExternalReferencesBank.Get(Path).UnityGameObject,
                    parent.TransformPoint(Position),
                    parent.rotation * Rotation,
                    new Vector3(parent.lossyScale.x * Scale.x, parent.lossyScale.y * Scale.y, parent.lossyScale.z * Scale.z)
                    );
                }
                else
                {
                    Header.ExternalReferencesBank.AddCopy(Header.ExternalReferencesBank.Get(Path).UnityGameObject,
                    Position,
                    Rotation,
                    Scale);
                }*/

                GameObject toCopy = null;
                try
                {
                    toCopy = Header.ExternalReferencesBank.Get(Path).UnityGameObject;
                }
                catch(System.Exception e)
                {
                    int a = 5 + 5;
                    a++;
                }


                //// Create an empty gameobject
                ///*UnityGameObject = GameObject.Instantiate(toCopy);*/

                UnityGameObject = new GameObject(toCopy.name);

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

                UnityGameObject.AddComponent<PutCopyMarker>().ToCopy = toCopy;

            }
        }
	}
}