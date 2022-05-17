using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Puzzle : MonoBehaviour
{
    const int resolutionU = Constants.puzzleResolutionU;
    const int resolutionV = Constants.puzzleResolutionV;
    [System.Flags]
    public enum GizmoMode { Nothing = 0, Vertices = 1, Normals = 0b10, Tangents = 0b100 }

    [SerializeField]
    int columnID;

    public int getColumnID()
    {
        return columnID;
    }
    float[] ringRadiusPercentages = new float[resolutionV + 1];
    float[] intendedConfiguration = new float[resolutionV + 1];

    [SerializeField]
    private GizmoMode gizmos;
    private SingleStreamCylindricalProceduralMesh puzzleColumn;
    private Mesh columnMesh;

    // the chosen z coordinate to put the mouse on (in order for the camera to track it)
    private float chosenZCoordinate;
    // the ring hovered over by the mouse
    int ringLevel = -1;
    const float minRadiusPercentage = 0.5f, maxRadiusPercentage = 1.5f;
    const float radiusModifyRate = 0.5f;
    const int numberOfAffectedNeighbourPairs = 4;

    [SerializeField, Range(80.0f, 99.0f)]
    float minValidScore = 95.0f;

    [SerializeField]
    bool isPreview = false;

    [SerializeField]
    bool isEditable = false;

    [SerializeField, Range(5, 50)]
    int steps = 5;

    private const float minIntensity = 0.5f;

    [SerializeField, Range(minIntensity, 7.0f)]
    float maxIntesity = 5.0f;

    private Vector3[] verticesInitialPos;
    public Button submitButton;
    public Button resetButton;
    public TMP_Text similarityText;

    void Update()
    {
        if (ringLevel > -1 && isEditable)
        {
            UpdateSimilarityText();
        }
    }

    public bool IsPuzzleSolved() => (GetSimilarity() >= minValidScore);

    float GetSimilarity()
    {
        float difference = PuzzleDataUtils.GetDifference(ringRadiusPercentages, intendedConfiguration);
        float maxDifferencePerRing = maxRadiusPercentage - minRadiusPercentage;
        float totalPossibleDiference = maxDifferencePerRing * resolutionV;
        return 100.0f * (1.0f - (difference / totalPossibleDiference));
    }

    void UpdateSimilarityText()
    {
        string similarity = GetSimilarity().ToString("0.00");
        similarityText.SetText("Similarity: " + similarity + "%");
    }

    void OnEnable()
    {
        puzzleColumn = new SingleStreamCylindricalProceduralMesh(GetComponent<MeshFilter>());
        PuzzleDataUtils.InitializeRingConfig(ringRadiusPercentages);
        PuzzleDataUtils.InitializeRingConfig(intendedConfiguration);
        if(isEditable || isPreview)
        {
            columnID = PlayerPrefs.GetInt("PuzzleID");
        }
        if (isEditable)
        {
            PuzzleDataUtils.InitializeRingConfig(ringRadiusPercentages);
            InitializePuzzleData(columnID);
            UpdateSimilarityText();
            SetButtonsEvents();
        }
        if(SavePuzzleData.instance.puzzleCollection.ContainsKey(columnID))
        {
            PuzzleData puzzleData = SavePuzzleData.instance.puzzleCollection.GetPuzzle(columnID);
            PuzzleDataUtils.CopyContents(puzzleData.GetRingRadiusPercentages(), ringRadiusPercentages);
            PuzzleDataUtils.CopyContents(puzzleData.GetIntendedConfiguration(), intendedConfiguration);
        }
    }

    void OnDrawGizmos()
    {
        if (gizmos == GizmoMode.Nothing || columnMesh == null)
        {
            return;
        }

        bool drawVertices = (gizmos & GizmoMode.Vertices) != 0;
        bool drawNormals = (gizmos & GizmoMode.Normals) != 0;
        bool drawTangents = (gizmos & GizmoMode.Tangents) != 0;

        Transform t = transform;
        for (int i = 0; i < columnMesh.vertices.Length; i++)
        {
            Vector3 position = t.TransformPoint(columnMesh.vertices[i]);
            if (drawVertices)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(position, 0.02f);
            }
            if (drawNormals)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(position, t.TransformDirection(columnMesh.normals[i]) * 0.2f);
            }
            if (drawTangents)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(position, t.TransformDirection(columnMesh.normals[i]) * 0.2f);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        columnMesh = puzzleColumn.GetMeshFilter().mesh;
        verticesInitialPos = columnMesh.vertices;
        if(SavePuzzleData.instance.puzzleCollection.ContainsKey(columnID))
        {
            PuzzleData puzzleData = SavePuzzleData.instance.puzzleCollection.GetPuzzle(columnID);
            PuzzleDataUtils.CopyContents(puzzleData.GetRingRadiusPercentages(), ringRadiusPercentages);
            PuzzleDataUtils.CopyContents(puzzleData.GetIntendedConfiguration(), intendedConfiguration);
            if (isPreview)
            {
                PuzzleDataUtils.CopyContents(puzzleData.GetIntendedConfiguration(), ringRadiusPercentages);
            }
        }
        RefreshMesh();
    }

    void InitializePuzzleData(int puzzleID)
    {
        if (!SavePuzzleData.instance.puzzleCollection.ContainsKey(puzzleID))
        {
            PuzzleData puzzleData = new PuzzleData();
            puzzleData.SetPuzzleID(puzzleID);
            PuzzleDataUtils.InitializeRingConfig(intendedConfiguration);
            SetRandomConfiguration(intendedConfiguration);
            puzzleData.SetIntendedConfiguration(intendedConfiguration);
            SavePuzzleData.instance.puzzleCollection.AddOrEditPuzzle(puzzleData);
        }
        else
        {
            PuzzleData puzzleData = SavePuzzleData.instance.puzzleCollection.GetPuzzle(puzzleID);
            PuzzleDataUtils.CopyContents(puzzleData.GetRingRadiusPercentages(), ringRadiusPercentages);
            PuzzleDataUtils.CopyContents(puzzleData.GetIntendedConfiguration(), intendedConfiguration);
        }
    }

    void SetButtonsEvents()
    {
        // initialize buttons' events
        if (!(submitButton is null))
        {
            submitButton.onClick.AddListener((UnityEngine.Events.UnityAction)SubmitPuzzle);
        }
        if (!(resetButton is null))
        {
            resetButton.onClick.AddListener((UnityEngine.Events.UnityAction)ResetPuzzle);
        }
    }

    void SetRadiusPercentages(float[] ringRadiusPercentages)
    {
        int size = Mathf.Min(ringRadiusPercentages.Length, resolutionV + 1);
        for (int i = 0; i < size; i++)
        {
            this.ringRadiusPercentages[i] = ringRadiusPercentages[i];
        }
    }

    void RefreshMesh(int first = 0, int last = resolutionV)
    {
        first = Mathf.Clamp(first, 0, resolutionV);
        last = Mathf.Clamp(last, 0, resolutionV);
        Vector3[] myVertices = columnMesh.vertices;
        for (int ringLevel = first; ringLevel <= last; ringLevel++)
        {
            for (int vi = ringLevel * (resolutionU + 1); vi < (ringLevel + 1) * (resolutionU + 1); vi++)
            {
                myVertices[vi].x = ringRadiusPercentages[ringLevel] * verticesInitialPos[vi].x;
                myVertices[vi].z = ringRadiusPercentages[ringLevel] * verticesInitialPos[vi].z;
            }
        }

        columnMesh.vertices = myVertices;
        columnMesh.RecalculateBounds();
    }

    void ModifyRing(float[] ringConfig, int ringLevel, float value)
        => ringConfig[ringLevel] = Mathf.Clamp(ringConfig[ringLevel] + value, minRadiusPercentage, maxRadiusPercentage);

    void ModifyNRings(float[] ringConfig, float perceivedChange, int ringLevel, bool continious = true)
    {
        int firstRing = Mathf.Max(0, ringLevel - numberOfAffectedNeighbourPairs);
        int lastRing = Mathf.Min(resolutionV, ringLevel + numberOfAffectedNeighbourPairs);
        float modificationRate;
        for (int ring = firstRing; ring <= lastRing; ring++)
        {
            modificationRate = perceivedChange * GetModificationRate(ring, ringLevel);
            if (continious)
                modificationRate *= Time.deltaTime;

            ModifyRing(ringConfig, ring, modificationRate);
        }
    }

    int GetAndSetRingLevel()
    {
        // get the mouse offset relative to the object
        Vector3 mouseOffset = gameObject.transform.position - GetMouseWorldPosition();
        // get the dimensions of the object
        Renderer objectRenderer = GetComponent<Renderer>();
        Vector3 objectSize = objectRenderer.bounds.size;
        // find out which ring is hovered over
        float relativeYFromBottom = Mathf.Clamp((objectSize.y / 2 - mouseOffset.y), 0.0f, (1.0f / resolutionV) + objectSize.y);
        float percentageY = relativeYFromBottom / objectSize.y;
        ringLevel = Mathf.Min((int)Mathf.Round(resolutionV * percentageY), resolutionV - 1);
        return ringLevel;
    }

    void OnMouseUp() => ringLevel = -1;

    void OnMouseDrag() => HandleMeshModify();

    void OnMouseDown() => GetAndSetRingLevel();

    void HandleMeshModify()
    {
        if (ringLevel <= -1 || !isEditable)
            return;

        float perceivedChange = Input.GetAxis("Mouse X") / 2.0f;
        ModifyNRings(ringRadiusPercentages, perceivedChange, ringLevel);
        RefreshMesh();
    }

    void SetRandomConfiguration(float[] ringConfig)
    {
        for (int i = 0; i < steps; i++)
        {
            float rate = Random.Range(minIntensity, maxIntesity) * 0.1f;
            if (Random.Range(0.0f, 1.0f) <= 0.5f)    // we have a decreasing event
                rate *= -1;
            int randomRing = Random.Range(0, resolutionV);
            ModifyNRings(ringConfig, rate, randomRing, false);
        }
    }

    float GetModificationRate(int currentRing, int chosenRing)
    {
        // get the distance to the principal ring (the one that is hovered over)
        int ringDistance = (int)Mathf.Abs(currentRing - chosenRing);
        // calculate the altered modification rate
        return radiusModifyRate * (1.0f - Mathf.Pow(((float)ringDistance / (numberOfAffectedNeighbourPairs + 1)), 2));
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 hoveredPixel = Input.mousePosition; // the mouse position on the screen (the position of the pixel hovered over)
        hoveredPixel.z = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
        return Camera.main.ScreenToWorldPoint(hoveredPixel);
    }

    public void SubmitPuzzle()
    {
        SavePuzzleData.instance.puzzleCollection.AddOrEditPuzzle(new PuzzleData(columnID, ringRadiusPercentages, intendedConfiguration));
        SceneManager.LoadScene("DemoScene");
    }

    public void ResetPuzzle()
    {
        PuzzleDataUtils.InitializeRingConfig(ringRadiusPercentages);
        RefreshMesh();
        UpdateSimilarityText();
        Debug.Log("Reset");
    }
}
