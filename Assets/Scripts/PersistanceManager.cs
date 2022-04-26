using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveData
{
    public static void SavePuzzleData (int puzzleID, float[] ringRadiusPercentages)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/puzzle.data";
        FileStream stream = new FileStream(path, FileMode.Create);

        PuzzleData data = new PuzzleData(puzzleID, ringRadiusPercentages);
        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static PuzzleData LoadPuzzleData ()
    {
        string path = Application.persistentDataPath + "/puzzle.data";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            PuzzleData data = formatter.Deserialize(stream) as PuzzleData;
            stream.Close();

            return data;
        }
        else
        {
            Debug.LogError("save file not found at path: " + path);
            return null;
        }
    }
}
