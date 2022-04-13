using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchPuzzle : MonoBehaviour
{
    [SerializeField]
    GameObject InteractPromt;
    // Start is called before the first frame update
    void Start()
    {
        InteractPromt.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player")) {
            InteractPromt.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.gameObject.CompareTag("Player")) {
            InteractPromt.SetActive(false);
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
