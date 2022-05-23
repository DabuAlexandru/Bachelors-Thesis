using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

public struct BranchMesh
{
    public Vector3[] vertices;
    public int[] indices;
}

public class TreeGenerator : MonoBehaviour 
{
    const int resolutionU = 12;
    const int resolutionV = 12;

    BranchGraphNode[] graph;
    const int graphComplexity = 3;

    [SerializeReference]
    Material treeMaterial;

    private void OnEnable() => graph = GraphGenerator.GenerateBranchGraph(graphComplexity);

    private void Start()
    {
        int n = graph.Length;
		int rootBranchIndex = 1; // the starting point of a branch formed by multiple segments
        BranchMesh branch = GetMainBranchMesh(rootBranchIndex, 0.2f);
        int rootBranchCount = (int)Mathf.Pow(2, graphComplexity - 1);
        float baseRadius = graphComplexity * 0.035f;

        GameObject tree = new GameObject("Tree");
        tree.AddComponent<CapsuleCollider>();
        CapsuleCollider treeCollider = tree.GetComponent<CapsuleCollider>();
        treeCollider.radius = baseRadius * 1.05f;
        treeCollider.height = graphComplexity * 0.9f;
        treeCollider.center = Vector3.up * (treeCollider.height / 2.0f);
        tree.transform.SetParent(this.transform);

        CreateBranch(0, baseRadius, tree.transform);
        for(int i = 0; i < rootBranchCount; i++)
        {
            int branchId = 2 * i + 1;
            float branchBaseRadius = baseRadius * (0.01f + 0.8f * (1.0f - ((float)i / (rootBranchCount + 1))));
            CreateBranch(branchId, branchBaseRadius, tree.transform);
        }
    }

    private void CreateBranch(int rootBranchIndex, float baseRadius, Transform parent)
    {
        BranchMesh branch = GetMainBranchMesh(rootBranchIndex, baseRadius);

        GameObject newBranch = new GameObject("Branch" + rootBranchIndex);
        newBranch.AddComponent<MeshFilter>();
        newBranch.AddComponent<MeshRenderer>();

        newBranch.GetComponent<MeshRenderer>().material = treeMaterial;

        Mesh branchMesh = newBranch.GetComponent<MeshFilter>().mesh;
        branchMesh.Clear();

        branchMesh.vertices = branch.vertices;
        branchMesh.triangles = branch.indices;
    
        branchMesh.RecalculateNormals();
        newBranch.transform.SetParent(parent);
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
            float branchLength = graph[budIndex].length;
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