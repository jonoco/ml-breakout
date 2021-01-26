using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{

    [SerializeField] PlayerSupervisor playerSupervisor;

    // The number of points destroying this block is worth.
    [SerializeField] int pointValue = 10;

    // Start is called before the first frame update
    void Start()
    {
        playerSupervisor = FindObjectOfType<PlayerSupervisor>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D other) 
    {
        playerSupervisor.BlockDestroyed(pointValue);
        Destroy(gameObject);
    }
}