using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Menu;
using Unity.Collections;
using UnityEditor.U2D.Path;

namespace UnityPoker
{
    public class Deck
    {
        public Deck()
        {
            deck = new Card[52];
            for (int i = 0; i < 52; i++) deck[i].value = 1ul << i;
            position = 0;
        }

        public Card[] deck { get; private set; }
        private int position { get; set; }

        public void Shuffle(System.Random random)
        {
            int n = deck.Length;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                ulong value = deck[k].value;
                deck[k] = deck[n];
                deck[n].value = value;
            }
            position = 0;
        }

        public Cards Draw(int count)
        {
            List<Card> drawingCards = new List<Card>();
            for (int i = 0; i < count; i++)
            {
                if (position < 52)
                {
                    drawingCards.Add(deck[position]);
                    position++;
                }
            }
            return new Cards(drawingCards);
        }
    }

    public class Players
    {
        public Players(uint playerCount)
        {
            list = new List<Player>();
            PopulatePlayers(playerCount);
        }

        public List<Player> list { get; private set; }
        public List<Player> activeList => list.FindAll(p => p.IsPlayingThisRound && p.IsPlayingThisGame);
        public int count => list.Count;

        private void PopulatePlayers(uint playerCount)
        {
            if (playerCount > 10) playerCount = 10;

            for (int i = 0; i < playerCount; i++)
            {
                list.Add(new Player());
                list[i].Seat = i; 
                Debug.Log($"Player: {i}");
            }

            Debug.Log(activeList.Count);
        }

        public void DealCards(Deck deck)
        {
            foreach (var player in list)
            {
                if(!player.IsPlayingThisGame)
                {
                    continue;
                }

                if(!player.IsPlayingThisRound)
                {
                    continue;
                }

                player.Hand = deck.Draw(2);
                Debug.Log($"Player: {player.Hand.debug}");
            }
        }
    }

    public class PokerLogic
    {
        public PokerLogic(Players players, Deck deck)
        {
            this.players = players;
            this.deck = deck; 
        }

        public Players players { get; private set; }
        public Deck deck { get; private set; }

        public int dealerSeat { get; private set; }
        public int sbSeat { get; private set; }
        public int bbSeat { get; private set; }

        public void SetDealer(int dealerSeat)
        {
            this.dealerSeat = dealerSeat;

            List<Player> activeList = players.activeList;

            if (activeList.Count == 2)
            {
                sbSeat = dealerSeat;
                bbSeat = activeList[(dealerSeat + 1) % activeList.Count].Seat;
            }
            else
            {
                sbSeat = activeList[(dealerSeat + 1) % activeList.Count].Seat;
                bbSeat = activeList[(dealerSeat + 2) % activeList.Count].Seat;
            }

            Debug.Log($"Dealer: PLAYER {dealerSeat}");
            Debug.Log($"SmallBlind: PLAYER {sbSeat}");
            Debug.Log($"BigBlind: PLAYER {bbSeat}");
        }
    }
}