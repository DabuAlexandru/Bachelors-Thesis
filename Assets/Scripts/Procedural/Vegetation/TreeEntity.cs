using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

public struct BranchMesh
{
    public Vector3[] vertices;
    public int[] indices;
}

public class TreeEntity
{
    const int resolutionU = 12;
    const int resolutionV = 12;

    private GameObject treeObject;
    public GameObject TreeObject { get => treeObject; }

    BranchGraphNode[] graph;
    Material treeMaterial;
    const int graphComplexity = 5;

    public TreeEntity(Material treeMaterial)
    {
        graph = GraphGenerator.GenerateBranchGraph(graphComplexity);
        this.treeMaterial = treeMaterial;
        InitializeTreeObject();
    }

    private void InitializeTreeObject()
    {
        int n = graph.Length;
		int rootBranchIndex = 1; // the starting point of a branch formed by multiple segments
        BranchMesh branch = GetMainBranchMesh(rootBranchIndex, 0.2f);
        int rootBranchCount = (int)Mathf.Pow(2, graphComplexity - 1);
        float baseRadius = graphComplexity * 0.035f;

        treeObject = new GameObject("Tree");
        treeObject.AddComponent<CapsuleCollider>();
        CapsuleCollider treeCollider = treeObject.GetComponent<CapsuleCollider>();

        const float approximatedSegmentHeight = 0.9f;
        const float radiusPercentOffset = 0.05f;

        treeCollider.radius = baseRadius * (1f + radiusPercentOffset);
        treeCollider.height = graphComplexity * approximatedSegmentHeight;
        treeCollider.center = Vector3.up * (treeCollider.height / 2.0f);

        CreateBranch(0, baseRadius, treeObject.transform);
        // const float min
        for(int i = 0; i < rootBranchCount; i++)
        {
            int branchId = 2 * i + 1;
            float branchBaseRadius = baseRadius * (0.01f + 0.65f * (1.0f - ((float)i / (rootBranchCount + 1))));
            CreateBranch(branchId, branchBaseRadius, treeObject.transform);
        }
    }

    private void CreateBranch(int rootBranchIndex, float baseRadius, Transform parent)
    {
        BranchMesh branch = GetMainBranchMesh(rootBranchIndex, baseRadius);

        GameObject newBranch = new GameObject("Branch" + rootBranchIndex);
        newBranch.AddComponent<MeshFilter>();
        newBranch.AddComponent<MeshRenderer>();

        newBranch.GetComponent<MeshRenderer>().material = treeMaterial;

        newBranch.transform.SetParent(parent);

        Mesh branchMesh = newBranch.GetComponent<MeshFilter>().sharedMesh;
        branchMesh = new Mesh();
        branchMesh.vertices = branch.vertices;
        branchMesh.triangles = branch.indices;
    
        CalculateUVs(branchMesh);
        RecalculateNormals(branchMesh);
        // branchMesh.RecalculateNormals();

        newBranch.GetComponent<MeshFilter>().sharedMesh = branchMesh;
    }

    private void RecalculateNormals(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = new Vector3[vertices.Length];
        int[] triangles = mesh.triangles;

        for(int i = 0; i < triangles.Length; i += 3)
        {
            // ti - vertex index
            int vi1 = triangles[i], vi2 = triangles[i + 1], vi3 = triangles[i + 2];
            Vector3 vertexA = vertices[vi1];
            Vector3 vertexB = vertices[vi2];
            Vector3 vertexC = vertices[vi3];

            Vector3 triangleNormal = CalculateSurfaceNormal(vertexA, vertexB, vertexC);

            IncrementNormals(normals, vi1, triangleNormal);
            IncrementNormals(normals, vi2, triangleNormal);
            IncrementNormals(normals, vi3, triangleNormal);
        }

        for(int i = 0; i < normals.Length; i++)
        {
            normals[i].Normalize();
        }

        mesh.normals = normals;
    }

    private void IncrementNormals(Vector3[] normals, int index, Vector3 triangleNormal)
    {
        int modulo = index % (resolutionU + 1);
        if(modulo == 0)
        {
            normals[index] += triangleNormal;
            normals[index + resolutionU] += triangleNormal;
        }
        else if(modulo == resolutionU)
        {
            normals[index - resolutionU] += triangleNormal;
            normals[index] += triangleNormal;
        }
        else
        {
            normals[index] += triangleNormal;
        }
    }

    private static Vector3 CalculateSurfaceNormal(Vector3 vertex1, Vector3 vertex2, Vector3 vertex3)
    {
        Vector3 edge12 = vertex2 - vertex1;
        Vector3 edge13 = vertex3 - vertex1;
        return Vector3.Cross(edge12, edge13).normalized;
    }

