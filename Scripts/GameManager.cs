using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

[ExecuteAlways]
public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject door;
    [SerializeField] GameObject player;
    [SerializeField] GameObject mesh;

    [SerializeField] Animator uiAnimator;

    [SerializeField] TextMeshProUGUI stopwatchTextUI;
    [SerializeField] RectTransform stopwatchBackgroundUI;
    [SerializeField] TextMeshProUGUI pauseStatesText;

    [SerializeField] GameDataManager gameDataManager;

    [SerializeField] float maxDifficulty;
    [Range(1, 2)]
    [SerializeField] float difficultySteepness;
    [SerializeField] float difficultyIncreaseOffset;
    [SerializeField] float endDifficultySlope;
    [SerializeField] float firstLevelDifficulty;
    [SerializeField] float widthDifficultyMultiplier;
    [SerializeField] float heightDifficultyMultiplier;

    [SerializeField] Material terrainMaterial;
    [SerializeField] TerrainMaterial[] terrainMats;

    public int mockLevelNum;
    int curLevelIdx;

    Door doorScript;
    PlayerMovement playerMovement;
    MeshGenerator meshGenerator;
    [SerializeField] AudioManager audioManager;

    float stopwatch = 0;
    float timeToLevelStopwatch = 0;
    bool isPlayingLevel;
    public static bool isPaused;

    private void Start()
    {
        //audioManager = Resources.FindObjectsOfTypeAll<AudioManager>()[0];

        doorScript = door.GetComponent<Door>();
        playerMovement = player.GetComponent<PlayerMovement>();
        meshGenerator = mesh.GetComponent<MeshGenerator>();

        if (gameDataManager.startAtLastLevelOpened)
        {
            timeToLevelStopwatch = gameDataManager.GetBestLevelTime(gameDataManager.lastOpenLevel);
            curLevelIdx = gameDataManager.lastOpenLevel;
        }
        //CreateLevel(curLevelIdx);
        StartCoroutine(createLevel());
        isPlayingLevel = true;

        //audioManager.StartMusic(this, transform);
    }

    IEnumerator createLevel()
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(Time.deltaTime * 2);
        Time.timeScale = 1;
        CreateLevel(curLevelIdx);
    }

    private void Update()
    {
        if (isPlayingLevel && Application.isPlaying)
        {
            stopwatch += Time.deltaTime;
            string newText = $"{curLevelIdx + 1} - {ConvertSecondsToString(stopwatch)}";
            stopwatchTextUI.text = newText;
            stopwatchBackgroundUI.localScale = new Vector3(newText.Length - 2, 1, 1);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause(!isPaused);
        }
    }

    LevelData GetLevelDataForLevel(int levelNum)
    {
        float numerator = levelNum * (maxDifficulty - firstLevelDifficulty);
        float denominator = levelNum + difficultyIncreaseOffset * Mathf.Pow(difficultySteepness, -levelNum);
        float linearIncrease = endDifficultySlope * levelNum;
        float levelDifficulty = numerator / denominator + firstLevelDifficulty + linearIncrease;

        LevelData levelData = new LevelData();

        float width = (levelDifficulty + 10) * widthDifficultyMultiplier;
        float height = (levelDifficulty + 10) * heightDifficultyMultiplier;

        levelData.terrainHeight = levelDifficulty;
        levelData.width = Mathf.RoundToInt(Mathf.Min(width, Mathf.Sqrt(62500f * widthDifficultyMultiplier / heightDifficultyMultiplier)));
        levelData.height = Mathf.RoundToInt(Mathf.Min(height, 62500f / levelData.width));

        levelData.terrainMat = terrainMats[Random.Range(0, terrainMats.Length)];

        return levelData;
    }

    public void CreateLevel(int levelIdx)
    {
        gameDataManager.TrySetNewLevelBest(curLevelIdx, timeToLevelStopwatch);

        LevelData levelData = GetLevelDataForLevel(levelIdx);

        meshGenerator.CreateMesh(levelData.width, levelData.height, Random.Range(-10000, 10000), Random.Range(-10000, 10000), levelData.terrainHeight);

        terrainMaterial.SetTexture("_FlatSurfaceTex", levelData.terrainMat.flatTexture);
        terrainMaterial.SetTexture("_SlopedSurfaceTex", levelData.terrainMat.slopedTexture);
        terrainMaterial.SetFloat("_FlatTexScale", levelData.terrainMat.flatTextureScale);
        terrainMaterial.SetFloat("_SlopedTexScale", levelData.terrainMat.slopedTextureScale);
        terrainMaterial.SetFloat("_TexTransitionSpeed", levelData.terrainMat.transitionSpeed);

        player.transform.position = new Vector3(Random.Range(levelData.width * 0.1f, levelData.width * 0.9f), 0, Random.Range(3, Mathf.Min(10, levelData.height)));
        door.transform.position = new Vector3(Random.Range(levelData.width * 0.1f, levelData.width * 0.9f), 0, Random.Range(levelData.height - 10, levelData.height - 5));

        playerMovement.InitPosition();
        doorScript.InitPosition();
    }

    public void FinishLevel()
    {
        uiAnimator.SetBool("IsTransitioning", true);

        isPlayingLevel = false;

        timeToLevelStopwatch += stopwatch;
        stopwatch = 0;
    }

    public void OnUILevelTransitionClosed()
    {
        curLevelIdx++;
        CreateLevel(curLevelIdx);

        uiAnimator.SetBool("IsTransitioning", false);
        isPlayingLevel = true;
    }

    public void TogglePause(bool paused)
    {
        float bestLevelTime = gameDataManager.GetBestLevelTime(curLevelIdx);
        float bestNextLevelTime = gameDataManager.GetBestLevelTime(curLevelIdx + 1);
        string curLevelTime = ConvertSecondsToString(stopwatch);
        string totalTime = ConvertSecondsToString(timeToLevelStopwatch + stopwatch);
        string bestTotalTime = bestLevelTime == -1 ? "0:00" : ConvertSecondsToString(bestNextLevelTime == -1 ? bestLevelTime + stopwatch : Mathf.Min(bestNextLevelTime,  bestLevelTime + stopwatch));
        pauseStatesText.text = $"{curLevelIdx + 1}\n{curLevelTime}\n{totalTime}\n{bestTotalTime}";

        isPaused = paused;
        uiAnimator.SetBool("Paused", paused);
        Time.timeScale = paused ? 0 : 1;
    }

    public void OpenMainMenu()
    {
        audioManager.StopAllLoops();

        TogglePause(false);
        SceneManager.LoadScene((int)Scenes.Menu);
    }

    string ConvertSecondsToString(float seconds)
    {
        float intSecs = Mathf.Floor(seconds);
        float milliseconds = Mathf.Floor((seconds - intSecs) * 100) / 100;
        return $"{intSecs}:{milliseconds.ToString("F").Substring(2)}";
    }
}

public struct LevelData
{
    public int width;
    public int height;
    public float terrainHeight;
    public TerrainMaterial terrainMat;
}

[System.Serializable]
public struct TerrainMaterial
{
    public Texture2D flatTexture;
    public Texture2D slopedTexture;
    public float flatTextureScale;
    public float slopedTextureScale;
    public float transitionSpeed;
}
