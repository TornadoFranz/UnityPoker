using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Poker;
using Poker.Math;

namespace Poker
{
    namespace Analytics
    {
        public class HandAnalysis
        {
            public HandAnalysis(double Bet, double Pot, int VillainCount, ulong Hand, ulong Board, int raisesThisGame)
            {
                this.VillainCount = VillainCount;
                this.Bet = Bet;
                this.Pot = Pot;

                PotAfterBet = Bet + Pot;
                PotOdds = Bet / PotAfterBet;
                WinOdds = PokerMath.WinOdds(Hand, Board, 0, VillainCount, 0.5);
                double decreaseWinOdds = 1+raisesThisGame * 0.1;
                WinOdds /= decreaseWinOdds;
                Equity = PotAfterBet * WinOdds;
                EV = PokerMath.ExpectedValue(Bet, Pot, WinOdds, 1 - WinOdds);
                MaxEV = PotAfterBet * (1 - PotOdds);
                AvgWinOdds = (double)1/(VillainCount+1);
            }

            public double VillainCount { private set; get; }
            public double Bet { private set; get; }
            public double Pot { private set; get; }
            public double PotAfterBet { private set; get; }
            public double PotOdds { private set; get; }
            public double WinOdds { private set; get; }
            public double Equity { private set; get; }
            public double EV { private set; get; }
            public double MaxEV { private set; get; }
            public double AvgWinOdds { private set; get; }

            private double Round(double value) => System.Math.Round(value, 2);

            public void DebugResults()
            {
                Debug.Log($"Pot Odds: {Round(PotOdds * 100)}% | Win Odds: {Round(WinOdds * 100)}% | Pot: {PotAfterBet}$ | Equity: {Round(Equity)}$ | EV: {Round(EV)}$ | MaxEV: {MaxEV}");
            }

        }
    }
}
