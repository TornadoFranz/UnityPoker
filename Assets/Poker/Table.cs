//using HoldemHand;
using System.Collections.Generic;
using UnityEngine;
using Poker.Extensions;
using System;
using Poker.AI;
using Poker.Events;
using Poker.History;
using Menu;

namespace Poker
{
    public enum BettingRounds
    {        
        PreFlop,
        Flop,
        Turn,
        River,
        Showdown,
        EndGame
    }

    public class Table
    {
        public List<PlayerPreset> PlayerPresets { get; private set; }
        public double SBAmount { get; private set; }
        public double BBAmount { get; private set; }
        public double Ante { get; private set; }



        public PokerEvents pokerEvents { get; set; }

        public Deck Deck { get; set; }
        public Cards Board { get; set; }

        public PlayerManager Players { get; set; }

        public GameHistory GameHistory { get; private set; }

        int CurrentPlayerID { get; set; }
        public Player CurrentPlayer => Players.List[CurrentPlayerID];
        public Player PreviousPlayer => Players.GetPreviousPlayerStillPlaying(CurrentPlayer.ID);
        public Player NextPlayer => Players.GetNextPlayerStillPlaying(CurrentPlayer.ID);

        public Player Hero { get; set; }
        public Player Dealer => Players.List[_buttonID];
        public Player SmallBlind => Players.List[_smallBlindID];
        public Player BigBlind => Players.List[_bigBlindID];
        public double MinBet { get; set; }
        public double MinRaise => MinBet == 0 ? BBAmount * 2 : MinBet * 2;
        public int RaisesThisRound { get; set; }
        public int RaisesThisGame { get; set; }
        public Pot Pot { get; set; }

        private int _buttonID = -1;
        private int _smallBlindID = 0;
        private int _bigBlindID = 1;

        public BettingRounds CurrentBettingRound { get; private set; }
        
        private System.Random random = new System.Random();


        public bool GameOn { get; set; } = false;


        public bool HeroOn { get; } = true;
        public bool ClearLogsOnStart { get; } = true;
        public bool WriteLogs { get; } = true;
        public bool PlayAnimations { get; } = true;


        public Table(List<PlayerPreset> PlayerPresets, uint SBAmount ,uint BBAmount, uint Ante)
        {
            this.PlayerPresets = PlayerPresets;
            this.SBAmount = SBAmount;
            this.BBAmount = BBAmount;
            this.Ante = Ante;
            pokerEvents = new PokerEvents();
            GameHistory = new GameHistory(ClearLogsOnStart);
            FillTable();
        }

        private void FillTable()
        {
            Players = new PlayerManager(new List<Player>());

            for (int i = 0; i < PlayerPresets.Count; i++)
            {
                PlayerPreset playerPreset = PlayerPresets[i];
                Players.List.Add(new Player(this, i, playerPreset.Name, (uint)playerPreset.Chips, new DonkeyAI(this, playerPreset.Tightness, playerPreset.Aggression), playerPreset.identity));
            }

            Hero = Players.GetNextHeroStillPlaying(CurrentPlayerID);
        }

        public void StartNewGame()
        {
            GameHistory = new GameHistory(false);
            RaisesThisGame = 0;
            Players.ResetAllPlayers();
            Players.GetPlayersStillPlaying();
            Pot = new Pot(Players);
            SetBlinds();
            Players.SetPositions(Dealer.ID);
            pokerEvents.SetTable(this);
            pokerEvents.NewGame(this);

            if (Players.ActiveList.Count == 1)
            {
                Debug.Log($"Winner: {Players.ActiveList[0].Name}");
                return;
            }

            GameHistory.SaveGameSetup(this);
            StartNewRound(BettingRounds.PreFlop);
        }

