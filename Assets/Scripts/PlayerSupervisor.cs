using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSupervisor : MonoBehaviour
{
    [SerializeField] Ball ball;
    [SerializeField] GameManager gameManager;
    [SerializeField] int activeBlocks;
    
    private int points = 0;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        activeBlocks = FindObjectsOfType<Block>().Length;
        ball = FindObjectOfType<Ball>();
    }

    public void StartGame()
    {
        LaunchBall();
    }

    public void PauseGame()
    {
        ball.gameObject.SetActive(false);
    }

    void LaunchBall()
    {
        ball.LaunchBall();
    }

    public void LoseColliderHit()
    {
        Destroy(ball.gameObject);
        
        gameManager.LoseGame();
    }

    public void BlockDestroyed(int pointValue)
    {
        points += pointValue;
        gameManager.UpdatePoints(points);

        --activeBlocks;
        if (activeBlocks <= 0)
        {
            gameManager.WinGame();
        }
    }
}
