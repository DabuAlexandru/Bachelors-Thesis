using UnityEngine;

public static class Noise {

	public static float[,] GetSmoothedNoiseMap(float[,] noiseMap, int filterSize = 1)
	{
		int width = noiseMap.GetLength(0), height = noiseMap.GetLength(1);
		float[,] smoothedMap = new float[width, height];
		int windowSize = 2 * filterSize + 1;
		for(int i = 0; i < width; i++)
		{
			for(int j = 0; j < height; j++)
			{
				float avg = 0.0f;
				int count = 0;
				for(int a = -filterSize; a < windowSize; a++)
				{
					if(i + a < 0 || i + a >= width)
						continue;
					for(int b = -filterSize; b < windowSize; b++)
					{
						if(j + b < 0 || j + b >= height)
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

	public static float[,] ApplyCurve(float[,] heightMap, AnimationCurve meshHeightCurve)
	{
		int mapWidth = heightMap.GetLength(0), mapHeight = heightMap.GetLength(1);
		for(int i = 0; i < mapWidth; i++)
		{
			for(int j = 0; j < mapHeight; j++)
			{
				heightMap[i, j] = meshHeightCurve.Evaluate(heightMap[i, j]);
			}
		}
		return heightMap;
	}

	public static float[,] GetNormalizedHeightMap(float[,] noiseGrid)
	{
		int mapWidth = noiseGrid.GetLength(0), mapHeight = noiseGrid.GetLength(1);
		return GetNormalizedHeightMap(mapWidth - 1, mapHeight - 1, noiseGrid);
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

	private static float GetPerlinInterval(float px, float py, float maxVal){
		return maxVal * (2 * Mathf.PerlinNoise(px, py) - 1.0f);
	}

	private static int GetPositionWithFlag(int index, int perturbation, int size, int filterFlag)
	{
		/* filterFlag: if the new position is out of bounds
			0 - get the point at the original position
			1 - clamp the position in the interval
			2 - wrap around
		*/
		int newIndex = index + perturbation;
		switch(filterFlag)
		{
			case 0:
				if(newIndex < 0 || newIndex >= size)
					newIndex = index;
				break;
			case 1:
				newIndex = Mathf.Clamp(newIndex, 0, size - 1);
				break;
			default:
			case 2:
				newIndex = Constants.Modulo(newIndex, size);
				break;
		}
		return newIndex;
	}

	const float scale = 1f;

	public static float[,] GetHeightMapWithPerturbation(float[,] noiseMap, float perturbation = 0.25f, int filterFlag = 1)
	{
		Vector2 offset = new Vector2(226.0f, 1234.0f);

		int width = noiseMap.GetLength(0), height = noiseMap.GetLength(1);
		float[,] modifiedMap = new float[width, height];

		for(int i = 0; i < width; i++)
		{
			for(int j = 0; j < height; j++)
			{
				float sampleX = (float)(i / scale * width - 0.5f);
				float sampleY = (float)(j / scale * height - 0.5f);

				int perturbationX = (int)Mathf.Floor(width * GetPerlinInterval(sampleX, sampleY, perturbation));
				int newX = GetPositionWithFlag(i, perturbationX, width, filterFlag);

				sampleX = (float)((i + offset.x) / scale * width - 0.5f);
				sampleY = (float)((j + offset.y) / scale * height - 0.5f);

				int perturbationY = (int)Mathf.Floor(height * GetPerlinInterval(sampleX, sampleY, perturbation));
				int newY = GetPositionWithFlag(j, perturbationY, height, filterFlag);

				modifiedMap[i, j] = noiseMap[newX, newY];
			}
		}

		return modifiedMap;
	}

}
