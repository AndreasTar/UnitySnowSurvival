using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FalloffGenerator_L
{
    public static float[,] GenerateFalloffMap(int size, float curveSteep, float curveFalloff)
    {
        float[,] map = new float[size, size];

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                float x = i / (float)size * 2 - 1;
                float y = j / (float)size * 2 - 1;

                float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                map[i, j] = Evaluate(value, curveSteep, curveFalloff);
            }
        }

        return map;
    }

    static float Evaluate(float value, float steep, float falloff)
    {
        return Mathf.Pow(value, steep) / (Mathf.Pow(value, steep) + Mathf.Pow(falloff - falloff * value, steep));
    }
}
