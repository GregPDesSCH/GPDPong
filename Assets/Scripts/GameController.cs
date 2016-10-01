/* 
    GPD Pong - 3D Pong clone developed using Unity

    By Gregory Desrosiers
    University of Waterloo, Software Engineering 2019


    Game Controller

    This class controls the game during gameplay, holding the many different
    elements that manage the game, including the pause menus,
    different UI elements, and important game properties including
    scores.


    Development Dates: June 2016 - August 2016
    File Name: GameController.cs

    Special Thanks to the staff who put the tutorials together on
    Unity Technologies and both Unity Forums and Unity Answers
    moderators for giving me the flexibility to ask various questions
    on how different things work and giving me different ways to program.

    http://gregpdessch.github.io/
    Source Code © 2016 Gregory Desrosiers. All rights reserved.

*/

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Audio;

// Enumeration for what kind of player the paddle is controlled by.
public enum PlayerNumber { PLAYER_ONE, PLAYER_TWO, OPPONENT };

public class GameController : MonoBehaviour 
{

    // Singleton Instance (we only need one instance of the game in the entire application)
    public static GameController gameController;


    // Game Objects
    public GameObject player1GoalZone;
    public GameObject player2GoalZone;
    
    private Paddle player1Paddle;
    private Paddle player2Paddle;

	private Ball pongBall; // Ball
	private GameObject mainCameraPivot; // Main camera component (for pivot operations)

    


    // UI Elements (for the user interface, including Pause Menu)
    public Text winText;
    public Text commentText;
    public Text playerScoreText;
    public Text opponentScoreText;
    public Text matchHeader;

    public GameObject pauseMenuPanel;
    public GameObject confirmRestartPanel;
    public GameObject confirmQuitPanel;
    public GameObject winnerPanel;

    public Button resumeButton;
    public Button restartButton;
    public Button restartYesButton;
    public Button restartNoButton;
    public Button quitGameButton;
    public Button quitYesButton;
    public Button quitNoButton;
    public Button winnerPlayAgainYesButton;
    public Button winnerPlayAgainNoButton;

    public Button websiteHyperlink;

    public Slider musicVolumeSlider;
    public Text musicVolumeLabel;
    public Slider soundVolumeSlider;
    public Text soundVolumeLabel;



    // Text Choices and the different messages displayed
    // (The two string arrays should be declared readonly because I only read from them, but it's not shown
    // here to keep the code original and the same as what the executed form looks like in the game.)
    public float probabilityOfDisplayingAMessage = 0.3f;
    public string[] rawPlayerGoalMessages = { "GOAL!!", "One point for you", "Right on!", "BAM", "Bullseye!", "You're going out.",
        "Keep it up!", "WHOO!"};
    public string[] opponentGoalMessage = { "Bad luck, player!", "C'mon, what you doing?", "Hey! Get moving, will you?",
        "Uh-oh, we're in trouble!", "But why?", "Oh, dangnabit.", "What the...?", "NOOOOO!!!"};

    // Camera Limits
	const float displacementFromCentreOfBoard = 2.45f; // The half-width of the board's play area
	const float maximumTiltAngle = 15.0f; // 15 degrees with respect to the vertical (Y-axis).

    

    // Audio Objects
    AudioSource musicAudioSource;

    GameObject soundEffectGameObject;
    [HideInInspector] public AudioSource ballHitsPaddleAudioSource;
    [HideInInspector] public AudioSource ballHitsWallAudioSource;
    [HideInInspector] public AudioSource goalAudioSource;

    AudioMixer gameAudioMixer;


    // Constants
    public const float pitchVariance = 0.15f;


