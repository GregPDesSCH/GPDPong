/* 
    GPD Pong - 3D Pong clone developed using Unity

    By Gregory Desrosiers
    University of Waterloo, Software Engineering 2019


    Player Paddle

    This is the paddle that the user controls in both one and two
    player modes. It's controlled by this script.


    Development Dates: June 2016 - August 2016
    File Name: Player.cs

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



public class Player : Paddle
{

    // Constants for the range that the paddle can move
    private float maxYPosition;
    private float minYPosition;

    // We override the start method.
    new void Start() 
    {
        base.Start();
        maxYPosition = 2.45f - board.transform.lossyScale.y;
        minYPosition = -2.45f + board.transform.lossyScale.y;
    }
	
    // Get the input from the player and change the speed vector the paddle relies on to move accordingly
    // at exactly every frame, but only if the game is in progress.
	void FixedUpdate () 
	{
		if (GameController.gameController.isTheGameOn())
		{
            float moveUpOrDown = 0.0f;

            // Get input from Player 1's input on keyboard
            if (typeOfPlayer == PlayerNumber.PLAYER_ONE)
                moveUpOrDown = Input.GetAxis("Player 1 Keyboard Input");
            // Get input from Player 2's input on keyboard
            else if (typeOfPlayer == PlayerNumber.PLAYER_TWO)
                moveUpOrDown = Input.GetAxis("Player 2 Keyboard Input");


            speedVector = new Vector3(0.0f, moveUpOrDown * Time.deltaTime * 2.8f, 0.0f);


            board.transform.position = new Vector3(board.transform.position.x,
                board.transform.position.y, Mathf.Clamp(board.transform.position.z + speedVector.y, minYPosition, maxYPosition));
		}
	}

}
