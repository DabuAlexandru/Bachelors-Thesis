using System.Collections.Generic;

[System.Serializable]
public class SavePuzzleData
{
    SavePuzzleData()
    {
        puzzleCollection = new PuzzleDictionary(GetLoadedPuzzleData());
    }
    private static readonly object myLock = new object();
    private static SavePuzzleData _instance;
    public static SavePuzzleData instance
    {
        get
        {
            lock (myLock)
            {
                if (_instance == null)
                {
                    _instance = new SavePuzzleData();
                }
                return _instance;
            }
        }
        set
        {
            if (value != null)
            {
                _instance = value;
            }
        }
    }

    public PuzzleDictionary GetLoadedPuzzleData()
    {
        return (PuzzleDictionary)PersistanceManager.LoadData(Constants.puzzleDataFile);
    }

    public void SavePuzzleDataToFile()
    {
        PersistanceManager.SaveData(Constants.puzzleDataFile, puzzleCollection);
    }

    public PuzzleDictionary puzzleCollection;
}
