using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Ball : MonoBehaviour
{
    [SerializeField] Paddle paddle;

    [SerializeField] bool hasStarted = false;

    [SerializeField] Vector2 launchVelocity = new Vector2(2f, 15f);

    [SerializeField] AudioClip[] bounceSounds;


    Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!hasStarted)
        {
            transform.position = new Vector2(paddle.transform.position.x, transform.position.y);
        }
    }

    public void LaunchBall()
    {
        hasStarted = true;
        rb.velocity = launchVelocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag != "Lose Collider")
        {
            AudioManager.Instance.PlaySound(bounceSounds[UnityEngine.Random.Range(0, bounceSounds.Length)]);
        }
    }
}
