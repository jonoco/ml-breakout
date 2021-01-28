using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] GameManager gameManager;
    [SerializeField] Paddle paddle;
    [SerializeField] Ball ball;

    [SerializeField] float minPaddlePosX = 0f;
    [SerializeField] float maxPaddlePosX = 16f;

    public float paddleMoveSpeed = 1f;

    public bool ballLaunched =  false;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        // Vector2 paddlePos = new Vector2(Input.mousePosition.x / Screen.width * 16f, paddle.transform.position.y);
        // paddlePos.x = Mathf.Clamp(paddlePos.x, minPaddlePosX, maxPaddlePosX);
        // paddle.transform.position = paddlePos;

        // if (Input.GetMouseButtonDown(0))
        // {
        //     LaunchBall();
        // }
    }

    public void MovePaddle(float pos)
    {
        Vector2 paddlePos = new Vector2(pos * 16f, paddle.transform.position.y);
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
}
