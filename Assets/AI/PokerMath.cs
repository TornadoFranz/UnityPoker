using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoldemHand;
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;
using System.Linq;
using Poker.History;
using Poker.Extensions;
using System.Text;

//DIAGNOSTICS
//Stopwatch m_stopwatch = new Stopwatch();
//m_stopwatch.Start();
////ACTION
//m_stopwatch.Stop();
//UnityEngine.Debug.Log($"YEP: { m_stopwatch.ElapsedMilliseconds} ms");

namespace Poker
{
    namespace Math
    {
        public static class PokerMath
        {
            static System.Random random = new System.Random();

            public static double ExpectedValue(double bet, double pot, double winOdds, double loseOdds)
            {
                return winOdds * pot - loseOdds * bet;
            }

            public static double CalculateDecentRaiseAmount()
            {

                //Stack size(in BB)  Opening size(in BB)
                //12 – 30       2x
                //31 – 75       2.2x
                //75 – 125      2.5x
                //125 +         3x

                //Your stack size(or effective stack) – The smaller that the average stacks are, the lower you should raise.
                //Keep your opponents guessing what hand you have – Do not change your bet sizing based on how good your hand is preflop.On micro stakes, you still maybe get away with this, but higher opponents will certainly pick up on this huge tell
                //Types of opponents that you are facing – Raise smaller and more often vs. nitty opponents. Raise bigger and tighter vs.fishy opponents
                //From which position you are playing – It is different if you are on small blind, or button, or early position
                //Adjusting to the situation – Are you playing CG or MTT with ante…

                return 0;
            }

            public static void IterateThroughHands(ulong partialHandmask, ulong deadCards, int cardCount)
            {
                long count = 0;

                /// iterate through all card hands
                /// that share the cards in our partial hand.
                foreach (ulong handmask in Hand.Hands(partialHandmask, deadCards, cardCount))
                {
                    count++;
                }

                UnityEngine.Debug.Log($"Total hands to check: {count}");
            }

            public static HashSet<ulong> HandTable(ulong dead)
            {
                HashSet<ulong> result = new HashSet<ulong>();
                foreach (ulong handmask in Hand.Hands(0Ul, dead, 2))
                {
                    result.Add(handmask);
                }
                return result;
            }

            public static Dictionary<int, HashSet<ulong>> GetHandRangesBasedOnPotOdds(this Dictionary<int, HashSet<ulong>> handRanges, PlayerManager players, GameHistory gameHistory, int heroID, ulong handmask, ulong boardmask, BettingRounds curBettingRound)
            {
                foreach (Player player in players.ActiveList)
                {
                    if(player.ID != heroID)
                    {
                        HistoryData data = gameHistory.GetPlayersLatestItem(player.ID); //CHange THIS TO NUT JUST LATEST ITEM BUT ALSO PREVIOUS ITEM IF THEY WERE MISSED BY PLAYER | CHANGE HASHSET HAND RANGE TO ACTUALL HANDRANGE CLASS

                        if (data != null)
                        {
                            HashSet<ulong> handRange = new HashSet<ulong>();
                            int activeVillains = data.ActiveVillains;
                            double pPotOdds = data.pPotOdds;

                            if (data.Action == PokerAction.Raise)
                            {
                                //pPotOdds = data.raiseThreshold; //Smaller handranges for players that raised. Why is it ... you may ask? I dunno, because I felt like it... //this is obviously more effective if we atually track player data
                            }

                            if (handRanges.ContainsKey(player.ID))
                            {
                                handRange = handRanges[player.ID];
                            }
                            else
                            {
                                handRange = HandTable(handmask);
                            }

                            if(data.curBettingRound == BettingRounds.PreFlop)
                            {
                                handRange = handRange.GetPreFlopHandRange(activeVillains, pPotOdds, handmask); //TODO: 
                            }
                            else
                            {
                                handRange = handRange.GetPostFlopHandRange(activeVillains, pPotOdds, handmask, boardmask);
                            }

                            UnityEngine.Debug.Log($"            PLAYER {player.ID}: {data.pPotOdds} | Handrangecount: {handRange.Count} | {handRange.Debug()}");
                            handRanges[player.ID] = handRange;
                        }
                    }
                }
                return handRanges;
            }

