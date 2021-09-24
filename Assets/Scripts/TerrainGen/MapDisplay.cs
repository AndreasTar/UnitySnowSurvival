using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    // This is the textureRenderer of the texture of the object.
    public Renderer textureRenderer;

    public void DrawNoiseMap(float[,] noiseMap)
    {
        int width = noiseMap.GetLength(0);
        int length = noiseMap.GetLength(1);

        // This is the texture of the object.
        Texture2D noiseTexture = new Texture2D(width, length);

        // This is the colorMap that the noiseTexture will apply as its texture.
        Color[] colorMap = new Color[width * length];

        for (int z = 0; z < length; z++)
        {
            for (int x = 0; x < width; x++)
            {
                colorMap[z * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, z]);

            }
        }

        noiseTexture.SetPixels(colorMap);
        noiseTexture.Apply();

        textureRenderer.sharedMaterial.mainTexture = noiseTexture;
        textureRenderer.transform.localScale = new Vector3(width, 1, length);
    }
}
