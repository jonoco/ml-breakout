using UnityEngine;

public class LoseCollider : MonoBehaviour
{
    [SerializeField] PlayerSupervisor playerSupervisor;

    private void Start()
    {
        if (!playerSupervisor)
            playerSupervisor = FindObjectOfType<PlayerSupervisor>();    
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        playerSupervisor.LoseColliderHit();
    }
}