            //TO DO: ALTERNATIVE FOR ALL STAGES OF GAME
            public static HashSet<ulong> GetPreFlopHandRange(this HashSet<ulong> handRange, int villainCount, double pPotOdds, ulong deadCards)
            {
                HashSet<ulong> newHandRange = new HashSet<ulong>();
                Dictionary<ulong, double> preFlopWinningOdds = GetPreflopWinningOdds(villainCount);
                foreach (ulong handmask in handRange)
                {
                    if (pPotOdds < preFlopWinningOdds[handmask])
                    {
                        newHandRange.Add(handmask);
                    }
                }

                return newHandRange;
            }

            //KINDA SCUFFED...
            public static HashSet<ulong> GetPostFlopHandRange(this HashSet<ulong> handRange, int villainCount, double pPotOdds, ulong handmask, ulong boardmask)
            {
                Dictionary<ulong, ulong> handDict = new Dictionary<ulong, ulong>();
                ulong handrank = 0;

                foreach (var hand in handRange)
                {
                    foreach (var wholeBoard in Hand.Hands(hand | boardmask, handmask, 7))
                    {
                        handrank += Hand.Evaluate(wholeBoard);
                    }
                    handDict[hand] = handrank;
                    handrank = 0;
                }
                
                var sortedDict = from entry in handDict orderby entry.Value ascending select entry;
                UnityEngine.Debug.Log(Hand.MaskToString(sortedDict.First().Key));
                UnityEngine.Debug.Log(Hand.MaskToString(sortedDict.Last().Key));

                //List<ulong> sortedHandList = handDict.Keys.ToList();
                //UnityEngine.Debug.Log($"WORST HAND: {Hand.MaskToString(sortedHandList[0])}");
                //UnityEngine.Debug.Log($"BEST HAND: {Hand.MaskToString(sortedHandList[sortedHandList.Count - 1])}");

                //UnityEngine.Debug.Log(handrankDict.Count);
                //foreach (var item in sortedDict)
                //{
                //    UnityEngine.Debug.Log($" {Hand.MaskToString(item.Key)} | {item.Value}");
                //}

                return handRange;
            }

            public static void CutPartOffHandRange(List<ulong> handrange, int villainCount, double pPotOdds)
            {
                int maxSize = handrange.Count;
                int lowSize = 0;
                int middle = maxSize / 2;
                int oldmiddle = 0;

                for (int i = 0; i < 9; i++)
                {
                    double winodds = WinOdds(handrange[middle], 0UL, 0UL, villainCount, 0.01);
                    UnityEngine.Debug.Log($"{Hand.MaskToString(handrange[middle])} | {middle} | {winodds}");
                    if (winodds > pPotOdds)
                    {
                        oldmiddle = middle;
                        middle = (middle + lowSize) / 2;
                        maxSize = oldmiddle;
                    }
                    else
                    {
                        oldmiddle = middle;
                        middle = (middle + maxSize) / 2;
                        lowSize = oldmiddle;
                    }
                }
                UnityEngine.Debug.Log($"Result: {handrange.Count - middle}");
            }


            public static void GetHandRangePercentage(this List<ulong> handrange, double percentage)
            {
                int removeIndex = (int)(handrange.Count * percentage);
                handrange.RemoveRange(removeIndex, handrange.Count - removeIndex);
                UnityEngine.Debug.Log(handrange.Count);
            }

            public static string Debug(this HashSet<ulong> handRange)
            {
                StringBuilder stringBuilder = new StringBuilder();

                foreach (ulong hand in handRange)
                {
                    stringBuilder.Append(Hand.MaskToString(hand).ShowSuitSymbols());
                    stringBuilder.Append(" | ");
                }

                return stringBuilder.ToString();
            }

            public static Dictionary<ulong, double> GetPreflopWinningOdds(int nopponents)
            {
                string json = File.ReadAllText(Application.streamingAssetsPath + $"/WinOdds/PreflopWinOdds_{nopponents}.json");
                return JsonConvert.DeserializeObject<Dictionary<ulong, double>>(json);
            }
         

