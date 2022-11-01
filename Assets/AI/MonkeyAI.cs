using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Poker.Math;
using Poker.Analytics;

namespace Poker
{
    namespace AI
    {
        public class MonkeyAI : PokerAI
        {
            public MonkeyAI(Table Table) : base(Table)
            {

            }

            //PLAYSTYLE
            float _tightness;
            float Tightness  //Loose - Tight
            {
                get { return _tightness; }
                set { _tightness = value; }
            }

            float _aggression;
            float Aggression //Passive - Aggressive
            {
                get { return _aggression; }
                set { _aggression = value; }
            }

            //CREATIVITY //Predictable - Unpredictable = Creativity describes how often this player deviates from his usual playstyle.

            protected override PokerAction DeterminePreflopAction()
            {
                return PreflopMonkeyAction();
            }

            protected override PokerAction DetermineFlopAction()
            {
                return PreflopMonkeyAction();
            }

            protected override PokerAction DetermineTurnAction()
            {
                return PreflopMonkeyAction();
            }

            protected override PokerAction DetermineRiverAction()
            {
                return PreflopMonkeyAction();
            }

            private PokerAction PreflopMonkeyAction()
            {
                Tightness = 0.5f; // 0 - 1
                Aggression = 0.5f; // 0 - 1

                int minCountVillains = Players.CountMinAmountOfOpponentsCalling(Hero.ID);
                int maxCountVillains = Players.ActiveList.Count - 1;
                //Table.RaisesThisRound();

                double bet = Table.MinBet - Hero.CurrentBet;
                double pot = Pot.TempAmount;
                double pPot = Pot.MaxPotentialAmount;
                double stackSize = Hero.Chips;
                double effStackSize = Players.GetEffectiveStackSize(Hero.ID); //Most amount of chips you can lose in a hand
                double M = effStackSize / (Table.SBAmount + Table.BBAmount + Table.Ante * Players.ListCount()); //amount of rounds you can survive till blinded out             
                double mRatio = M * Players.ListCount() / 10;
                double qRatio = stackSize / Players.GetAvgStackSize();

                double PotAfterBet = bet + pot;
                double PotOdds = bet / PotAfterBet;
                double pPotOdds = bet / pPot;            //use this for fold threshold
                double pPotOdds_2 = Table.MinBet / pPot; //use this for raise threshold (so people with the option to check dont just raise because of good pot odds)

                handRanges = handRanges.GetHandRangesBasedOnPotOdds(Players, GameHistory, Hero.ID, Hero.Hand.value, Table.Board.value, CurrentBettingRound); //CONTINUE WIHT OLD HAND RANGES INSTEAD OF RESETTING
                double WinOdds = PokerMath.WinOddsHandRange(handRanges, Hero.Hand.value, Table.Board.value, 0UL, Players.GetVillainList(Hero.ID), 0.1);
                double EV = PokerMath.ExpectedValue(bet, pPot, WinOdds, 1 - WinOdds);

                double posStrength = Players.GetPosStrength(Hero.ID);                         // 0.1 - 1     | smallblind -> dealer
                double foldMultiplier = Lerp(1.5, 0.5, Average(posStrength, Tightness));      // 0.5 - 1.5   | threshold to fold increases depending on tightness/posStrength
                double raiseMultiplier = Lerp(0.5, 1.5, Average(posStrength, Aggression));    // 0.5 - 1.5   | threshold to raise increases depending on aggression/posStrength
                double avgBetMultiplier = 1 + Table.Players.GetCallingPlayersBetPercentage(); // 1 - 2       | threshold to call/raise decreases depending how much chips of his stack villain invested
                double mMultiplier = Lerp(1, 1.5, Clamp(0, 1, Normalize(1, 20, mRatio)));                               // 1 - 1.5     | aggression increases the lower the mRatio

                double foldThreshold = pPotOdds * foldMultiplier * avgBetMultiplier;
                double raiseThreshold = pPotOdds_2 * mMultiplier * (2 - raiseMultiplier) * avgBetMultiplier;

                Debug.Log($"Hand: {Hand.debug} | pPotOdds: {Round(pPotOdds)}% | WinOdds: {Round(WinOdds)}% | FoldThreshold: {Round(foldThreshold)}% | RaiseThreshold: {Round(raiseThreshold)}% |");
                //Debug.Log(mMultiplier * (2 - raiseMultiplier) * avgBetMultiplier); //you could use this to decrease handranges after raise

                //FOLD THRESHOLD
                if (WinOdds < foldThreshold)
                {
                    _betAmount = 0;
                    return PokerAction.Fold;
                }

                //RAISE THRESHOLD
                if (WinOdds > raiseThreshold)
                {
                    double stackSizeRatio = Normalize(Players.GetLowStack(), Players.GetHighStack(), stackSize);
                    double twoBetSize = Lerp(2, 3, stackSizeRatio);
                    _betAmount = (uint)Round(Table.MinBet * twoBetSize * raiseMultiplier);
                    return PokerAction.Raise;
                }

                //CALL THRESHOLD
                _betAmount = Table.MinBet;
                return PokerAction.Call;
            }

            private PokerAction PostFlopMonkeyAction()
            {
                return PokerAction.Call;
            }

            private PokerAction RandomAction()
            {
                int rnd = Random.Range(0, 6);
                if (rnd == 0)
                {
                    return PokerAction.Fold;
                }
                else if (rnd == 1)
                {
                    _betAmount = 50;
                    return PokerAction.Call;
                }
                else
                {
                    return PokerAction.Call;
                }
            }
        }
    }
}
