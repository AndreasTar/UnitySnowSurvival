using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structs
{

    public struct NoiseData
    {
        public readonly int seed;
        public readonly float scale;
        public readonly int octaves;
        public readonly float persistance;
        public readonly float lacunarity;
        public readonly Vector2 offset;

        public NoiseData(int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
        {
            this.seed = seed;
            this.scale = scale;
            this.octaves = octaves;
            this.persistance = persistance;
            this.lacunarity = lacunarity;
            this.offset = offset;
        }
    }
}