    private static void CalculateUVs(Mesh mesh)
    {
        int numOfVertices = mesh.vertices.Length;
        Vector2[] uvs = new Vector2[numOfVertices];
        int index = 0;
        for(int v = 0; v < (int)Mathf.Floor(numOfVertices / (resolutionU + 1)); v++)
        {
            for(int u = 0; u <= resolutionU; u++)
            {
                uvs[index] = new Vector2(u / (float)resolutionU, v / (float)resolutionV);
                index++;
            }
        }

        mesh.uv = uvs;
    }

    private Vector3 ProjectPointFromLocalToObject(BranchGraphNode baseInfo, Vector3 localPosition, int v)
    {
        Vector3 chosenPoint = baseInfo.GetPointLinear((float)v / resolutionV);
        Vector3 pointAtBase = new Vector3(localPosition.x, 0.0f, localPosition.z);
        Vector3 localRotatedPoint = Quaternion.FromToRotation(Vector3.up, baseInfo.growthDirection) * pointAtBase;
        return localRotatedPoint + chosenPoint;
    }

    BranchMesh GetMainBranchMesh(int rootBranchIndex, float baseRadius)
    {
        int branchSegments = graphComplexity + 1 - (int)Mathf.Floor(Mathf.Log(rootBranchIndex + 1, 2));
        int budIndex = rootBranchIndex;
        BranchMesh branchMesh = new BranchMesh();
        BranchMesh branchConfig = GetBranchMesh();

        int composedResolutionV = branchSegments * resolutionV;
        branchMesh.vertices = new Vector3[(resolutionU + 1) * (composedResolutionV + 1)];
        branchMesh.indices = new int[6 * resolutionU * composedResolutionV];
        int mainVi = 0, mainTi = 0;
        Vector3 translateBranch = Vector3.zero;
        int branchRing = 0;
        for(int i = 0; i < branchSegments; i++)
        {
            translateBranch = graph[budIndex].budPosition;
            int vi = 0, ti = 0;
            int indicesOffset = (Mathf.Min(1, i) * ((resolutionU + 1) * (i * resolutionV)));
            for (int v = 0; v <= resolutionV; v++)
            {
                if(v != 0 || i == 0)
                    branchRing = (int)Mathf.Floor(mainVi / (resolutionU + 1));
                float radius = (1.0f - (float)branchRing / (composedResolutionV + 1)) * baseRadius;
                for (int u = 0; u <= resolutionU; u++)
                {
                    if(v != 0 || i == 0) // we don't want to include the first ring from each new branch (we wield them together)
                    {
                        Vector3 branchVertex = radius * branchConfig.vertices[vi];
                        branchMesh.vertices[mainVi] = ProjectPointFromLocalToObject(graph[budIndex], branchVertex, v);
                        mainVi++; 
                    }
                    vi++;

                    if (v < resolutionV && u < resolutionU)
                    {
                        for(int k = 0; k < 6; k++)
                        {
                            branchMesh.indices[mainTi] = branchConfig.indices[ti] + indicesOffset;
                            mainTi++; ti++;
                        }
                    }
                }
            }
            budIndex = 2 * budIndex + 2;
            
        }
        return branchMesh;
    }

    BranchMesh GetBranchMesh()
    {
        BranchMesh branchMesh = new BranchMesh();
        branchMesh.vertices = new Vector3[(resolutionU + 1) * (resolutionV + 1)];
        branchMesh.indices = new int[6 * resolutionU * resolutionV];

        int vi = 0, ti = 0;
        for (int v = 0; v <= resolutionV; v++)
        {
            for (int u = 0; u <= resolutionU; u++)
            {
                Vector3 position = new Vector3();
                position.x = cos(2 * PI * u / resolutionU);
                position.y = (float)v / resolutionV;
                position.z = sin(2 * PI * u / resolutionU);

                branchMesh.vertices[vi] = position;
                vi++;

                if (v < resolutionV && u < resolutionU)
                {
                    int currentIndex = v * (resolutionU + 1) + u;
                    int rightIndex = v * (resolutionU + 1) + (u + 1);
                    int topRightIndex = (v + 1) * (resolutionU + 1) + (u + 1);
                    int topIndex = (v + 1) * (resolutionU + 1) + u;

                    branchMesh.indices[ti] = currentIndex;
                    branchMesh.indices[ti + 1] = topRightIndex;
                    branchMesh.indices[ti + 2] = rightIndex;
                    ti += 3;

                    branchMesh.indices[ti] = currentIndex;
                    branchMesh.indices[ti + 1] = topIndex;
                    branchMesh.indices[ti + 2] = topRightIndex;
                    ti += 3;
                }
            }
        }

        return branchMesh;
    }
}