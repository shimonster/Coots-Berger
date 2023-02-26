using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class GameDataManager : ScriptableObject
{
    [SerializeField] List<float> bestTimeToLevels = new List<float>();
    public int lastOpenLevel { get; private set; } = -1;
    public bool canStartAtLastOpenLevel
    {
        get
        {
            return lastOpenLevel != -1;
        }
    }
    public bool startAtLastLevelOpened
    {
        get
        {
            return _startAtLastLevelOpened;
        }
        set
        {
            _startAtLastLevelOpened = lastOpenLevel == -1 ? false : value;
        }
    }
    bool _startAtLastLevelOpened;
    public bool isPlayingMusic;

    private void OnEnable()
    {
        isPlayingMusic = false;
        lastOpenLevel = bestTimeToLevels.Count != 0 ? lastOpenLevel : -1;
    }


    public void TrySetNewLevelBest(int levelIdx, float seconds)
    {
        lastOpenLevel = levelIdx;

        if (bestTimeToLevels.Count <= levelIdx || bestTimeToLevels[levelIdx] > seconds)
        {
            bestTimeToLevels.Add(seconds);
        }
    }

    public float GetBestLevelTime(int levelIdx)
    {
        if (bestTimeToLevels.Count > levelIdx)
        {
            return bestTimeToLevels[levelIdx];
        }
        else
        {
            return -1;
        }
    }

    public bool CheckIfBestTimeForLevel(int levelIdx, float seconds)
    {
        return bestTimeToLevels[levelIdx] > seconds;
    }
}
