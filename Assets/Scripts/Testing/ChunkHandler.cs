/*using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class ChunkHandler : MonoBehaviour
{
    public Transform viewer;
    public static float viewDistance = 300;

    public static Vector2 viewerPos;
    Vector2 viewerPosOld;

    const float viewerMoveThreshForUpdate = 20f;
    const float sqrViewerMoveThreshForUpdate = viewerMoveThreshForUpdate * viewerMoveThreshForUpdate;

    int chunksVisibleNum;

    ChunkGenerator chunkGenerator;
    public MeshFilter meshFilter;

    Dictionary<Vector2, Chunk> chunkDictionary = new Dictionary<Vector2, Chunk>();
    static List<Chunk> chunksVisibleLastUpdate = new List<Chunk>();

    void Start()
    {
        chunksVisibleNum = Mathf.RoundToInt(viewDistance / 128);
    }

    void Update()
    {
        viewerPos = new Vector2(viewer.position.x, viewer.position.z);

        if ((viewerPosOld - viewerPos).sqrMagnitude > sqrViewerMoveThreshForUpdate)
        {
            viewerPosOld = viewerPos;
            HandleVisibleChunks();
        }
    }

    public void SetMeshFilter(MeshFilter meshF)
    {
        this.meshFilter = meshF;
    }

    public void HandleVisibleChunks()
    {
        for (int i = 0; i < chunksVisibleLastUpdate.Count; i++)
        {
            chunksVisibleLastUpdate[i].SetVisible(false);
        }
        chunksVisibleLastUpdate.Clear();

        int chunkCoordX = Mathf.RoundToInt(viewerPos.x / 128);
        int chunkCoordY = Mathf.RoundToInt(viewerPos.y / 128);

        for (int yOffset = -chunksVisibleNum; yOffset <= chunksVisibleNum; yOffset++)
        {
            for (int xOffset = -chunksVisibleNum; xOffset <= chunksVisibleNum; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(chunkCoordX + xOffset, chunkCoordY + yOffset);

                if (chunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    chunkDictionary[viewedChunkCoord].UpdateThisChunk();
                }
                else
                {
                    Chunk generatedChunk = chunkGenerator.GenerateChunk(viewedChunkCoord, transform);
                    chunkDictionary.Add(viewedChunkCoord, generatedChunk);
                }

            }
        }
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------

    [RequireComponent(typeof(MeshFilter))]
    public class Chunk
    {
        GameObject chunkObject;
        Vector2 chunkPos;
        Bounds chunkBounds;

        ChunkData chunkData;
        ChunkHandler chunkHandler;
        CollisionMesh collisionMesh;

        MeshFilter chunkMeshFilter;
        MeshRenderer chunkMeshRenderer;
        MeshCollider chunkMeshCollider;

        public Chunk(Vector2 chunkPos, Transform parent, Material chunkMaterial, float[,] heightMap, float[,] heatMap, float[,] moistureMap)
        {
            this.chunkPos = chunkPos * 128;
            Vector3 chunkPosV3 = new Vector3(chunkPos.x, 0, chunkPos.y);
            chunkBounds = new Bounds(chunkPos, Vector2.one * 64);

            chunkObject = new GameObject("Chunk : x =" + chunkPos.x + ", y = " + chunkPos.y);

            chunkMeshFilter = chunkObject.AddComponent<MeshFilter>();
            chunkMeshRenderer = chunkObject.AddComponent<MeshRenderer>();
            chunkMeshCollider = chunkObject.AddComponent<MeshCollider>();

            chunkMeshRenderer.material = chunkMaterial;

            chunkMeshFilter.sharedMesh = chunkData.createChunkMesh();

            chunkHandler.SetMeshFilter(chunkMeshFilter);

            chunkData = new ChunkData(heightMap, heatMap, moistureMap);
            chunkData.FlatShadeChunk();
        }

        public void UpdateThisChunk()
        {
            float viewerDstFromNearestEdge = Mathf.Sqrt(chunkBounds.SqrDistance(viewerPos));
            bool visible = viewerDstFromNearestEdge <= viewDistance;

            if (visible)
            {
                if (collisionMesh.hasMesh)
                {
                    chunkMeshCollider.sharedMesh = collisionMesh.mesh;
                }
                else if (!collisionMesh.hasRequestedMesh)
                {
                    // collisionMesh.RequestMesh(mapData);
                }

                chunksVisibleLastUpdate.Add(this);
            }

            SetVisible(visible);
        }

        public void SetVisible(bool visible)
        {
            chunkObject.SetActive(visible);
        }

        public bool isVisible()
        {
            return chunkObject.activeSelf;
        }
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------

    public class ChunkData
    {
        Vector3[] vertices;
        int[] triangles;
        Vector2[] uvs;
        float[] heightMultipliers;

        int[] vertexIndex;
        int triangleIndex;

        float[,] heightMap;
        float[,] heatMap;
        float[,] moistureMap;

        public ChunkData(float[,] heightMap, float[,] heatMap, float[,] moistureMap)
        {
            vertices = new Vector3[128 * 128];
            uvs = new Vector2[128 * 128];
            triangles = new int[127 * 127 * 6];
            heightMultipliers = new float[128 * 128];

            this.heightMap = heightMap;
            this.heatMap = heatMap;
            this.moistureMap = moistureMap;
        }

        public void AddVertex(Vector3 vertexPosition, int vertexIndex)
        {
            vertices[vertexIndex] = vertexPosition;
            //uvs[vertexIndex] = uv;
        }

        public void AddTriangle(int a, int b, int c)
        {
            triangles[triangleIndex] = a;
            triangles[triangleIndex + 1] = b;
            triangles[triangleIndex + 2] = c;
            triangleIndex += 3;
        }

        public void FlatShadeChunk()
        {
            Vector3[] flatShadedVertices = new Vector3[triangles.Length];
            Vector2[] flatShadedUVs = new Vector2[triangles.Length];

            for (int i = 0; i < triangles.Length; i++)
            {
                flatShadedVertices[i] = vertices[triangles[i]];
                flatShadedUVs[i] = uvs[triangles[i]];
                triangles[i] = i;
            }

            vertices = flatShadedVertices;
            uvs = flatShadedUVs;
        }

        public Mesh createChunkMesh()
        {
            Mesh chunkMesh = new Mesh();

            for (int x = 0; x < 128; x++)
            {
                for (int z = 0; z < 128; z++)
                {
                    AddVertex(new Vector3(x, 0, z), x + z);
                    AddTriangle(x + z, x + z + 1, x + z + 2);

                }
            }

            chunkMesh.vertices = vertices;
            chunkMesh.triangles = triangles;

            chunkMesh.RecalculateNormals();

            // chunkMesh = ChunkGenerator.EditChunkMesh(vertices, heightMap, heightMultipliers);

            return chunkMesh;
        }
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------

    public class ChunkGenerator
    {
        Chunk chunk;
        //MapGenerator mapGenerator;

        float[,] heightMap;
        float[,] heatMap;
        float[,] moistureMap;
        Color[] colorMap;

        Material chunkMaterial; //Basicaly the colormap but with different struct.

        public Chunk GenerateChunk(Vector2 chunkPosition, Transform parent)
        {
            heatMap = MapGenerator.GenerateHeatMap();
            moistureMap = MapGenerator.GenerateMoistureMap();

            chunk = new Chunk(chunkPosition, parent, chunkMaterial, heightMap, heatMap, moistureMap);
            return chunk;
        }

        public static Mesh EditChunkMesh(Vector3[] vertices, float[,] heightMap, float[] heightMultipliers)
        {
            return new Mesh();
        }

    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------

    class CollisionMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;

        Action updateCallback;

        public CollisionMesh(Action updateCallback)
        {
            this.updateCallback = updateCallback;
        }

        void OnMeshDataRecieved(MeshData meshData)
        {
            mesh = meshData.CreateMesh();
            hasMesh = true;

            updateCallback();
        }

        public void RequestMesh(MapData mapData)
        {
            hasRequestedMesh = true;
            //worldMapGen.RequestMeshData(mapData, OnMeshDataRecieved);
        }
    }
}



*/