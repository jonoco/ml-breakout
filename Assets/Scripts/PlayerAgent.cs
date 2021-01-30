using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class PlayerAgent : Agent
{
    [SerializeField] PlayerSupervisor playerSupervisor;
    [SerializeField] Ball ball;
    [SerializeField] Paddle paddle;
    public float minPaddlePosX = 1f;
    public float maxPaddlePosX = 15f;
    public float screenWidth = 16f;
    public float screenHeight = 12f;
    public float paddleMoveSpeed = 100f;
    public bool playerReady =  false;
    public float blockReward = .1f;
    public float losePenalty = -1f;
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

    public void MovePaddle(float pos)
    {
        // Calculate the eased paddle movement
        smoothMovementChange = Mathf.MoveTowards(smoothMovementChange, pos, 2f * Time.fixedDeltaTime);
        
        // Calculate the new paddle position
        Vector3 paddlePos = paddle.transform.position;
        paddlePos.x = paddlePos.x + smoothMovementChange * Time.fixedDeltaTime * paddleMoveSpeed;
        paddlePos.x = Mathf.Clamp(paddlePos.x, minPaddlePosX, maxPaddlePosX);
        paddle.transform.position = paddlePos;
    }

    public void StartGame()
    {   
        if (!playerReady)
        {
            playerReady = true;
            playerSupervisor.PlayerReady();
        }
    }

    public void BlockHit()
    {
        AddReward(blockReward);
    }

    public void PaddleHit()
    {
        AddReward(paddleReward);
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
