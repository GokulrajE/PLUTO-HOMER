﻿using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;
using UnityEditor.SceneManagement;
using NeuroRehabLibrary;


public class UIManagerPP : MonoBehaviour
{
    GameObject[] pauseObjects, finishObjects;
    public BoundController rightBound;
    public BoundController leftBound;
    public bool isFinished;
    public bool isPressed=false;
    public bool playerWon, enemyWon;
    public AudioClip[] audioClips; 
    public int win;
    private bool isPaused = true;
    private GameSession currentGameSession;

    void Start()
    {
        PlutoComm.OnButtonReleased += onPlutoButtonReleased;
        pauseObjects = GameObject.FindGameObjectsWithTag("ShowOnPause");
        finishObjects = GameObject.FindGameObjectsWithTag("ShowOnFinish");
        hideFinished();
        StartNewGameSession();


    }
    void Update()
    {
        CheckGameEndConditions();
        if (isFinished)
        {
            showFinished();
           gameData.isGameLogging = false;
        }
        if ((Input.GetKeyDown(KeyCode.P) && !isFinished) || (isPressed && !isFinished))
        {
            if (!isPaused)
            {
                pauseGame();
            }
            else
            {
                resumeGame();
            }
            isPressed = false; 
        }
    }


    private void CheckGameEndConditions()
    {
        if (rightBound.enemyScore >= gameData.winningScore && !isFinished)
        {
            isFinished = true;
            enemyWon = true;
            playerWon = false;
            gameEnd();
        }
        else if (leftBound.playerScore >= gameData.winningScore && !isFinished)
        {
            isFinished = true;
            enemyWon = false;
            playerWon = true;
            gameEnd();
        }
    }
    private void gameEnd()
    {
        Camera.main.GetComponent<AudioSource>().Stop();
        playAudio(enemyWon ? 1 : 0);
        gameData.reps = 0;
        showFinished();
        EndCurrentGameSession();
    }
 private void pauseGame()
    {
        Time.timeScale = 0;
        isPaused = true;
        showPaused();
        gameData.isGameLogging = false;
        Debug.Log("Game Paused");
    }

    private void resumeGame()
    {
        Time.timeScale = 1;
        isPaused = false;
        hidePaused();
        gameData.isGameLogging = true;
        Debug.Log("Game Unpaused");
    }


    private void onPlutoButtonReleased()
    {
        isPressed = true;
    }
        //Reloads the Level
  public void LoadScene(string sceneName)
    {
        EndCurrentGameSession();
       SceneManager.LoadScene(sceneName);
    }

    //Reloads the Level
    public void Reload()
    {
        EndCurrentGameSession();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    void playAudio(int clipNumber)
    {
        AudioSource audio = GetComponent<AudioSource>();
        audio.clip = audioClips[clipNumber];
        audio.Play();
    } 
    public void showPaused()
    {
        foreach (GameObject g in pauseObjects)
        {
            g.SetActive(true);
      
        }
    }
    public void hidePaused()
    {
        foreach (GameObject g in pauseObjects)
        {
            g.SetActive(false);
           
        }
    }
    public void showFinished()
    {
        foreach (GameObject g in finishObjects)
        {
            g.SetActive(true);
        }
        Debug.Log("Player movement time to this point: " + gameData.moveTime.ToString("F2") + " seconds");

    }
    public void hideFinished()
    {
        foreach (GameObject g in finishObjects)
        {
            g.SetActive(false);
        }

    }
    private void OnDestroy()
    {
        if (ConnectToRobot.isPLUTO)
        {
            PlutoComm.OnButtonReleased -= onPlutoButtonReleased;
        }

        EndCurrentGameSession();
    }
    void StartNewGameSession()
    {
        currentGameSession = new GameSession
        {
            GameName = "PING-PONG",
            Assessment = 0 // Example assessment value, adjust as needed
        };

        SessionManager.Instance.StartGameSession(currentGameSession);
        Debug.Log($"Started new game session with session number: {currentGameSession.SessionNumber}");

        SetSessionDetails();
    }
    private void SetSessionDetails()
    {
        string device = "PLUTO"; 
        string assistMode = "Null"; 
        string assistModeParameters = "Null"; 
        string deviceSetupLocation = "CMC-Bioeng-dpt"; // Set the device setup location
        string gameParameter = "YourGameParameter"; // Set the game parameter
       // Set the game parameter

        string mech = AppData.selectMechanism;
        SessionManager.Instance.SetDevice(device, currentGameSession);
        SessionManager.Instance.SetAssistMode(assistMode, assistModeParameters, currentGameSession);
        SessionManager.Instance.SetDeviceSetupLocation(deviceSetupLocation, currentGameSession);
        SessionManager.Instance.SetGameParameter(gameParameter, currentGameSession);
        SessionManager.Instance.mechanism(mech, currentGameSession);
    }
    void EndCurrentGameSession()
    {
        if (currentGameSession != null)
        {
            string trialdata = AppData.trialDataFileLocation;
            string movetime = gameData.moveTime.ToString("F0");
            SessionManager.Instance.SetTrialDataFileLocation(trialdata, currentGameSession);
            SessionManager.Instance.moveTime(movetime, currentGameSession);
            SessionManager.Instance.EndGameSession(currentGameSession);
        }
    }
}
