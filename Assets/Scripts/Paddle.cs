/* 
    GPD Pong - 3D Pong clone developed using Unity

    By Gregory Desrosiers
    University of Waterloo, Software Engineering 2019


    Paddle Class

    This class holds only common properties between paddles; the 
    board itself, a speed vector, a property called Score, and a 
    collision detector for the north and south walls.


    Development Dates: June 2016 - August 2016
    File Name: Paddle.cs

    Version 1.1 Update: September 25, 2016 - September 28, 2016
    - Resetting the position is now in its own function.

    Special Thanks to the staff who put the tutorials together on
    Unity Technologies and both Unity Forums and Unity Answers
    moderators for giving me the flexibility to ask various questions
    on how different things work and giving me different ways to program.

    http://gregpdessch.github.io/
    Source Code © 2016 Gregory Desrosiers. All rights reserved.

*/

using UnityEngine;
using System.Collections;


public class Paddle : MonoBehaviour
{
    protected Rigidbody board;
    protected Vector3 speedVector;
    public PlayerNumber typeOfPlayer;

    protected int score;
    public int Score
    {
        get
        {
            return score;
        }
        set
        {
            score = value;
        }
    }

    // Get the board Rigidbody component and initialize speed.
    protected void Start()
    {
        board = GetComponent<Rigidbody>();
        speedVector = new Vector3(0.0f, 0.0f, 0.0f);

    }

    // Prevent the paddle from clipping through one of the two walls.
    void OnCollisionEnter(Collision otherObject)
    {
        if (otherObject.gameObject.CompareTag("North Wall") || otherObject.gameObject.CompareTag("South Wall"))
            speedVector = Vector3.zero;
    }

    // Get the paddle's speed vector.
    public Vector3 getSpeedVector()
    {
        return speedVector;
    }

    public void resetPosition()
    {
        transform.position = new Vector3(
            transform.position.x,
            transform.position.y,
            0.0f);
    }

    // Reset the paddle's position and score field when the user wants to restart
    // the match or we have a game over.
    public void reset()
    {
        Score = 0;

        resetPosition();
    }
}
