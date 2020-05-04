
namespace UFLT.DataTypes.Enums
{
    /// <summary>
    /// Projection used by the file, if any. This applies if the OpenFlight file is a terrain.
    /// </summary>
    public enum Projection : int
    {
        FlatEarth   = 0,
        Trapezoidal = 1,
        RoundEarth  = 2,
        Lambert     = 3,
        UTM         = 4,
        Geodetic    = 5,
        Geocentric  = 6
    }
}