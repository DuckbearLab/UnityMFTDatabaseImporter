
namespace UFLT.DataTypes.Enums
{
    /// <summary>
    /// Face draw type.
    /// </summary>
    public enum DrawType : sbyte
    {
        DrawSolidWithBackfaceCulling = 0,
        DrawSolidNobackfaceCulling = 1,
        DrawWireFrameAndClose = 2,
        DrawWireFrame = 3,
        SurroundWithwireFrameInAlternateColor = 4,
        OmnidirectionalLight = 8,
        UnidirectionalLight = 9,
        BidirectionalLight = 10
    }
}