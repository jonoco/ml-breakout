using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class AgentRicky : PlayerAgent
{
    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);

        float distanceToBall = Vector2.Distance(paddle.transform.position, ball.transform.position);
        sensor.AddObservation(distanceToBall);
    }
}
