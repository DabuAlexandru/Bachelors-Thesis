using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameHandlerIsland : MonoBehaviour 
{
    [SerializeReference] TextMeshPro textmeshPro;
    private Vector2 playerMapCoords;
    private Vector2 playerChunkCoords;
    private Transform Player;
    private bool changeLOD;

    private void Start()
    {
        Player = GameObject.FindWithTag("Player").transform;
        IslandGenerator.PlacePlayer();
        playerMapCoords = new Vector2(-1f, -1f);
        playerChunkCoords = new Vector2(-1f, -1f);
        IslandGenerator.UpdateMapLOD(Player);
        changeLOD = false;
    }

    private void Update()
    {
        if(IslandGenerator.GetCoordsOnMap(Player) != playerMapCoords)
        {
            playerMapCoords = IslandGenerator.GetCoordsOnMap(Player);
            changeLOD = true;
        }

        if(IslandGenerator.GetCoordsOnChunk(Player) != playerChunkCoords)
        {
            playerChunkCoords = IslandGenerator.GetCoordsOnChunk(Player);
            changeLOD = true;
        }

        if(changeLOD)
        {
            IslandGenerator.UpdateMapLOD(Player);
            changeLOD = false;
        }
    }
}