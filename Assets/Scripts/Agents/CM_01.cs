﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CM_01 : PlayerAgent
{
    void Awake()
    {
        timeLimit = 120.0f;
        blockReward = 0.05f;
        loseBallPenalty = 0.0f;
        paddleReward = 0.0f;
        timeoutPenalty = 0.0f;
    }
}
