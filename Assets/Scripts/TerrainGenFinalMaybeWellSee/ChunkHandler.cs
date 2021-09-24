using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ChunkHandler : MonoBehaviour
{
    [Header("Player")]
    public Transform player;                // the transform of the player object
    public static Vector2 playerPos;        // the position of the player on x-z axis
    Vector2 playerPosOld;                   // the position of the player in last update
    public static float viewDistance = 64*3; // view distance from player

    [Header("Perlin Noise Values")]
    public int seed; //101
    [Range(1, 250)]
    public float scale; //24
    [Range(0, 50)]
    public int octaves; //4
    [Range(0, 1)]
    public float persistance; //.8f
    [Range(1, 50)]
    public float lacunarity; //1
    public Vector2 offset; //10,10

    Structs.NoiseData noiseData;

    const float sqrViewerMoveThreshForUpdate = 20f * 20f; // amount of movement of player required for chunks to update 

    int chunksVisibleNum; // number of chunks that should be visible in each axis around the player in a circle
    GameObject chunksParent;

    Dictionary<Vector2, Chunk> chunkDictionary = new Dictionary<Vector2, Chunk>();  // all generated chunks
    static List<Chunk> chunksVisibleLastUpdate = new List<Chunk>();                 // chunks that were active last update

    private void Start()
    {
        chunksVisibleNum = Mathf.RoundToInt(viewDistance / 64); //  2,34 but gets rounded to 2
        chunksParent = new GameObject("--CHUNKS PARENT--");
        chunksParent.transform.position = Vector3.up;

        noiseData = new Structs.NoiseData(seed, scale, octaves, persistance, lacunarity, offset);
    }

    /* 
     * in every frame update, it takes the position of the player in x-z axis
     * and determines if the player has moved enough for it to update.
     * If he has, calls HandleVisibleChunks();
     */
    private void FixedUpdate()
    {
        playerPos = new Vector2(player.position.x, player.position.z);

        if ((playerPosOld - playerPos).sqrMagnitude > sqrViewerMoveThreshForUpdate)
        {
            playerPosOld = playerPos;
            HandleVisibleChunks();
        }
    }

    public void HandleVisibleChunks()
    {
        for (int i = 0; i < chunksVisibleLastUpdate.Count; i++)
        {
            chunksVisibleLastUpdate[i].SetActive(false);
        }
        chunksVisibleLastUpdate.Clear();

        int chunkCoordX = Mathf.RoundToInt(playerPos.x / 126);
        int chunkCoordY = Mathf.RoundToInt(playerPos.y / 126);
        
        for (int yOffset = -chunksVisibleNum; yOffset <= chunksVisibleNum; yOffset++)
        {
            for (int xOffset = -chunksVisibleNum; xOffset <= chunksVisibleNum; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(chunkCoordX + xOffset, chunkCoordY + yOffset);
                //Vector2 chunkCoords = new Vector2((playerPos.x / 126) + xOffset, (playerPos.y / 126) + yOffset);

                if (chunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    chunkDictionary[viewedChunkCoord].UpdateThisChunk();
                }
                else
                {
                    Chunk generatedChunk = GenerateChunk(viewedChunkCoord, chunksParent.transform, noiseData);
                    chunkDictionary.Add(viewedChunkCoord, generatedChunk);
                }

            }
        }
    }

    public bool chunkExists(Vector2 coords)
    {
        return chunkDictionary.ContainsKey(coords);
    }

    public float[,] getChunkNoise(Vector2 coords)
    {
        if (chunkExists(coords))
        {
            return chunkDictionary[coords].getNoiseMap();
        }
        return null;
    }

    public Chunk GenerateChunk(Vector2 chunkCoord, Transform parent, Structs.NoiseData noiseData)
    {
        return new Chunk(chunkCoord, parent, this, noiseData);
    }

    public void AddToChunkVisLastUpdateList(Chunk chunk)
    {
        chunksVisibleLastUpdate.Add(chunk);
    }
    
    public float GetViewDist()
    {
        return viewDistance;
    }

    public Vector2 GetPlayerPos()
    {
        return playerPos;
    }

    /*private void OnDrawGizmos()
    {
        if (chunkDictionary.Count == 0) return;

        int chunkCoordX = Mathf.RoundToInt(playerPos.x / 128);
        int chunkCoordY = Mathf.RoundToInt(playerPos.y / 128);
        for (int yOffset = -chunksVisibleNum; yOffset <= chunksVisibleNum; yOffset++)
        {
            for (int xOffset = -chunksVisibleNum; xOffset <= chunksVisibleNum; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(chunkCoordX + xOffset, chunkCoordY + yOffset);

                if (chunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    Gizmos.DrawSphere(new Vector3(viewedChunkCoord.x*32,0,viewedChunkCoord.y*32), 2f);
                }
            }
        }
    }*/
}

