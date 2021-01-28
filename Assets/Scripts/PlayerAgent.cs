using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class PlayerAgent : Agent
{
    [SerializeField] GameManager gameManager;
    [SerializeField] Ball ball;
    [SerializeField] Paddle paddle;
    public float minPaddlePosX = 0f;
    public float maxPaddlePosX = 16f;
    public float screenWidth = 16f;
    public float screenHeight = 12f;
    public float paddleMoveSpeed = 100f;
    public bool ballLaunched =  false;
    public float blockReward = .1f;
    public float losePenalty = -1f;

    private float smoothMovementChange = 0f;


    private void Start() 
    {
         gameManager = FindObjectOfType<GameManager>();
         ball = FindObjectOfType<Ball>();
         paddle = FindObjectOfType<Paddle>();
    }

    public override void OnEpisodeBegin()
    {
        ballLaunched = false;
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
        float paddleXPos = (vectorAction[0] + 1f) / 2f;     // Normalize to [0-1]
        bool launchBall = vectorAction[1] > 0;              // Determine whether to launch the ball

        MovePaddle(vectorAction[0]);
        if (launchBall)
            LaunchBall();
    }

    public override void Heuristic(float[] actionsOut)
    {
        // Normalize to [-1, 1]
        float paddlePos = (Input.mousePosition.x / Screen.width) - (paddle.transform.position.x / screenWidth);
         
        bool launchBall = Input.GetMouseButton(0);
        
        actionsOut[0] = paddlePos;
        actionsOut[1] = launchBall ? 1 : -1;
    }

    public void MovePaddle(float pos)
    {
        smoothMovementChange = Mathf.MoveTowards(smoothMovementChange, pos, 2f * Time.fixedDeltaTime);
        Vector3 paddlePos = paddle.transform.position;
        paddlePos.x = paddlePos.x + smoothMovementChange * Time.fixedDeltaTime * paddleMoveSpeed;
        paddlePos.x = Mathf.Clamp(paddlePos.x, minPaddlePosX, maxPaddlePosX);
        paddle.transform.position = paddlePos;
    }

    public void LaunchBall()
    {   
        if (!ballLaunched)
        {
            ballLaunched = true;
            gameManager.StartGame();
        }
    }

    public void BlockHit()
    {
        AddReward(blockReward);
    }

    public void LoseGame()
    {
        AddReward(losePenalty);
        EndEpisode();
    }

    public void WinGame()
    {
        EndEpisode();
    }
}
