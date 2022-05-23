using UnityEngine;
using Unity.Mathematics;
using System.Runtime.InteropServices;

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
}
