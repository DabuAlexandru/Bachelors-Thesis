using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor (typeof (TreeGenerator))]
public class TreeGeneratorEditor : Editor 
{
    public override void OnInspectorGUI() {
		TreeGenerator mapGen = (TreeGenerator)target;

		if (DrawDefaultInspector ()) {
            // mapGen.ChangeLOD ();
		}

		if (GUILayout.Button ("Generate")) {
			mapGen.ChangeLOD ();
		}
	}
}

public class TreeGenerator : MonoBehaviour 
{
    [SerializeReference]
    private Material treeMaterial;
    TreeEntity tree;

    [SerializeField]
    [Range(0, 3)]
    private int LOD = 0;

    public void Start()
    {
        Random.InitState(3);
        tree = new TreeEntity(treeMaterial);
    }

    public void ChangeLOD() 
    {
        Random.InitState(3);
        if(tree == null)
        {
            tree = new TreeEntity(treeMaterial);
        }
        for(int i = 0; i < 1; i++) 
        {
            tree.TreeObject.transform.position = new Vector3(7 * i % 35, 7 * Mathf.Floor(i / 5), 0f);
            tree.ModifyLODTree(LOD);
        }
    }
}