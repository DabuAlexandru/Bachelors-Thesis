using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using static Unity.Mathematics.math;

using Vertex = Utils.Vertex;

public static class TerrainMeshGenerator
{
    public static ProceduralPlaneMesh GenerateTerrainMesh(float[,] heightMap, int levelOfDetail = 0, float meshHeightMultiplier = 1.0f) {
        int resolution = heightMap.GetLength(0) - 1;

        int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
		int modifiedResolution = resolution / meshSimplificationIncrement;
        float[,] rescaledHeightMap = new float[modifiedResolution + 1, modifiedResolution + 1];
        
        for(int u = 0; u <= modifiedResolution; u++)
        {
            for(int v = 0; v <= modifiedResolution; v++)
            {
                rescaledHeightMap[u, v] = heightMap[u * meshSimplificationIncrement, v * meshSimplificationIncrement];
            }
        }
        ProceduralPlaneMesh procPlaneMesh = new ProceduralPlaneMesh(modifiedResolution);
        procPlaneMesh.ApplyHeightMap(rescaledHeightMap, meshHeightMultiplier);
        return procPlaneMesh;
    }

    public static Vector3[,] GetNormalsFromHeightMap(float[,] heightMap, int chunkResolution, float meshHeightMultiplier = 1.0f)
    {
        int resolution = heightMap.GetLength(0) - 1;
        Vector3[,] vertexNormals = new Vector3[resolution + 1, resolution + 1];

        for(int v = 0; v < resolution; v++)
        {
            for(int u = 0; u < resolution; u++)
            {
                float x1 = (float)u / chunkResolution - 0.5f;
                float x2 = (float)(u + 1) / chunkResolution - 0.5f;
                float z1 = (float)v / chunkResolution - 0.5f;
                float z2 = (float)(v + 1) / chunkResolution - 0.5f;

                Vector3 vertexA = new Vector3(x1, heightMap[u, v] * meshHeightMultiplier, z1);
                Vector3 vertexB = new Vector3(x2, heightMap[u + 1, v] * meshHeightMultiplier, z1);
                Vector3 vertexC = new Vector3(x2, heightMap[u + 1, v + 1] * meshHeightMultiplier, z2);
                Vector3 vertexD = new Vector3(x1, heightMap[u, v + 1] * meshHeightMultiplier, z2);

                Vector3 triangleACBNormal = CalculateSurfaceNormal(vertexA, vertexC, vertexB);
                Vector3 triangleADCNormal = CalculateSurfaceNormal(vertexA, vertexD, vertexC);

                vertexNormals[u, v] += triangleACBNormal + triangleADCNormal; // A
                vertexNormals[u + 1, v] += triangleACBNormal; // B
                vertexNormals[u + 1, v + 1] += triangleACBNormal + triangleADCNormal; // C
                vertexNormals[u, v + 1] += triangleADCNormal; // D
            }
        }
        for(int v = 0; v <= resolution; v++)
        {
            for(int u = 0; u <= resolution; u++)
            {
                vertexNormals[u, v].Normalize();
            }
        }

        return vertexNormals;
    }

    private static Vector3 CalculateSurfaceNormal(Vector3 vertex1, Vector3 vertex2, Vector3 vertex3)
    {
        Vector3 edge12 = vertex2 - vertex1;
        Vector3 edge13 = vertex3 - vertex1;
        return Vector3.Cross(edge12, edge13).normalized;
    }

    public static Vector2[] GetTreesOnHeightMap(float[,] heightMap, DistributionParams distributionParams)
        => GetTreesOnHeightMap(heightMap, distributionParams.MinTerrainHeight, distributionParams.MaxTerrainHeight, 
            distributionParams.MaxElevationDifference, distributionParams.WindowSize, distributionParams.Variance, distributionParams.Randomize);

