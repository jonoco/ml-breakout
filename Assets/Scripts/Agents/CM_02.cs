using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CM_02 : PlayerAgent
{
    void Awake()
    {
        timeLimit = 120.0f;
        blockReward = 0.5f;
        loseBallPenalty = -0.5f;
        paddleReward = 0.001f;
        timeoutPenalty = 0.0f;
    }
}
