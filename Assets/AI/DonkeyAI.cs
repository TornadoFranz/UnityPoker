using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Poker.Math;
using Poker.Analytics;
using Poker.History;

namespace Poker
{
    namespace AI
    {

        public class DonkeyAI : PokerAI
        {

            public DonkeyAI(Table Table, float tightness, float aggression) : base(Table)
            {
                Tightness = tightness;
                Aggression = aggression;
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
                return PreflopDonkeyAction();
            }

            protected override PokerAction DetermineFlopAction()
            {
                return PostflopDonkeyAction();
            }

            protected override PokerAction DetermineTurnAction()
            {
                return PostflopDonkeyAction();
            }

            protected override PokerAction DetermineRiverAction()
            {
                return PostflopDonkeyAction();
            }

            private PokerAction PreflopDonkeyAction()
            {
                int minCountVillains = Players.CountMinAmountOfOpponentsCalling(Hero.ID);
                int maxCountVillains = Players.ActiveList.Count - 1;

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

                //handRanges = handRanges.GetHandRangesBasedOnPotOdds(Players, GameHistory, Hero.ID, Hero.Hand.value, Table.Board.value, CurrentBettingRound); //CONTINUE WIHT OLD HAND RANGES INSTEAD OF RESETTING
                //double WinOdds = PokerMath.WinOddsHandRange(handRanges, Hero.Hand.value, Table.Board.value, 0UL, Players.GetVillainList(Hero.ID), 0.1);


                //Dictionary<ulong, double> preflopWinOdds = PokerMath.GetPreflopWinningOdds(maxCountVillains); //Newtonsoft.Json doesn't work in build, so fuck me right?
                //double WinOdds = preflopWinOdds[Hero.Hand.value];
                double WinOdds = PokerMath.WinOdds(Hero.Hand.value, Table.Board.value, 0UL, maxCountVillains, 0.1);

                double EV = PokerMath.ExpectedValue(bet, pPot, WinOdds, 1 - WinOdds);

                double posStrength = Players.GetPosStrength(Hero.ID);                         // 0.1 - 1     | smallblind -> dealer
                double foldMultiplier = Lerp(1.5, 0.5, Average(posStrength, Tightness));      // 0.5 - 1.5   | threshold to fold increases depending on tightness/posStrength
                double raiseMultiplier = Lerp(0.5, 1.5, Average(posStrength, Aggression));    // 0.5 - 1.5   | threshold to raise increases depending on aggression/posStrength
                double avgBetMultiplier = 1 + Table.Players.GetCallingPlayersBetPercentage(); // 1 - 2       | threshold to call/raise decreases depending how much chips of his stack villain invested
                double mMultiplier = Lerp(1, 1.5, Clamp(0, 1, Normalize(1, 20, mRatio)));                               // 1 - 1.5     | aggression increases the lower the mRatio
                double raiseNrMultiplier = 1 * System.Math.Pow(1.25, Table.RaisesThisRound);


                double foldThreshold = pPotOdds * foldMultiplier * avgBetMultiplier * raiseNrMultiplier;
                double raiseThreshold = pPotOdds_2 * mMultiplier * (2 - raiseMultiplier) * avgBetMultiplier * raiseNrMultiplier;

                raiseThreshold = Clamp(0, 1, raiseThreshold);
                foldThreshold = Clamp(0, 1, foldThreshold);

                //Debug.Log($"Hand: {Hand.debug} | pPotOdds: {Round(pPotOdds)}% | WinOdds: {Round(WinOdds)}% | FoldThreshold: {Round(foldThreshold)}% | RaiseThreshold: {Round(raiseThreshold)}% |");
                //Debug.Log(mMultiplier * (2 - raiseMultiplier) * avgBetMultiplier); //you could use this to decrease handranges after raise


                //Dictionary<ulong, double> preflopWinOdds = PokerMath.GetPreflopWinningOdds(maxCountVillains);
                
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
                    _betAmount = System.Math.Max(Table.MinRaise, (uint)Round(pot * twoBetSize * raiseMultiplier));
                    return PokerAction.Raise;
                }

