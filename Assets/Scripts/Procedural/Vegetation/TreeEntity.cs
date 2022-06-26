using UnityEngine;
// using Unity.Mathematics;
using static Unity.Mathematics.math;

public class TreeEntity
{
    const int resolutionU = 12;
    const int resolutionV = 12;

    private MeshStruct[] branchMeshes;
    private MeshStruct[] leavesMeshes;

    private bool applyCurves = false;

    private GameObject treeObject;
    public GameObject TreeObject { get => treeObject; }

    BranchGraphNode[] graph;
    Material treeMaterial;
    Material leavesMaterial;

    const int graphComplexity = 4;
    const int maximumLOD = 3;

    public void ModifyLODTree(int LOD = 0)
    {
        LOD = Mathf.Clamp(LOD, 0, maximumLOD);
        for (int i = 0; i < branchMeshes.Length; i++)
        {
            ModifyLODCircularObject(leavesMeshes[i], LOD);
            leavesMeshes[i].mesh.triangles = SimplifyTrianglesForSphere(leavesMeshes[i].mesh.triangles, resolutionU / (LOD + 1), resolutionV / (LOD + 1));
            ModifyLODCircularObject(branchMeshes[i], LOD);
        }
    }

    private void ModifyLODCircularObject(MeshStruct baseMeshStruct, int LOD = 0)
    {
        Mesh objectMesh = baseMeshStruct.mesh;

        int numOfVertices = baseMeshStruct.vertices.Length;

        int resU = resolutionU;
        int resV = numOfVertices / (resolutionU + 1) - 1; // we wielded together more branches
        int vi = 0;

        Vector3[] vertices = new Vector3[(resU + 1) * (resV + 1)];
        Vector3[] normals = new Vector3[(resU + 1) * (resV + 1)];
        Vector2[] uvs = new Vector2[(resU + 1) * (resV + 1)];

        for (int v = 0; v <= resV; v += (LOD + 1))
        {
            for (int u = 0; u <= resU; u += (LOD + 1))
            {
                int globalVi = v * (resU + 1) + u;
                vertices[vi] = baseMeshStruct.vertices[globalVi];
                normals[vi] = baseMeshStruct.normals[globalVi];
                uvs[vi] = baseMeshStruct.uvs[globalVi];
                vi++;
            }
        }
        int[] triangles = Utils.GetTrianglesFromCircularMesh(resU / (LOD + 1), resV / (LOD + 1));

        objectMesh.vertices = vertices;
        objectMesh.normals = normals;
        objectMesh.uv = uvs;
        objectMesh.triangles = triangles;
    }

    public TreeEntity(Material treeMaterial, Material leavesMaterial, bool applyCurves = false)
    {
        graph = GraphGenerator.GenerateBranchGraph(graphComplexity - 1, applyCurves);
        this.treeMaterial = treeMaterial;
        this.leavesMaterial = leavesMaterial;
        this.applyCurves = applyCurves;
        InitializeTreeObject();
    }

    private void InitializeTreeObject()
    {
        int n = graph.Length;
        // int rootBranchIndex = 1; // the starting point of a branch formed by multiple segments
        int rootBranchCount = (int)Mathf.Pow(2, graphComplexity - 1) - 1;
        float baseRadius = 0.175f;
        float baseRadiusReductionRate = 0.7f;

        treeObject = new GameObject("Tree");
        treeObject.AddComponent<CapsuleCollider>();
        CapsuleCollider treeCollider = treeObject.GetComponent<CapsuleCollider>();

        const float approximatedSegmentHeight = 0.9f;
        const float radiusPercentOffset = 0.05f;

        treeCollider.radius = baseRadius * (1f + radiusPercentOffset);
        treeCollider.height = graphComplexity * approximatedSegmentHeight;
        treeCollider.center = Vector3.up * (treeCollider.height / 2.0f);

        branchMeshes = new MeshStruct[rootBranchCount + 1];
        branchMeshes[0] = CreateBranch(0, baseRadius);

        // treeObject.AddComponent<MeshCollider>();
        // treeObject.GetComponent<MeshCollider>().sharedMesh = branchMeshes[0].mesh;
        // const float min
        for (int i = 0; i < rootBranchCount; i++)
        {
            int branchId = 2 * i + 1;
            float branchBaseRadius = baseRadius * Mathf.Pow(baseRadiusReductionRate, Mathf.Log(branchId, 2) + 1);

            branchMeshes[i + 1] = CreateBranch(branchId, branchBaseRadius);
        }

        int firstBranchIndex = (int)Mathf.Pow(2f, graphComplexity - 1) - 1;
        int lastBranchIndex = 2 * firstBranchIndex;

        leavesMeshes = new MeshStruct[lastBranchIndex - firstBranchIndex + 1];
        for (int i = firstBranchIndex; i <= lastBranchIndex; i++)
        {
            leavesMeshes[i - firstBranchIndex] = CreateLeaves(i);
        }
    }

