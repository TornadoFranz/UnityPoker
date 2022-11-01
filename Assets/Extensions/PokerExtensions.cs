using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Poker
{
    namespace Extensions
    {
        public static class PokerExtensions
        {
            public static string ShowSuitSymbols(this string cards)
            {
                foreach (var suit in cards)
                {
                    switch (suit)
                    {
                        case ('c'):
                            cards = cards.Replace('c', '♣');
                            break;
                        case ('d'):
                            cards = cards.Replace('d', '♦');
                            break;
                        case ('h'):
                            cards = cards.Replace('h', '♥');
                            break;
                        case ('s'):
                            cards = cards.Replace('s', '♠');
                            break;
                    }
                }

                return cards;
            }

            public static ulong Combined(this List<Card> cards)
            {
                ulong cardValues = 0;
                for (int i = 0; i < cards.Count; i++)
                {
                    cardValues |= cards[i].value;
                }
                return cardValues;
            }

            public static ulong BitSum(this HashSet<ulong> handRange)
            {
                ulong sum = 0;
                foreach (ulong handMask in handRange)
                {
                    sum |= handMask;
                }
                return sum;
            }
        }
    }
}
