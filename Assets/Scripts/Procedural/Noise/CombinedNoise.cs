using UnityEngine;

public static class CombinedNoise
{
    public static float[,] GenerateNoiseMap(
        int mapWidth, int mapHeight, int seed, 
        int cellDensity, float c1, float c2,
        float displacementRange = 1.0f, float displacementReductionRate = 2.0f,
        float voronoiBias = 0.333f, int smoothFilterSize = 0)
    {
        float[,] diSqNoise = DiamondSquareNoise.GenerateNoiseMap(mapWidth, mapHeight, seed, displacementRange, displacementReductionRate, smoothFilterSize);
        float[,] vrnNoise  = Voronoi.GenerateNoiseMap(mapWidth, mapHeight, seed, cellDensity, c1, c2);
        float[,] noiseMap = new float[(mapWidth + 1), (mapHeight + 1)];

        for(int i = 0; i <= mapHeight; i++)
        {
            for(int j = 0; j <= mapWidth; j++)
            {
                // we add some perlin noise over to give it a more random look
                noiseMap[i, j] = (1 - voronoiBias) * diSqNoise[i, j] + voronoiBias * vrnNoise[i, j];
                // apply a perturbation filter
                noiseMap[i, j] = ((0.3f * (Mathf.PerlinNoise(i, j) * 0.75f - 0.25f)) + noiseMap[i, j]) / 1.3f;
            }
        }

        return noiseMap;
    }
}