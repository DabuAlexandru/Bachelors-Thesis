using UnityEngine;

public class PuzzlesHandler : MonoBehaviour
{
    public Puzzle[] puzzles;
    public bool HasBeenValidated()
    {
        bool isActive = true;
        foreach (Puzzle puzzle in puzzles)
        {
            isActive = isActive && puzzle.IsPuzzleSolved();
        }
        return isActive;
    }
}
