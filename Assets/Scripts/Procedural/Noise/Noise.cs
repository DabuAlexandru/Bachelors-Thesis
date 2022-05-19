using UnityEngine;

using NoiseFunction = Constants.NoiseFunction;

public static class Noise {

	public static float[,] GetSmoothedNoiseMap(float[,] noiseMap, int filterSize = 1)
	{
		int width = noiseMap.GetLength(0), height = noiseMap.GetLength(1);
		float[,] smoothedMap = new float[width, height];
		int windowSize = 2 * filterSize + 1;
		for(int i = 0; i < height; i++)
		{
			for(int j = 0; j < width; j++)
			{
				float avg = 0.0f;
				int count = 0;
				for(int a = -filterSize; a < windowSize; a++)
				{
					if(i + a < 0 || i + a >= height)
						continue;
					for(int b = -filterSize; b < windowSize; b++)
					{
						if(j + b < 0 || j + b >= width)
							continue;
						avg += noiseMap[i + a, j + b];
						count++;
					}
				}
				smoothedMap[i, j] = avg / count;
			}
		}
		return smoothedMap;
	}

	public static float[,] GetNormalizedHeightMap(int mapWidth, int mapHeight, float[,] noiseGrid)
	{
		float[,] noiseMap = new float[(mapWidth + 1), (mapHeight + 1)];
		float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        for (int y = 0; y <= mapHeight; y++)
        {
            for (int x = 0; x <= mapWidth; x++)
            {
                float noiseHeight = noiseGrid[x, y];
                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y <= mapHeight; y++)
        {
            for (int x = 0; x <= mapWidth; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }
		return noiseMap;
	}

}