    // Use this for initialization
    void Start () 
	{
        // Singleton Instance Load
        if (gameController == null)
            gameController = this;
        else
            Destroy(gameObject);


        // Load all necessary objects
        mainCameraPivot = GameObject.FindGameObjectWithTag("Camera Element");
        player1Paddle = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        player2Paddle = GameObject.FindGameObjectWithTag("Opponent").GetComponent<Opponent>();
        pongBall = GameObject.FindGameObjectWithTag("Ball").GetComponent<Ball>();

        musicAudioSource = GetComponent<AudioSource>();
        soundEffectGameObject = GameObject.FindGameObjectWithTag("SFX");
        ballHitsPaddleAudioSource = soundEffectGameObject.GetComponentsInChildren<AudioSource>()[0];
        ballHitsWallAudioSource = soundEffectGameObject.GetComponentsInChildren<AudioSource>()[1];
        goalAudioSource = soundEffectGameObject.GetComponentsInChildren<AudioSource>()[2];





        // Disable the child sphere collider and the AI script if there are two people playing
        if (ApplicationManager.applicationManager.numberOfPlayersInGame == NumberOfPlayers.TWO_PLAYER)
        {
            player2Paddle.GetComponent<Opponent>().enabled = false;
            player2Paddle.GetComponent<Player>().enabled = true;

            player2Paddle.GetComponentsInChildren<SphereCollider>()[0].enabled = false;
            player2Paddle.GetComponentsInChildren<SphereCollider>()[1].enabled = false;
        }





        // Set the colors on the rendering components of the paddles
        player1Paddle.GetComponent<Renderer>().material.color = ApplicationManager.applicationManager.player1PaddleColor;
        player2Paddle.GetComponent<Renderer>().material.color = ApplicationManager.applicationManager.player2PaddleColor;



        /* 
           Load the appropriate music sequence, as specified by difficulty and game state,
           directly onto the audio source, then set the attentuation on the mixer appropriately.
           This makes sure that the music is balanced with the sounds.
        */
        gameAudioMixer = musicAudioSource.outputAudioMixerGroup.audioMixer;

        if (ApplicationManager.applicationManager.numberOfPlayersInGame == NumberOfPlayers.TWO_PLAYER)
        {
            musicAudioSource.clip = Resources.Load("_Music/580752_Electronic-Battlefield") as AudioClip;
            gameAudioMixer.SetFloat("musicAttenuation", -9f);
        }
        else if (ApplicationManager.applicationManager.opponentDifficultyLevel == OpponentDifficulty.EASY)
        {
            musicAudioSource.clip = Resources.Load("_Music/8bit Dungeon Level") as AudioClip;
            gameAudioMixer.SetFloat("musicAttenuation", 0f);
        }
        else if (ApplicationManager.applicationManager.opponentDifficultyLevel == OpponentDifficulty.MEDIUM)
        {
            musicAudioSource.clip = Resources.Load("_Music/Mega Rust") as AudioClip;
            gameAudioMixer.SetFloat("musicAttenuation", -2f);
        }
        else if (ApplicationManager.applicationManager.opponentDifficultyLevel == OpponentDifficulty.HARD)
        {
            musicAudioSource.clip = Resources.Load("_Music/Visager_-_22_-_Battle_Loop") as AudioClip;
            gameAudioMixer.SetFloat("musicAttenuation", -9f);
        }



        // Set appropriate volumes
        musicAudioSource.volume = ApplicationManager.applicationManager.musicVolume;
        ballHitsPaddleAudioSource.volume = ApplicationManager.applicationManager.soundVolume;
        ballHitsWallAudioSource.volume = ApplicationManager.applicationManager.soundVolume;
        goalAudioSource.volume = ApplicationManager.applicationManager.soundVolume;

        musicVolumeSlider.value = ApplicationManager.applicationManager.musicVolume * 10.0f;
        soundVolumeSlider.value = ApplicationManager.applicationManager.soundVolume * 10.0f;

        musicVolumeLabel.text = (musicVolumeSlider.value * 10) + "%";
        soundVolumeLabel.text = (soundVolumeSlider.value * 10) + "%";



        // Initialize the tags for collision detection
        player1GoalZone.tag = "Player's Goal";
        player2GoalZone.tag = "Opponent's Goal";


        // Initialize the text elements in the game
		winText.text = "";
        commentText.text = "";

        if (ApplicationManager.applicationManager.numberOfPlayersInGame == NumberOfPlayers.ONE_PLAYER)
        {
            playerScoreText.text = "Player: 0";
            opponentScoreText.text = "CPU: 0";
        }
        else
        {
            playerScoreText.text = "Player 1: 0";
            opponentScoreText.text = "Player 2: 0";
        }

        playerScoreText.color = ApplicationManager.applicationManager.player1PaddleColor;
        opponentScoreText.color = ApplicationManager.applicationManager.player2PaddleColor;

        matchHeader.text = ApplicationManager.applicationManager.numberOfGoalsToWin + "-Point Match";



        // Add listeners to the interface buttons and hyperlink
        winnerPlayAgainYesButton.onClick.AddListener(() => playAgain());
        winnerPlayAgainNoButton.onClick.AddListener(() => quitGame());

        restartButton.onClick.AddListener(() => displayConfirmRestartPanel());
        restartYesButton.onClick.AddListener(() => playAgain());
        restartNoButton.onClick.AddListener(() => displayPauseMenuPanel());
        resumeButton.onClick.AddListener(() => resumePlay());

        quitYesButton.onClick.AddListener(() => quitGame());
        quitNoButton.onClick.AddListener(() => displayPauseMenuPanel());
        quitGameButton.onClick.AddListener(() => displayConfirmQuitPanel());

        websiteHyperlink.onClick.AddListener(() => openWebsiteLink());

        musicVolumeSlider.onValueChanged.AddListener(delegate { updateMusicVolume(); });
        soundVolumeSlider.onValueChanged.AddListener(delegate { updateSoundVolume(); });


        // Start playing the music
        musicAudioSource.Play();

       

    }

