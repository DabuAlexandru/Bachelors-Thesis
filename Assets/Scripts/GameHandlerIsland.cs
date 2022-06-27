using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameHandlerIsland : MonoBehaviour
{
    [SerializeReference] TextMeshProUGUI timerText;
    [SerializeReference] TextMeshProUGUI collectibleText;
    [SerializeReference] GameObject collectible;
    private Vector2 playerMapCoords;
    private Vector2 playerChunkCoords;
    private Transform Player;
    private bool changeLOD;
    private GameObject[] collectibles;
    private const int startValueTimer = 450;
    private const int collectiblesCount = 5;
    private int collectiblesFound;

    private void Start()
    {
        // Player = GameObject.FindWithTag("Player").transform;
        Player = transform;
        IslandGenerator.PlacePlayer();
        playerMapCoords = new Vector2(-1f, -1f);
        playerChunkCoords = new Vector2(-1f, -1f);
        IslandGenerator.UpdateMapLOD(Player);
        changeLOD = false;
        StartCoroutine(Timer(startValueTimer));
        collectibles = new GameObject[collectiblesCount];
        Vector3[] positions = IslandGenerator.GetEmptyPlaces(collectiblesCount);
        GameObject collectiblesParent = new GameObject("Collectibles");
        for (int i = 0; i < collectiblesCount; i++)
        {
            collectibles[i] = GameObject.Instantiate(collectible);
            collectibles[i].transform.position = positions[i];
            collectibles[i].transform.SetParent(collectiblesParent.transform);
        }
        collectiblesFound = 0;
    }

    private string FormatSecondsToString(int timerValue)
    {
        int minutes = timerValue / 60;
        int seconds = timerValue - minutes * 60;
        return minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    private void CollectedEvent()
    {
        collectiblesFound++;
        if (collectiblesFound == collectiblesCount)
            GameWon();
        collectibleText.SetText(collectiblesFound + " / " + collectiblesCount);
    }

    int currentTimerValue;
    private IEnumerator Timer(int timerValue = 10)
    {
        currentTimerValue = timerValue;
        while (currentTimerValue > 0)
        {
            timerText.SetText(FormatSecondsToString(currentTimerValue));
            yield return new WaitForSeconds(1.0f);
            currentTimerValue--;
        }
        GameOver();
    }

    private void GameWon()
    {
        Debug.Log("GameWon");
        //
    }

    private void GameOver()
    {
        Debug.Log("GameOver");
        //
    }

    private void Update()
    {
        if (IslandGenerator.GetCoordsOnMap(Player) != playerMapCoords)
        {
            playerMapCoords = IslandGenerator.GetCoordsOnMap(Player);
            changeLOD = true;
        }

        if (IslandGenerator.GetCoordsOnChunk(Player) != playerChunkCoords)
        {
            playerChunkCoords = IslandGenerator.GetCoordsOnChunk(Player);
            changeLOD = true;
        }

        if (changeLOD)
        {
            IslandGenerator.UpdateMapLOD(Player);
            changeLOD = false;
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Collectible")
        {
            Destroy(other.gameObject);
            CollectedEvent();
        }
    }
}