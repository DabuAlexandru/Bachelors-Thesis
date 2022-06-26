using UnityEngine;
using System.Collections.Generic;

using NoiseFunction = Constants.NoiseFunction;

public static class IslandGenerator
{
    private static int resolution;
    private static int mapChunkSize;
    private static GameObject island;
    private static float[,] heightMap;
    private const float minHeight = 0.15f;
    private static float meshHeightMultiplier;

    private class IslandChunk
    {
        private int chunkLOD;
        public int ChunkLOD { get => chunkLOD; set { chunkLOD = value; UpdateIslandChunkLOD(); } }

        private GameObject chunkObject;
        public GameObject ChunkObject { get => chunkObject; set => chunkObject = value; }

        private ProceduralPlaneMesh planeObject;
        public ProceduralPlaneMesh PlaneObject { get => planeObject; set => planeObject = value; }

        private List<TreeEntity> trees;
        public List<TreeEntity> Trees { get => trees; }

        public void AddTree(TreeEntity tree) => trees.Add(tree);
        public TreeEntity GetTree(int i) => trees[i];

        public void UpdateIslandChunkLOD()
        {
            this.PlaneObject.SimplifyMesh(chunkLOD);
            List<TreeEntity> trees = this.trees;
            foreach (TreeEntity tree in this.trees)
            {
                tree.ModifyLODTree(chunkLOD);
            }
        }

        public IslandChunk()
        {
            trees = new List<TreeEntity>();
        }

    }

    private static IslandChunk[,] islandChunks;

    private static float islandObjectScale;

    public static void GenerateIsland(int resolution, int mapChunkSize, NoiseSettings noiseSettings, DistributionParams distributionParams,
        NoiseFunction noiseFunction, Material terrainMaterial, Material treeMaterial, Material leavesMaterial
    )
    {
        IslandGenerator.resolution = resolution;
        IslandGenerator.mapChunkSize = mapChunkSize;

        if (island == null)
            InitializeIslandChunks();

        int seed = noiseSettings.GeneralNoiseParams.Seed;
        int mapResolution = resolution * mapChunkSize;

        heightMap = new float[mapResolution + 1, mapResolution + 1];

        if (noiseFunction == NoiseFunction.Perlin)
        {
            heightMap = Noise.GenerateHeightMap(mapResolution, mapResolution, seed, noiseSettings.PerlinNoiseParams);
        }
        else if (noiseFunction == NoiseFunction.DiamondSquare)
        {
            heightMap = Noise.GenerateHeightMap(mapResolution, mapResolution, seed, noiseSettings.DiamondSquareNoiseParams);
        }
        else if (noiseFunction == NoiseFunction.Voronoi)
        {
            heightMap = Noise.GenerateHeightMap(mapResolution, mapResolution, seed, noiseSettings.VoronoiDiagramParams);
        }
        else if (noiseFunction == NoiseFunction.Combined)
        {
            heightMap = Noise.GenerateHeightMap(mapResolution, mapResolution, seed,
                noiseSettings.CombinedNoiseParams, noiseSettings.DiamondSquareNoiseParams, noiseSettings.VoronoiDiagramParams);
        }

        AnimationCurve meshHeightCurve = noiseSettings.GeneralNoiseParams.MeshHeightCurve;
        meshHeightMultiplier = noiseSettings.GeneralNoiseParams.MeshHeightMultiplier;
        // modify the height map
        for (int v = 0; v <= mapResolution; v++)
        {
            for (int u = 0; u <= mapResolution; u++)
            {
                heightMap[u, v] = meshHeightCurve.Evaluate(heightMap[u, v]);
                heightMap[u, v] *= EvaluateShore((float)u / mapResolution, (float)v / mapResolution);
            }
        }

        Vector3[,] vertexNormals = TerrainMeshGenerator.GetNormalsFromHeightMap(heightMap, mapChunkSize, meshHeightMultiplier);

        float[,] chunkHeightMap = new float[mapChunkSize + 1, mapChunkSize + 1];
        for (int j = 0; j < resolution; j++)
        {
            for (int i = 0; i < resolution; i++)
            {
                Vector2 offset = new Vector2(i * mapChunkSize, j * mapChunkSize);
                Vector3[] chunkVertexNormals = new Vector3[(mapChunkSize + 1) * (mapChunkSize + 1)];
                int normalsIndex = 0;
                for (int v = 0; v <= mapChunkSize; v++)
                {
                    for (int u = 0; u <= mapChunkSize; u++)
                    {
                        int globalX = (int)offset.x + u;
                        int globalY = (int)offset.y + v;

                        chunkHeightMap[u, v] = heightMap[globalX, globalY];
                        chunkVertexNormals[normalsIndex] = vertexNormals[globalX, globalY];
                        normalsIndex++;
                    }
                }
                islandChunks[i, j].PlaneObject = TerrainMeshGenerator.GenerateTerrainMesh(chunkHeightMap, 0, meshHeightMultiplier);
                islandChunks[i, j].PlaneObject.SetNormals(chunkVertexNormals);
            }
        }
        if (island == null)
            InitializeIslandObject(terrainMaterial, heightMap, meshHeightMultiplier);
        Vector2[] trees = TerrainMeshGenerator.GetTreesOnHeightMap(heightMap, distributionParams);
        AddTreesToIsland(trees, treeMaterial, leavesMaterial, heightMap, meshHeightMultiplier);
        UpdateChunkMeshes();
    }

