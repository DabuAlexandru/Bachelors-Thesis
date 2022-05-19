using UnityEngine;

public static class Constants
{
    // puzzle constants
    public const int puzzleResolutionU = 30;
    public const int puzzleResolutionV = 30;
    public const string puzzleDataFile = "puzzle";

    // terrain constants
    public const int terrainResolutionU = 240;
    public const int terrainResolutionV = 240;

    public static float Frac(float x) => Mathf.Abs(x % 1);

    public static Vector2 Vector2Modulo(Vector2 vec, int exp) => new Vector2(vec.x % exp, vec.y % exp);

    public static int Modulo(int x, int m)
    {
        m = Mathf.Max(1, m);
        return (m + x % m) % m;
    }

    public enum NoiseFunction {Perlin, DiamondSquare, Voronoi, Combined};
}
