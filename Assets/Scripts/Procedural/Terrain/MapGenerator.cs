using UnityEngine;
using System.Collections;

using NoiseFunction = Constants.NoiseFunction;

public class MapGenerator : MonoBehaviour {

	public enum DrawMode {NoiseMap, Mesh};
	public DrawMode drawMode;

	public NoiseFunction noiseFunction;

	const int mapChunkSize = 240;

	[Header("General")]
	[Range(0, 6)]
	public int levelOfDetail;
	public int seed;
	public float meshHeightMultiplier;
	public AnimationCurve meshHeightCurve;
	[Range(0.001f, 1.999f)]
	public float exponent;

	[Header("Perlin")]
	[Range(1f, 100f)]
	public float noiseScale;
	[Range(1, 10)]
	public int octaves;
	public float persistance;
	[Range(1, 5)]
	public float lacunarity;
	public Vector2 offset;

	[Header("Diamond-Square")]
	public float randRange;
	public float reductionRate;

	[Header("Voronoi")]
	public int cellDensity = 2;
	public int c1 = 0;
	public int c2 = 0;

	public bool autoUpdate;

	public void GenerateMap() {
		float[,] noiseMap = GenerateNoiseMap();

		MapDisplay display = FindObjectOfType<MapDisplay> ();
		if (drawMode == DrawMode.NoiseMap) {
			display.DrawTexture (TextureGenerator.TextureFromHeightMap (noiseMap));
		}
		else if (drawMode == DrawMode.Mesh) {
			display.DrawMesh (TerrainMeshGenerator.GenerateTerrainMesh (noiseMap, meshHeightMultiplier, levelOfDetail));
		}
	}

	private float[,] GenerateNoiseMap()
	{
		if(noiseFunction == NoiseFunction.DiamondSquare)
		{
			return DiamondSquareNoise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, randRange, reductionRate);
		}
		else if(noiseFunction == NoiseFunction.Voronoi)
		{
			return Voronoi.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, cellDensity, c1, c2);
		}
		else if(noiseFunction == NoiseFunction.Combined)
		{
			return CombinedNoise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, cellDensity, c1, c2, randRange, reductionRate);
		}
		return PerlinNoise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset);
	}

	void OnValidate() {
		if (lacunarity < 1) {
			lacunarity = 1;
		}
		if (octaves < 0) {
			octaves = 0;
		}
		if(cellDensity < 2)
		{
			cellDensity = 2;
		}
		if(cellDensity > 2000)
		{
			cellDensity = 2000;
		}
	}
}