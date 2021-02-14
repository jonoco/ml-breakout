using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{

    [SerializeField] public PlayerSupervisor playerSupervisor;
    [SerializeField] GameManager gameManager;
    [SerializeField] GameObject particleVFX;

    // The number of points destroying this block is worth.
    [SerializeField] int pointValue = 10;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        if (!playerSupervisor)
            playerSupervisor = FindObjectOfType<PlayerSupervisor>();
    }

    private void OnCollisionEnter2D(Collision2D other) 
    {
        if (!gameManager.trainingMode)
            TriggerParticles();

        playerSupervisor.BlockDestroyed(pointValue);
        Destroy(gameObject);
    }

    private void TriggerParticles()
    {
        GameObject particle = Instantiate(particleVFX, transform.position, transform.rotation);
        ParticleSystem ps = particle.GetComponent<ParticleSystem>();
        Destroy(particle, ps.main.duration + ps.main.startLifetime.constant);
    }
}