        public void StartNewRound(BettingRounds bettingRound)
        {
            GameOn = true;
            CurrentBettingRound = bettingRound;
            RaisesThisRound = 0;
            CollectPot();          
            Players.ResetAllActions();
            NextPlayersTurn(_buttonID);

            switch (CurrentBettingRound)
            {
                case BettingRounds.PreFlop:
                    DealCards();
                    PayBlinds();
                    pokerEvents.NewRound(this);
                    break;
                case BettingRounds.Flop:
                    Board.Add(Deck.Draw(3));
                    pokerEvents.NewRound(this);
                    break;
                case BettingRounds.Turn:
                    Board.Add(Deck.Draw(1));
                    pokerEvents.NewRound(this);
                    break;
                case BettingRounds.River:
                    Board.Add(Deck.Draw(1));
                    pokerEvents.NewRound(this);
                    break;
                case BettingRounds.Showdown:
                    Board.Add(Deck.Draw(5 - Board.list.Count));      
                    CheckForWinner();
                    pokerEvents.NewRound(this);
                    if (WriteLogs) GameHistory.SaveGameHistory(this);
                    pokerEvents.EndGame(this, true);
                    GameOn = false;
                    if (!PlayAnimations) StartNewGame();
                    break;
                case BettingRounds.EndGame:
                    CheckForWinner();
                    if(WriteLogs) GameHistory.SaveGameHistory(this);
                    pokerEvents.EndGame(this, false);
                    GameOn = false;
                    if(!PlayAnimations) StartNewGame();
                    break;
            }
        }

        private void DealCards()
        {
            Board = new Cards();
            Deck = new Deck();
            Deck.Shuffle(random);

            foreach (var player in Players.List)
            {
                if(player.IsPlayingThisGame)
                {
                    player.Hand = new Cards(Deck.Draw(2));
                }
            }

            if(Hero.Hand != null) pokerEvents.SetHeroHand(Hero.Hand.list, HeroOn);            
        }

        private void SetBlinds()
        {
            _buttonID = Players.GetNextPlayerStillPlaying(_buttonID).ID;
            _smallBlindID = Players.GetNextPlayerStillPlaying(_buttonID).ID;
            _bigBlindID = Players.GetNextPlayerStillPlaying(_smallBlindID).ID;
        }

        private void PayBlinds()
        {
            MinBet = BBAmount + Ante;
            Players.PayAntes(Ante);
            SmallBlind.Bet(SBAmount);
            BigBlind.Bet(BBAmount);
            NextPlayersTurn(_bigBlindID);
        }

        private void CollectPot()
        {
            MinBet = 0;
            Pot.CollectAllBets();
        }


        public void PlayerAction(PokerAction pokerAction, double betAmount)
        {
            GameHistory.Add(new HistoryData(CurrentPlayer.Name, CurrentPlayer.ID, CurrentPlayer.Chips, Pot.TempAmount, Pot.MaxPotentialAmount, CurrentPlayer.CurrentBet, MinBet, CurrentPlayer.pokerAI.BetAmount, Players.ActiveList.Count, pokerAction, CurrentBettingRound));

            switch (pokerAction)
            {
                case PokerAction.Fold:
                    CurrentPlayer.Fold();
                    break;
                case PokerAction.Call:
                    CurrentPlayer.Call();
                    break;
                case PokerAction.Raise:
                    CurrentPlayer.Raise((uint)betAmount);
                    break;
                default:
                    break;
            }
        }

        public void CheckForRoundEnd()
        {
            List<Player> playersLeft = Players.GetPlayersStillPlaying();
            Players.SetPositions(Dealer.ID);

            if (Players.OnePlayerLeft(playersLeft))
            {
                StartNewRound(BettingRounds.EndGame);
                return;
            }

            if (Players.EveryoneIsAllIn(playersLeft))
            {
                StartNewRound(BettingRounds.Showdown);
                return;
            }

            if (Players.NoActionsLeft(playersLeft))
            {
                StartNewRound(CurrentBettingRound + 1);
                return;
            }

            NextPlayersTurn(CurrentPlayer.ID);
        }        

        public void NextPlayersTurn(int currentID)
        {
            pokerEvents.EndTurn(this);
            CurrentPlayerID = Players.GetNextPlayerStillPlaying(currentID).ID;
        }

        public void CheckForWinner()
        {
            Evaluator evaluator = new Evaluator();
            evaluator.SetHandRanks(Players, Board.list);
            Pot.SplitPotToWinners();
        }
    }
}

