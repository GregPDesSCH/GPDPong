/* 
    GPD Pong - 3D Pong clone developed using Unity

    By Gregory Desrosiers
    University of Waterloo, Software Engineering 2019


    Application Manager

    This script manages the entire game application, from when it's 
    executed to when it's terminated. It's the only script that doesn't get 
    destroyed when it asks for different scenes to be loaded, as some of the 
    data has to hold up different states for the game scene to load properly.


    Development Dates: June 2016 - August 2016
    File Name: ApplicationManager.cs

    Special Thanks to the staff who put the tutorials together on
    Unity Technologies and both Unity Forums and Unity Answers
    moderators for giving me the flexibility to ask various questions
    on how different things work and giving me different ways to program.

    http://gregpdessch.github.io/
    Source Code © 2016 Gregory Desrosiers. All rights reserved.

*/

using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

// Variables for the different states
public enum BoardColor { RED, BLUE, YELLOW, GREEN, ORANGE, PURPLE };
public enum GameState { TITLE_SCREEN, COUNT_DOWN, PAUSE_SCREEN, GAME_ON, GAME_OVER };
public enum NumberOfPlayers { ONE_PLAYER, TWO_PLAYER };
public enum OpponentDifficulty { EASY, MEDIUM, HARD };


public class ApplicationManager : MonoBehaviour
{
    // Singleton Instance (only need on application manager)
    public static ApplicationManager applicationManager;


    /* 
        Persistent Game Variables 
        We hide these from the inspector, but make them public because the different
        scene-dependent scripts will need to access these for execution to be done properly.
    */

    // Opponent Difficulty
    [HideInInspector] public OpponentDifficulty opponentDifficultyLevel = OpponentDifficulty.EASY;

    // Game Tag
    [HideInInspector] public GameState currentGameState = GameState.TITLE_SCREEN;

    // Number of Players
    [HideInInspector]
    public NumberOfPlayers numberOfPlayersInGame = NumberOfPlayers.ONE_PLAYER;

    // Goals to Win
    [HideInInspector]
    public byte numberOfGoalsToWin;

    // Paddle Colors
    [HideInInspector]
    public Color player1PaddleColor;
    [HideInInspector]
    public Color player2PaddleColor;
    [HideInInspector]
    public readonly Color[] listOfColors =
        {Color.red, Color.blue, Color.yellow, Color.green, new Color(1.0f, 140.0f / 255.0f, 0.0f),
            new Color(160.0f / 255.0f, 32.0f / 255.0f, 240.0f / 255.0f) };

    // Audio Volume Settings
    [HideInInspector]
    public float musicVolume = 1.0f;
    [HideInInspector]
    public float soundVolume = 1.0f;



    // Goals needed to win
    public readonly byte[] goalsToWinArray = { 5, 10, 20, 50 };


    // Use this for initialization
    void Start ()
    {
        // Set singleton instance
        if (applicationManager == null)
            applicationManager = this;
        else
            Destroy(gameObject);

        // Keep the singleton instance as is regardless of which scene is loaded
        DontDestroyOnLoad(applicationManager);


        // Set target frame rate to 60 frames per second
        Application.targetFrameRate = 60;

        // Allow the number of samples per frame to be dynamic.
        Profiler.maxNumberOfSamplesPerFrame = -1;
    }


    // Load the gameplay scene.
    public void LoadGameScene()
    {
        if (SceneManager.GetActiveScene().name == "Title Screen")
            SceneManager.LoadScene("Pong");
        else
            SceneManager.LoadScene("Pong (Standalone)");
    }

    // Load the menu scene
    public void LoadMenuScene()
    {
        if (SceneManager.GetActiveScene().name == "Pong")
            SceneManager.LoadScene("Title Screen");
        else
            SceneManager.LoadScene("Title Screen (Standalone)");
    }

    // Open my personal website in an external window with the user's default Internet browser.
    public void OpenPersonalWebsite()
    {
        if (Application.platform != RuntimePlatform.WebGLPlayer)
            Application.OpenURL("http://gregpdessch.github.io");
        else
            Application.ExternalEval("window.open(\'http://gregpdessch.github.io\')");
    }
}