public class Chunk
{

    GameObject chunkObject; //the chunk GameObject
    Bounds chunkBounds;     //the bounds of the chunk !!DONT KNOW HOW IT WORKS EXACTLY OR WHAT IT DOES!!

    ChunkHandler chunkHandler; // reference to ChunkHandler class
    MapGenerator mapGenerator; // reference to MapGenerator class

    float[,] noiseMap; // the perlin noise map of this chunk

    MeshFilter meshFilter;      //the MeshFilter of this chunk
    MeshRenderer meshRenderer;  //the MeshRenderer of this chunk
    MeshCollider meshCollider;

    static int csize = 64; // is used instead of always writing 64
    int xW = csize; // xWidth, always 64
    int zD = csize; // zDepth, always 64
    Vector2[] uvs; //  the uvs of the chunk
    Vector3 chunkRelativePos; // the position of the chunk relative to its parent->(TerrainChunkGen), see its declaration for details

    Vector3[] vertices; //the vertices of this chunk. It gets populated with 64*64 !!!!
    int[] triangles;    //the triangles of this chunk. It gets populated at 63*63*6

    float offset;
    Mesh mesh;


    public Chunk(Vector2 chunkCoordRounded, Transform parent, ChunkHandler chunkHandler, Structs.NoiseData noiseData)
    {
        this.chunkHandler = chunkHandler;
        mesh = new Mesh(); //creates new mesh empty for the object
        offset = 10f;
        Vector2 offset2D = new Vector2(offset, offset);
        //noiseMap = MapGenerator.GeneratePerlinNoiseMap(csize, 101, 24, 4, .8f, 1, chunkCoordRounded * csize + noiseData.offset);
        noiseMap = MapGenerator.GeneratePerlinNoiseMap(csize, noiseData.seed, noiseData.scale, noiseData.octaves, noiseData.persistance, noiseData.lacunarity, chunkCoordRounded * csize + noiseData.offset);
        CreateChunkMesh(chunkCoordRounded, noiseData.offset);
        
        //UpdateMesh();
        //RecalculateUVS();
        FlatShadeMesh();

        Vector3 boundCenter = new Vector3(chunkCoordRounded.x * 126, 0, chunkCoordRounded.y * 126);
        chunkBounds = new Bounds(boundCenter, Vector3.one * csize);
        
        HandleComponents(chunkCoordRounded);
        chunkRelativePos = new Vector3(chunkCoordRounded.x * 126, 1, chunkCoordRounded.y * 126);
        chunkObject.transform.position = chunkRelativePos;                                          //sets its position to be relative to its parent->(TerrainChunkGen), see above line for details
        chunkObject.transform.rotation = parent.rotation;                                           //sets its rotation to be relative to its parent->(TerrainChunkGen)
        chunkObject.transform.SetParent(parent);                                                    //sets parent to be said object
        chunkObject.layer = 9;                                                                      //sets the layer of the chunk to be 9 aka "Ground"

        SetActive(false);
        UpdateThisChunk();
    }

