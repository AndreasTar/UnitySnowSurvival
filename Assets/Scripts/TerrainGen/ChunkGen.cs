using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ChunkGen : MonoBehaviour
{
    Chunk_A[,] chunksMap;
    static WorldMapGen worldMapGen;

    public static float maxViewDist = 300;
    const float viewerMoveThresholdForChunkUpdate = 20f;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

    public Transform viewer;
    public Material mapMaterial;
    public static Vector2 viewerPosition;
    Vector2 viewerPositionOld;

    const int chunkSize = 128;
    int chunksVisible;

    Dictionary<Vector2, Chunk_A> chunkDictionary = new Dictionary<Vector2, Chunk_A>();
    static List<Chunk_A> chunksVisibleLastUpdate = new List<Chunk_A>();

    void Start()
    {
        worldMapGen = FindObjectOfType<WorldMapGen>();
        chunksVisible = Mathf.RoundToInt(maxViewDist / chunkSize);

        UpdateVisibleChunks();
    }

    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
        {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }
    }

    void UpdateVisibleChunks()
    {
        for (int i = 0; i < chunksVisibleLastUpdate.Count; i++)
        {
            chunksVisibleLastUpdate[i].SetVisible(false);
        }
        chunksVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y);

        for (int yOffset = -chunksVisible; yOffset <= chunksVisible; yOffset++)
        {
            for (int xOffset = -chunksVisible; xOffset <= chunksVisible; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (chunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    chunkDictionary[viewedChunkCoord].UpdateChunk();
                }
                else
                {
                    chunkDictionary.Add(viewedChunkCoord, new Chunk_A(viewedChunkCoord, transform, mapMaterial));
                }
            }
        }
    }


    public class Chunk_A
    {
        GameObject meshObject;
        Vector2 chunkPosition;
        Bounds bounds;

        MeshFilter meshFilter;
        MeshRenderer meshRenderer;
        MeshCollider meshCollider;

        CollisionMesh_A collisionMesh;

        float[,] heatMap;
        float[,] noiseMap;

        MeshData meshData;
        MapData mapData;

        bool mapDataReceived;
        public bool hasHeatMap;

        public Chunk_A(Vector2 position, Transform parent, Material material)
        {
            chunkPosition = position;
            bounds = new Bounds(chunkPosition, Vector2.one * 128);

            Vector3 chunkPositionV3 = new Vector3(chunkPosition.x, 0, chunkPosition.y);

            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();
            meshRenderer.material = material;

            meshObject.transform.parent = parent;
            meshObject.transform.position = chunkPositionV3;
            meshObject.transform.localScale = Vector3.one;

            SetVisible(false);
        }

        void OnMapDataReceived(MapData mapData)
        {
            this.mapData = mapData;
            mapDataReceived = true;

            Texture2D texture = PRIMAL_TextureGen.TextureFromColorMap(mapData.colorMap, WorldMapGen.worldMapSize, WorldMapGen.worldMapSize);
            meshRenderer.material.mainTexture = texture;

            UpdateChunk();
        }

        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }

        public bool isVisible()
        {
            return meshObject.activeSelf;
        }

        public void UpdateChunk()
        {
            if (mapDataReceived)
            {
                float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                bool visible = viewerDstFromNearestEdge <= maxViewDist;

                if (visible)
                {
                    if (collisionMesh.hasMesh)
                    {
                        meshCollider.sharedMesh = collisionMesh.mesh;
                    }
                    else if (!collisionMesh.hasRequestedMesh)
                    {
                        collisionMesh.RequestMesh(mapData);
                    }

                    chunksVisibleLastUpdate.Add(this);
                }

                SetVisible(visible);
            }
        }
    }

    class CollisionMesh_A
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;

        Action updateCallback;

        public CollisionMesh_A(Action updateCallback)
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
            worldMapGen.RequestMeshData(mapData, OnMeshDataRecieved);
        }
    }
}