using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Ball : MonoBehaviour
{
    [SerializeField] Paddle paddle;
    [SerializeField] GameManager gameManager;

    public bool hasStarted = false;

    [SerializeField] bool startWithRandomVelocityTF = true;
    [SerializeField] Vector2 staticStartVelocity = new Vector2(1f,7.5f);

    [SerializeField] Vector2 launchVelocityMin = new Vector2(-2f, 15f);
    [SerializeField] Vector2 launchVelocityMax = new Vector2(2f, 15f);

    new Rigidbody2D rigidbody;
    [SerializeField] AudioClip[] bounceSounds;

    // ------- Ball Changing Velocity
    [Header("Ball Velocity Increase Settings")]

    [Tooltip("Include ball speed increase in game? True/False")]
    [SerializeField] public bool increaseBallSpeedTF = true;
        
    [Tooltip("How frequently the ball speed is increased, in seconds")]
    [Range(0.1f,15)]
    [SerializeField] float ballSpeedIncreaseIncrementInSeconds = 2.5f;

    [Tooltip("% the game speed increases from prior game speed, after each increment.")]
    [Range(0,25)]
    [SerializeField] float ballSpeedIncreasePctEachIncrement = 5f;

    // To keep track of when the last speed increase was to determine if a new speed incr. is needed
    private float previousIncreaseTimeInSeconds = 0;
    // ----------------------------------------------------------

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        gameManager = FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!hasStarted)
        {
            transform.localPosition = new Vector2(paddle.transform.localPosition.x, transform.localPosition.y);
        }
        else 
        {
            IncreaseBallSpeed();
        }

    }

    public void LaunchBall()
    {
        if (!hasStarted)
        {
            hasStarted = true;
            SetStartingVelocity();
        }
    }

    public void SetStartingVelocity()
    {
        if(startWithRandomVelocityTF)
        {
            SetRandomStartingVelocity();
        }
        else 
        {
            SetStaticStartingVelocity();
        }
    }

    public void SetRandomStartingVelocity()
    {
        float launchVelocityX = UnityEngine.Random.Range(launchVelocityMin.x, launchVelocityMax.x);
        float launchVelocityY = UnityEngine.Random.Range(launchVelocityMin.y, launchVelocityMax.y);
        rigidbody.velocity = new Vector2(launchVelocityX, launchVelocityY);
    }

    public void SetStaticStartingVelocity()
    {
        rigidbody.velocity = new Vector2(staticStartVelocity[0], staticStartVelocity[1]);
    }

    public bool IsNextSpeedIncreaseIncrement()
    {
        return (Time.time - previousIncreaseTimeInSeconds) > ballSpeedIncreaseIncrementInSeconds;
    }

    public void SetPreviousIncreaseTime()
    {
        previousIncreaseTimeInSeconds = Time.time;
    }

    public void SetNewBallVelocity()
    {
        rigidbody.velocity *= (1 + (float)ballSpeedIncreasePctEachIncrement/100);
    }

    public void IncreaseBallSpeed()
    {
        if(IsNextSpeedIncreaseIncrement() && increaseBallSpeedTF)
        {
            Debug.Log("Ball speed has been increased.");
            SetPreviousIncreaseTime();
            SetNewBallVelocity();
        }        
    }

    public void ResetBall()
    {
        hasStarted = false;
        if (rigidbody)
            rigidbody.velocity = Vector2.zero;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (gameManager.trainingMode)
            return;
            
        if (collision.gameObject.tag != "Lose Collider")
        {
            if (bounceSounds.Length > 0 && AudioManager.Instance)
            {
                AudioManager.Instance.PlaySound(bounceSounds[UnityEngine.Random.Range(0, bounceSounds.Length)]);
            }
        }
    }


}
