using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator
{
    /* 
     * 'Width' is how big the map will be (in this project its always csizeXcsize)
     * 'Seed' is ... well.. seed. Same seed will return same map, regardless of the other values, altough appearence may be different due to them.
     * 'Scale' controls how small the map appears. Imagine it like zooming in-out with small-big numbers (like 200-800)
     * 'Octaves' refer to # of individual layers of noise.
     * 'Persistence' controls the decrease in amplitude of each octave. 0 is flat, 1 is normal.
     * 'Lacunarity' controls the increase in frequency of each octave.
     * Width(x) is ->, Length(z) is v (aka Depth)
     */
    public static float[,] GeneratePerlinNoiseMap(int mapWidth, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        // The noise Image in 2D array form
        float[,] noiseMap = new float[mapWidth, mapWidth];

        // Pseudo-Random number Generator
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        for (int i = 0; i < octaves; i++)
        {
            int random = prng.Next(-100000, 100000);
            float offsetX = random + offset.x;
            float offsetZ = random + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetZ);

            maxPossibleHeight += amplitude;
            amplitude *= persistance;
        }

        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        for (int z = 0; z < mapWidth; z++)
        {
            for (int x = 0; x < mapWidth; x++)
            {

                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    // Divides the samples by a factor between .6-100.
                    // Scale closer to .6, noise map zooms out basically. Closer to 100, noise map zooms in, without losing resolution.
                    // (.6 is used because anything lower than that breaks everything for some reason.)
                    float sampleX = (x - (mapWidth/2) + octaveOffsets[i].x) / scale * frequency;
                    float sampleZ = (z - (mapWidth/2) + octaveOffsets[i].y) / scale * frequency;

                    float perlinNoiseValue = Mathf.PerlinNoise(sampleX, sampleZ) * 2 - 1;
                    noiseHeight += perlinNoiseValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                noiseMap[x, z] = noiseHeight;
            }
        }

        for (int z = 0; z < mapWidth; z++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float normalizedHeight = (noiseMap[x, z] + 1) / (maxPossibleHeight);
                noiseMap[x, z] = Mathf.Clamp(normalizedHeight, int.MinValue, int.MaxValue);
            }
        }

        return noiseMap;
    }

    public static float[,] GenerateHeatMap()
    {
        float[,] heatMap = new float[128, 128];
        return heatMap;
    }

    public static float[,] GenerateMoistureMap()
    {
        float[,] moistureMap = new float[128, 128];
        return moistureMap;
    }

    // Remaps any range of values to another one.
    // Ex: the range 0 - 1 to -10 - 10, so a value of 0.5 in 0-1 is 0 in -10-10.
    // Use this math for anything you might need.

    // return finalRangeMin + (value - startRangeMin) * (finalRangeMax - finalRangeMin) / (startRangeMax - startRangeMin);
}
