using UnityEngine;
using Unity.Mathematics;
using System.Runtime.InteropServices;

public struct MeshStruct
{
    public Mesh mesh;

    public Vector3[] vertices;
    public Vector3[] normals;
    public Vector2[] uvs;
    public int[] triangles;

    public void ExtractInfoFromSelf()
    {
        vertices = mesh.vertices;
        normals = mesh.normals;
        uvs = mesh.uv;
        triangles = mesh.triangles;
    }

    public void ReconfigureMesh()
    {
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;
    }
}

public static class Utils
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        public float3 position, normal;
        public half4 tangent;
        public half2 texCoord0;
    }

    public static Vector3 GetPointOnQuadraticBezierCurve(Vector3 P0, Vector3 P1, Vector3 P2, float t)
    {
        Vector3 x0 = Mathf.Pow((1 - t), 2f) * P0;
        Vector3 x1 = 2 * t * (1 - t) * P1;
        Vector3 x2 = Mathf.Pow(t, 2f) * P2;
        return x0 + x1 + x2;
    }

    // function that calculates the two triangles of a square from a mesh
    public static void CalculateTriangles(int[] triangles, int ti, int u, int v, int resolution)
    {
        int currentIndex = v * (resolution + 1) + u;
        int rightIndex = v * (resolution + 1) + (u + 1);
        int topRightIndex = (v + 1) * (resolution + 1) + (u + 1);
        int topIndex = (v + 1) * (resolution + 1) + u;

        triangles[ti] = currentIndex;
        triangles[ti + 1] = topRightIndex;
        triangles[ti + 2] = rightIndex;
        ti += 3;

        triangles[ti] = currentIndex;
        triangles[ti + 1] = topIndex;
        triangles[ti + 2] = topRightIndex;
        ti += 3;
    }

    public static int[] GetTrianglesFromCircularMesh(int resU, int resV)
    {
        int[] triangles = new int[6 * resU * resV];
        int ti = 0;
        for (int v = 0; v < resV; v++)
        {
            for (int u = 0; u < resU; u++)
            {
                Utils.CalculateTriangles(triangles, ti, u, v, resU);
                ti += 6;
            }
        }
        return triangles;
    }
}
