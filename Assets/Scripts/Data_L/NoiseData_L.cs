﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class NoiseData_L : UpdateableData_L
{
    public Noise.NormalizeMode normalizeMode;

    [Range(1, 250)]
    public float noiseScale;

    [Header("Noise Settings")]
    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    protected override void OnValidate()
    {
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 0)
        {
            octaves = 0;
        }

        base.OnValidate();
    }
}
