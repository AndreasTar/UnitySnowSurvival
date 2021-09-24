using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PRIMAL_TextureGen
{
    // Creates the texture from a colorMap and applies it to the GameObject
    public static Texture2D TextureFromColorMap(Color[] colorMap, int width, int length)
    {
        Texture2D texture = new Texture2D(width, length);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        texture.SetPixels(colorMap);
        texture.Apply();
        return texture;
    }

    public static Texture2D ColorMapFromHeightMap(float[,] noiseMap)
    {
        int width = noiseMap.GetLength(0);
        int length = noiseMap.GetLength(1);

        // This is the texture of the object.
        //Texture2D noiseTexture = new Texture2D(width, length);

        // This is the colorMap that the noiseTexture will apply as its texture.
        Color[] colorMap = new Color[width * length];

        for (int y = 0; y < length; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
            }
        }

        return TextureFromColorMap(colorMap, width, length);
    }
}
