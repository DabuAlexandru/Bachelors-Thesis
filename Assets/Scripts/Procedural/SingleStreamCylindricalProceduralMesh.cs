using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using static Unity.Mathematics.math;

using Vertex = Utils.Vertex;

public class SingleStreamCylindricalProceduralMesh
{
    const int resolutionU = Constants.puzzleResolutionU;
    const int resolutionV = Constants.puzzleResolutionV;
    private MeshFilter meshFilter;

    // dimensions of the generated shape
    private const float radius = 0.5f;
    private const float height = 2.0f;
    // specifications of the mesh
    private Bounds bounds = new Bounds(Vector3.zero, new Vector3(radius * 2f, height, radius * 2f));
    private const int vertexCount = (resolutionU + 1) * (resolutionV + 1);
    private const int indexCount = 6 * resolutionU * resolutionV;
    private const int vertexAttributeCount = 4; // four attributes: a position, a normal, a tangent, and a set of texture coordinates

    public SingleStreamCylindricalProceduralMesh(MeshFilter meshFilter)
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
            for (int u = 0; u <= resolutionU; u++)
            {
                vertex.position.x = radius * cos(2 * PI * u / resolutionU);
                vertex.position.z = radius * sin(2 * PI * u / resolutionU);

                vertex.normal.xz = vertex.position.xz;
                vertex.normal.y = 0.0f;

                vertex.tangent.x = half(-vertex.position.z);
                vertex.tangent.y = half(0.0f);
                vertex.tangent.z = half(vertex.position.x);

                vertex.position.y = height * ((float)v / resolutionV) - height / 2;
                vertex.texCoord0.x = half((float)u / resolutionU);
                vertex.texCoord0.y = half((float)v / resolutionV);

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