using UnityEngine;

using NoiseFunction = Constants.NoiseFunction;

public static class IslandGenerator
{
    private static int resolution;
    private static int mapChunkSize;
    private static ProceduralPlaneMesh[,] terrainChunks;
    private static GameObject[,] islandChunkObjects;
    private static GameObject island;

    public static void GenerateIsland(int resolution, int mapChunkSize, GeneralNoiseParams generalNoiseParams,
        PerlinNoiseParams perlinNoiseParams, DiamondSquareNoiseParams diamondSquareNoiseParams,
        VoronoiDiagramParams voronoiDiagramParams, CombinedNoiseParams combinedNoiseParams,
        NoiseFunction noiseFunction, Material terrainMaterial
    )
    {
        IslandGenerator.resolution = resolution;
        IslandGenerator.mapChunkSize = mapChunkSize;
        IslandGenerator.terrainChunks = new ProceduralPlaneMesh[resolution, resolution];
        int seed = generalNoiseParams.Seed;
        int mapResolution = resolution * (mapChunkSize + 1) - 1;

        float[,] heightMap = new float[mapResolution + 1, mapResolution + 1];
        
        if(noiseFunction == NoiseFunction.Perlin)
        {
            heightMap = Noise.GenerateHeightMap(mapResolution, mapResolution, seed, perlinNoiseParams);
        }
        else if(noiseFunction == NoiseFunction.DiamondSquare)
		{
			heightMap = Noise.GenerateHeightMap(mapResolution, mapResolution, seed, diamondSquareNoiseParams);
		}
		else if(noiseFunction == NoiseFunction.Voronoi)
		{
			heightMap = Noise.GenerateHeightMap(mapResolution, mapResolution, seed, voronoiDiagramParams);
		}
		else if(noiseFunction == NoiseFunction.Combined)
		{
			heightMap = Noise.GenerateHeightMap(mapResolution, mapResolution, seed, 
				combinedNoiseParams, diamondSquareNoiseParams, voronoiDiagramParams);
		}

        float[,] chunkHeightMap = new float[mapChunkSize + 1, mapChunkSize + 1];
        for(int i = 0; i < resolution; i++)
        {
            for(int j = 0; j < resolution; j++)
            {
                Vector2 offset = new Vector2(i * (mapChunkSize + 1), j * (mapChunkSize + 1));
                for(int k = 0; k <= mapChunkSize; k++)
                {
                    for(int l = 0; l <= mapChunkSize; l++)
                    {
                        chunkHeightMap[k, l] = heightMap[(int)offset.x + k, (int)offset.y + l];
                    }
                }
                terrainChunks[i, j] = TerrainMeshGenerator.GenerateTerrainMesh(chunkHeightMap, 0, generalNoiseParams.MeshHeightMultiplier);
            }
        }
        if(island == null) 
            InitializeIslandObject(terrainMaterial);
        UpdateChunkMeshes();
    }

    private static void InitializeIslandObject(Material terrainMaterial)
    {
        island = new GameObject("Island");
        islandChunkObjects = new GameObject[resolution, resolution];
        
        for(int i = 0; i < resolution; i++)
        {
            for(int j = 0; j < resolution; j++)
            {
                GameObject terrainChunk = new GameObject("TerrainChunk" + (i * resolution + j));
            
                terrainChunk.AddComponent<MeshFilter>();
                terrainChunk.AddComponent<MeshRenderer>();

                terrainChunk.GetComponent<MeshRenderer>().material = terrainMaterial;

                terrainChunk.transform.SetParent(island.transform);

                terrainChunk.transform.position = new Vector3(i, 0.0f, j);
                islandChunkObjects[i, j] = terrainChunk;
            }
        }
        island.transform.localScale = new Vector3(mapChunkSize, 1.0f, mapChunkSize);
    }

    private static void UpdateChunkMeshes()
    {
        for(int i = 0; i < resolution; i++)
        {
            for(int j = 0; j < resolution; j++)
            {
                GameObject terrainChunk = islandChunkObjects[i, j];
                Mesh terrainChunkMesh = terrainChunk.GetComponent<MeshFilter>().sharedMesh;
                terrainChunkMesh = terrainChunks[i, j].GetMesh();

                terrainChunkMesh.RecalculateNormals();
                terrainChunkMesh.RecalculateBounds();

                terrainChunk.GetComponent<MeshFilter>().sharedMesh = terrainChunkMesh;
            }
        }
    }
}