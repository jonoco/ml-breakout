using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CM_03 : PlayerAgent
{
    void Awake()
    {
        timeLimit = 120.0f;
        blockReward = 5.0f;
        loseBallPenalty = -50.0f;
        paddleReward = 0.1f;
        timeoutPenalty = 0.0f;
    }
}
