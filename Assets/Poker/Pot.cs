using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Poker
{
    public class Pot
    {
        public double Amount { get; private set; }
        public double TempAmount => Players.GetTempPotAmount();  
        public double MaxPotentialAmount => Players.GetMaxPotentialPotAmount();  
        public PlayerManager Players { get; private set; }
        public List<SidePot> SidePots { get; set; } = new List<SidePot>();
        public HashSet<int> Winners
        {
            get
            {
                HashSet<int> winners = new HashSet<int>();
                foreach (var sidePot in SidePots)
                {
                    winners.UnionWith(sidePot.Winners);
                }
                return winners;
            }
        }

        public Pot(PlayerManager Players)
        {
            this.Players = Players;
            Amount = 0;
        }

        public void CollectAllBets()
        {
            Amount = Players.CollectAllBets();
        }

        public void SplitPotToWinners()
        {
            CreateSidePots();

            double previousAmount = 0;
            foreach (var pot in SidePots)
            {
                pot.Amount = pot.AmountAfterCutoff - previousAmount;
                previousAmount = pot.AmountAfterCutoff;
                PayPotToWinners(pot);
            }
        }

        private void CreateSidePots()
        {
            List<Player> orderedPlayers = Players.List.OrderBy(o => o.AmountInvested).ToList();
            double highestAmountInvested = orderedPlayers[orderedPlayers.Count - 1].AmountInvested;

            double cutoff = 0;
            foreach (var player in orderedPlayers)
            {
                if (player.IsAllIn)
                {
                    if(cutoff < player.AmountInvested)
                    {
                        cutoff = player.AmountInvested;
                        SidePot newSidePot = new SidePot(cutoff, Players);
                        SidePots.Add(newSidePot);
                    }
                }
            }

            if(cutoff < highestAmountInvested) SidePots.Add(new SidePot(highestAmountInvested, Players));
        }

        private void PayPotToWinners(SidePot pot)
        {
            HashSet<int> winner = pot.DetectWinners();
            foreach (var ID in winner)
            {
                double amountWon = pot.Amount / winner.Count;
                Players.List[ID].Chips += amountWon;
                Players.List[ID].AmountWon += amountWon;
            }
        }
    }

    public class SidePot
    {
        public double Amount { get; set; }
        public double AmountAfterCutoff { get; set; }
        public double Cutoff { get; set; }
        public List<Player> Contender { get; set; } = new List<Player>();
        public HashSet<int> Winners { get; set; } = new HashSet<int>();

        public SidePot(double Cutoff, PlayerManager Players)
        {
            this.Cutoff = Cutoff;
            AmountAfterCutoff = SetAmount(Players);
            Contender = SetContender(Players);
        }

        public double SetAmount(PlayerManager players)
        {
            double amount = 0;
            foreach (var player in players.List)
            {
                amount += System.Math.Min(player.AmountInvested, Cutoff);
            }
            return amount;
        }

        public List<Player> SetContender(PlayerManager players)
        {
            foreach (var player in players.List)
            {
                if (player.IsPlayingThisRound)
                {
                    if (player.AmountInvested >= Cutoff)
                    {
                        Contender.Add(player);
                    }
                }
            }
            return Contender;
        }

        public HashSet<int> DetectWinners()
        {            
            uint highestHandrank = 0;
            foreach (var player in Contender)
            {
                if (player.HandRank > highestHandrank)
                {
                    Winners.Clear();
                    highestHandrank = player.HandRank;
                    Winners.Add(player.ID);
                }
                else if (player.HandRank == highestHandrank)
                {
                    Winners.Add(player.ID);
                }
            }

            return Winners;
        }
    }
}
