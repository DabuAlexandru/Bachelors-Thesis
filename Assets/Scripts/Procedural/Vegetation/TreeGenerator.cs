using UnityEngine;

public class TreeGenerator : MonoBehaviour 
{
    [SerializeReference]
    private Material treeMaterial;

    private void Start() 
    {
        for(int i = 0; i < 1; i++) 
        {
            GameObject tree = new TreeEntity(treeMaterial).TreeObject;
            tree.transform.position = new Vector3(7 * i % 35, 7 * Mathf.Floor(i / 5), 0f);
        }
    }
}