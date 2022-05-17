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
}
