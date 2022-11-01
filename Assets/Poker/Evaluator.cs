using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoldemHand;
using Poker.Extensions;

namespace Poker
{
    public class Evaluator
    {        
        public void SetHandRanks(PlayerManager players, List<Card> board)
        {
            foreach (var player in players.List)
            {
                if (player.IsPlayingThisRound)
                {
                    player.HandRank = Hand.Evaluate(player.Hand.list.Combined() + board.Combined(), 7);
                }
            }
        }
    }
}

