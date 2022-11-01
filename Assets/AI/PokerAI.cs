using Poker.History;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Poker
{
    namespace AI
    {
        public abstract class PokerAI
        {
            //LOOSENESS/TIGHTENESS
            //AGRESSIVE/PASSIVE

            public PokerAI(Table Table)
            {
                this.Table = Table;
            }

            protected double Round(double value) => System.Math.Round(value, 2);
            protected double Lerp(double min, double max, double value) => (min + (max - min) * value);
            protected double Normalize(double min, double max, double value) => (value - min) / (max - min);
            protected double Average(double value_0, double value_1) => (value_0 + value_1) / 2;
            protected double Clamp(double min, double max, double value)
            {
                if (value < min)
                {
                    value = min;
                }
                else if (value > max)
                {
                    value = max;
                }

                return value;
            }


            public Dictionary<int, HashSet<ulong>> handRanges = new Dictionary<int, HashSet<ulong>>();
            protected Table Table { get; private set; }
            protected Cards Hand => Table.CurrentPlayer.Hand;
            protected Cards Board => Table.Board;
            protected BettingRounds CurrentBettingRound => Table.CurrentBettingRound;
            protected PlayerManager Players => Table.Players;
            protected Pot Pot => Table.Pot;
            protected Player Hero => Table.CurrentPlayer;
            protected GameHistory GameHistory => Table.GameHistory;

            protected double _betAmount { get; set; }
            public virtual double BetAmount => _betAmount;
            

            public PokerAction DetermineAction()
            {
                switch (CurrentBettingRound)
                {
                    case BettingRounds.PreFlop:
                        return DeterminePreflopAction();                        
                    case BettingRounds.Flop:
                        return DetermineFlopAction();
                    case BettingRounds.Turn:
                        return DetermineTurnAction();
                    case BettingRounds.River:
                        return DetermineRiverAction();
                    default:
                        Debug.LogError("AI CALLED DURING WRONG BETTING ROUND!");
                        return PokerAction.Fold;
                }
            }

            protected abstract PokerAction DeterminePreflopAction();
            protected abstract PokerAction DetermineFlopAction();
            protected abstract PokerAction DetermineTurnAction();
            protected abstract PokerAction DetermineRiverAction();
        }
    }
}