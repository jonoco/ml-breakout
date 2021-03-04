using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class AgentLucy : PlayerAgent
{

    public override void CollectObservations(VectorSensor sensor)
    {
         if (!paddle || !ball)
            return;
        
        base.CollectObservations(sensor);

        // Direction of ball
        Vector2 ballDir = ball.GetComponent<Rigidbody2D>().velocity.normalized;
        sensor.AddObservation(ballDir);
        
        // Direction of paddle
        float paddleDir = paddle.smoothMovementChange;
        sensor.AddObservation(paddleDir);
    }
}