    void HandleComponents(Vector2 chunkCoordRounded)
    {
        chunkObject = new GameObject("Chunk X : " + chunkCoordRounded.x + " Z : " + chunkCoordRounded.y);   //creates new GameObject with name its coords
        chunkObject.AddComponent<MeshFilter>();                                                             //adds MeshFilter to that object
        meshFilter = chunkObject.GetComponent<MeshFilter>();                                                //sets objects meshFilter to be that MeshFilter
        chunkObject.AddComponent<MeshRenderer>();                                                           //adds MeshRenderer to that object
        meshRenderer = chunkObject.GetComponent<MeshRenderer>();                                            //sets objects meshRenderer to be that MeshRenderer
        chunkObject.AddComponent<MeshCollider>();                                                           //adds MeshCollider to that object
        meshCollider = chunkObject.GetComponent<MeshCollider>();                                            //sets objects meshCollider to be that MeshCollider
        meshFilter.sharedMesh = mesh;                                                                       //sets the mesh of the MeshFilter to the assigned mesh
        meshCollider.sharedMesh = mesh;                                                                     //tells the MeshCollider to actually make colliders for the mesh
        meshRenderer.material.mainTexture = TextureGenerator.TextureFromHeightMap(noiseMap);                //sets the texture of the object to be its perlin noiseMap --CHANGE--
        meshRenderer.material.SetTextureOffset("_MainTex", new Vector2(.5f, .5f));                          //offsets the texture by .5 on both axis, because for some reason its not on center
        meshRenderer.material.shader = Shader.Find("Unlit/Texture");                                        //sets the shader rendering to be Unlit/Texture and not shaded texture
    }

