using UnityEngine;
using System.Collections;
using UFLT.Records;
using UFLT.DataTypes.Enums;

namespace UFLT.Utils
{
	/// <summary>
	/// Represents a material created from various OpenFlight records/fields. 
	/// We want to defer creating the actual unity material as long as possible as this has to be done in the main thread.
	/// </summary>
	public class IntermediateMaterial
	{
		#region Properties

		/// <summary>
		/// Collates related data in order to generate Unity materials.
		/// </summary>		
		public MaterialBank MaterialBank
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the unity material.
		/// This could be null if the material has not been imported into the scene yet.
		/// </summary>		
		private Material _UnityMaterial;
		public Material UnityMaterial
		{
			get
			{
				if (_UnityMaterial == null)
				{
					_UnityMaterial = CreateUnityMaterial();
				}
				return _UnityMaterial;
			}
		}

		/// <summary>
		/// The material palette if one exists, this contains the standard material 
		/// atributes such as diffuse, specular and alpha.
		/// Can be null.
		/// </summary>
		public MaterialPalette Palette
		{
			get;
			set;
		}

		/// <summary>
		/// Main texture, can be null.
		/// </summary>
		public TexturePalette MainTexture
		{
			get;
			set;
		}

		/// <summary>
		/// Detail texture, can be null.
		/// </summary>
		public TexturePalette DetailTexture
		{
			get;
			set;
		}

		/// <summary>
		/// Transparency
		/// 0 = Opaque
		/// 65535 = Totally clear
		/// </summary>
		public ushort Transparency
		{
			get;
			set;
		}

		/// <summary>
		/// Light mode.
		/// </summary>
		public LightMode LightMode
		{
			get;
			set;
		}

		#endregion Properties     

		/// <summary>
		/// Initializes a new instance of the <see cref="UFLT.Utils.IntermediateMaterial"/> class.
		/// </summary>
		/// <param name='bank'>Material bank, used for finding materials.</param>
		/// <param name='mp'>Material palette or null.</param>
		/// <param name='main'>Main texture or null.</param>
		/// <param name='detail'>Detail texture or null.</param>
		/// <param name='transparancy'>Transparancy</param>
		/// <param name='lm'>Light mode.</param>
		public IntermediateMaterial(MaterialBank bank, MaterialPalette mp, TexturePalette main, TexturePalette detail, ushort transparancy, LightMode lm)
		{
			MaterialBank = bank;
			Palette = mp;
			MainTexture = main;
			DetailTexture = detail;
			Transparency = transparancy;
			LightMode = lm;
		}

		/// <summary>
		/// Creates the unity material.
		/// </summary>		
		protected virtual Material CreateUnityMaterial()
		{
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Simple Lit"));

            mat.SetColor("_BaseColor", Color.white);

            Texture2D texture = null;
            if (MainTexture != null)
                texture = MaterialBank.FindOrCreateTexture(MainTexture);

            bool textureTransparent = texture != null && (texture.format == TextureFormat.Alpha8 || texture.format == TextureFormat.ARGB32 || texture.format == TextureFormat.ARGB4444 || texture.format == TextureFormat.RGBA32 || texture.format == TextureFormat.RGBA4444 || texture.format == TextureFormat.DXT5 || texture.format == TextureFormat.DXT5Crunched);
            if (textureTransparent)
                mat.EnableKeyword("_ALPHATEST_ON");
            
			if (Palette != null)
			{
				mat.color = Palette.Diffuse;

				if (mat.HasProperty("_Emissive")) // Emissive color of a material (used in vertexlit shaders). 
				{
					mat.SetColor("_Emissive", Palette.Emissive);
				}

				if (mat.HasProperty("_SpecColor"))
				{
					mat.SetColor("_SpecColor", Palette.Specular);
				}

				if (mat.HasProperty("_Shininess"))
				{
					mat.SetFloat("_Shininess", Palette.Shininess / 128f);
				}
			}

            if (mat.HasProperty("_BaseMap"))
            {
                if (texture != null)
                {
                    mat.SetTexture("_BaseMap", texture);
                    mat.name = texture.name;
                }
                else if(MainTexture != null)
                {
	                mat.name = MainTexture.FileName;
                }
            }

            // DuckbearLab: FIX! It is not used now :D
			/*if (Palette != null)
			{
				mat.name = Palette.ID;
			}*/

			return mat;
		}

		/// <summary>
		/// Compares various material attrbiutes to see if this material matches.
		/// </summary>
		/// <param name="mp"></param>
		/// <param name="main"></param>
		/// <param name="detail"></param>
		/// <param name="trans"></param>
		/// <param name="lm"></param>
		/// <returns></returns>
		public bool Equals(MaterialPalette mp, TexturePalette main, TexturePalette detail, ushort trans, LightMode lm)
		{
			if (!MaterialPalette.Equals(Palette, mp)) return false;
			if (!TexturePalette.Equals(MainTexture, main)) return false;
			if (!TexturePalette.Equals(DetailTexture, detail)) return false;
			if (Transparency != trans) return false;
			if (LightMode != lm) return false;
			return true;
		}
	}
}