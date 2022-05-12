using UnityEngine;
using TMPro;

public class InfoBoardHandler : MonoBehaviour
{
    [SerializeField]
    string infoText;

    [SerializeReference]
    GameObject infoPanel;
    private TextMeshProUGUI infoTextObject;

    // Start is called before the first frame update
    void Start()
    {
        infoTextObject = infoPanel.GetComponentInChildren<TextMeshProUGUI>(true);
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Player"))
        {
            infoTextObject.SetText(infoText);
            infoPanel.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.gameObject.CompareTag("Player"))
        {
            infoPanel.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
