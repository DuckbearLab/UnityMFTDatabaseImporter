
namespace UFLT.DataTypes.Enums
{
    /// <summary>
    /// Face light mode
    /// </summary>
    public enum LightMode : byte
    {
        /// <summary>
        /// Use face color, not illuminated (Flat)
        /// </summary>
        Flat = 0,

        /// <summary>
        /// Use vertex colors, not illuminated (Gouraud)
        /// </summary>        
        Gouraud = 1,

        /// <summary>
        /// Use face color and vertex normals (Lit)
        /// </summary>
        Lit = 2,

        /// <summary>
        /// Use vertex colors and vertex normals (Lit Gouraud)
        /// </summary>
        LitGouraud = 3
    }
}