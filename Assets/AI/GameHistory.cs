using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Poker;
using System.IO;
using System.Linq;
using HoldemHand;

namespace Poker
{
    namespace History
    {
        public class GameHistory
        {

            private List<HistoryData> HistoryList = new List<HistoryData>();
            public string Content { get; set; }

            public GameHistory(bool clearFile)
            {
                if (clearFile)
                {                    
                    File.WriteAllText(Application.streamingAssetsPath + $"/Logs/GameHistory.txt", "");
                }
            }

            public void SaveGameSetup(Table table)
            {
                Content = string.Empty;
                foreach (var player in table.Players.ActiveList)
                {
                    Content += $"Seat {player.ID}: {player.Name} ({player.Chips} in chips)\n";
                }

                if (table.SmallBlind.Chips <= table.SBAmount) Content += $"{table.SmallBlind.Name}: posts small blind {table.SmallBlind.Chips} and is all-in\n";
                else Content += $"{table.SmallBlind.Name}: posts small blind {table.SBAmount}\n";
                if (table.BigBlind.Chips <= table.BBAmount) Content += $"{table.BigBlind.Name}: posts big blind {table.BigBlind.Chips} and is all-in\n";
                else Content += $"{table.BigBlind.Name}: posts big blind {table.BBAmount}\n";
            }

            public void SavePlayerHistory(Table table)
            {
                BettingRounds curBettingRound = (BettingRounds)2;

                foreach (var historyData in HistoryList)
                {
                    if (curBettingRound != historyData.curBettingRound)
                    {
                        Content += $"*** {historyData.curBettingRound} *** {table.Board.Debug(historyData.curBettingRound)} | Pot: {historyData.Pot}\n";
                    }

                    curBettingRound = historyData.curBettingRound;
                    Content += $"{historyData.Debug()}\n";
                }
            }

            public void SaveGameEnd(Table table)
            {

                if (table.CurrentBettingRound == BettingRounds.Showdown)
                {
                    Content += $"*** {BettingRounds.Showdown} *** | Pot: {table.Pot.Amount}\n";

                    foreach (var player in table.Players.ActiveList)
                    {
                        Content += $"{player.Name}: shows [{player.Hand.debug}] ({Hand.DescriptionFromMask(player.Hand.value | table.Board.value)})\n";
                    }
                }

                Content += $"*** Summary *** | Pot: {table.Pot.Amount}\n";

                foreach (var winnerID in table.Pot.Winners)
                {
                    Player player = table.Players.List[winnerID];
                    Content += $"{player.Name}: collected {player.AmountWon} from pot\n";
                    Content += $"{player.Name}: won {player.AmountWon-player.AmountInvested} chips\n";
                }
                Content += "\n \n \n";
                //*** SUMMARY *** //WRITE STATUS REPORTS FOR INDIVIDUALL SITUATIONS, MAYBE WHO CARES...
                //Total pot 350 | Rake 18
                //Board[8h 7s 8d Th 2c]
                //Seat 1: adevlupec(big blind) showed[Qs Ts] and won(332) with two pair, Tens and Eights
                //Seat 2: Dette32 mucked[5s Kc]
                //Seat 3: Drug08(button) mucked[4d 6h]
                //Seat 4: FluffyStutt(small blind) folded before Flop                
            }

            public void WriteToLog()
            {                
                File.AppendAllText(Application.streamingAssetsPath + $"/Logs/GameHistory.txt", Content);
            }

            public void SaveGameHistory(Table table)
            {
                SavePlayerHistory(table);
                SaveGameEnd(table);
                WriteToLog();
            }

            public void Add(HistoryData historyData)
            {
                HistoryList.Add(historyData);
            }

            //USE THIS WITH AI THAT CAN READ GAME HISTORY CURRENTLY UNUSED
            //ONLY WORKS WITH PREFLOP SO FAR
            public HistoryData GetPlayersLatestItem(int ID) //GETTING JUST THE LATES WILL PROBABLY NOT BE OPTIMAL
            {
                List<HistoryData> playersHistoryList = HistoryList.Where(p => p.ID == ID).ToList();
                
                if(playersHistoryList.Count == 0)
                {
                    return null;
                }

                return playersHistoryList.Last();
            }
        }

        public class HistoryData
        {
            public HistoryData(string Name, int ID, double Chips, double Pot, double pPot, double curBet, double minBet, double betAmount, int ActivePlayers, PokerAction Action, BettingRounds curBettingRound) //replace this with solution thats also used inside ai's
            {
                this.Name = Name;
                this.ID = ID;
                this.Chips = Chips;
                this.Pot = Pot;
                this.pPot = pPot;
                this.curBet = curBet;                                           
                this.minBet = minBet;
                this.betAmount = betAmount;
                this.ActivePlayers = ActivePlayers;
                this.Action = Action;
                this.curBettingRound = curBettingRound;


                Bet = minBet - curBet;
                PotOdds = Bet / (Bet + Pot);
                pPotOdds = Bet / pPot;
                ActiveVillains = ActivePlayers - 1;
                PotOddsAfterAction = (betAmount - curBet) / ((betAmount-curBet) + Pot);
            }

            public string Name { get; private set; }
            public int ID { get; private set; }
            public double Chips { get; private set; }
            public double Pot { get; private set; }
            public double pPot { get; private set; }
            public double curBet { get; private set; }
            public double minBet { get; private set; }
            public double betAmount { get; private set; }
            public int ActivePlayers { get; private set; }
            public PokerAction Action { get; private set; }
            public BettingRounds curBettingRound { get; private set; }
            
            public double Bet { get; private set; }
            public double PotOdds { get; private set; }
            public double pPotOdds { get; private set; }
            public double PotOddsAfterAction { get; private set; }
            public int ActiveVillains { get; private set; }


            public string Debug()
            {
                switch (Action)
                {
                    case PokerAction.Fold:
                        return $"{Name}: folds";
                    case PokerAction.Call:
                        if(Bet == 0) return $"{Name}: checks";
                        if(Chips <= minBet) return $"{Name}: callls {curBet + Chips} and is all-in";
                        return $"{Name}: callls {minBet}";
                    case PokerAction.Raise:
                        if(betAmount >= Chips) return $"{Name}: bets {curBet + Chips} and is all-in";
                        return $"{Name}: bets {betAmount}";                    
                    default:
                        UnityEngine.Debug.LogError($"{Name}: ACTION IS NULL");
                        break;
                }

                return $"{Name}: {PokerActions.ToString(Action)} {betAmount}";
            }
        }
    }
}

