﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class PlayerAgent : Agent
{
    public PlayerSupervisor playerSupervisor;
    public Ball ball;
    public Paddle paddle;
    public float minPaddlePosX = 1f;
    public float maxPaddlePosX = 15f;
    public float screenWidth = 16f;
    public float screenHeight = 12f;
   
    [Range(10f, 200f)]
    public float paddleMoveSpeed = 100f;

    [Range(.1f, 10f)]
    public float moveStep = 2f;
    public bool playerReady =  false;
    public float blockReward = .5f;
    public float losePenalty = -10f;
    public float paddleReward = .1f;
    private float smoothMovementChange = 0f;

    private void Awake()
    {
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController)
            Destroy(playerController.gameObject);    
    }

    private void Start() 
    {
        playerSupervisor = FindObjectOfType<PlayerSupervisor>();
        ball = FindObjectOfType<Ball>();
        paddle = FindObjectOfType<Paddle>();
    }

    public override void OnEpisodeBegin()
    {
        // Reset any Agent state

        playerReady = false;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Paddle x-axis position [0-1]
        sensor.AddObservation(paddle.transform.position.x / screenWidth);

        // Ball x and y-axis positions [0-1]
        sensor.AddObservation(ball.transform.position.x / screenWidth);
        sensor.AddObservation(ball.transform.position.y / screenHeight);
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        // Determine paddle position
        float paddleXPos = vectorAction[0]; 
        
        // Determine whether to launch the ball and start the game
        bool launchBall = vectorAction[1] > 0;             

        MovePaddle(paddleXPos);
        if (launchBall)
            StartGame();
    }

    public override void Heuristic(float[] actionsOut)
    {
        // Normalize paddle input to [-1, 1]
        float paddlePos = (Input.mousePosition.x / Screen.width) - (paddle.transform.position.x / screenWidth);
         
        bool launchBall = Input.GetMouseButton(0);
        
        actionsOut[0] = paddlePos;
        actionsOut[1] = launchBall ? 1 : -1;
    }

    /// <summary>
    /// Move the player's paddle
    /// </summary>
    /// <param name="pos">Relative position in the range [-1, 1]</param>
    public virtual void MovePaddle(float pos)
    {
        // Calculate the eased paddle movement
        smoothMovementChange = Mathf.MoveTowards(smoothMovementChange, pos, moveStep * Time.fixedDeltaTime);
        
        // Calculate the new paddle position
        Vector3 paddlePos = paddle.transform.position;
        paddlePos.x = paddlePos.x + smoothMovementChange * Time.fixedDeltaTime * paddleMoveSpeed;
        paddlePos.x = Mathf.Clamp(paddlePos.x, minPaddlePosX, maxPaddlePosX);
        paddle.transform.position = paddlePos;
    }

    public virtual void StartGame()
    {   
        if (!playerReady)
        {
            playerReady = true;
            playerSupervisor.PlayerReady();
        }
    }

    public virtual void BlockHit()
    {
        AddReward(blockReward);
    }

    public virtual void PaddleHit()
    {
        AddReward(paddleReward);
    }

    public virtual void LoseGame()
    {
        AddReward(losePenalty);
        EndEpisode();
    }

    public virtual void WinGame()
    {
        EndEpisode();
    }
}