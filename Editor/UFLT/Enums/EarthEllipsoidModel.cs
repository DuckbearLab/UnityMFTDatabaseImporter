
namespace UFLT.DataTypes.Enums
{
    /// <summary>
    /// Earth Ellipsoid Model. 
    /// </summary>
    public enum EarthEllipsoidModel : int
    {
        WGS_1984    = 0,
        WGS_1972    = 1,
        Bessel      = 2,
        Clarke_1866 = 3,
        NAD_1927    = 4,
        UserDefined = -1
    }
}