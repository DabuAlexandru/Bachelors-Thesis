using UnityEngine;

public struct BranchGraphNode
{
    public Vector3 growthDirection; // the directional vector of growth
    public Vector3 budPosition; // the node of origin of the branch
    public Vector3 bezierControlPoint;
    public float length;

    public BranchGraphNode(Vector3 growthDirection, Vector3 budPosition, Vector3 bezierControlPoint, float length)
    {
        this.growthDirection = growthDirection;
        this.budPosition = budPosition;
        this.bezierControlPoint = bezierControlPoint;
        this.length = length;
    }

    /** Get a point on the branch by the percentage of the distance from the bud **/
    public Vector3 GetPointLinear(float p = 1.0f) => (budPosition + growthDirection * length * p);
    public Vector3 GetPointBezier(float t = 1.0f) => Utils.GetPointOnQuadraticBezierCurve(budPosition, bezierControlPoint, GetPointLinear(), t);
}

public static class GraphGenerator
{
    private const float branchSeparationAngle = 40.0f; // the angle of separation from the main branch
    private const float rotationVariationAngle = 30.0f; // offset on the rotation angle to have some variation
    private const float bezierHeight = 0.2f;
    private const float bezierPointVariation = 0.2f;
    private const float budOriginPercent = 0.75f;
    private const float lengthReductionRate = 0.825f;

    private const float elevationY = -0.15f;

    public static BranchGraphNode[] GenerateBranchGraph(int maxGrowthStep)
    {
        // every branch spurs two new branches, one extension and one new bud, so we have a binary tree
        BranchGraphNode[] branches = new BranchGraphNode[(int)Mathf.Pow(2.0f, maxGrowthStep + 1) - 1];
        branches[0] = new BranchGraphNode(Vector3.up, Vector3.zero + elevationY * Vector3.up, Vector3.zero, 1.0f);
        branches[0].bezierControlPoint = CalculateBezierControlPoint(branches[0]);
        // we want to generate for each branch two new branches, and each step the number of branches gets doubled
        for(int i = 0, pw = 1; i < maxGrowthStep; i++, pw *= 2)
        {
            for(int j = pw - 1; j <= 2 * (pw - 1); j++)
            {
                // the left leaf of the binary tree is represented by the bud on the side
                branches[2 * j + 1] = GenerateBranchGraphNode(branches[j], false);
                // the right leaf is the extension of the current branch
                branches[2 * j + 2] = GenerateBranchGraphNode(branches[j], true);
            }
        }
        return branches;
    }

    private static Vector3 CalculateBezierControlPoint(BranchGraphNode graphNode)
    {
        float p = Random.Range(0.5f - bezierPointVariation, 0.5f + bezierPointVariation);
        Vector3 chosenPoint = graphNode.GetPointLinear(p);
        float variationY = Random.Range(0.1f, 360.0f);
        Vector3 localTranslatedPoint = Quaternion.Euler(0.0f, variationY, 0.0f) * (Vector3.left * bezierHeight);
        Vector3 localRotatedPoint = Quaternion.FromToRotation(Vector3.up, graphNode.growthDirection) * localTranslatedPoint;
        return localRotatedPoint + chosenPoint;
    }

    private static Quaternion GetRotationVariation()
    {
        float variationX = Random.Range(-rotationVariationAngle / 2.0f, rotationVariationAngle / 2.0f);
        float variationZ = Random.Range(-rotationVariationAngle / 2.0f, rotationVariationAngle / 2.0f);
        return Quaternion.Euler(variationX, 0.0f, variationZ);
    }

    static Vector3 ApplyRotationVariation(Vector3 direction) 
        => Quaternion.FromToRotation(Vector3.one, direction) * GetRotationVariation() * Vector3.one;

    static BranchGraphNode GenerateBranchGraphNode(BranchGraphNode parent, bool expansion)
    {
        BranchGraphNode newNode = new BranchGraphNode();
        if(expansion)
        {
            newNode.budPosition = parent.GetPointLinear();
            newNode.growthDirection = ApplyRotationVariation(parent.growthDirection);
        }
        else
        {
            newNode.budPosition = parent.GetPointLinear(budOriginPercent);
            float variationY = Random.Range(0.001f, 360.0f);
            newNode.growthDirection =
                Quaternion.FromToRotation(Vector3.up, parent.growthDirection) *
                Quaternion.Euler(branchSeparationAngle, variationY, 0.0f) *
                ApplyRotationVariation(Vector3.up);
        }
        newNode.length = parent.length * lengthReductionRate;
        newNode.bezierControlPoint = CalculateBezierControlPoint(newNode);
        return newNode;
    }
}