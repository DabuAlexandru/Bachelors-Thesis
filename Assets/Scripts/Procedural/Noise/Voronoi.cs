using UnityEngine;

public static class Voronoi
{
    private const int numOfFeaturePointsPerCell = 2;
    private const float epsilon = 0.00001f;
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, int cellDensity, float c1, float c2)
    {
        int mapResolution = (int)Mathf.Max(mapWidth, mapHeight); // the resolution of the noiseMap
        int cellSize = (int)Mathf.Max(2.0f, Mathf.Ceil((float)(mapResolution + 1) / cellDensity));
        mapResolution = cellSize * cellDensity - 1;
        float[,] voronoiMap = GenerateVoronoiMap(mapResolution, seed, cellDensity, c1, c2);

        return Noise.GetNormalizedHeightMap(mapWidth, mapHeight, voronoiMap);
    }

    private static float[,] GenerateVoronoiMap(int resolution, int seed, int cellDensity, float c1, float c2)
    {
        int cellSize = (int)((resolution + 1) / cellDensity);
        float[,] voronoiMap = new float[resolution + 1, resolution + 1];
        Vector2[] featurePoints = new Vector2[cellDensity * cellDensity * numOfFeaturePointsPerCell];
        Random.InitState(seed);
        // Generate the feature points
        int index = 0;
        for (int i = 0; i < cellDensity; i++)
        {
            for (int j = 0; j < cellDensity; j++)
            {
                for (int k = 0; k < numOfFeaturePointsPerCell; k++)
                {
                    // we get a random feature point inside the current cell, but with an offset of epsilon, to not have it on the edge
                    float fPointx = Random.Range(i * cellSize, (i + 1) * cellSize - epsilon);
                    float fPointy = Random.Range(j * cellSize, (j + 1) * cellSize - epsilon);
                    featurePoints[index] = new Vector2(fPointx, fPointy);
                    index++;
                }
            }
        }
        for (int x = 0; x <= resolution; x++)
        {
            for (int y = 0; y <= resolution; y++)
            {
                Vector2 cellCoords = new Vector2(x / cellSize, y / cellSize);
                voronoiMap[x, y] = GetHeightAtPoint(new Vector2(x, y), featurePoints, cellDensity, cellCoords, c1, c2);
            }
        }

        return voronoiMap;
    }

    private static float GetHeightAtPoint(Vector2 currentPoint, Vector2[] featurePoints, int cellDensity, Vector2 cellCoords, float c1, float c2)
    {
        Vector2 d = new Vector2(0.0f, 0.0f);
        int cx = (int)Mathf.Floor(cellCoords.x), cy = (int)Mathf.Floor(cellCoords.y);
        for (int i = Mathf.Max(0, cx - 2); i < Mathf.Min(cellDensity, cx + 2); i++)
        {
            for (int j = Mathf.Max(0, cy - 2); j < Mathf.Min(cellDensity, cy + 2); j++)
            {
                if((i == cx - 2 || i == cx + 2) && (j == cy - 2 || j == cy + 2))
                    continue;
                int index = i * cellDensity + j;
                for(int k = 0; k < numOfFeaturePointsPerCell; k++)
                {
                    Vector2 featurePoint = featurePoints[index * numOfFeaturePointsPerCell + k];
                    float distance = SpcDistance(currentPoint, featurePoint); // proposed speed optimization
                    if(distance < d.x || d.x == 0.0f)
                    {
                        d.y = d.x;
                        d.x = distance;
                    }
                    else if(distance < d.y || d.y == 0.0f)
                    {
                        d.y = distance;
                    }
                }
            }
        }
        return c1 * d.x + c2 * d.y; 
    }

    private static float SpcDistance(Vector2 a, Vector2 b) => Mathf.Pow(a.x - b.x, 2) + Mathf.Pow(a.y - b.y, 2);
}