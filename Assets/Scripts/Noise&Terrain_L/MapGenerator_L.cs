using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class MapGenerator_L : MonoBehaviour
{
    public enum DrawMode { NoiseMap, ColorMap, Mesh, FalloffMap };
    public DrawMode drawMode;

    public TerrainData terrainData;
    public NoiseData_L noiseData;

    [Range(0, 6)]
    public int editorPreviewLOD;

    public bool autoUpdate;

    public TerrainType[] regions;
    static MapGenerator_L instance;

    float[,] falloffMap;

    Queue<MapThreadInfo<MapData_L>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData_L>>();
    Queue<MapThreadInfo<MeshData_L>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData_L>>();

    private void Awake()
    {
        falloffMap = FalloffGenerator_L.GenerateFalloffMap(mapChunkSize, terrainData.curveSteepness, terrainData.curveFalloff);
    }

    void OnValuesUpdated()
    {
        if (!Application.isPlaying)
        {
            DrawMapInEditor();
        }
    }

    public static int mapChunkSize
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<MapGenerator_L>();
                return 95;
            }

            if (instance.terrainData.useFlatShading)
            {
                return 95;
            }
            else
            {
                return 239;
            }
        }
    }

    public void DrawMapInEditor()
    {
        MapData_L mapData = GenerateMapData(Vector2.zero);

        MapDisplay_L display = FindObjectOfType<MapDisplay_L>();
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGen_L.TextureFromHeightMap(mapData.heightMap));
        }
        else if (drawMode == DrawMode.ColorMap)
        {
            display.DrawTexture(TextureGen_L.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGen_L.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, editorPreviewLOD, terrainData.useFlatShading), TextureGen_L.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.FalloffMap)
        {
            display.DrawTexture(TextureGen_L.TextureFromHeightMap(FalloffGenerator_L.GenerateFalloffMap(mapChunkSize, terrainData.curveSteepness, terrainData.curveFalloff)));
        }
    }

    public void RequestMapData(Vector2 center, Action<MapData_L> callback)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(center, callback);
        };

        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 center, Action<MapData_L> callback)
    {
        MapData_L mapData = GenerateMapData(center);
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData_L>(callback, mapData));
        }
    }

    public void RequestMeshData(MapData_L mapData, int lod, Action<MeshData_L> callback)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(mapData, lod, callback);
        };
        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData_L mapData, int lod, Action<MeshData_L> callback)
    {
        MeshData_L meshData = MeshGen_L.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, lod, terrainData.useFlatShading);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData_L>(callback, meshData));
        }
    }

    void Update()
    {
        if (mapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData_L> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData_L> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    MapData_L GenerateMapData(Vector2 center)
    {
        float[,] noiseMap = Noise.GenerateNoiseMap_L(mapChunkSize + 2, mapChunkSize + 2, noiseData.seed, noiseData.noiseScale, noiseData.octaves, noiseData.persistance, noiseData.lacunarity, center + noiseData.offset, noiseData.normalizeMode);

        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];

        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                if (terrainData.useFalloff)
                {
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);
                }

                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight >= regions[i].height)
                    {
                        colorMap[y * mapChunkSize + x] = regions[i].color;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        return new MapData_L(noiseMap, colorMap);
    }

    private void OnValidate()
    {
        if (terrainData != null)
        {
            terrainData.OnValuesUpdated -= OnValuesUpdated;
            terrainData.OnValuesUpdated += OnValuesUpdated;
        }
        if (noiseData != null)
        {
            noiseData.OnValuesUpdated -= OnValuesUpdated;
            noiseData.OnValuesUpdated += OnValuesUpdated;
        }

        falloffMap = FalloffGenerator_L.GenerateFalloffMap(mapChunkSize, terrainData.curveSteepness, terrainData.curveFalloff);
    }

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}

[Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
}

public struct MapData_L
{
    public readonly float[,] heightMap;
    public readonly Color[] colorMap;

    public MapData_L(float[,] heightMap, Color[] colorMap)
    {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }
}