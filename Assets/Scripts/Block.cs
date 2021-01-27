using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{

    [SerializeField] PlayerSupervisor playerSupervisor;
    [SerializeField] GameObject particleVFX;

    // The number of points destroying this block is worth.
    [SerializeField] int pointValue = 10;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("THIS IS A TEST");
        playerSupervisor = FindObjectOfType<PlayerSupervisor>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D other) 
    {
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
