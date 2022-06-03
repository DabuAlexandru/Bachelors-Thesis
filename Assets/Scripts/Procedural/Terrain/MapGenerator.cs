using UnityEngine;
using System.Collections;

using NoiseFunction = Constants.NoiseFunction;

[System.Serializable]
public class GeneralNoiseParams
{
	[Range(0, 6)]
	[SerializeField] int levelOfDetail;
	public int LevelOfDetail { get => levelOfDetail; }

	[SerializeField] int seed;
	public int Seed { get => seed; }

	[SerializeField] float meshHeightMultiplier;
	public float MeshHeightMultiplier { get => meshHeightMultiplier; }

	[SerializeField] AnimationCurve meshHeightCurve;
	public AnimationCurve MeshHeightCurve { get => meshHeightCurve; }
}

[System.Serializable]
public class PerlinNoiseParams
{
	[Range(1f, 100f)]
	[SerializeField] float noiseScale;
	public float NoiseScale { get => noiseScale; }

	[Range(1, 10)]
	[SerializeField] int octaves;
	public int Octaves { get => octaves; }

	[SerializeField] float persistence;
	public float Persistence { get => persistence; }

	[Range(1, 5)]
	[SerializeField] float lacunarity;
	public float Lacunarity { get => lacunarity; }

	[SerializeField] Vector2 offset;
	public Vector2 Offset { get => offset; }
}

[System.Serializable]
public class DiamondSquareNoiseParams
{
	[Range(0f, 100.0f)]
	[SerializeField] float randRange;
	public float RandRange { get => randRange; }

	[Range(0.0001f, 1.0f)]
	[SerializeField] float persistence;
	public float Persistence { get => persistence; }
}

[System.Serializable]
public class VoronoiDiagramParams
{
	[Range(2, 20)]
	[SerializeField] int cellDensity = 2;
	public int CellDensity { get => cellDensity; }

	[SerializeField] int c1;
	public int C1 { get => c1; }
	
	[SerializeField] int c2;
	public int C2 { get => c2; }
}

[System.Serializable]
public class CombinedNoiseParams
{
	[Range(0.0f, 1.0f)]
	[SerializeField] float perturbation;
	public float Perturbation { get => perturbation; }

	[Range(0, 2)]
	[SerializeField] int filterFlag;
	public int FilterFlag { get => filterFlag; }
}

[System.Serializable]
public class NoiseSettings
{
	[SerializeField] GeneralNoiseParams generalNoiseParams;
	public GeneralNoiseParams GeneralNoiseParams { get => generalNoiseParams; }
	
	[SerializeField] PerlinNoiseParams perlinNoiseParams;
	public PerlinNoiseParams PerlinNoiseParams { get => perlinNoiseParams; }

	[SerializeField] DiamondSquareNoiseParams diamondSquareNoiseParams;
	public DiamondSquareNoiseParams DiamondSquareNoiseParams { get => diamondSquareNoiseParams; }

	[SerializeField] VoronoiDiagramParams voronoiDiagramParams;
	public VoronoiDiagramParams VoronoiDiagramParams { get => voronoiDiagramParams; }
	
	[SerializeField] CombinedNoiseParams combinedNoiseParams;
	public CombinedNoiseParams CombinedNoiseParams { get => combinedNoiseParams; }
}

[System.Serializable]
public class DistributionParams
{
	[SerializeField, Min(1)] int windowSize = 1;
	public int WindowSize { get => windowSize; }

	[SerializeField, Range(0f, 1f)] float minTerrainHeight = 0.1f;
	public float MinTerrainHeight { get => minTerrainHeight; }
	
	[SerializeField, Range(0f, 1f)] float maxTerrainHeight = 0.9f;
	public float MaxTerrainHeight { get => maxTerrainHeight; }

	[SerializeField, Range(0f, 0.2f)] float maxElevationDifference = 0.03f;
	public float MaxElevationDifference { get => maxElevationDifference; }

	[SerializeField, Min(0)] int variance = 1;
	public int Variance { get => variance; }

	[SerializeField] bool randomize = false;
	public bool Randomize { get => randomize; }
}

public class MapGenerator : MonoBehaviour {

	enum DrawMode {NoiseMap, Mesh, TreeMap, Island};
	[SerializeField] DrawMode drawMode;

	[SerializeField] NoiseFunction noiseFunction;

	[SerializeReference] Material terrainMaterial;

	[SerializeReference] Material treeMaterial;

	[SerializeReference] Material leavesMaterial;

	[SerializeField] DistributionParams distributionParams;

	const int mapChunkSize = 120;
	const int islandChunkSize = 120;
	const int islandChunkCount = 4;

	[SerializeField] NoiseSettings noiseSettings;

	[SerializeField] bool autoUpdate;

	public bool ShouldAutoUpdate() => autoUpdate;

	public void GenerateMap() {
		float[,] noiseMap = Noise.GenerateHeightMap(mapChunkSize, mapChunkSize, noiseFunction, noiseSettings);
		AnimationCurve meshHeightCurve = noiseSettings.GeneralNoiseParams.MeshHeightCurve;
		noiseMap = Noise.ApplyCurve(noiseMap, meshHeightCurve);

		MapDisplay display = FindObjectOfType<MapDisplay> ();
		if (drawMode == DrawMode.NoiseMap) {
			display.DrawTexture (TextureGenerator.TextureFromHeightMap(noiseMap));
		}
		if (drawMode == DrawMode.TreeMap) {
			float[,] heightMap = new float[mapChunkSize + 1, mapChunkSize + 1];
			display.DrawTexture(TextureGenerator.TextureFromHeightMapWithTrees(noiseMap, TerrainMeshGenerator.GetTreesOnHeightMap(noiseMap, distributionParams)));
		}
		else if (drawMode == DrawMode.Mesh) {
			display.DrawMesh (
				TerrainMeshGenerator.GenerateTerrainMesh(noiseMap, noiseSettings.GeneralNoiseParams.LevelOfDetail, noiseSettings.GeneralNoiseParams.MeshHeightMultiplier)
			);
		}
		else if (drawMode == DrawMode.Island) {
			IslandGenerator.GenerateIsland(islandChunkCount, islandChunkSize, noiseSettings, distributionParams, noiseFunction, terrainMaterial, treeMaterial, leavesMaterial);
		}
	}

	private float[,] GenerateNoiseMap()
	{
		int seed = noiseSettings.GeneralNoiseParams.Seed;
		if(noiseFunction == NoiseFunction.DiamondSquare)
		{
			return Noise.GenerateHeightMap(mapChunkSize, mapChunkSize, seed, noiseSettings.DiamondSquareNoiseParams);
		}
		else if(noiseFunction == NoiseFunction.Voronoi)
		{
			return Noise.GenerateHeightMap(mapChunkSize, mapChunkSize, seed, noiseSettings.VoronoiDiagramParams);
		}
		else if(noiseFunction == NoiseFunction.Combined)
		{
			return Noise.GenerateHeightMap(mapChunkSize, mapChunkSize, seed, 
				noiseSettings.CombinedNoiseParams, noiseSettings.DiamondSquareNoiseParams, noiseSettings.VoronoiDiagramParams);
		}
		return Noise.GenerateHeightMap(mapChunkSize, mapChunkSize, seed, noiseSettings.PerlinNoiseParams);
	}

	private void OnEnable() => GenerateMap();
}