    private MeshStruct CreateBranch(int rootBranchIndex, float baseRadius)
    {
        MeshStruct branch = GetMainBranchMesh(rootBranchIndex, baseRadius);

        GameObject newBranch = new GameObject("Branch" + (int)(1 + (rootBranchIndex - 1) / 2));
        branch = InitializeCircularMeshShape(newBranch, treeMaterial, branch, Vector3.zero, Vector3.one, Vector3.zero);

        return branch;
    }

    private MeshStruct CreateLeaves(int index)
    {
        MeshStruct leaves = GetLeavesMesh();
        Vector3 reposition = applyCurves ? graph[index].GetPointBezier(0.9f) : graph[index].GetPointLinear(0.9f);
        Vector3 rescale = new Vector3(1f, 0.875f, 1f) * 0.24f * GetBranchLengthByIndex(index);
        Vector3 rotate = new Vector3(Random.Range(0.01f, 360.0f), Random.Range(0.01f, 360.0f), Random.Range(0.01f, 360.0f));

        GameObject newLeaves = new GameObject("Leaf" + index);
        leaves = InitializeCircularMeshShape(newLeaves, leavesMaterial, leaves, reposition, rescale, rotate);
        leaves.mesh.triangles = SimplifyTrianglesForSphere(leaves.mesh.triangles);
        return leaves;
    }

    private int[] SimplifyTrianglesForSphere(int[] triangles, int resU = resolutionU, int resV = resolutionV)
    {
        int[] newTriangleList = new int[triangles.Length - 2 * 3 * resU];
        int ti = 0;
        for (int v = 0; v < resV; v++)
        {
            for (int i = 6 * resU * v; i < 6 * resU * (v + 1); i += 3)
            {
                int a = triangles[i], b = triangles[i + 1], c = triangles[i + 2];
                if (v == 0)
                {
                    if (a <= resU) a = 0;
                    if (b <= resU) b = 0;
                    if (c <= resU) c = 0;
                }
                else if (v == resV - 1)
                {
                    if (a >= (resU + 1) * resV) a = (resU + 1) * (resV + 1) - 1;
                    if (b >= (resU + 1) * resV) b = (resU + 1) * (resV + 1) - 1;
                    if (c >= (resU + 1) * resV) c = (resU + 1) * (resV + 1) - 1;
                }
                if (a == b || b == c || a == c)
                    continue;
                newTriangleList[ti] = a;
                newTriangleList[ti + 1] = b;
                newTriangleList[ti + 2] = c;
                ti += 3;
            }
        }
        return newTriangleList;
    }

    private int GetBranchLengthByIndex(int branchIndex)
    {
        int length = 1;
        while (branchIndex % 2 == 0 && branchIndex != 0)
        {
            branchIndex = branchIndex / 2 - 1;
            length++;
        }
        return length;
    }

    private MeshStruct InitializeCircularMeshShape(GameObject gameObject, Material material, MeshStruct baseMeshStruct, Vector3 reposition, Vector3 rescale, Vector3 rotate)
    {
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();

        gameObject.GetComponent<MeshRenderer>().material = material;

        gameObject.transform.SetParent(treeObject.transform);
        gameObject.transform.position = reposition;
        gameObject.transform.localScale = rescale;
        gameObject.transform.Rotate(rotate, Space.Self);

        Mesh objectMesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
        objectMesh = new Mesh();
        objectMesh.vertices = baseMeshStruct.vertices;
        objectMesh.triangles = baseMeshStruct.triangles;

        CalculateUVs(objectMesh);
        RecalculateNormals(objectMesh);

        baseMeshStruct.mesh = objectMesh;
        baseMeshStruct.normals = objectMesh.normals;
        baseMeshStruct.uvs = objectMesh.uv;

        gameObject.GetComponent<MeshFilter>().sharedMesh = objectMesh;

        return baseMeshStruct;
    }

    private void RecalculateNormals(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = new Vector3[vertices.Length];
        int[] triangles = mesh.triangles;

        for (int i = 0; i < triangles.Length; i += 3)
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

        for (int i = 0; i < normals.Length; i++)
        {
            normals[i].Normalize();
        }

        mesh.normals = normals;
    }

