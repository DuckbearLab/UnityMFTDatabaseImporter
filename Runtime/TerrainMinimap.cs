using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainMinimap : MonoBehaviour {

    public Texture2D MapTexture;
    public Texture2D[] MapTextureParts;
    public int MapTexturePartsColumns;
    public int MapTexturePartsRows;
    public Vector2 MapMin;
    public Vector2 MapMax;

    void Awake()
    {
        MapTexture = new Texture2D(1024 * MapTexturePartsColumns, 1024 * MapTexturePartsRows, TextureFormat.DXT1, false);

        int i = 0;
        for (int y = 0; y < MapTexturePartsRows; y++)
            for (int x = 0; x < MapTexturePartsColumns; x++)
                Graphics.CopyTexture(MapTextureParts[i++], 0, 0, 0, 0, 1024, 1024, MapTexture, 0, 0, 1024 * x, 1024 * y);
    }

}
