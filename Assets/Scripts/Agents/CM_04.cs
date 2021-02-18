using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class CM_04 : PlayerAgent
{
    void Awake()
    {
        timeLimit = 120.0f;
        blockReward = 5.0f;
        loseBallPenalty = -50.0f;
        paddleReward = 0.1f;
        timeoutPenalty = 0.0f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);

        Block[] blocks = playerSupervisor.GetComponentsInChildren<Block>();
        foreach (Block block in blocks)
        {
            sensor.AddObservation(block.transform.localPosition.x / playerSupervisor.instanceWidth);
            sensor.AddObservation(block.transform.localPosition.y / playerSupervisor.instanceHeight);
        }

    }
}