    public static Vector2[] GetTreesOnHeightMap(float[,] heightMap, float minTerrainHeight, float maxTerrainHeight, float maxElevationDifference, int windowSize = 1, int variance = 2, bool applyRandom = false)
    {
        int mapWidth = heightMap.GetLength(0), mapHeight = heightMap.GetLength(1);
 
        int treeIndex = 0;
        int dirU = 1, dirV = 1; // the direction of the applied offset

        int maxU = mapWidth - windowSize - variance;
        int maxV = mapHeight - windowSize - variance;

        int numPointsU = (maxU - windowSize - 1) / (2 * windowSize) + 1;
        int numPointsV = (maxV - windowSize - 1) / (2 * windowSize + Mathf.Min(1, variance)) + 1;

        int incremU = 2 * windowSize;
        int incremV = 2 * windowSize + Mathf.Min(1, variance);

        int emptySpaceU = mapWidth - (numPointsU * incremU + variance);
        int emptySpaceV = mapHeight - (numPointsV * incremV + variance);

        Vector2[] treePositions = new Vector2[numPointsU * numPointsV];

        int offsetU = 0;
        for(int v = windowSize; v < maxV; v += incremV)
        {
            int offsetV = 0;
            for(int u = windowSize; u < maxU; u += incremU)
            {
                int treeU = u + offsetU + emptySpaceU / 2, treeV = v + offsetV + emptySpaceV / 2;
                if(applyRandom)
                {
                    treeU += (int)Random.Range(-(windowSize - 1), (windowSize - 1));
                    treeV += (int)Random.Range(-(windowSize - 1), (windowSize - 1));
                }

                float minHeight = heightMap[treeU, treeV], maxHeight = heightMap[treeU, treeV];

                if(heightMap[treeU, treeV] >= minTerrainHeight && heightMap[treeU, treeV] <= maxTerrainHeight)
                {
                    for(int k = -1; k <= 1; k++)
                    {
                        for(int l = -1; l <= 1; l++)
                        {
                            minHeight = Mathf.Min(heightMap[treeU + l, treeV + k], minHeight);
                            maxHeight = Mathf.Max(heightMap[treeU + l, treeV + k], maxHeight);
                        }
                    }

                    if(maxHeight - minHeight <= maxElevationDifference) 
                    {
                        treePositions[treeIndex] = new Vector2(treeU, treeV);
                        treeIndex++;
                    }
                }
                
                if(variance > 0)
                {
                    offsetV += dirV;
                    if(offsetV < 0 || offsetV > variance)
                    {
                        dirV *= -1;
                        offsetV += 2 * dirV;
                    }
                }
            }
            if(variance > 0)
            {
                offsetU += dirU;
                if(offsetU < 0 || offsetU > variance)
                {
                    dirU *= -1;
                    offsetU += 2 * dirU;
                }
            }
        }

        Vector2[] resizedTreePositions = new Vector2[treeIndex];
        for(int i = 0; i < treeIndex; i++)
        {
            resizedTreePositions[i] = treePositions[i];
        }
        return resizedTreePositions;
    }
}

public class ProceduralPlaneMesh 
{
    private Mesh planeMesh;
    private int resolution;
    private int vertexCount;
    private int indexCount;

    private MeshFilter meshFilter;

    // specifications of the mesh
    private Bounds bounds = new Bounds(Vector3.zero, new Vector3(1.0f, 0.0f, 1.0f));
        
    private const int vertexAttributeCount = 4; // four attributes: a position, a normal, a tangent, and a set of texture coordinates

    public ProceduralPlaneMesh(int resolution)
    {
        this.resolution = resolution;
        vertexCount = (resolution + 1) * (resolution + 1);
        indexCount = 6 * resolution * resolution;

        InitializeProceduralMesh();
    }

