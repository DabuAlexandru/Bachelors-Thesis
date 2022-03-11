using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InfoBox : MonoBehaviour
{
    [SerializeField]
    string infoText;

    [SerializeReference]
    TextMeshPro textmeshPro;

    private Transform myCamera;

    // Start is called before the first frame update
    void Start()
    {
        // Set
        myCamera = Camera.main.transform;
        // At start make the object non visible
        // gameObject.SetActive(false);
        // Set the text of the info box
        textmeshPro.SetText(infoText);
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(2 * transform.position - myCamera.position);
    }
}
