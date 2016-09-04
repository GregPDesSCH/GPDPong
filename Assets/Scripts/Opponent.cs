/* 
    GPD Pong - 3D Pong clone developed using Unity

    By Gregory Desrosiers
    University of Waterloo, Software Engineering 2019


    Opponent Paddle

    An extension of the Paddle class, but has functions for behaving
    as if this is a computer-controlled paddle instead of having
    the user control it.


    Development Dates: June 2016 - August 2016
    File Name: Opponent.cs

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

public class Opponent : Paddle
{

    public Ball pongBall; // Ball

    // Component for detecting the ball
    private SphereCollider ballDetector;


    /* Constants */
    // Ball Detection Radius
    private const float easyBallDetectionRadius = 5.0f;
    private const float mediumBallDetectionRadius = 10.0f;
    private const float hardBallDetectionRadius = 15.0f;

    // Acceleration Value
    private const float easyAccelerationValue = 1.0f;
    private const float mediumAccelerationValue = 1.5f;
    private const float hardAccelerationValue = 2.0f;



    // Fields in predicting the ball's z-coordinate
    private float displacementFromBallToWall = 0.0f;
    private float predictedZPositionOfWhereTheBallWillBe = 0.0f;
    
    // Fields for linear kinematics
    private float speedBefore = 0.0f;
    private float accelerationValue = 0.0f;
    private float startingZPosition = 0.0f;

    // Fields for probability (Medium Difficulty only)
    private float randomValue;
    private const float decisionThresholdValue = 0.50f;

    // Initialize the opponent. It's overridden so that we add extra operations to initialize this
    // as an opponent player.
    new void Start()
    {

        // Initialize the components of this paddle object and get the ball detector component.
        base.Start();
        ballDetector = GameObject.FindWithTag("Opponent").GetComponentInChildren<SphereCollider>();

        /* 
            Easy Level
            The paddle has a subtle reaction model by using a small radius and low acceleration. 
        */
        if (ApplicationManager.applicationManager.opponentDifficultyLevel == OpponentDifficulty.EASY)         
        {
            ballDetector.radius = easyBallDetectionRadius;
            accelerationValue = easyAccelerationValue;
        }

        /* 
            Medium Level 
            The reaction is a little quicker, but sort of between a quick react and a slow react,
            with a radius that is larger than half of the playing field, and an acceleration
            model 1.5 times as that of the player's.
        */
        else if (ApplicationManager.applicationManager.opponentDifficultyLevel == OpponentDifficulty.MEDIUM)  
        {
            ballDetector.radius = mediumBallDetectionRadius;
            accelerationValue = mediumAccelerationValue;
        }

        /* 
            Hard Level 
            The paddle quickly reacts to the ball coming its way with a humongous detection range and high acceleration.
        */
        else                                           
        {
            ballDetector.radius = hardBallDetectionRadius;
            accelerationValue = hardAccelerationValue;
        }
    }

	// The opponent moves back and forth consistently at regular intervals with its speed vector.
	void FixedUpdate () 
	{
		if (GameController.gameController.isTheGameOn() && 
            ApplicationManager.applicationManager.numberOfPlayersInGame == NumberOfPlayers.ONE_PLAYER)
		{
            // If the ball is moving away from the paddle, have the paddle come to a stop with a graceful deceleration.
            if (!ballIsHeadingTowardsPaddle())
                speedVector = new Vector3(0f, calculatePaddleMovementAfterBallIsBounced() * Time.deltaTime * 2.5f, 0f);

			board.transform.Translate (speedVector);
		}
	}


    void OnTriggerEnter(Collider collider)
    {
        // Predict where the ball will be based on the Z position as soon as the paddle detects it.
        if ((collider.CompareTag("Ball") || collider.CompareTag("Ball Detector 2")) && 
            (ApplicationManager.applicationManager.opponentDifficultyLevel == OpponentDifficulty.MEDIUM ||
            ApplicationManager.applicationManager.opponentDifficultyLevel == OpponentDifficulty.HARD))
        {
            predictedZPositionOfWhereTheBallWillBe = ballsPredictedZPositionInOpponentZone();
            startingZPosition = transform.position.z;
            randomValue = Random.value;
        }
    }


    // When the ball is detected and is heading towards the paddle, we calculate what the speed vector would be.
    void OnTriggerStay(Collider collider)
    {
        if (collider.CompareTag("Ball") && ballIsHeadingTowardsPaddle())
        {

            /* 
               The paddle simply follows the ball in a clumsy way when we are playing in Easy Mode,
               or on Medium Mode when we fall below the threshold; in other words, there's a 50% chance
               when the paddle decides to move in a clumsy way when it detects the ball.
            */

            if (ApplicationManager.applicationManager.opponentDifficultyLevel == OpponentDifficulty.EASY ||
                ApplicationManager.applicationManager.opponentDifficultyLevel == OpponentDifficulty.MEDIUM &&
                randomValue <= decisionThresholdValue)
                speedVector = new Vector3(0f, calculatePaddleMovement() * Time.deltaTime * 2.5f, 0f);
            else
                speedVector = new Vector3(0f, calculatePaddleMovementBasedOnPaddlesPositionToPrediction()
                    * Time.deltaTime * 2.5f, 0f);



        }
    }



    /* . 
        Follow the ball based on its z position, and have the paddle move, with a speed 
        limit so that it does not move too fast.
    */
	float calculatePaddleMovement()
	{
		float newSpeed = 0.0f, testValue1 = speedWithUpwardsAcceleration(),
		testValue2 = speedWithDownwardsAcceleration();

		if (testValue2 >= -1.0f && testValue1 <= 1.0f) // Speed limits
		{
			if (pongBall.transform.position.z > 
                board.transform.position.z)
				newSpeed = testValue1;
			else
				newSpeed = testValue2;
		}
		
		speedBefore = newSpeed;
		return newSpeed;
	}

    // Bring the paddle to a complete stop over a period of time instead of at the next frame.
    float calculatePaddleMovementAfterBallIsBounced()
    {
        float newSpeed = 0.0f, testValue1 = speedWithUpwardsAcceleration(),
        testValue2 = speedWithDownwardsAcceleration();

        if (testValue2 >= -1.0f && testValue1 <= 1.0f) // Speed limits
        {
            if (speedVector.y < -0.05f)
                newSpeed = testValue1;
            else if (speedVector.y > 0.05f)
                newSpeed = testValue2;
            else
                newSpeed = 0.0f;
        }

        speedBefore = newSpeed;
        return newSpeed;
    }

    // On Medium and Hard Levels, trace the paddle's position, then react apppropriately.
    float calculatePaddleMovementBasedOnPaddlesPositionToPrediction()
    {
        float newSpeed = 0.0f, testValue1 = speedWithUpwardsAcceleration(),
        testValue2 = speedWithDownwardsAcceleration();

        if (testValue2 >= -2.0f && testValue1 <= 2.0f) // Speed limits
        {
            // The paddle is at the predicted z coordinate and should stop moving.
            if (Mathf.Abs(predictedZPositionOfWhereTheBallWillBe - transform.position.z) < 0.02f)
                newSpeed = 0.0f;
            // The paddle is below the midpoint of acceleration that's above.
            else if (transform.position.z < (startingZPosition + predictedZPositionOfWhereTheBallWillBe) / 2)
                newSpeed = testValue1;
            // The paddle is above the midpoint of acceleration that's below.
            else
                newSpeed = testValue2;
        }

        speedBefore = newSpeed;
        return newSpeed;
    }



    /* 
       Predict where the ball would be at the opponent's strike zone by calculating
       the distance the ball will travel overtime in the Z axis, then scaling it down
       repeatedly to within the goal range with the help of loops.

       Initially, this would be done using Unity raycasts, but they were difficult
       to deal with even though it would involve less code and loops.
    */
    float ballsPredictedZPositionInOpponentZone()
    {
        // Scalar for Speed Vector
        float scalarForSpeedVector = Mathf.Abs(transform.position.x - pongBall.getPosition().x) 
            / Mathf.Abs(pongBall.getSpeedVector().x);

        // Scaled Y-Value for speed vector
        float scaledZValueForSpeedVector = (scalarForSpeedVector * pongBall.getSpeedVector()).z;

        // Retrieve the Y value based on these float values
        if (scaledZValueForSpeedVector + pongBall.getPosition().z < 2.35f && scaledZValueForSpeedVector + pongBall.getPosition().z > -2.35f)
            return scaledZValueForSpeedVector + pongBall.getPosition().z;

        // The ball is moving downwards and has to traverse across the width of the board at least once.
        if (scaledZValueForSpeedVector < 0.0f && Mathf.Abs(scaledZValueForSpeedVector) >= 4.8f)
        {
            while (scaledZValueForSpeedVector + pongBall.getPosition().z <= -2.35f)
                scaledZValueForSpeedVector += 4.8f;

            return scaledZValueForSpeedVector + pongBall.getPosition().z;
        }

        // The ball is moving upwards and has to traverse across the width of the board at least once.
        if (scaledZValueForSpeedVector > 0.0f && Mathf.Abs(scaledZValueForSpeedVector) >= 4.8f)
        {
            while (scaledZValueForSpeedVector + pongBall.getPosition().z >= 2.35f)
                scaledZValueForSpeedVector -= 4.8f;

            return scaledZValueForSpeedVector + pongBall.getPosition().z;
        }


        // Otherwise, simply find where the ball would be based on relative positioning and
        // how much farther the ball has to go in the Z axis.
        displacementFromBallToWall = 2.35f - Mathf.Abs(pongBall.getPosition().z);

        if (scaledZValueForSpeedVector < 0.0f && scaledZValueForSpeedVector >= -4.8f)
        {
            scaledZValueForSpeedVector += displacementFromBallToWall;
            return -2.35f - scaledZValueForSpeedVector;
        }


        scaledZValueForSpeedVector -= displacementFromBallToWall;
        return 2.35f - scaledZValueForSpeedVector;
    }
    

    // Get the next instant speeds based on what was the speed before and the change over the time frame.
    float speedWithUpwardsAcceleration()
    {
        return speedBefore + accelerationValue * Time.deltaTime;
    }

    float speedWithDownwardsAcceleration()
    {
        return speedBefore - accelerationValue * Time.deltaTime;
    }



    /* 
        We check to see whether the ball is heading towards the paddle upon detection by 
        checking whether the speed vector's x component is positive, as this paddle is
        always on the right side of the board.
    */
    bool ballIsHeadingTowardsPaddle()
    {
        return pongBall.getSpeedVector().x >= 0.0f;
    }

}