    public static Vector2 GetCoordsOnMap(Transform transform)
    {
        float x = transform.position.x, z = transform.position.z;
        int halfSize = (int)resolution * mapChunkSize / 2;
        int chunkU = (int)Mathf.Floor((WorldToMapCoord(x) + halfSize) / mapChunkSize);
        int chunkV = (int)Mathf.Floor((WorldToMapCoord(z) + halfSize) / mapChunkSize);
        return new Vector2(chunkU, chunkV);
    }

    public static Vector2 GetCoordsOnChunk(Transform transform)
    {
        float x = transform.position.x, z = transform.position.z;
        int halfSize = (int)resolution * mapChunkSize / 2;
        float chunkU = (WorldToMapCoord(x) + halfSize) / mapChunkSize;
        float chunkV = (WorldToMapCoord(z) + halfSize) / mapChunkSize;
        
        chunkU -= Mathf.Floor(chunkU);
        chunkV -= Mathf.Floor(chunkV);

        int posU = (int)Mathf.Round(2f * (chunkU - .5f));
        int posV = (int)Mathf.Round(2f * (chunkV - .5f));

        return new Vector2(posU, posV);
    }

    public static void UpdateMapLOD(Transform transform)
    {
        Vector2 mapCoords = GetCoordsOnMap(transform);
        Vector2 chunkCoords = GetCoordsOnChunk(transform);
        int[,] mapLOD = new int[resolution, resolution];
        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                mapLOD[i, j] = Constants.maxLOD;
            }
        }
        mapLOD[(int)mapCoords.x, (int)mapCoords.y] = 0;
        Vector2[] neighbours = { new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, -1), new Vector2(0, 1) };
        Queue<Vector2> cells = new Queue<Vector2>();
        cells.Enqueue(new Vector2(mapCoords.x, mapCoords.y));
        Vector2 newPos = new Vector2();

        newPos.x = mapCoords.x + chunkCoords.x; newPos.y = mapCoords.y;
        if (chunkCoords.x != 0 && IsValidPos(mapLOD, newPos))
        {
            cells.Enqueue(newPos);
            mapLOD[(int)newPos.x, (int)newPos.y] = 0;
        }

        newPos.x = mapCoords.x; newPos.y = mapCoords.y + chunkCoords.y;
        if (chunkCoords.y != 0 && IsValidPos(mapLOD, newPos))
        {
            cells.Enqueue(newPos);
            mapLOD[(int)newPos.x, (int)newPos.y] = 0;
        }

        newPos.x = mapCoords.x + chunkCoords.x; newPos.y = mapCoords.y + chunkCoords.y;
        if (chunkCoords.x != 0 && chunkCoords.y != 0 && IsValidPos(mapLOD, newPos))
        {
            cells.Enqueue(newPos);
            mapLOD[(int)newPos.x, (int)newPos.y] = 0;
        }

        while (cells.Count != 0)
        {
            Vector2 currentCell = cells.Dequeue();
            int newLOD = mapLOD[(int)currentCell.x, (int)currentCell.y] + 1;
            for (int i = 0; i < neighbours.Length; i++)
            {
                newPos = currentCell + neighbours[i];
                if (IsValidPos(mapLOD, newPos))
                {
                    if (mapLOD[(int)newPos.x, (int)newPos.y] > newLOD)
                    {
                        mapLOD[(int)newPos.x, (int)newPos.y] = newLOD;
                        cells.Enqueue(newPos);
                    }
                }
            }
        }

        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                if (mapLOD[i, j] != islandChunks[i, j].ChunkLOD)
                {
                    // set new LOD and update chunk
                    islandChunks[i, j].ChunkLOD = mapLOD[i, j];
                }
            }
        }
    }

    private static bool IsValidPos(int[,] map, Vector2 pos)
    {
        int width = map.GetLength(0), height = map.GetLength(1);
        if ((int)pos.x < 0 || (int)pos.x >= width) return false;
        if ((int)pos.y < 0 || (int)pos.y >= height) return false;
        return true;
    }

    public static void SimplifyIslandChunk(int i, int j, int LOD = 0)
    {
        IslandChunk islandChunk = islandChunks[i, j];
        islandChunk.PlaneObject.SimplifyMesh(LOD);
        List<TreeEntity> trees = islandChunk.Trees;
        foreach (TreeEntity tree in islandChunk.Trees)
        {
            tree.ModifyLODTree(LOD);
        }
    }

    public static void PlacePlayer()
    {
        Transform Player = GameObject.FindWithTag("Player").transform;
        Player.SetParent(island.transform);
        int step = (int)Mathf.Floor(mapChunkSize * 0.1f);
        int minIndex = Mathf.Min(0, resolution - 1), maxIndex = resolution / 2;
        for (int j = minIndex; j < maxIndex; j++)
        {
            for (int i = minIndex; i < maxIndex; i++)
            {
                for (int k = step; k < mapChunkSize - step; k += step)
                {
                    int u = i * mapChunkSize + k, v = j * mapChunkSize + k;
                    if (heightMap[u, v] > minHeight)
                    {
                        Player.transform.position = new Vector3(MapToWorldCoord(u), heightMap[u, v] * meshHeightMultiplier + 1f, MapToWorldCoord(v));
                        Player.SetParent(island.transform);
                        return;
                    }
                }
            }
        }
    }
    private static void InitializeIslandChunks()
    {
        islandChunks = new IslandChunk[resolution, resolution];
        for (int j = 0; j < resolution; j++)
        {
            for (int i = 0; i < resolution; i++)
            {
                islandChunks[i, j] = new IslandChunk();
            }
        }
    }

    private static float EvaluateShore(float x, float y)
    {
        const float a = 0.25f, b = 0.7f;
        const float vMax = 0.4f, vMin = -0.15f;
        float circleValue = Mathf.Clamp(RoundedSquare(x, y, a, b), vMin, vMax);
        return Mathf.Pow((circleValue - vMin) / (vMax - vMin), 2f);
    }

    private static float RoundedSquare(float x, float y, float a, float b)
    {
        float u = Mathf.Pow(Mathf.Max(Mathf.Abs(2 * x - 1) - a, 0f), 2.0f);
        float v = Mathf.Pow(Mathf.Max(Mathf.Abs(2 * y - 1) - a, 0f), 2.0f);
        return b * b - (u + v);
    }

    private static void InitializeIslandObject(Material terrainMaterial, float[,] heightMap, float meshHeightMultiplier)
    {
        island = new GameObject("Island");

        for (int j = 0; j < resolution; j++)
        {
            for (int i = 0; i < resolution; i++)
            {
                GameObject terrainChunk = new GameObject("TerrainChunk" + (j * resolution + i));

                terrainChunk.AddComponent<MeshFilter>();
                terrainChunk.AddComponent<MeshRenderer>();
                terrainChunk.AddComponent<MeshCollider>();
                terrainChunk.GetComponent<MeshCollider>();

                terrainChunk.GetComponent<MeshRenderer>().material = terrainMaterial;

                terrainChunk.transform.SetParent(island.transform);

                terrainChunk.transform.position = new Vector3((i + 0.5f) - resolution / 2, 0.0f, (j + 0.5f) - (resolution + 1) / 2);
                islandChunks[i, j].ChunkObject = terrainChunk;
            }
        }
        islandObjectScale = Constants.islandScale / resolution;
        island.transform.localScale = new Vector3(islandObjectScale, 1.0f, islandObjectScale);
    }

    private static float MapToWorldCoord(float coord) => islandObjectScale * (coord / mapChunkSize - .5f * resolution);
    private static float WorldToMapCoord(float coord) => coord * mapChunkSize / islandObjectScale + .5f * resolution;
    private static void AddTreesToIsland(Vector2[] trees, Material treeMaterial, Material leavesMaterial, float[,] heightMap, float meshHeightMultiplier)
    {
        int width = heightMap.GetLength(0), height = heightMap.GetLength(1);
        for (int ti = 0; ti < trees.Length; ti++)
        {
            int u = (int)MapToWorldCoord(trees[ti].x),
                v = (int)MapToWorldCoord(trees[ti].y);
            int i = (int)Mathf.Floor(trees[ti].x / mapChunkSize), 
                j = (int)Mathf.Floor(trees[ti].y / mapChunkSize);

            TreeEntity tree = new TreeEntity(treeMaterial, leavesMaterial, true);
            islandChunks[i, j].AddTree(tree);
            tree.TreeObject.transform.position = new Vector3(u, heightMap[(int)trees[ti].x, (int)trees[ti].y] * meshHeightMultiplier, v);
            tree.TreeObject.transform.SetParent(islandChunks[i, j].ChunkObject.transform);
        }
    }

    private static Vector3 FromHeightMapToWorldCoord(float px, float height, float pz)
        => new Vector3((px - 0.5f) * resolution * mapChunkSize, height, (pz - 0.5f) * resolution * mapChunkSize);

    private static void UpdateChunkMeshes()
    {
        for (int j = 0; j < resolution; j++)
        {
            for (int i = 0; i < resolution; i++)
            {
                GameObject terrainChunk = islandChunks[i, j].ChunkObject;
                Mesh terrainChunkMesh = terrainChunk.GetComponent<MeshFilter>().sharedMesh;
                terrainChunkMesh = islandChunks[i, j].PlaneObject.GetMesh();
                terrainChunkMesh.normals = islandChunks[i, j].PlaneObject.GetMesh().normals;
                terrainChunkMesh.RecalculateBounds();
                terrainChunk.GetComponent<MeshFilter>().sharedMesh = terrainChunkMesh;
                terrainChunk.GetComponent<MeshCollider>().sharedMesh = terrainChunkMesh;
            }
        }
    }
}