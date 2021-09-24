using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseGen
{

    // 'Octaves' refer to # of individual layers of noise.
    // 'Lacunarity' controls the increase in frequency of each octave.
    // 'Persistence' controls the decrease in amplitude of each octave. 0 is flat, 1 is normal.
    // 'Seed' is ... well.. seed. Same seed will return same map, regardless of the other values.
    // Width(x) is ->, Length(z) is v
    public static float[,] GeneratePerlinNoiseMap(int mapWidth, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        // The noise Image in 2D array form
        float[,] noiseMap = new float[mapWidth, mapWidth];

        // Pseudo-Random number Generator
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetZ = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetZ);
        }

        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        for (int z = 0; z < mapWidth; z++)
        {
            for (int x = 0; x < mapWidth; x++)
            {

                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    // Divides the samples by a factor between .6-100.
                    // Scale closer to .6, noise map zooms out basically. Closer to 100, noise map zooms in, without losing resolution.
                    // (.6 is used because anything lower than that breaks everything for some reason.)
                    float sampleX = (x / scale) * frequency + octaveOffsets[i].x;
                    float sampleZ = (z / scale) * frequency + octaveOffsets[i].y;

                    float perlinNoiseValue = Mathf.PerlinNoise(sampleX, sampleZ) * 2 - 1;
                    noiseHeight += perlinNoiseValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }

                noiseMap[x, z] = noiseHeight;
            }
        }

        for (int z = 0; z < mapWidth; z++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, z] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, z]);
            }
        }

        return noiseMap;
    }

    public static float[,] GenerateHeatMap()
    {
        float[,] heatMap = new float[128, 128];
        return heatMap;
    }

    // Remaps any range of values to another one.
    // Ex: the range 0 - 1 to -10 - 10, so a value of 0.5 in 0-1 is 0 in -10-10.
    // Use this math for anything you might need.

    // return finalRangeMin + (value - startRangeMin) * (finalRangeMax - finalRangeMin) / (startRangeMax - startRangeMin);
}