                //CALL THRESHOLD
                _betAmount = Table.MinBet;
                return PokerAction.Call;
            }

            private PokerAction PostflopDonkeyAction()
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
                double avgWinodds = (double)1 / maxCountVillains; //NOT EXACTLY AVERAGE BUT ITS SUPPOSED TO BE THIS WAY

                double PotAfterBet = bet + pot;
                double PotOdds = bet / PotAfterBet;
                double pPotOdds = bet / pPot;            //use this for fold threshold
                double pPotOdds_2 = Table.MinRaise / pPot; //use this for raise threshold (so people with the option to check dont just raise because of good pot odds)

                //handRanges = handRanges.GetHandRangesBasedOnPotOdds(Players, GameHistory, Hero.ID, Hero.Hand.value, Table.Board.value, CurrentBettingRound); //CONTINUE WIHT OLD HAND RANGES INSTEAD OF RESETTING
                //double WinOdds = PokerMath.WinOddsHandRange(handRanges, Hero.Hand.value, Table.Board.value, 0UL, Players.GetVillainList(Hero.ID), 0.1);
                double WinOdds = PokerMath.WinOdds(Hero.Hand.value, Table.Board.value, 0UL, maxCountVillains, 0.1);
                double EV = PokerMath.ExpectedValue(bet, pPot, WinOdds, 1 - WinOdds);

                double posStrength = Players.GetPosStrength(Hero.ID);                         // 0.1 - 1     | smallblind -> dealer
                double foldMultiplier = Lerp(1.5, 0.5, Average(posStrength, Tightness));      // 0.5 - 1.5   | threshold to fold increases depending on tightness/posStrength
                double raiseMultiplier = Lerp(0.5, 1.5, Average(posStrength, Aggression));    // 0.5 - 1.5   | threshold to raise increases depending on aggression/posStrength
                double avgBetMultiplier = 1 + Table.Players.GetCallingPlayersBetPercentage(); // 1 - 2       | threshold to call/raise decreases depending how much chips of his stack villain invested
                double mMultiplier = Lerp(1, 1.5, Clamp(0, 1, Normalize(1, 20, mRatio)));     // 1 - 1.5     | aggression increases the lower the mRatio
                double raiseNrMultiplier = 1 * System.Math.Pow(1.25, Table.RaisesThisGame);

                double foldThreshold = pPotOdds * foldMultiplier * avgBetMultiplier * raiseNrMultiplier;
                double raiseThreshold = pPotOdds_2 * mMultiplier * (2 - raiseMultiplier) * avgBetMultiplier * raiseNrMultiplier;

                raiseThreshold = Clamp(0, 0.95, raiseThreshold);
                foldThreshold = Clamp(0, 0.9, foldThreshold);

                //Debug.Log($"Hand: {Hand.debug} | pPotOdds: {Round(pPotOdds)}% | WinOdds: {Round(WinOdds)}% | FoldThreshold: {Round(foldThreshold)}% | RaiseThreshold: {Round(raiseThreshold)}% |");

                //FOLD THRESHOLD
                if (WinOdds < foldThreshold)
                {
                    return PokerAction.Fold;
                }

                //RAISE THRESHOLD
                if(Table.RaisesThisRound == 0 && Table.RaisesThisGame < 2)
                {
                    if (WinOdds > avgWinodds * (2 - raiseMultiplier))
                    {
                        double stackSizeRatio = Normalize(Players.GetLowStack(), Players.GetHighStack(), stackSize);
                        double betSize = Lerp(0.25, 1.5, stackSizeRatio);
                        _betAmount = System.Math.Max(Table.MinRaise, (uint)Round(pot * betSize * raiseMultiplier));
                        return PokerAction.Raise;
                    }
                }
                else
                {
                    if (WinOdds > raiseThreshold)
                    {
                        double stackSizeRatio = Normalize(Players.GetLowStack(), Players.GetHighStack(), stackSize);
                        double betSize = Lerp(0.25, 1.5, stackSizeRatio);
                        _betAmount = System.Math.Max(Table.MinRaise, (uint)Round(pot * betSize * raiseMultiplier));
                        return PokerAction.Raise;
                    }
                }

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