    void CreateChunkMesh(Vector2 centerChunkCoords, Vector2 offset2D)
    {
        int vertCounter = 0;
        int triangCounter = 0;
        vertices = new Vector3[xW * zD]; // 4096 = 64x64
        triangles = new int[(xW-1) * (zD-1) * 6]; // 23814 = 63x63x6

        /* chunk mesh gets generated from bottom-left to top-right, going 1 step right for full cycle up
         * so the order is basically :    
         * 
         * 4  9  14  19  24
         * 3  8  13  18  23
         * 2  7  12  17  22
         * 1  6  11  16  21
         * 0  5  10  15  20
         * 
         */

        float verticeHeight;
        float neighborHeight1;
        float neighborHeight2;
        float neighborHeight3;

        // `x TODO: Refactor all this. because now maps that already have been generated before are re-generated. Maybe save the maps somewhere?
        // Did that. Now it checks the chunkDictionary for the chunk and if it does exist, returns the chunks noiseMap

        Vector2 tempCoordsB = new Vector2(centerChunkCoords.x, centerChunkCoords.y - 1);
        Vector2 tempCoordsU = new Vector2(centerChunkCoords.x, centerChunkCoords.y + 1);
        Vector2 tempCoordsL = new Vector2(centerChunkCoords.x - 1, centerChunkCoords.y);
        Vector2 tempCoordsR = new Vector2(centerChunkCoords.x + 1, centerChunkCoords.y);
        Vector2 tempCoordsBR = new Vector2(centerChunkCoords.x + 1, centerChunkCoords.y - 1);
        Vector2 tempCoordsBL = new Vector2(centerChunkCoords.x - 1, centerChunkCoords.y - 1);
        Vector2 tempCoordsUR = new Vector2(centerChunkCoords.x + 1, centerChunkCoords.y + 1);
        Vector2 tempCoordsUL = new Vector2(centerChunkCoords.x - 1, centerChunkCoords.y + 1);

        float[,] botMap = (chunkHandler.getChunkNoise(tempCoordsB) != null) ? chunkHandler.getChunkNoise(tempCoordsB) : 
            MapGenerator.GeneratePerlinNoiseMap(csize, 101, 24, 4, .8f, 1, tempCoordsB * csize + offset2D);
        float[,] topMap = (chunkHandler.getChunkNoise(tempCoordsU) != null) ? chunkHandler.getChunkNoise(tempCoordsU) : 
            MapGenerator.GeneratePerlinNoiseMap(csize, 101, 24, 4, .8f, 1, tempCoordsU * csize + offset2D);
        float[,] leftMap = (chunkHandler.getChunkNoise(tempCoordsL) != null) ? chunkHandler.getChunkNoise(tempCoordsL) : 
            MapGenerator.GeneratePerlinNoiseMap(csize, 101, 24, 4, .8f, 1, tempCoordsL * csize + offset2D);
        float[,] rightMap = (chunkHandler.getChunkNoise(tempCoordsR) != null) ? chunkHandler.getChunkNoise(tempCoordsR) : 
            MapGenerator.GeneratePerlinNoiseMap(csize, 101, 24, 4, .8f, 1, tempCoordsR * csize + offset2D);
        float[,] botrightMap = (chunkHandler.getChunkNoise(tempCoordsBR) != null) ? chunkHandler.getChunkNoise(tempCoordsBR) : 
            MapGenerator.GeneratePerlinNoiseMap(csize, 101, 24, 4, .8f, 1, tempCoordsBR * csize + offset2D);
        float[,] botleftMap = (chunkHandler.getChunkNoise(tempCoordsBL) != null) ? chunkHandler.getChunkNoise(tempCoordsBL) : 
            MapGenerator.GeneratePerlinNoiseMap(csize, 101, 24, 4, .8f, 1, tempCoordsBL * csize + offset2D);
        float[,] toprightMap = (chunkHandler.getChunkNoise(tempCoordsUR) != null) ? chunkHandler.getChunkNoise(tempCoordsUR) : 
            MapGenerator.GeneratePerlinNoiseMap(csize, 101, 24, 4, .8f, 1, tempCoordsUR * csize + offset2D);
        float[,] topleftMap = (chunkHandler.getChunkNoise(tempCoordsUL) != null) ? chunkHandler.getChunkNoise(tempCoordsUL) : 
            MapGenerator.GeneratePerlinNoiseMap(csize, 101, 24, 4, .8f, 1, tempCoordsUL * csize + offset2D);

        for (int x = -(xW / 2); x < xW / 2; x++)
        {
            for (int z = -(zD / 2); z < zD / 2; z++)
            {
                verticeHeight = noiseMap[x + (xW / 2), z + (zD / 2)] * 80;

                // 'case' arguments HAVE to be hardcoded(basically static), they cant be references to objects/variables.
                // must handle corners first so the 'switch' doesnt go into a 'case' scenario with '_' in any position

                switch((x, z))
                {
                    case (31, -32): // bot-right corner
                        neighborHeight1 = botMap[xW - 1, zD - 1] * 80;
                        neighborHeight2 = rightMap[0, 0] * 80;
                        neighborHeight3 = botrightMap[0, zD - 1] * 80;
                        verticeHeight = (neighborHeight1 + neighborHeight2 + neighborHeight3 + verticeHeight) / 4;
                        break;
                    case (-32, -32): // bot-left corner
                        neighborHeight1 = botMap[0, zD - 1] * 80;
                        neighborHeight2 = leftMap[xW - 1, 0] * 80;
                        neighborHeight3 = botleftMap[xW - 1, zD - 1] * 80;
                        verticeHeight = (neighborHeight1 + neighborHeight2 + neighborHeight3 + verticeHeight) / 4;
                        break;
                    case (31, 31): // top-right corner
                        neighborHeight1 = topMap[xW - 1, 0] * 80;
                        neighborHeight2 = rightMap[0, zD - 1] * 80;
                        neighborHeight3 = toprightMap[0, 0] * 80;
                        verticeHeight = (neighborHeight1 + neighborHeight2 + neighborHeight3 + verticeHeight) / 4;
                        break;
                    case (-32, 31): // top-left corner
                        neighborHeight1 = topMap[0, 0] * 80;
                        neighborHeight2 = leftMap[xW - 1, zD - 1] * 80;
                        neighborHeight3 = topleftMap[xW - 1, 0] * 80;
                        verticeHeight = (neighborHeight1 + neighborHeight2 + neighborHeight3 + verticeHeight) / 4;
                        break;
                    case (_, -32): // bot side
                        neighborHeight1 = botMap[x + (xW / 2), zD - 1] * 80;
                        verticeHeight = (neighborHeight1 + verticeHeight) / 2;
                        break;
                    case (_, 31): // top side
                        neighborHeight1 = topMap[x + (xW / 2), 0] * 80;
                        verticeHeight = (neighborHeight1 + verticeHeight) / 2;
                        break;
                    case (-32, _): // left side
                        neighborHeight1 = leftMap[xW - 1, z + (zD / 2)] * 80;
                        verticeHeight = (neighborHeight1 + verticeHeight) / 2;
                        break;
                    case (31, _): // right side
                        neighborHeight1 = rightMap[0, z + (zD / 2)] * 80;
                        verticeHeight = (neighborHeight1 + verticeHeight) / 2;
                        break;
                    default:
                        break;

                }
                
                // if its 2*x it leaves a 1 unit gap between each chunk so we add 1/64*x. Same for z
                vertices[vertCounter] = new Vector3(2*x + 1/csize*Mathf.Abs(x), verticeHeight, 2*z + 1/csize*Mathf.Abs(z)); 
                vertCounter++;
            }
        }

        vertCounter = 0;

        for (int z = 0; z < zD; z++)
        {
            for (int x = 0; x < xW; x++)
            {
                if (x < xW - 1 && z < zD - 1)
                {
                    triangles[0 + triangCounter] = vertCounter;
                    triangles[1 + triangCounter] = vertCounter + xW + 1;
                    triangles[2 + triangCounter] = vertCounter + xW;
                    triangles[3 + triangCounter] = vertCounter + xW + 1;
                    triangles[4 + triangCounter] = vertCounter;
                    triangles[5 + triangCounter] = vertCounter + 1;
                    vertCounter++;
                    triangCounter += 6;
                }
            }
            vertCounter++;
        }

    }

