using UnityEngine;

public static class PerlinNoise {

	public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistence, float lacunarity, Vector2 offset) {
		float[,] noiseMap = new float[(mapWidth + 1),(mapHeight + 1)];

		System.Random prng = new System.Random (seed);
		Vector2[] octaveOffsets = new Vector2[octaves];
		for (int i = 0; i < octaves; i++) {
			float offsetX = prng.Next (-100000, 100000) + offset.x;
			float offsetY = prng.Next (-100000, 100000) + offset.y;
			octaveOffsets [i] = new Vector2 (offsetX, offsetY);
		}

		if (scale <= 0) {
			scale = 0.0001f;
		}

		float maxNoiseHeight = float.MinValue;
		float minNoiseHeight = float.MaxValue;

		float halfWidth = (mapWidth + 1) / 2f;
		float halfHeight = (mapHeight + 1) / 2f;


		for (int y = 0; y <= mapHeight; y++) {
			for (int x = 0; x <= mapWidth; x++) {
		
				float amplitude = 1;
				float frequency = 1;
				float noiseHeight = 0;

				for (int i = 0; i < octaves; i++) {
					float sampleX = (x-halfWidth) / (mapWidth + 1) * scale * frequency + octaveOffsets[i].x * frequency;
					float sampleY = (y-halfHeight) / (mapHeight + 1) * scale * frequency + octaveOffsets[i].y * frequency;

					float perlinValue = Mathf.PerlinNoise (sampleX, sampleY) * 2 - 1;
					noiseHeight += perlinValue * amplitude;

					amplitude *= persistence;
					frequency *= lacunarity;
				}

				if (noiseHeight > maxNoiseHeight) {
					maxNoiseHeight = noiseHeight;
				} else if (noiseHeight < minNoiseHeight) {
					minNoiseHeight = noiseHeight;
				}
				noiseMap [x, y] = noiseHeight;
			}
		}

		return Noise.GetNormalizedHeightMap(mapWidth, mapHeight, noiseMap);
	}

}
