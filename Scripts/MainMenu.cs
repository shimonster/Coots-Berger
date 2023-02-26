using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Scenes
{
    Menu,
    Game
}

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameDataManager gameDataManager;
    [SerializeField] GameObject continueFromLastOpenLevelButton;
    [SerializeField] SoundDummy music;

    Animator mainMenuAnimator;
    [SerializeField] AudioManager audioManager;

    private void Start()
    {
        //audioManager = Resources.FindObjectsOfTypeAll<AudioManager>()[0];

        continueFromLastOpenLevelButton.SetActive(gameDataManager.canStartAtLastOpenLevel);
        mainMenuAnimator = GetComponent<Animator>();
        gameDataManager.isPlayingMusic = false;
        audioManager.StopAllLoops();
        if (!gameDataManager.isPlayingMusic)
        {
            audioManager.StartMusic(music, music.gameObject.transform);
            gameDataManager.isPlayingMusic = true;
        }

        Application.targetFrameRate = 60;
    }

    public void ToggleCredits(bool showCredits)
    {
        mainMenuAnimator.SetBool("OpenCredits", showCredits);
    }

    public void StartPlayAnimation(bool startAtLastOpenLevel)
    {
        gameDataManager.startAtLastLevelOpened = startAtLastOpenLevel;
        mainMenuAnimator.SetTrigger("Play");
    }

    public void OnFinishPlayAnimation()
    {
        SceneManager.LoadScene(1/*(int) Scenes.Game*/);
    }
}