    void FlatShadeMesh()
    {
        Vector3[] flatShadedVertices = new Vector3[triangles.Length];
        //Vector2[] flatShadedUVs = new Vector2[triangles.Length];

        uvs = new Vector2[triangles.Length];

        for (int i = 0; i < triangles.Length; i++)
        {
            flatShadedVertices[i] = vertices[triangles[i]];

            uvs[i] = new Vector2(flatShadedVertices[i].x * 1 / 126, flatShadedVertices[i].z * 1 / 126); // creates uvs for each vertex, but remaps them to range 0-1 from 0-126
        }

        for (int i = 0; i < triangles.Length; i++)
        {
            triangles[i] = i;
        }

        mesh.Clear();
        mesh.vertices = flatShadedVertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        vertices = flatShadedVertices;

        mesh.normals = CalculateNormals();
    }

    public void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    public void SetActive(bool active)
    {
        chunkObject.SetActive(active);
    }

    public void RecalculateUVS()
    {
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
        } 
        mesh.uv = uvs;
    }

    public void UpdateThisChunk()
    {
        float viewerDstFromNearestEdge = Mathf.Sqrt(chunkBounds.SqrDistance(new Vector3(chunkHandler.GetPlayerPos().x, 0, chunkHandler.GetPlayerPos().y)));
        bool visible = viewerDstFromNearestEdge <= chunkHandler.GetViewDist();

        if (visible)
        {
            chunkHandler.AddToChunkVisLastUpdateList(this);
        }

        SetActive(visible);
    }

    Vector3[] CalculateNormals()
    {
        Vector3[] vertexNormals = new Vector3[vertices.Length];
        int triangleCount = triangles.Length / 3;
        for (int i = 0; i < triangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = triangles[normalTriangleIndex];
            int vertexIndexB = triangles[normalTriangleIndex + 1];
            int vertexIndexC = triangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            vertexNormals[vertexIndexA] += triangleNormal;
            vertexNormals[vertexIndexB] += triangleNormal;
            vertexNormals[vertexIndexC] += triangleNormal;
        }

        for (int i = 0; i < vertexNormals.Length; i++)
        {
            vertexNormals[i].Normalize();
        }

        return vertexNormals;
    }

    Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
    {
        Vector3 pointA = vertices[indexA];
        Vector3 pointB = vertices[indexB];
        Vector3 pointC = vertices[indexC];

        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;
        return Vector3.Cross(sideAB, sideAC);
    }


    public float[,] getNoiseMap()
    {
        return noiseMap;
    }

    private void OnDrawGizmos()
    {
        if (vertices == null) return;
        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.DrawSphere(vertices[i], .1f);
        }
    }
}
