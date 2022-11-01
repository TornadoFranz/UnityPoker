using System.Collections.Generic;
using UnityEngine;
using Poker.AI;
using Poker.Events;
using Poker.History;

namespace Poker
{
    public enum Identity
    {
        Human,
        AI,
    }

    public class Player
    {
        public int ID { get; private set; } //ID REPRESENTS THE SEAT NR.
        public string Name { get; private set; }
        public double Chips { get; set; }
        public Table table { get; private set; }
        public PokerAI pokerAI { get; set; }
        public Cards Hand { get; set; }
        public bool IsPlayingThisRound { get; set; } = true;
        public bool IsPlayingThisGame { get; set; } = true;
        public bool DidActionThisRound { get; set; }
        public bool IsAllIn { get; set; }
        public double AmountInvested { get; set; }
        public double AmountWon { get; set; }
        public uint HandRank { get; set; }
        public string CurrentAction { get; private set; }
        public PokerAction Action { get; private set; }
        public Identity identity { get; set; }
        public double CurrentBet { get; set; }
        public int Position { get; set; }
        public double TotalChips => Chips + AmountInvested;
        public double PercentAmountInvested => AmountInvested / TotalChips;

        public PokerEvents pokerEvents => table.pokerEvents;


        public Player(Table table, int ID, string Name, double Chips, PokerAI pokerAI, Identity identity)
        {
            this.table = table;
            this.ID = ID;
            this.Name = Name;
            this.Chips = Chips;
            this.pokerAI = pokerAI;
            this.identity = identity;
        }

        public void Fold()
        {
            CurrentAction = "FOLD";
            Action = PokerAction.Fold;
            IsPlayingThisRound = false;
            ResolveAction();
        }

        public void Call()
        {            
            CurrentAction = "CALL";
            Action = PokerAction.Call;
            Bet(table.MinBet - CurrentBet);
            ResolveAction();
        }

        public void Raise(uint raiseAmount)
        {
            CurrentAction = "RAISE";
            Action = PokerAction.Raise;
            Bet(System.Math.Max(raiseAmount, table.MinRaise) - CurrentBet);
            ResolveAction();
        }

        public void Bet(double betAmount)
        {
            if(betAmount == 0)
            {
                //Action = PokerAction.Check;
                CurrentAction = "CHECK";
            }

            if(betAmount >= Chips) //CANT BET MORE THEN CHIP AMOUNT
            {
                //Action = PokerAction.AllIn;
                CurrentAction = "ALLIN";
                betAmount = Chips;
                IsAllIn = true;
            }

            if(betAmount+CurrentBet > table.MinBet) //CHECK FOR RAISE
            {
                table.MinBet = betAmount + CurrentBet;
                table.RaisesThisRound++;
                table.RaisesThisGame++;
                table.Players.ResetAllActions();
            }

            Chips -= betAmount;
            AmountInvested += betAmount;
            CurrentBet += betAmount;
            pokerEvents.Bet(this);
        }

        public void AIAction()
        {
            if (table.GameOn)
            {
                if (Chips == 0)
                {
                    Call();
                    return;
                }

                if (identity == Identity.AI)
                {
                    PokerAction pokerAction = pokerAI.DetermineAction();
                    table.PlayerAction(pokerAction, pokerAI.BetAmount);
                }
            }
        }

        public void HumanAction()
        {
            if (table.GameOn)
            {
                if (Chips == 0)
                {
                    Call();
                    return;
                }

                if (identity == Identity.Human)
                {
                    pokerEvents.NewTurn(table);
                    pokerEvents.HeroTurn();
                }
            }
        }

        public void ResetPlayer()
        {
            IsPlayingThisRound = true;
            IsAllIn = false;
            DidActionThisRound = false;
            CurrentAction = "";
            AmountInvested = 0;
            AmountWon = 0;
            HandRank = 0;
            Hand = null;

            if (Chips == 0) //BOOT PLAYERS IF OUT OF MONEY
            {
                IsPlayingThisRound = false;
                IsPlayingThisGame = false;
            }
        }

        private void ResolveAction()
        {
            //DO HISTORY DATA CALCULATIONS HERE AND THEN SAVE IT AT HISTOYDATA
            //Debug.Log(table.minBet - CurrentBet);
            //table.GameHistory.Add(table.CurrentBettingRound, new HistoryData(ID, table.Pot.TempAmount, table.Pot.MaxPotentialAmount, CurrentBet , table.MinBet, Action, table.Players.ActiveList.Count)); //TODO: cur and min bet already changed value at this point, find solution for this...
            DidActionThisRound = true;
            table.CheckForRoundEnd();

            if (table.Hero != null)
            {
                if (table.Players.MultipleHumansLeft())
                {
                    if (table.Hero.ID != ID) table.pokerEvents.SetHeroHand(table.Hero.Hand.list, table.HeroOn); //SWAP HANDS IN CASE MULTIPLE PLAYUER CONTROLED SEATS AT TABLE

                }
            }
        }
    }
}

