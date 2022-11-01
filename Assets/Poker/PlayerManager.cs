using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Poker
{
    public class PlayerManager
    {
        public PlayerManager (List<Player> List)
        {
            this.List = List;
            this.ActiveList = List;
        }

        public List<Player> List { get; private set; }

        public List<Player> ActiveList { get; private set; }

        public Player GetNextPlayerStillPlaying(int currentID)
        {
            do
            {
                currentID = GetNextPlayerID(currentID);
            }
            while (!List[currentID].IsPlayingThisRound);

            return List[currentID];
        }

        public Player GetPreviousPlayerStillPlaying(int currentID)
        {
            do
            {
                currentID = GetPreviousPlayerID(currentID);
            }
            while (!List[currentID].IsPlayingThisRound);

            return List[currentID];
        }

        public Player GetNextHeroStillPlaying(int currentID)
        {
            for (int i = 0; i < List.Count; i++)
            {
                if(List[i].IsPlayingThisGame)
                {
                    if (List[currentID].identity == Identity.Human)
                    {
                        return List[currentID];
                    }
                }

                currentID = GetNextPlayerID(currentID);
                Debug.Log(currentID);
            }

            return null;
        }

        public Player GetNextPlayer(int currentID)
        {
            return List[GetNextPlayerID(currentID)];
        }

        public Player GetPreviousPlayer(int currentID)
        {
            return List[GetPreviousPlayerID(currentID)];
        }

        private int GetNextPlayerID(int currentID)
        {
            currentID++;
            if (currentID >= List.Count)
            {
                currentID = 0;
            }
            return currentID;
        }

        private int GetPreviousPlayerID(int currentID)
        {
            currentID--;
            if(currentID < 0)
            {
                currentID = List.Count-1;
            }
            return currentID;
        }

        public int ListCount()
        {
            int count = 0;
            foreach (var player in List)
            {
                if(player.IsPlayingThisGame)
                {
                    count++;
                }
            }
            return count;
        }

        public void SetPositions(int dealerID)
        {
            //int curID = dealerID;
            //for (int i = ActiveList.Count-1; i > 0; i--)
            //{
            //    curID = GetNextPlayerStillPlaying(curID).ID;
            //    List[curID].Position = i;
            //}

            int curID = dealerID;
            for (int i = 0; i < ActiveList.Count; i++)
            {
                curID = GetNextPlayerStillPlaying(curID).ID;
                List[curID].Position = i+1;
            }
        }

        public void PayAntes(double ante)
        {
            if(ante == 0)
            {
                return;
            }

            foreach (var player in ActiveList)
            {
                player.Bet((uint)ante);
            }
        }

        public double CollectAllBets()
        {
            double pot = 0;
            foreach (var Player in List)
            {
                pot += Player.AmountInvested;
                Player.CurrentBet = 0;
            }
            return pot;
        }

        public double GetDeadMoney() //INVESTED CHIPS OF PLAYERS HTAT FOLDDED
        {
            double pot = 0;
            foreach (var Player in List)
            {
                if(!Player.IsPlayingThisRound)
                {
                    pot += Player.AmountInvested;
                }
            }
            return pot;
        }

        public double GetTempPotAmount()
        {
            double pot = 0;
            foreach (var Player in List)
            {
                pot += Player.AmountInvested;
            }
            return pot;
        }

        public double GetMaxPotentialPotAmount()
        {
            double pot = GetTempPotAmount();
            foreach (var Player in ActiveList)
            {
                if(!Player.DidActionThisRound)
                {
                    pot += (Player.table.MinBet - Player.CurrentBet); 
                }
            }
            return pot;
        }

        public double GetPot()
        {
            double pot = 0;
            foreach (var Player in List)
            {
                pot += Player.AmountInvested;
            }
            return pot;
        }

        public double GetpPot(double newMinBet)
        {
            double pot = GetPot();
            foreach (var Player in ActiveList)
            {
                if (!Player.DidActionThisRound)
                {
                    pot += (newMinBet - Player.CurrentBet);
                }
            }
            return pot;
        }

        public double GetMinPotentialPotAmount(int heroID)
        {
            double pot = List[heroID].table.MinBet * (CountMinAmountOfOpponentsCalling(heroID)+1) + GetDeadMoney();

            return pot;
        }
        
        public double GetEffectiveStackSize(int heroID)
        {
            return System.Math.Min(List[heroID].Chips, BiggestStackStillinPlay());
        }

        public double BiggestStackStillinPlay()
        {
            double highStack = 0;
            foreach (var player in ActiveList)
            {
                if(highStack < player.Chips)
                {
                    highStack = player.Chips;
                }
            }
            return highStack;
        }

        public double GetCallingPlayersBetPercentage()
        {
            int count = 0;
            double avgBetPercentage = 0;
            foreach (var Player in ActiveList)
            {
                if(Player.DidActionThisRound)
                {
                    count++;
                    if (avgBetPercentage == 0)
                    {
                        avgBetPercentage = Player.PercentAmountInvested;
                        continue;
                    }
                    avgBetPercentage += Player.PercentAmountInvested;
                }
            }

            if(count != 0)
            {
                return avgBetPercentage/count;
            }

            return 0;
        }

        public double GetAvgStackSize()
        {
            int count = 0;
            double avgStackSize = 0;
            foreach (var Player in List)
            {
                if(Player.IsPlayingThisGame)
                {
                    avgStackSize += Player.Chips;
                    count++;
                }
            }
            return avgStackSize/count;
        }

        public double GetHighStack()
        {
            double highStack = 0;
            foreach (var Player in List)
            {
                if (Player.IsPlayingThisGame)
                {
                    if(Player.Chips>highStack)
                    {
                        highStack = Player.Chips;
                    }
                }
            }
            return highStack;
        }

        public double GetLowStack()
        {
            double lowStack = double.MaxValue;
            foreach (var Player in List)
            {
                if (Player.IsPlayingThisGame)
                {
                    if (Player.Chips < lowStack)
                    {
                        lowStack = Player.Chips;
                    }
                }
            }
            return lowStack;
        }

        public double GetPosStrength(int ID)
        {            
            return (double)List[ID].Position / ActiveList.Count;
        }

        public List<Player> GetPlayersStillPlaying()
        {
            ActiveList = new List<Player>();
            foreach (var Player in List)
            {
                if(Player.IsPlayingThisRound)
                {
                    ActiveList.Add(Player);
                }
            }
            return ActiveList;
        }

        public int CountMinAmountOfOpponentsCalling(int heroID)
        {
            int count = 0;
            foreach (var Player in ActiveList)
            {
                if(Player.ID == heroID) //DON'T COUNT YOURSELF
                {
                    continue;
                }

                if (Player.DidActionThisRound) //COUNTS PLAYERS THAT ALREADY CALLED
                {
                    count++;
                    continue;
                }

                if((uint)Player.table.MinBet - (uint)Player.CurrentBet == 0) //COUNTS PEOPLE THAT JUST HAVE TO CHECK
                {
                    count++;
                    continue;
                }

                //if ((uint)Player.table.MinBet - (uint)Player.CurrentBet <= (uint)Player.table.MinBet/2) //COUNTS PEOPLE THAT ALREAD BET >= THEN THE AMOUNT TO CALL (for example SB)
                //{
                //    count++;
                //    continue;
                //}
            }
            return count;
        }

        public List<Player> GetVillainList(int heroID)
        {
            return ActiveList.Where(x => x.ID != heroID).ToList();
        }

        public bool OnePlayerLeft(List<Player> players)
        {
            if(players.Count <= 1)
            {
                return true;
            }

            return false;
        }

        public bool EveryoneIsAllIn(List<Player> players) //FUCK ME...
        {
            int allInCount = 0;
            Player playerWhosNotAllIn = null;
            double amountInvested = 0;
            foreach (var player in players)
            {
                if (player.IsAllIn)
                {
                    allInCount++;
                    if(amountInvested <= player.AmountInvested)
                    {
                        amountInvested = player.AmountInvested;
                    }
                }
                else
                {
                    playerWhosNotAllIn = player;
                }
            }

            if(allInCount >= players.Count)
            {
                return true;                            
            }
            else if(allInCount >= players.Count - 1)
            {
                if (playerWhosNotAllIn.AmountInvested >= amountInvested)
                {
                    return true;
                }
            }

            return false;
        }

        public double TotalChips()
        {
            double totalChips = 0;
            foreach (var player in List)
            {
                totalChips += player.Chips;
            }
            return totalChips;
        }

        public bool NoActionsLeft(List<Player> players)
        {
            foreach (var Player in players)
            {
                if (!Player.DidActionThisRound)
                {
                    return false;
                }
            }

            return true;
        }

        public bool MultipleHumansLeft()
        {
            List<Player> humansLeft = ActiveList.Where(p => p.identity == Identity.Human).ToList();
            if(humansLeft.Count > 1)
            {
                return true;
            }
            return false;
        }


        public void ResetAllActions()
        {
            foreach (var player in ActiveList)
            {
                if (!player.IsAllIn)
                {
                    player.DidActionThisRound = false;
                }
            }
        }

        public void ResetAllPlayers()
        {
            foreach (var Player in List)
            {
                Player.ResetPlayer();
            }
        }
    }
}
