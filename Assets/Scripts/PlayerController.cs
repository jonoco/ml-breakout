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

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 paddlePos = new Vector2(Input.mousePosition.x / Screen.width * 16f, paddle.transform.position.y);
        paddlePos.x = Mathf.Clamp(paddlePos.x, minPaddlePosX, maxPaddlePosX);
        paddle.transform.position = paddlePos;

        if (Input.GetMouseButtonDown(0))
        {
            gameManager.StartGame();
        }
    }
}
