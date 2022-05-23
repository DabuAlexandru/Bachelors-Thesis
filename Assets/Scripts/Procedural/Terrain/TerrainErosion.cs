using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TerrainErosion
{
    public static float[,] ThermalErosion(float[,] heightMap, float coef, float talus)
    {
        int width = heightMap.GetLength(0), height = heightMap.GetLength(1);
        float[,] thermalEroded = new float[width, height];

        Vector2[] neighborhood = {
            new Vector2(-1, -1),
            new Vector2(-1, 1),
            new Vector2(1, -1),
            new Vector2(1, 1)
        };

        return thermalEroded;
    }
}
