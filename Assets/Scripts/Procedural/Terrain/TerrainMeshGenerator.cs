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
