/* 
    GPD Pong - 3D Pong clone developed using Unity

    By Gregory Desrosiers
    University of Waterloo, Software Engineering 2019

    Ball

    This is the behavior of the ball that the paddles must hit
    for it to bounce back and forth between them as the original
    Pong game has done.

    Development Dates: June 2016 - August 2016
    File Name: Ball.cs

    Special Thanks to the staff who put the tutorials together on
    Unity Technologies and both Unity Forums and Unity Answers
    moderators for giving me the flexibility to ask various questions
    on how different things work and giving me different ways to program.

    http://gregpdessch.github.io/
    Source Code © 2016 Gregory Desrosiers. All rights reserved.

*/


using UnityEngine;
using System.Collections;

public class Ball : MonoBehaviour 
{
	private Rigidbody ball;

	private const float MAX_DX = 10f;
	private const float MAX_DY = 5f;
    private const float MIN_DX = 1f;
	private bool gameOver;
    private Vector3 speedVector;

    


	// Use this for initialization
	void Start () 
	{
		StartCoroutine(pause());
        speedVector = new Vector3(0f, 0f, 0f);
        resetMovementComponents();
    }

    // Places the ball in the middle of the board and decides an initial velocity vector
    // because in Pong, the ball decides which direction and speed the ball moves at first.
	void resetMovementComponents()
	{
        /* 
           I'm not frantically sure why I'm retrieving the rigidbody component every time I reset the position
           of the ball and I'm accessing the position from the parent of the component when I can just 
           simply use gameObject.

           I think the best coding practice is to have the rigidbody component retrieved in the Start
           function and just use the same thing.
        */
		ball = GetComponent<Rigidbody>();
		ball.transform.transform.position = new Vector3(0, 0.5f, 0);

	
		float randomizationFactorX = ((float)Random.value) - 0.42f, randomizationFactorY = ((float)Random.value) - 0.32f;
	
		while ((randomizationFactorX < -0.42f || randomizationFactorX > 0.42f) ||
		       (randomizationFactorX > -0.3f && randomizationFactorX < 0.3f))
			randomizationFactorX = ((float)Random.value)  - 0.42f;
	
		while ((randomizationFactorY < -0.32f || randomizationFactorY > 0.32f) ||
		       (randomizationFactorY > -0.15f && randomizationFactorY > 0.15f))
			randomizationFactorY = ((float)Random.value) - 0.32f;

        speedVector = new Vector3(randomizationFactorX * MAX_DX, 0f, randomizationFactorY * MAX_DY);

	}

    // A helper function to not only reset the components, but pause the ball from moving for around 1.5 seconds.
    public void reset()
    {
        StartCoroutine(pause());
        resetMovementComponents();
    }


	
	// Physics Code
	void FixedUpdate () 
	{
        // Move the ball while the game is in progress.
        if (GameController.gameController.isTheGameOn())
            ball.transform.Translate(speedVector * Time.deltaTime);
	}


    // Collision Code
    // NOTE: isKinematic and isTrigger in the inspector both must be disabled in order for this function to be called.
	void OnCollisionEnter(Collision other) 
	{
        // This is where we hit either a wall or a paddle.
        if (other.gameObject.CompareTag("North Wall") || other.gameObject.CompareTag("South Wall") ||
            other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Opponent"))
        {
            // Bounce off the wall, but define some limits to make sure the ball does move in the opposing direction as
            // expected.
            speedVector = Vector3.Reflect(speedVector, other.contacts[0].normal);

            float speedX = speedVector.x, speedZ = 0.0f;

            if (speedX < MIN_DX && speedX > -MIN_DX)
            {
                if (speedX >= 0)
                    speedX = MIN_DX;
                else
                    speedX = -MIN_DX;
            }

            /* 
                Add a vertical speed component to the ball from the paddle's vertical speed. (+ if upwards, - if downwards)
            */
            if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Opponent"))
            {
                GameController.gameController.ballHitsPaddleAudioSource.Play();
                speedZ = ((Paddle)other.gameObject.GetComponent<MonoBehaviour>()).getSpeedVector().y / (Time.deltaTime * 2.5f);
            }
            else
                GameController.gameController.ballHitsWallAudioSource.Play();

            speedVector = new Vector3(speedX, 0f, speedVector.z + speedZ);

        }

        // If we hit a goal zone, we need to find out which goal we hit, play a sound effect, reset the ball,
        // and pause. The last three tasks are only done while the game is in progress.
        else if (other.gameObject.CompareTag("Player 1 Goal") || other.gameObject.CompareTag("Player 2 Goal") || 
            other.gameObject.CompareTag("Player's Goal") || other.gameObject.CompareTag("Opponent's Goal"))
        {
            if (other.gameObject.CompareTag("Player 1 Goal") || other.gameObject.CompareTag("Player's Goal")) 
                // We hit an invisible region corresponding to the player's goal zone
            {
                GameController.gameController.incrementPlayerGoals();

                if (!GameController.gameController.playerWins() && 
                    Random.value < GameController.gameController.probabilityOfDisplayingAMessage
                    && ApplicationManager.applicationManager.numberOfPlayersInGame == NumberOfPlayers.ONE_PLAYER) 
                    // A goal message for Player 1
                {
                    GameController.gameController.commentText.text = 
                        GameController.gameController.rawPlayerGoalMessages[
                            Random.Range(0, GameController.gameController.rawPlayerGoalMessages.Length)];
                }
            }
            else if (other.gameObject.CompareTag("Player 2 Goal") || other.gameObject.CompareTag("Opponent's Goal"))
            // We hit an invisible region corresponding to the opponent's goal zone
            {
                GameController.gameController.incrementOpponentGoals();

                if (!GameController.gameController.opponentWins() && 
                    Random.value < GameController.gameController.probabilityOfDisplayingAMessage
                    && ApplicationManager.applicationManager.numberOfPlayersInGame == NumberOfPlayers.ONE_PLAYER) 
                    // A goal message for Player 2
                {
                    GameController.gameController.commentText.text = 
                        GameController.gameController.opponentGoalMessage[
                            Random.Range(0, GameController.gameController.opponentGoalMessage.Length)];
                }

            }

            if (GameController.gameController.isTheGameOn())
            {
                GameController.gameController.goalAudioSource.Play();
                StartCoroutine(pause());
                resetMovementComponents();
            }
        }
    }

    // A helper function to get the ball's speed vector.
    public Vector3 getSpeedVector()
    {
        return speedVector;
    }

    // A helper function to get the ball's position.
    public Vector3 getPosition()
    {
        return transform.position;
    }


    // Pause for 1.5 seconds, then resume.
    // This is also where we have a message at the start of a match, and reset the comment as sometimes
    // comments do show up randomly in one-player mode.
	public IEnumerator pause()
	{
        if (ApplicationManager.applicationManager.currentGameState != GameState.GAME_ON)
            GameController.gameController.commentText.text = "Ready?";

        Time.timeScale = 0.0001f;
		yield return new WaitForSeconds(1.5f * Time.timeScale);
		Time.timeScale = 1f;

        if (ApplicationManager.applicationManager.currentGameState != GameState.GAME_ON)
        {
            GameController.gameController.commentText.text = "GO!";
            ApplicationManager.applicationManager.currentGameState = GameState.GAME_ON;
            yield return new WaitForSeconds(0.6f);
        }
        GameController.gameController.commentText.text = "";
    }

}
