using Poker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Poker.Events;
public interface IPokerEventListener
{
    PokerEvents pokerEvents { get; }

    void Awake();
    void RegisterPokerEvents();
    void DeregisterPokerEvents();
}
