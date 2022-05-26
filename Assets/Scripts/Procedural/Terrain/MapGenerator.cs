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
	[Range(1f, 300f)]
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
	[Range(0.0001f, 100.0f)]
	[SerializeField] float randRange;
	public float RandRange { get => randRange; }

	[Range(0.0001f, 10.0f)]
	[SerializeField] float reductionRate;
	public float ReductionRate { get => reductionRate; }
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

public class MapGenerator : MonoBehaviour {

	enum DrawMode {NoiseMap, Mesh, Island};
	[SerializeField] DrawMode drawMode;

	[SerializeField] NoiseFunction noiseFunction;

	[SerializeReference] Material terrainMaterial;

	const int mapChunkSize = 240;

	[SerializeField] GeneralNoiseParams generalNoiseParams;
	
	[SerializeField] PerlinNoiseParams perlinNoiseParams;

	[SerializeField] DiamondSquareNoiseParams diamondSquareNoiseParams;

	[SerializeField] VoronoiDiagramParams voronoiDiagramParams;
	
	[SerializeField] CombinedNoiseParams combinedNoiseParams;

	[SerializeField] bool autoUpdate;

	public bool ShouldAutoUpdate() => autoUpdate;

	public void GenerateMap() {
		float[,] noiseMap = GenerateNoiseMap();
		noiseMap = Noise.ApplyCurve(noiseMap, generalNoiseParams.MeshHeightCurve);

		MapDisplay display = FindObjectOfType<MapDisplay> ();
		if (drawMode == DrawMode.NoiseMap) {
			display.DrawTexture (TextureGenerator.TextureFromHeightMap(noiseMap));
		}
		else if (drawMode == DrawMode.Mesh) {
			display.DrawMesh (
				TerrainMeshGenerator.GenerateTerrainMesh(noiseMap, generalNoiseParams.LevelOfDetail, generalNoiseParams.MeshHeightMultiplier)
			);
		}
		else if (drawMode == DrawMode.Island) {
			IslandGenerator.GenerateIsland(3, mapChunkSize, generalNoiseParams, perlinNoiseParams, diamondSquareNoiseParams, voronoiDiagramParams, combinedNoiseParams, noiseFunction, terrainMaterial);
		}
	}

	private float[,] GenerateNoiseMap()
	{
		int seed = generalNoiseParams.Seed;
		if(noiseFunction == NoiseFunction.DiamondSquare)
		{
			return Noise.GenerateHeightMap(mapChunkSize, mapChunkSize, seed, diamondSquareNoiseParams);
		}
		else if(noiseFunction == NoiseFunction.Voronoi)
		{
			return Noise.GenerateHeightMap(mapChunkSize, mapChunkSize, seed, voronoiDiagramParams);
		}
		else if(noiseFunction == NoiseFunction.Combined)
		{
			return Noise.GenerateHeightMap(mapChunkSize, mapChunkSize, seed, 
				combinedNoiseParams, diamondSquareNoiseParams, voronoiDiagramParams);
		}
		return Noise.GenerateHeightMap(mapChunkSize, mapChunkSize, seed, perlinNoiseParams);
	}

	private void OnEnable() => GenerateMap();
}