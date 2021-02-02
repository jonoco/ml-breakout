using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paddle : MonoBehaviour
{
    [SerializeField] PlayerSupervisor playerSupervisor;
    private void Start() 
    {
        playerSupervisor = FindObjectOfType<PlayerSupervisor>();
    }

    private void OnCollisionEnter2D(Collision2D other) 
    {
        playerSupervisor.PaddleHit();
    }
}
