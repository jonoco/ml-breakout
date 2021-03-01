using UnityEngine;

public class Paddle : MonoBehaviour
{
    [SerializeField] PlayerSupervisor playerSupervisor;

    public float smoothMovementChange = 0f;

    private void Start() 
    {
        if (!playerSupervisor)
            playerSupervisor = FindObjectOfType<PlayerSupervisor>();
    }

    private void OnCollisionEnter2D(Collision2D other) 
    {
        playerSupervisor.PaddleHit();
    }

    /// <summary>
    /// Move the player's paddle
    /// </summary>
    /// <param name="pos">Relative position in the range [-1, 1]</param>
    public virtual void MovePaddle(float pos)
    {
        // Calculate the eased paddle movement
        smoothMovementChange = Mathf.MoveTowards(smoothMovementChange, pos, playerSupervisor.moveStep * Time.fixedDeltaTime);
        
        // Calculate the new paddle position
        Vector3 paddlePos = transform.localPosition;
        paddlePos.x = paddlePos.x + smoothMovementChange * Time.fixedDeltaTime * playerSupervisor.paddleMoveSpeed;
        paddlePos.x = Mathf.Clamp(paddlePos.x, playerSupervisor.minPaddlePosX, playerSupervisor.maxPaddlePosX);
        transform.localPosition = paddlePos;
    }
}
