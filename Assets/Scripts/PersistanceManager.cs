using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class PersistenceManager
{
    public static void SaveData (string fileName, object dataToBeSaved)
    {
        string path = Application.persistentDataPath + "/" + fileName + ".data";

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Create);

        formatter.Serialize(stream, dataToBeSaved);
        stream.Close();
    }

    public static object LoadData (string fileName)
    {
        string path = Application.persistentDataPath + "/" + fileName + ".data";
        if (!File.Exists(path))
        {
            SavePuzzleData.CreatPuzzleDataSaveFile();
        }
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Open);

        object dataToBeLoaded = formatter.Deserialize(stream);
        stream.Close();

        return dataToBeLoaded;
    }
}