    private void InitializeProceduralMesh()
    {
        Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
        Mesh.MeshData meshData = meshDataArray[0];

        var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(
            vertexAttributeCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory
        );
        vertexAttributes[0] = new VertexAttributeDescriptor(dimension: 3);
        vertexAttributes[1] = new VertexAttributeDescriptor(
            VertexAttribute.Normal, dimension: 3
        );
        vertexAttributes[2] = new VertexAttributeDescriptor(
            VertexAttribute.Tangent, VertexAttributeFormat.Float16, 4
        );
        vertexAttributes[3] = new VertexAttributeDescriptor(
            VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2
        );
        meshData.SetVertexBufferParams(vertexCount, vertexAttributes);
        vertexAttributes.Dispose();
        NativeArray<Vertex> vertices = meshData.GetVertexData<Vertex>();

        meshData.SetIndexBufferParams(indexCount, IndexFormat.UInt16);
        NativeArray<ushort> triangleIndices = meshData.GetIndexData<ushort>();

        var vertex = new Vertex();
        int vi = 0, ti = 0;
        for (int v = 0; v <= resolution; v++)
        {
            vertex.position.z = (float)v / resolution - 0.5f;
            vertex.texCoord0.y = half((float)v / resolution);

            vertex.normal.y = 1.0f;
            vertex.tangent.xw = half2(half(1.0f), half(-1.0f));

            for (int u = 0; u <= resolution; u++)
            {
                vertex.position.x = (float)u / resolution - 0.5f;
                vertex.texCoord0.x = half((float)u / resolution);

                vertices[vi] = vertex;
                vi++;

                if (v < resolution && u < resolution)
                {
                    int currentIndex = v * (resolution + 1) + u;
                    int rightIndex = v * (resolution + 1) + (u + 1);
                    int topRightIndex = (v + 1) * (resolution + 1) + (u + 1);
                    int topIndex = (v + 1) * (resolution + 1) + u;

                    triangleIndices[ti] = (ushort)currentIndex;
                    triangleIndices[ti + 1] = (ushort)topRightIndex;
                    triangleIndices[ti + 2] = (ushort)rightIndex;
                    ti += 3;

                    triangleIndices[ti] = (ushort)currentIndex;
                    triangleIndices[ti + 1] = (ushort)topIndex;
                    triangleIndices[ti + 2] = (ushort)topRightIndex;
                    ti += 3;
                }
            }
        }

        meshData.subMeshCount = 1;
        meshData.SetSubMesh(0, new SubMeshDescriptor(0, indexCount)
        {
            bounds = bounds,
            vertexCount = vertexCount
        }, MeshUpdateFlags.DontRecalculateBounds);

        this.planeMesh = new Mesh
        {
            bounds = bounds,
            name = "Procedural Mesh"
        };

        vertices.Dispose();
        triangleIndices.Dispose();

        Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, planeMesh);
    }

    public void SetNormals(Vector3[] normals) => this.planeMesh.normals = normals;

    public Mesh GetMesh() => this.planeMesh;

    // public void ApplyHeightMapLOD(float[])

    public void ApplyHeightMap(float[,] heightMap, float amplitude = 1.0f)
    {
        Vector3[] myVertices = planeMesh.vertices;
        int vi = 0;
        for(int v = 0; v <= resolution; v++)
        {
            for(int u = 0; u <= resolution; u++)
            {
                myVertices[vi].y = amplitude * heightMap[u,v];
                vi++;
            }
        }

        planeMesh.vertices = myVertices;
        planeMesh.RecalculateNormals();
        planeMesh.RecalculateBounds();
    }

    public float[,] ExtractHeightMap()
    {
        Vector3[] myVertices = planeMesh.vertices;
        float[,] heightMap = new float[resolution + 1, resolution + 1];
        int vi = 0;
        for(int v = 0; v <= resolution; v++)
        {
            for(int u = 0; u <= resolution; u++)
            {
                heightMap[u, v] = myVertices[vi].y;
                vi++;
            }
        }
        return Noise.GetNormalizedHeightMap(heightMap);
    }
}