    void Update()
    {
        // Pressing down on the P key during gameplay toggles Pause Mode.
        if (Input.GetKeyDown(KeyCode.P) && (ApplicationManager.applicationManager.currentGameState == GameState.GAME_ON ||
            ApplicationManager.applicationManager.currentGameState == GameState.PAUSE_SCREEN)) 
        {
            ApplicationManager.applicationManager.currentGameState =
                ApplicationManager.applicationManager.currentGameState == GameState.GAME_ON ? GameState.PAUSE_SCREEN : GameState.GAME_ON;
            Time.timeScale = ApplicationManager.applicationManager.currentGameState == GameState.PAUSE_SCREEN ? 0 : 1;

            if (ApplicationManager.applicationManager.currentGameState == GameState.GAME_ON)
            {
                musicAudioSource.Play();
                hidePauseMenuPanel();
            }
            else
            {
                musicAudioSource.Pause();
                displayPauseMenuPanel();
            }
        }


        // Display the Play Again screen once either player gets the required number of goals
        // to win the game.
        if ((playerWins() || opponentWins()) && !winnerPanel.activeSelf)
        {
            disableGame();
            winnerPanel.SetActive(true);
            goalAudioSource.pitch = 1.0f;
            StartCoroutine(PlayWinnerSound());

            if (ApplicationManager.applicationManager.numberOfPlayersInGame == NumberOfPlayers.ONE_PLAYER)
            {
                if (playerWins())
                    winText.text = "YOU WIN!!";
                else
                    winText.text = "YOU LOSE!!";
            }
            else
            {
                if (playerWins())
                    winText.text = "Player 1 wins!";
                else
                    winText.text = "Player 2 wins!";
            }
        }
    }
	
	void FixedUpdate () 
	{
        // Tilt the board back and forth based on the ball's y position while the game is in progress.
        if (ApplicationManager.applicationManager.currentGameState == GameState.GAME_ON)
            mainCameraPivot.transform.rotation = Quaternion.Euler (new Vector3(- maximumTiltAngle * (pongBall.transform.position.z / displacementFromCentreOfBoard), 0.0f));
	}


    public void resetPaddlePositions()
    {
        player1Paddle.resetPosition();
        player2Paddle.resetPosition();
    }

    // Update Player 1's goals and reset the camera.
	public void incrementPlayerGoals()
	{
        player1Paddle.Score++;
        mainCameraPivot.transform.rotation = Quaternion.Euler (new Vector3(0.0f, 0.0f));
        outputPlayerGoals();
	}
	
    // Change the text showing the player the number of goals they have.
	public void outputPlayerGoals()
	{
        if (ApplicationManager.applicationManager.numberOfPlayersInGame == NumberOfPlayers.ONE_PLAYER)
            playerScoreText.text = "Player: " + player1Paddle.Score.ToString();
        else
            playerScoreText.text = "Player 1: " + player1Paddle.Score.ToString();
    }

    // Update Player 2's goals and reset the camera.
    public void incrementOpponentGoals()
	{
        player2Paddle.Score++;
        mainCameraPivot.transform.rotation = Quaternion.Euler (new Vector3(0.0f, 0.0f));
        outputOpponentGoals();
	}
	
