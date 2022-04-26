using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PuzzleDictionary
{
    Dictionary<int, PuzzleData> puzzleDict;

    PuzzleDictionary()
    {
        puzzleDict = new Dictionary<int, PuzzleData>();
    }

    PuzzleDictionary(Dictionary<int, PuzzleData> puzzleDictionary)
    {
        this.puzzleDict = new Dictionary<int, PuzzleData>(puzzleDictionary);
    }

    public void AddOrEditPuzzle(PuzzleData newPuzzle)
    {
        if(puzzleDict.ContainsKey(newPuzzle.puzzleID))
        {
            puzzleDict[newPuzzle.puzzleID] = newPuzzle;
        }
        else
        {
            puzzleDict.Add(newPuzzle.puzzleID, newPuzzle);
        }
    }

    public PuzzleData getPuzzle(int puzzleID)
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
}
