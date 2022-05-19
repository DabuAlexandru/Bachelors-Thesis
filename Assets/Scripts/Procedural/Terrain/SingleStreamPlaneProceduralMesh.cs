using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using static Unity.Mathematics.math;

using Vertex = Utils.Vertex;

public class SingleStreamPlaneProceduralMesh
{
    const int resolutionU = Constants.terrainResolutionU;
    const int resolutionV = Constants.terrainResolutionV;
    private MeshFilter meshFilter;

    // specifications of the mesh
    private Bounds bounds = new Bounds(Vector3.zero, new Vector3(1.0f, 0.0f, 1.0f));
    private const int vertexCount = (resolutionU + 1) * (resolutionV + 1);
    private const int indexCount = 6 * resolutionU * resolutionV;
    private const int vertexAttributeCount = 4; // four attributes: a position, a normal, a tangent, and a set of texture coordinates

    public SingleStreamPlaneProceduralMesh(MeshFilter meshFilter)
    {
        InitializeProceduralMesh(meshFilter);
        this.meshFilter = meshFilter;
    }

    void InitializeProceduralMesh(MeshFilter meshFilter)
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
        for (int v = 0; v <= resolutionV; v++)
        {
            vertex.position.z = (float)v / resolutionU - 0.5f;
            vertex.texCoord0.y = half((float)v / resolutionV);

            vertex.normal.y = 1.0f;
            vertex.tangent.xw = half2(half(1.0f), half(-1.0f));

            for (int u = 0; u <= resolutionU; u++)
            {
                vertex.position.x = (float)u / resolutionU - 0.5f;
                vertex.texCoord0.x = half((float)u / resolutionU);

                vertices[vi] = vertex;
                vi++;

                if (v < resolutionV && u < resolutionU)
                {
                    int currentIndex = v * (resolutionU + 1) + u;
                    int rightIndex = v * (resolutionU + 1) + (u + 1);
                    int topRightIndex = (v + 1) * (resolutionU + 1) + (u + 1);
                    int topIndex = (v + 1) * (resolutionU + 1) + u;

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

        var mesh = new Mesh
        {
            bounds = bounds,
            name = "Procedural Mesh"
        };

        vertices.Dispose();
        triangleIndices.Dispose();

        Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
        meshFilter.mesh = mesh;
    }

    public MeshFilter GetMeshFilter()
    {
        return meshFilter;
    }
}
