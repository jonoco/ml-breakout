using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class AgentRicky : PlayerAgent
{
    public override void CollectObservations(VectorSensor sensor)
    {
         if (!paddle || !ball)
            return;
        
        base.CollectObservations(sensor);

        // Direction of ball
        Vector2 ballDir = ball.GetComponent<Rigidbody2D>().velocity.normalized;
        sensor.AddObservation(ballDir);
        
        // Distance of ball to the paddle; use the diagonal length as maximum distance
        float distanceToBall = Vector2.Distance(paddle.transform.position, ball.transform.position) / playerSupervisor.instanceDiagonalSize;
        sensor.AddObservation(distanceToBall);
    }
}
