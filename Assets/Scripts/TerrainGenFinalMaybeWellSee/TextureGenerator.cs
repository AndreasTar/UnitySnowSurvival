using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureGenerator
{
    public static Texture2D TextureFromColorMap(Color[] colorMap, int size)
    {
        Texture2D texture = new Texture2D(size, size);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        texture.SetPixels(colorMap);
        texture.Apply();

        return texture;
    }

    public static Texture2D TextureFromHeightMap(float[,] heightMap)
    {
        int sizeAxis = heightMap.GetLength(0);

        //Creates an 1D array and for every value in the heightMap (0 to 1) creates a grey-scale color (0 is black, 1 is white),
        //and stores it to send it to TextureFromColorMap
        Color[] colormap = new Color[sizeAxis * sizeAxis];
        for (int y = 0; y < sizeAxis; y++)
        {
            for (int x = 0; x < sizeAxis; x++)
            {
                colormap[y * sizeAxis + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
            }
        }

        return TextureFromColorMap(colormap, sizeAxis);
    }
}
