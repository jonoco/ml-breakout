using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CM_01 : PlayerAgent
{
    void Awake()
    {
        timeLimit = 0f;
        blockReward = .5f;
        loseBallPenalty = -10f;
        paddleReward = .1f;
        timeoutPenalty = 0f;
    }
}
