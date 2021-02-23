using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class PlayerAgent : Agent
{
    public PlayerSupervisor playerSupervisor;
    public Ball ball;
    public Paddle paddle;

    [Header("Training rules")]

    [Tooltip("Limit to wait before ending training (0 eliminates timeout)")]
    [Range(0f, 300f)]
    public float timeLimit = 0f;
    public float blockReward = .5f;
    public float loseBallPenalty = -10f;
    public float paddleReward = .1f;
    public float timeoutPenalty = 0f;
    
    [Tooltip("Reward per second for play duration")]
    public float timeRewardFactor = 0f;

    private void Start() 
    {
        if (!playerSupervisor)
            playerSupervisor = FindObjectOfType<PlayerSupervisor>();
        
        if(!ball)
            ball = FindObjectOfType<Ball>();
        
        if(!paddle)
            paddle = FindObjectOfType<Paddle>();
    }

    public override void OnEpisodeBegin()
    {
        // Reset any Agent state
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (!paddle || !ball)
            return;

        // Paddle x-axis position [0-1]
        sensor.AddObservation(paddle.transform.localPosition.x / playerSupervisor.instanceWidth);

        // Ball x and y-axis positions [0-1]
        sensor.AddObservation(ball.transform.localPosition.x / playerSupervisor.instanceWidth);
        sensor.AddObservation(ball.transform.localPosition.y / playerSupervisor.instanceHeight);
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        // Determine paddle position
        float paddleXPos = vectorAction[0]; 
        
        // Determine whether to launch the ball and start the game
        bool launchBall = vectorAction[1] > 0;             

        paddle.MovePaddle(paddleXPos);
        if (launchBall)
            StartGame();
    }

    public override void Heuristic(float[] actionsOut)
    {
        float mousePos = (Camera.main.ScreenToWorldPoint(Input.mousePosition).x - transform.position.x) / playerSupervisor.instanceWidth;
        mousePos = Mathf.Clamp(mousePos, 0, 1);

        // Normalize paddle input to [-1, 1]
        float paddlePos = mousePos - (paddle.transform.localPosition.x / playerSupervisor.instanceWidth);
         
        bool launchBall = Input.GetMouseButton(0);
        
        actionsOut[0] = paddlePos;
        actionsOut[1] = launchBall ? 1 : -1;
    }

    public virtual void StartGame()
    {   
        playerSupervisor.PlayerReady();
    }

    public virtual void LoseGame()
    {
        EndEpisode();
    }

    public virtual void WinGame()
    {
        EndEpisode();
    }

    public virtual void BlockHit()
    {
        AddReward(blockReward);
    }

    public virtual void PaddleHit()
    {
        AddReward(paddleReward);
    }

    public virtual void LoseBall()
    {
        AddReward(loseBallPenalty);
    }

    public virtual void Timeout()
    {
        AddReward(timeoutPenalty);
    }

    public virtual void TotalPlayTime(float playTime) 
    {
        AddReward(playTime * timeRewardFactor);
    }
}
