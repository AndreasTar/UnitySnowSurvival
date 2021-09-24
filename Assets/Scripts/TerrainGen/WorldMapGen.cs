using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class WorldMapGen : MonoBehaviour
{
    // Controls the size of the world, consequently the size of the worldNoiseMap.
    // Tiny is , Small is, Medium is, Big is, Huge is
    public enum WorldSize { Tiny, Small, Medium, Big, Huge };
    [Header("World Attributes")]
    public static WorldSize worldSize;

    public enum DebugDisplayMode { Noise, Heat, Biomes }
    [Header("Display Mode")]
    public DebugDisplayMode displayMode;


    [Header("Noise Attributes")]
    [Range((float)0.6, 500)]
    public float noiseScale;
    [Range(1, 20)]
    public int octaves;
    [Range(0, (float)1.5)]
    public float persistance;
    public float lacunarity;
    public Vector2 offset;
    public int seed;

    [Header("Mesh Settings")]
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    public bool autoUpdate;

    ChunkGen[,] chunksArray;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    public void GenerateWorldMap()
    {

        float[,] noiseMap;
        //float[,] noiseMap2;
        //chunksArray = chunks.GenerateChunkArray(worldMapSize);
        //chunks.AddHeatmapToEveryChunk(worldMapSize);

        // Each world chunk is 128x128
        //biomeMap = new BiomeData[worldMapSize, worldMapSize];

        //if (displayMode == DebugDisplayMode.Noise)
        //{
        //    noiseMap = NoiseGen.GeneratePerlinNoiseMap(128, 128, seed, noiseScale, octaves, persistance, lacunarity, offset);
        //}
        //else if (displayMode == DebugDisplayMode.Heat)
        //{
        //    noiseMap = chunksArray[1,1].GetHeatmap();
        //}
        //else
        //{
        //    noiseMap = NoiseGen.GeneratePerlinNoiseMap(128, 128, seed, noiseScale, octaves, persistance, lacunarity, offset);
        //}

        //Vector2 offset2 = new Vector2(offset.x + 5, offset.y);
        noiseMap = NoiseGen.GeneratePerlinNoiseMap(128, seed, noiseScale, octaves, persistance, lacunarity, offset);
        //noiseMap2 = NoiseGen.GeneratePerlinNoiseMap(128, 128, seed, noiseScale, octaves, persistance, lacunarity, offset2);

        MapDisplay display = FindObjectOfType<MapDisplay>();
        display.DrawNoiseMap(noiseMap);
        //display.DrawNoiseMap(noiseMap2);
    }

    void Update()
    {
        if (mapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    public void RequestMapData(Vector2 center, Action<MapData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(center, callback);
        };

        new Thread(threadStart).Start();
    }

    public void RequestMeshData(MapData mapData, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(mapData, callback);
        };
        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 center, Action<MapData> callback)
    {
        MapData mapData = GenerateMapData(center);
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    void MeshDataThread(MapData mapData, Action<MeshData> callback)
    {
        MeshData meshData = MeshGen.GenerateMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    MapData GenerateMapData(Vector2 center)
    {
        float[,] noiseMap = NoiseGen.GeneratePerlinNoiseMap(128 + 2, seed, noiseScale, octaves, persistance, lacunarity, center + offset);

        Color[] colorMap = new Color[128 * 128];

        float[,] heatMap = NoiseGen.GenerateHeatMap();

        return new MapData(noiseMap, colorMap, heatMap);
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

    public static int worldMapSize
    {
        get
        {
            // Max world size is 4096 total chunks so 64 chunks by 64 chunks
            if (worldSize == WorldSize.Tiny)
            {
                return 64;
            }
            // Max world size is 5184 total chunks so 72 chunks by 72 chunks
            if (worldSize == WorldSize.Small)
            {
                return 72;
            }
            // Max world size is 7056 total chunks so 84 chunks by 84 chunks
            if (worldSize == WorldSize.Medium)
            {
                return 84;
            }
            // Max world size is 10000 total chunks so 100 chunks by 100 chunks
            if (worldSize == WorldSize.Big)
            {
                return 100;
            }
            // Max world size is 2^14 = 16384 total chunks so 128 chunks by 128 chunks
            else
            {
                return 128;
            }
        }
    }
}
