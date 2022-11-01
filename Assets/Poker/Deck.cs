using System;
using System.Collections.Generic;
using UnityEngine;

namespace Poker
{
	public class Deck
	{
		public Card[] cards;
		private int position;
		
		public Deck()
		{
			cards = new Card[52];
			for (int i = 0; i < 52; i++) cards[i].value = 1ul << i;
			position = 0;
		}

		public void Shuffle(System.Random random)
		{
			int n = cards.Length;
			while (n > 1)
			{
				n--;
				int k = random.Next(n + 1);
				ulong value = cards[k].value;
				cards[k] = cards[n];
				cards[n].value = value;
			}
			position = 0;
		}

		public List<Card> Draw(int count)
		{
			List<Card> drawingCards = new List<Card>();
            for (int i = 0; i < count; i++)
            {
				if (position < 52)
				{
					drawingCards.Add(cards[position]);
					position++;
				}
			}
			return drawingCards;
		}
	}
}
