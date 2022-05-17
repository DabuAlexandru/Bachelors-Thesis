using System;
using UnityEngine;

public class PuzzleDataUtils
{
    public static void InitializeRingConfig(float[] ringConfig)
    {
        int n = ringConfig.Length;
        for(int i = 0; i < n; i++)
        {
            ringConfig[i] = 1.0f;
        }
    }

    public static void CopyContents(float[] from, float[] to)
    {
        int n = Mathf.Min(from.Length, to.Length);
        Array.Copy(from, to, n);
    }

    public static float GetDifference(float[] arr1, float[] arr2)
    {
        float totalDiff = 0.0f;
        int n = Mathf.Min(arr1.Length, arr2.Length);
        for (int i = 0; i < n; i++)
        {
            totalDiff += Mathf.Abs(arr1[i] - arr2[i]);
        }
        return totalDiff;
    }
}

[System.Serializable]
public class PuzzleData
{
    const int resolutionV = Constants.puzzleResolutionV;
    private int puzzleID;
    private float[] ringRadiusPercentages;
    private float[] intendedConfiguration;

    public int GetPuzzleID() => puzzleID;
    public void SetPuzzleID(int puzzleID) => this.puzzleID = puzzleID;

    public float[] GetRingRadiusPercentages() => ringRadiusPercentages;

    public void SetRingRadiusPercentages(float[] ringRadiusPercentages)
        => PuzzleDataUtils.CopyContents(ringRadiusPercentages, this.ringRadiusPercentages);

    public float[] GetIntendedConfiguration() => intendedConfiguration;

    public void SetIntendedConfiguration(float[] intendedConfiguration)
        => PuzzleDataUtils.CopyContents(intendedConfiguration, this.intendedConfiguration);

    public PuzzleData ()
    {
        this.puzzleID = 0;

        this.ringRadiusPercentages = new float[resolutionV + 1];
        PuzzleDataUtils.InitializeRingConfig(this.ringRadiusPercentages);

        this.intendedConfiguration = new float[resolutionV + 1];
        PuzzleDataUtils.InitializeRingConfig(this.intendedConfiguration);
    }

    public PuzzleData (PuzzleData puzzleData) 
        => SetPuzzleData(puzzleData.puzzleID, puzzleData.ringRadiusPercentages, puzzleData.intendedConfiguration);

    public PuzzleData (int puzzleID, float[] ringRadiusPercentages, float[] intendedConfiguration) 
        => SetPuzzleData(puzzleID, ringRadiusPercentages, intendedConfiguration);

    private void SetPuzzleData(int puzzleID, float[] ringRadiusPercentages, float[] intendedConfiguration)
    {
        this.puzzleID = puzzleID;

        this.ringRadiusPercentages = new float[resolutionV + 1];
        SetRingRadiusPercentages(ringRadiusPercentages);

        this.intendedConfiguration = new float[resolutionV + 1];
        SetIntendedConfiguration(intendedConfiguration);
    }
}
