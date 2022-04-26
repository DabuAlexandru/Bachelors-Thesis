using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PuzzleData
{
    const int resolutionV = Constants.resolutionV;
    public int puzzleID;
    public float[] ringRadiusPercentages;

    public PuzzleData (PuzzleData puzzleData)
    {
        setPuzzleData(puzzleData.puzzleID, puzzleData.ringRadiusPercentages);
    }

    public PuzzleData (int puzzleID, float[] ringRadiusPercentages)
    {
        setPuzzleData(puzzleID, ringRadiusPercentages);
    }

    public void setPuzzleData(int puzzleID, float[] ringRadiusPercentages)
    {
        this.puzzleID = puzzleID;
        this.ringRadiusPercentages = new float[resolutionV + 1];
        for(int i = 0; i <= resolutionV; i++) {
            this.ringRadiusPercentages[i] = ringRadiusPercentages[i];
        }
    }
}