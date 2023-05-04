using Poker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPoker
{
    public class Poker
    {
        //GameStateManager gameStateManager;
        Players players { get; set; }
        Deck deck { get; set; }
        PokerLogic pokerLogic { get; set; }

        public Poker()
        {
            // Create PlayerList
            Debug.Log("___CREATE PLAYERLIST___");
            players = new Players(4);            

            // Create Deck
            Debug.Log("___CREATE DECK___");
             
            // Create PokerLogic
            Debug.Log("___CREATE POKERLOGIC___");
            pokerLogic = new PokerLogic(players, deck);

            // Start Round

            // Shuffle Deck
            deck = new Deck();
            deck.Shuffle(new System.Random());

            // Set Blinds
            pokerLogic.SetDealer(0);

            // Deal Cards
            players.DealCards(deck);
            

            // Betting Round - Pre-Flop

            // Collect Pot

            // Show Flop

            // Betting Round - Flop

            // Collect Pot

            // Show Turn

            // Betting Round - Turn

            // Collect Pot

            // Show River

            // Betting Round - River

            // Collect Pot

            // Reveal Cards

            // Evaluate Winners

            // Payout Pot

            // End Round
        }
    }
}
