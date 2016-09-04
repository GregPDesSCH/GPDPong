/* 
    GPD Pong - 3D Pong clone developed using Unity

    By Gregory Desrosiers
    University of Waterloo, Software Engineering 2019


    Menu Controller

    This script controls the menus in the Title Scene.


    Development Dates: June 2016 - August 2016
    File Name: MenuController.cs

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
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour {
    
    // These are the different components and groups of the different menus available
    // in the title scene.
    public GameObject selectionMenu;
    public GameObject bottomSection;
    public GameObject onePlayerMenu;
    public GameObject twoPlayerMenu;
    public GameObject copyrightSection;

    public Button onePlayerButton;
    public Button twoPlayerButton;
    public Button copyrightsButton;
    public Button backButton;
    public Button startButton;
    public Button copyrightsBackButton;
    public Button personalWebsiteLink;

    public Dropdown opponentDifficultyDropdown;
    public Dropdown singlePlayerColorDropdown;
    public Dropdown player1ColorDropdown;
    public Dropdown player2ColorDropdown;
    public Dropdown goalsToWinDropdown;

    public Text loadingText;

	void Start ()
    {
        // Initialize the dropdown for Player 2's color to something different
        player2ColorDropdown.value = 1;


        /* Add the interactive listeners to the appropriate components. */
        onePlayerButton.onClick.AddListener(() => GoToOnePlayerMenu());
        twoPlayerButton.onClick.AddListener(() => GoToTwoPlayerMenu());
        copyrightsButton.onClick.AddListener(() => GoToCopyrightsScreen());
        backButton.onClick.AddListener(() => GoBackFromPlayerMenu());
        startButton.onClick.AddListener(() => StartTheGame());
        copyrightsBackButton.onClick.AddListener(() => LeaveCopyrightsScreen());
        personalWebsiteLink.onClick.AddListener(() => ApplicationManager.applicationManager.OpenPersonalWebsite());


        // We insert delegates here because onValueChanged needs to have a function object that returns an int.
        player1ColorDropdown.onValueChanged.AddListener(delegate {
            CheckColorValuesInTwoPlayerMenu();
        });
        player2ColorDropdown.onValueChanged.AddListener(delegate {
            CheckColorValuesInTwoPlayerMenu();
        });
    }

    // Open the 1 Player Menu.
    void GoToOnePlayerMenu()
    {
        ApplicationManager.applicationManager.numberOfPlayersInGame = NumberOfPlayers.ONE_PLAYER;

        onePlayerMenu.SetActive(true);
        DisableSelectionMenu();
    }

    // Open the 2 Player Menu.
    void GoToTwoPlayerMenu()
    {
        ApplicationManager.applicationManager.numberOfPlayersInGame = NumberOfPlayers.TWO_PLAYER;

        twoPlayerMenu.SetActive(true);
        DisableSelectionMenu();
    }

    // Open the Copyrights Box.
    void GoToCopyrightsScreen()
    {
        selectionMenu.SetActive(false);
        personalWebsiteLink.gameObject.SetActive(false);
        copyrightSection.SetActive(true);
    }

    // Leave the Copyrights Box.
    void LeaveCopyrightsScreen()
    {
        selectionMenu.SetActive(true);
        personalWebsiteLink.gameObject.SetActive(true);
        copyrightSection.SetActive(false);
    }

    // Leave either the 1 Player or 2 Player Menus and open the Main Screen
    void GoBackFromPlayerMenu()
    {
        if (onePlayerMenu.activeInHierarchy)
            onePlayerMenu.SetActive(false);
        else if (twoPlayerMenu.activeInHierarchy)
            twoPlayerMenu.SetActive(false);

        bottomSection.SetActive(false);
        selectionMenu.SetActive(true);
    }

    // Simply replaces the selection menu with two buttons and a dropdown for the number of goals to win.
    void DisableSelectionMenu()
    {
        bottomSection.SetActive(true);
        selectionMenu.SetActive(false);
    }


    // We disable the start button when both players have the same color.
    void CheckColorValuesInTwoPlayerMenu()
    {
        if (player1ColorDropdown.value == player2ColorDropdown.value)
            startButton.interactable = false;
        else
            startButton.interactable = true;
    }

    


    void StartTheGame()
    {
        loadingText.gameObject.SetActive(true);

        /* Set the required properties of the game. */
        // One Player Mode
        if (ApplicationManager.applicationManager.numberOfPlayersInGame == NumberOfPlayers.ONE_PLAYER)
        {
            ApplicationManager.applicationManager.opponentDifficultyLevel =
                (OpponentDifficulty)opponentDifficultyDropdown.value;

            // Player color
            ApplicationManager.applicationManager.player1PaddleColor =
                    ApplicationManager.applicationManager.listOfColors[singlePlayerColorDropdown.value];

            // Opponent color (making sure the color decided isn't the same as the player's)
            do
            {
                ApplicationManager.applicationManager.player2PaddleColor =
                    ApplicationManager.applicationManager.listOfColors[Random.Range(0, 5)];

            } while (ApplicationManager.applicationManager.player1PaddleColor ==
                ApplicationManager.applicationManager.player2PaddleColor);

        }
        // Two Player Mode
        else
        {
            // Assign the Player 1 color to that in the application manager
            ApplicationManager.applicationManager.player1PaddleColor =
                    ApplicationManager.applicationManager.listOfColors[player1ColorDropdown.value];

            // Assign the Player 2 color to that in the application manager
            ApplicationManager.applicationManager.player2PaddleColor =
                    ApplicationManager.applicationManager.listOfColors[player2ColorDropdown.value];
        }

        

        // Goals to win the game
        ApplicationManager.applicationManager.numberOfGoalsToWin =
            ApplicationManager.applicationManager.goalsToWinArray[goalsToWinDropdown.value];

        // Load the scene, then start gameplay
        ApplicationManager.applicationManager.LoadGameScene();
    }
}
