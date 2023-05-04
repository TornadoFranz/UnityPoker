using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Poker.Extensions;
using HoldemHand;
using System.Linq;
using Poker;

namespace UnityPoker
{
    public struct Card
    {
        public ulong value { get; set; }
        public int ID => (int)Mathf.Log(value, 2);        
    }

    public struct Cards
    {
        public Cards(List<Card> list)
        {
            this.list = list;
        }

        public List<Card> list { private set; get; }
        public ulong mask => (ulong)list.Sum(card => (long)card.value);
        public string debug => PokerExtensions.ShowSuitSymbols(HoldemHand.Hand.MaskToString(mask));
    }
}
