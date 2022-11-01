using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Poker.Extensions;
using HoldemHand;
using System.Linq;

namespace Poker
{
    public struct Card
    {
        public ulong value { get; set; }
        public int ID => (int) Mathf.Log(value, 2);
    }

    public struct HandCards
    {
        public HandCards(ulong card_0, ulong card_1)
        {
            this.card_0 = card_0;
            this.card_1 = card_1;
        }

        public ulong card_0 { get; set; }
        public ulong card_1 { get; set; }
    }

    public class Cards
    {
        public Cards() { }

        public Cards(List<Card> cards) { list = cards; }

        public List<Card> list { get; set; } = new List<Card>();
        public ulong value => list.Combined();
        public string debug => Hand.MaskToString(value).ShowSuitSymbols();
        public string Debug(BettingRounds bettingRound)
        {
            int num = BoardCardCount(bettingRound);
            if(num == 0) return "";            
            return $"[{new Cards(list.GetRange(0, Mathf.Min(num, list.Count))).debug}]";
        }

        public void Add(List<Card> cards)
        {
            foreach (Card card in cards)
            {
                list.Add(card);
            }
        }

        public int BoardCardCount(BettingRounds bettingRound)
        {
            switch (bettingRound)
            {
                case BettingRounds.PreFlop:
                    return 0;
                case BettingRounds.Flop:
                    return 3;
                case BettingRounds.Turn:
                    return 4;
                case BettingRounds.River:
                    return 5;
                case BettingRounds.Showdown:
                    return 5;
                case BettingRounds.EndGame:
                    return 0;
                default:
                    return 0;
            }   
        }
    }
}
