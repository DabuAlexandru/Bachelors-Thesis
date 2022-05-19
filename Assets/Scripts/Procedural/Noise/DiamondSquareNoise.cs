using UnityEngine;

public static class DiamondSquareNoise
{
    private const float minHeight = -10.0f;
    private const float maxHeight = 10.0f;
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float displacementRange = 1.0f, float displacementReductionRate = 2.0f, int smoothFilterSize = 0)
    {
        float mapResolution = Mathf.Max(mapWidth, mapHeight); // the resolution of the noiseMap
        int exp = (int)Mathf.Ceil(Mathf.Log(mapResolution, 2)); // the exponent of the diamond-square grid (size: (2 ^ exp + 1) ^ 2)

        float[,] noiseGrid = GenerateDiamondSquareGrid(exp, seed, displacementRange, displacementReductionRate);
        if(smoothFilterSize > 1)
        {
            noiseGrid = Noise.GetSmoothedNoiseMap(noiseGrid, smoothFilterSize);
        }
        return Noise.GetNormalizedHeightMap(mapWidth, mapHeight, noiseGrid);
    }

    private static int Modulo(int x, int m) => Constants.Modulo(x, m);

    public static float[,] GenerateDiamondSquareGrid(int mapWidth, int mapHeight, int seed, float displacementRange = 1.0f, float displacementReductionRate = 2.0f)
    {
        float mapResolution = Mathf.Max(mapWidth, mapHeight); // the resolution of the noiseMap
        int exp = (int)Mathf.Ceil(Mathf.Log(mapResolution, 2)); // the exponent of the diamond-square grid (size: (2 ^ exp + 1) ^ 2)

        float[,] noiseGrid = GenerateDiamondSquareGrid(exp, seed, displacementRange, displacementReductionRate);
        return noiseGrid;
    }

    public static float[,] GenerateDiamondSquareGrid(int exp, int seed, float displacementRange = 1.0f, float displacementReductionRate = 2.0f)
    {
        Random.InitState(seed);
        float[] initialRange = { minHeight, maxHeight };
        exp = Mathf.Max(exp, 0);
        int resolution = (int)Mathf.Pow(2.0f, (float)exp);
        float[,] grid = new float[resolution + 1, resolution + 1];
        // set a random height to each of the 4 corners
        grid[0, 0] = Random.Range(initialRange[0], initialRange[1]);
        grid[0, resolution] = Random.Range(initialRange[0], initialRange[1]);
        grid[resolution, 0] = Random.Range(initialRange[0], initialRange[1]);
        grid[resolution, resolution] = Random.Range(initialRange[0], initialRange[1]);

        int windowSize = resolution;
        while (windowSize > 1)
        {
            int halfSize = windowSize / 2;

            // square step
            for (int x = 0; x < resolution; x += windowSize)
            {
                for (int y = 0; y < resolution; y += windowSize)
                {
                    float avg = (
                            grid[x, y] +
                            grid[x, y + windowSize] +
                            grid[x + windowSize, y] +
                            grid[x + windowSize, y + windowSize]
                        ) / 4.0f;
                    float offset = Random.Range(-Mathf.Abs(displacementRange), Mathf.Abs(displacementRange));
                    grid[x + halfSize, y + halfSize] = avg + offset;
                }
            }

            // diamond step
            for (int x = 0; x <= resolution; x += halfSize)
            {
                for (int y = (x + halfSize) % windowSize; y <= resolution; y += windowSize)
                {
                    int count = 0;
                    float avg = 0.0f;
                    if(x - halfSize >= 0)
                    {
                        avg += grid[x - halfSize, y];
                        count++;
                    }
                    if(y - halfSize >= 0)
                    {
                        avg += grid[x, y - halfSize];
                        count++;
                    }
                    if(y + halfSize <= resolution)
                    {
                        avg += grid[x, y + halfSize];
                        count++;
                    }
                    if(x + halfSize <= resolution)
                    {
                        avg += grid[x + halfSize, y];
                        count++;
                    }
                    // float avg = (
                    //         grid[Modulo(x - halfSize, resolution + 1), y] +
                    //         grid[x, Modulo(y - halfSize, resolution + 1)] +
                    //         grid[x, Modulo(y + halfSize, resolution + 1)] +
                    //         grid[Modulo(x + halfSize, resolution + 1), y]
                    //     ) / 4.0f;
                    avg /= count;
                    float offset = Random.Range(-Mathf.Abs(displacementRange), Mathf.Abs(displacementRange));
                    grid[x, y] = avg + offset;
                }
            }

            windowSize /= 2;
            displacementRange = Mathf.Max(displacementRange / displacementReductionRate, 0.1f);
        }

        return grid;
    }

}
