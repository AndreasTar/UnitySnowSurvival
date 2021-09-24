using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGen
{
    public static MeshData GenerateMesh(float[,] heightMap, float heightMultiplier, AnimationCurve _heightCurve)
    {
        AnimationCurve heightCurve = new AnimationCurve(_heightCurve.keys);

        int borderedSize = heightMap.GetLength(0);
        int meshSize = borderedSize;
        int meshSizeUnsimplified = borderedSize - 2;

        float topLeftX = (meshSizeUnsimplified - 1) / -2f;
        float topLeftZ = (meshSizeUnsimplified - 1) / 2f;

        int verticesPerLine = (borderedSize - 1);

        MeshData meshData = new MeshData(verticesPerLine);
        int[,] vertexIndicesMap = new int[borderedSize, borderedSize];
        int meshVertexIndex = 0;
        int borderVertexIndex = -1;

        for (int y = 0; y < borderedSize; y++)
        {
            for (int x = 0; x < borderedSize; x++)
            {
                bool isBorderVertex = (y == 0 || y == borderedSize - 1 || x == 0 || x == borderedSize - 1);

                if (isBorderVertex)
                {
                    vertexIndicesMap[x, y] = borderVertexIndex;
                    borderVertexIndex--;
                }
                else
                {
                    vertexIndicesMap[x, y] = meshVertexIndex;
                    meshVertexIndex++;
                }
            }
        }

        for (int y = 0; y < borderedSize; y++)
        {
            for (int x = 0; x < borderedSize; x++)
            {
                int vertexIndex = vertexIndicesMap[x, y];

                Vector2 percent = new Vector2((x - 1) / (float)meshSize, (y - 1) / (float)meshSize);
                float height = heightCurve.Evaluate(heightMap[x, y]) * heightMultiplier;
                Vector3 vertexPosition = new Vector3(topLeftX + percent.x * meshSizeUnsimplified, height, topLeftZ - percent.y * meshSizeUnsimplified);

                meshData.AddVertex(vertexPosition, percent, vertexIndex);

                if (x < borderedSize - 1 && y < borderedSize - 1)
                {
                    int a = vertexIndicesMap[x, y];
                    int b = vertexIndicesMap[x + 1, y];
                    int c = vertexIndicesMap[x, y + 1];
                    int d = vertexIndicesMap[x + 1, y + 1];

                    meshData.AddTriangle(a, d, c);
                    meshData.AddTriangle(d, a, b);
                }
            }
        }
        meshData.FlatShading();

        return meshData;
    }
}
