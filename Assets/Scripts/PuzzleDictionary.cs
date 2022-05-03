using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PuzzleDictionary
{
    Dictionary<int, PuzzleData> puzzleDict;

    public PuzzleDictionary()
    {
        puzzleDict = new Dictionary<int, PuzzleData>();
    }

    public PuzzleDictionary(PuzzleDictionary puzzleCollection)
    {
        this.puzzleDict = puzzleCollection.puzzleDict;
    }

    public PuzzleDictionary(Dictionary<int, PuzzleData> puzzleDictionary)
    {
        this.puzzleDict = new Dictionary<int, PuzzleData>(puzzleDictionary);
    }

    public void AddOrEditPuzzle(PuzzleData newPuzzle)
    {
        int puzzleID = newPuzzle.GetPuzzleID();
        if(puzzleDict.ContainsKey(puzzleID))
        {
            puzzleDict[puzzleID] = newPuzzle;
        }
        else
        {
            puzzleDict.Add(puzzleID, newPuzzle);
        }
    }

    public PuzzleData GetPuzzle(int puzzleID)
    {
        if(puzzleDict.ContainsKey(puzzleID))
        {
            return puzzleDict[puzzleID];
        }
        else
        {
            Debug.LogErrorFormat("The requested puzzle of key {0}, doesn't exist!", puzzleID);
            return null;
        }
    }

    public bool ContainsKey(int puzzleID) => puzzleDict.ContainsKey(puzzleID);
}
