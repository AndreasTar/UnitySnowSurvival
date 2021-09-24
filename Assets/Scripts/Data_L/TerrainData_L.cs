using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class TerrainData : UpdateableData_L
{
    public bool useFlatShading;

    public float uniformScale = 2.5f;
    [Header("Falloff")]
    public bool useFalloff;
    [Range(0, 8)]
    public float curveSteepness = 3;
    [Range(0, 10)]
    public float curveFalloff = 2.2f;

    [Header("Mesh Settings")]
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;
}
