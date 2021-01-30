using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    [SerializeField] Paddle paddle;

    public bool hasStarted = false;

    [SerializeField] Vector2 launchVelocityMin = new Vector2(-2f, 15f);
    [SerializeField] Vector2 launchVelocityMax = new Vector2(2f, 15f);

    new Rigidbody2D rigidbody;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
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
        if (!hasStarted)
        {
            hasStarted = true;
            float launchVelocityX = UnityEngine.Random.Range(launchVelocityMin.x, launchVelocityMax.x);
            float launchVelocityY = UnityEngine.Random.Range(launchVelocityMin.y, launchVelocityMax.y);
            rigidbody.velocity = new Vector2(launchVelocityX, launchVelocityY);
        }
    }

    public void ResetBall()
    {
        hasStarted = false;
        if (rigidbody)
            rigidbody.velocity = Vector2.zero;
    }
}
