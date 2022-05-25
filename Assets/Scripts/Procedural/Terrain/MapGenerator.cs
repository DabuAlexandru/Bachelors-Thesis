using UnityEngine;
using System.Collections;

using NoiseFunction = Constants.NoiseFunction;

public class MapGenerator : MonoBehaviour {

	enum DrawMode {NoiseMap, Mesh};
	[SerializeField] DrawMode drawMode;

	[SerializeField] NoiseFunction noiseFunction;

	const int mapChunkSize = 240;

	[Header("General")]
	[Range(0, 6)]
	[SerializeField] int levelOfDetail;
	[SerializeField] int seed;
	[SerializeField] float meshHeightMultiplier;
	[SerializeField] AnimationCurve meshHeightCurve;
	
	[Header("Perlin")]
	[Range(1f, 100f)]
	[SerializeField] float noiseScale;
	[Range(1, 10)]
	[SerializeField] int octaves;
	[SerializeField] float persistence;
	[Range(1, 5)]
	[SerializeField] float lacunarity;
	[SerializeField] Vector2 offset;

	[Header("Diamond-Square")]
	[Range(0.0001f, 100.0f)]
	[SerializeField] float randRange;
	[Range(0.0001f, 10.0f)]
	[SerializeField] float reductionRate;

	[Header("Voronoi")]
	[SerializeField] int cellDensity = 2;
	[SerializeField] int c1 = 0;
	[SerializeField] int c2 = 0;
	
	[Header("Combined")]
	[Range(0.0f, 1.0f)]
	[SerializeField] float perturbation = 0.25f;
	[Range(0, 2)]
	[SerializeField] int filterFlag = 1;

	[SerializeField] bool autoUpdate;

	public bool ShouldAutoUpdate() => autoUpdate;

	public void GenerateMap() {
		float[,] noiseMap = GenerateNoiseMap();
		noiseMap = Noise.ApplyCurve(noiseMap, meshHeightCurve);

		MapDisplay display = FindObjectOfType<MapDisplay> ();
		if (drawMode == DrawMode.NoiseMap) {
			display.DrawTexture (TextureGenerator.TextureFromHeightMap(noiseMap));
		}
		else if (drawMode == DrawMode.Mesh) {
			display.DrawMesh (
				TerrainMeshGenerator.GenerateTerrainMesh(noiseMap, levelOfDetail, meshHeightMultiplier)
			);
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
			return CombinedNoise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, cellDensity, c1, c2, randRange, reductionRate, perturbation, filterFlag);
		}
		return PerlinNoise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistence, lacunarity, offset);
	}

	void OnValidate() 
	{
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

	private void OnEnable() => GenerateMap();
}