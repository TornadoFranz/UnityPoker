using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Poker;
using System;

public struct HandWinOdds
{
    public HandWinOdds(ulong hand, double winOdds)
    {
        this.hand = hand;
        this.winOdds = winOdds;
    }

    public ulong hand { get; set; }
    public double winOdds { get; set; }
}