            public static double WinOdds(ulong pocket, ulong board, ulong dead, int numOpponents, double duration)
            {
                int count = 0;
                double num = 0.0;
                double num2 = 0.0;
                foreach (ulong item in Hand.RandomHands(board, dead | pocket, 5, duration))
                {
                    count++;
                    ulong num3 = dead | item | pocket;
                    uint num4 = Hand.Evaluate(pocket | item);
                    bool flag = true;
                    bool flag2 = true;
                    for (int i = 0; i < numOpponents; i++)
                    {
                        ulong num5 = Hand.RandomHand(num3, 2);
                        uint num6 = Hand.Evaluate(num5 | item);
                        num3 |= num5;
                        if (num4 < num6)
                        {
                            flag = (flag2 = false);
                            break;
                        }

                        if (num4 <= num6)
                        {
                            flag = false;
                        }
                    }

                    if (flag)
                    {
                        num += 1.0;
                    }
                    else if (flag2)
                    {
                        num += 0.5;
                    }

                    num2 += 1.0;
                }

                if (num2 != 0.0)
                {
                    return num / num2;
                }

                return 0.0;
            }            
            
            public static double WinOddsHandRange(Dictionary<int, HashSet<ulong>> handRanges, ulong pocket, ulong board, ulong dead, List<Player> villains, double duration)
            {
                double num = 0.0;
                double num2 = 0.0;
                foreach (ulong item in Hand.RandomHands(board, dead | pocket, 5, duration))
                {
                    ulong num3 = dead | item | pocket;
                    uint num4 = Hand.Evaluate(pocket | item);
                    bool flag = true;
                    bool flag2 = true;
                    foreach (Player villain in villains)
                    {
                        ulong num5 = Hand.RandomHand(num3, 2);

                        if (handRanges.ContainsKey(villain.ID))
                        {
                            HashSet<ulong> newHandRange = handRanges[villain.ID].HandRangeDeadMask(item);
                            num5 = RandomHand(newHandRange.ToList());
                        }
                        else
                        {
                            num5 = Hand.RandomHand(num3, 2);
                        }

                        uint num6 = Hand.Evaluate(num5 | item);
                        num3 |= num5;
                        if (num4 < num6)
                        {
                            flag = (flag2 = false);
                            break;
                        }

                        if (num4 <= num6)
                        {
                            flag = false;
                        }
                    }                   

                    if (flag)
                    {
                        num += 1.0;
                    }
                    else if (flag2)
                    {
                        num += 0.5;
                    }

                    num2 += 1.0;
                }

                if (num2 != 0.0)
                {
                    return num / num2;
                }

                return 0.0;
            }

            public static ulong RandomHand(List<ulong> handRange)
            {
                if (handRange.Count == 0)
                {
                    UnityEngine.Debug.LogError("Mistake In Prediciting HandRange -> (no possible handrange left)");
                    return 0;
                }

                return handRange[random.Next(handRange.Count)];
            }


            public static HashSet<ulong> MaskToList(ulong handMask)
            {
                HashSet<ulong> maskList = new HashSet<ulong>();

                foreach (ulong mask in CardMasks(handMask))
                {
                    maskList.Add(mask);
                }

                return maskList;
            }


            public static HashSet<ulong> HandRangeDeadMask(this HashSet<ulong> handRange, ulong deadmask)
            {
                HashSet<ulong> newHandRange = new HashSet<ulong>();
                HashSet<ulong> deadMaskList = MaskToList(deadmask);
                foreach (ulong handmask in handRange)
                {
                    HashSet<ulong> handMaskList = MaskToList(handmask);
                    handMaskList.IntersectWith(deadMaskList);

                    if (handMaskList.Count == 0)
                    {
                        newHandRange.Add(handmask);
                    }
                }

                return newHandRange;
            }

            public static IEnumerable<ulong> CardMasks(ulong mask)
            {
                for (int i = 51; i >= 0; i--)
                {
                    if (((1L << i) & (long)mask) != 0)
                    {
                        yield return Hand.CardMasksTable[i];
                    }
                }
            }
        }
    }
}
