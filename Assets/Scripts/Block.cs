using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{

    [SerializeField] PlayerSupervisor playerSupervisor;

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
        playerSupervisor.BlockDestroyed();
        Destroy(gameObject);
    }
}