    private void IncrementNormals(Vector3[] normals, int index, Vector3 triangleNormal)
    {
        int modulo = index % (resolutionU + 1);
        if (modulo == 0)
        {
            normals[index] += triangleNormal;
            normals[index + resolutionU] += triangleNormal;
        }
        else if (modulo == resolutionU)
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
        for (int v = 0; v < (int)Mathf.Floor(numOfVertices / (resolutionU + 1)); v++)
        {
            for (int u = 0; u <= resolutionU; u++)
            {
                uvs[index] = new Vector2(u / (float)resolutionU, v / (float)resolutionV);
                index++;
            }
        }

        mesh.uv = uvs;
    }

    private Vector3 ProjectPointFromLocalToObject(BranchGraphNode baseInfo, Vector3 localPosition, int v)
    {
        Vector3 chosenPoint = applyCurves ? baseInfo.GetPointBezier((float)v / resolutionV) : baseInfo.GetPointLinear((float)v / resolutionV);
        Vector3 pointAtBase = new Vector3(localPosition.x, 0.0f, localPosition.z);
        Vector3 localRotatedPoint = Quaternion.FromToRotation(Vector3.up, baseInfo.growthDirection) * pointAtBase;
        return localRotatedPoint + chosenPoint;
    }

    MeshStruct GetMainBranchMesh(int rootBranchIndex, float baseRadius)
    {
        int branchSegments = graphComplexity - (int)Mathf.Floor(Mathf.Log(rootBranchIndex + 1, 2));
        int budIndex = rootBranchIndex;
        MeshStruct branchMesh = new MeshStruct();
        MeshStruct branchConfig = GetBranchMesh();

        int composedResolutionV = branchSegments * resolutionV;
        branchMesh.vertices = new Vector3[(resolutionU + 1) * (composedResolutionV + 1)];
        branchMesh.triangles = new int[6 * resolutionU * composedResolutionV];
        int mainVi = 0, mainTi = 0;
        Vector3 translateBranch = Vector3.zero;
        int branchRing = 0;
        for (int i = 0; i < branchSegments; i++)
        {
            translateBranch = graph[budIndex].budPosition;
            int vi = 0, ti = 0;
            int indicesOffset = (Mathf.Min(1, i) * ((resolutionU + 1) * (i * resolutionV)));
            for (int v = 0; v <= resolutionV; v++)
            {
                if (v != 0 || i == 0)
                    branchRing = (int)Mathf.Floor(mainVi / (resolutionU + 1));
                float radius = (1.0f - (float)branchRing / (composedResolutionV + 1)) * baseRadius;
                for (int u = 0; u <= resolutionU; u++)
                {
                    if (v != 0 || i == 0) // we don't want to include the first ring from each new branch (we wield them together)
                    {
                        Vector3 branchVertex = radius * branchConfig.vertices[vi];
                        branchMesh.vertices[mainVi] = ProjectPointFromLocalToObject(graph[budIndex], branchVertex, v);
                        mainVi++;
                    }
                    vi++;

                    if (v < resolutionV && u < resolutionU)
                    {
                        for (int k = 0; k < 6; k++)
                        {
                            branchMesh.triangles[mainTi] = branchConfig.triangles[ti] + indicesOffset;
                            mainTi++; ti++;
                        }
                    }
                }
            }
            budIndex = 2 * budIndex + 2;

        }
        return branchMesh;
    }

    MeshStruct GetBranchMesh()
    {
        MeshStruct branchMesh = new MeshStruct();
        branchMesh.vertices = new Vector3[(resolutionU + 1) * (resolutionV + 1)];
        branchMesh.triangles = new int[6 * resolutionU * resolutionV];

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
                    Utils.CalculateTriangles(branchMesh.triangles, ti, u, v, resolutionU);
                    ti += 6;
                }
            }
        }

        return branchMesh;
    }

    MeshStruct GetLeavesMesh()
    {
        MeshStruct leavesMesh = new MeshStruct();
        leavesMesh.vertices = new Vector3[(resolutionU + 1) * (resolutionV + 1)];
        leavesMesh.triangles = new int[6 * resolutionU * resolutionV];

        int vi = 0, ti = 0;
        for (int v = 0; v <= resolutionV; v++)
        {
            for (int u = 0; u <= resolutionU; u++)
            {
                Vector3 position = new Vector3();
                float circleRadius = sin(PI * v / resolutionV);
                position.x = sin(PI * v / resolutionV) * cos(2 * PI * u / resolutionU);
                position.y = -cos(PI * v / resolutionV);
                position.z = sin(PI * v / resolutionV) * sin(2 * PI * u / resolutionU);

                leavesMesh.vertices[vi] = position;
                vi++;

                if (v < resolutionV && u < resolutionU)
                {
                    Utils.CalculateTriangles(leavesMesh.triangles, ti, u, v, resolutionU);
                    ti += 6;
                }
            }
        }

        return leavesMesh;
    }
}