    // Change the text showing how many goals the opponent or Player 2 has.
	public void outputOpponentGoals()
	{
        if (ApplicationManager.applicationManager.numberOfPlayersInGame == NumberOfPlayers.ONE_PLAYER)
            opponentScoreText.text = "CPU: " + player2Paddle.Score.ToString();
        else
            opponentScoreText.text = "Player 2: " + player2Paddle.Score.ToString();
    }

    // A helper function to tell whether the game is in progress or not.
    public bool isTheGameOn()
    {
        return !(playerWins() || opponentWins()) 
            && ApplicationManager.applicationManager.currentGameState == GameState.GAME_ON;
    }

    // Stop the music, then change game state.
    public void disableGame()
    {
        musicAudioSource.Stop();
        ApplicationManager.applicationManager.currentGameState = GameState.GAME_OVER;
    }

    // Did Player 1 won the game?
	public bool playerWins()
	{
		return player1Paddle.Score == ApplicationManager.applicationManager.numberOfGoalsToWin;
	}
    
    // Did Player 2 won the game?
	public bool opponentWins()
	{
		return player2Paddle.Score == ApplicationManager.applicationManager.numberOfGoalsToWin;
	}






    /* User Interface Event Listeners */

    public void displayConfirmRestartPanel() // Open the Confirm Restart panel.
    {
        pauseMenuPanel.SetActive(false);
        confirmRestartPanel.SetActive(true);
    }

    public void displayConfirmQuitPanel() // Open the Confirm Quit panel.
    {
        pauseMenuPanel.SetActive(false);
        confirmQuitPanel.SetActive(true);
    }

    public void displayPauseMenuPanel() // Open the Pause Menu panel.
    {
        if (confirmRestartPanel.activeSelf)
            confirmRestartPanel.SetActive(false);
        else if (confirmQuitPanel.activeSelf)
            confirmQuitPanel.SetActive(false);

        pauseMenuPanel.SetActive(true);
    }

    public void hidePauseMenuPanel() // Close the Pause Menu panel.
    {
        pauseMenuPanel.SetActive(false);
    }


    // Change the volume of the music based on a slider in the Pause Menu.
    public void updateMusicVolume()
    {
        ApplicationManager.applicationManager.musicVolume = musicVolumeSlider.value / 10.0f;
        musicAudioSource.volume = ApplicationManager.applicationManager.musicVolume;
        musicVolumeLabel.text = (musicVolumeSlider.value * 10) + "%";
    }

    // Change the volume of the sound effects based on a slider in the Pause Menu.
    public void updateSoundVolume()
    {
        ApplicationManager.applicationManager.soundVolume = soundVolumeSlider.value / 10.0f;

        ballHitsPaddleAudioSource.volume = ApplicationManager.applicationManager.soundVolume;
        ballHitsWallAudioSource.volume = ApplicationManager.applicationManager.soundVolume;
        goalAudioSource.volume = ApplicationManager.applicationManager.soundVolume;

        soundVolumeLabel.text = (soundVolumeSlider.value * 10) + "%";
    }




    public void resumePlay()
    {
        hidePauseMenuPanel();
        musicAudioSource.Play();
        ApplicationManager.applicationManager.currentGameState = GameState.GAME_ON;
        Time.timeScale = 1;
    }

    public void playAgain()
    {
        pongBall.reset();
        player1Paddle.reset();
        player2Paddle.reset();

        outputPlayerGoals();
        outputOpponentGoals();

        if (confirmRestartPanel.activeSelf)
            confirmRestartPanel.SetActive(false);

        winnerPanel.SetActive(false);
        mainCameraPivot.transform.rotation = Quaternion.Euler(new Vector3(0.0f, 0.0f));
        musicAudioSource.Stop();
        musicAudioSource.Play();
    }

    public void quitGame()
    {
        ApplicationManager.applicationManager.LoadMenuScene();
    }


    IEnumerator PlayWinnerSound()
    {
        float clipLength = goalAudioSource.clip.length;

        for (int i = 0; i < 15; i++)
        {
            goalAudioSource.PlayOneShot(goalAudioSource.clip);
            yield return new WaitForSeconds(clipLength + 0.06f);
        }
    }



    public void openWebsiteLink()
    {
        ApplicationManager.applicationManager.currentGameState = GameState.PAUSE_SCREEN;
        musicAudioSource.Pause();
        displayPauseMenuPanel();
        ApplicationManager.applicationManager.OpenPersonalWebsite();
    }
}
