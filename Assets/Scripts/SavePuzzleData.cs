using System.Collections.Generic;

[System.Serializable]
public class SavePuzzleData
{
    private static SavePuzzleData _instance;
    public static SavePuzzleData instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = new SavePuzzleData();
            }
            return _instance;
        }
        set
        {
            if(value != null)
            {
                _instance = value;
            }
        }
    }
    public PuzzleDictionary puzzleCollection;
}
