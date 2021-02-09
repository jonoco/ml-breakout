using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BoundaryName {
    Default,
    Ceiling,
    Wall,
}

public class BoundaryCollider : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] PlayerSupervisor playerSupervisor;

    public BoundaryName boundaryName = BoundaryName.Default;

    private void Start()
    {
        playerSupervisor = FindObjectOfType<PlayerSupervisor>();    
    }

    private void OnCollisionEnter2D(Collision2D other) 
    {
        playerSupervisor.BoundaryHit(boundaryName);
    }
}
