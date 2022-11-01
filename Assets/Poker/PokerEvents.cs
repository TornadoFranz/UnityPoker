using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Poker
{
    namespace Events
    {
        public class PokerEvents
        {
            public event Action<Table> onSetTable;
            public event Action<Table> onNewGame;
            public event Action<Table> onNewRound;
            public event Action<Table> onNewTurn;
            public event Action<Table> onEndTurn;
            public event Action<Table, bool> onEndGame;

            public event Action<List<Card>, bool> onSetHeroHand;
            public event Action<int, int, int> onSetButton;
            public event Action<List<Player>> onSetChips;
            public event Action<Player> onBet;
            public event Action<PlayerManager, int> onEndPlayerTurn;
            public event Action<Cards> onSetBoard;
            public event Action<int> onCollectBets;
            public event Action onHeroTurn;
            public event Action onEndHeroTurn;
            public event Action<HashSet<int>> onSetWinners;
            public event Action<int, string> onPlayerAction;
            public event Action<List<Player>> onClearPlayerActions;

            public void SetTable(Table table)
            {
                onSetTable?.Invoke(table);
            }

            public void NewGame(Table table)
            {
                onNewGame?.Invoke(table);
            }

            public void NewRound(Table table)
            {
                onNewRound?.Invoke(table);
            }

            public void NewTurn(Table table)
            {
                onNewTurn?.Invoke(table);
            }

            public void EndTurn(Table table)
            {
                onEndTurn?.Invoke(table);
            }

            public void EndGame(Table table, bool revealHands)
            {
                onEndGame?.Invoke(table, revealHands);
            }

            public void SetButton(int btnID, int sbID, int bbID)
            {
                onSetButton?.Invoke(btnID, sbID, bbID);
            }

            public void SetHeroHand(List<Card> cardList, bool heroOn)
            {
                onSetHeroHand?.Invoke(cardList, heroOn);
            }

            public void SetChips(List<Player> players)
            {
                onSetChips?.Invoke(players);
            }

            public void Bet(Player player)
            {
                onBet?.Invoke(player);
            }

            public void EndPlayerTurn(PlayerManager players, int currentID)
            {
                onEndPlayerTurn?.Invoke(players, currentID);
            }

            public void SetBoard(Cards cards)
            {
                onSetBoard?.Invoke(cards);
            }

            public void CollectBets(int pot)
            {
                onCollectBets?.Invoke(pot);
            }

            public void HeroTurn()
            {
                onHeroTurn?.Invoke();
            }

            public void EndHeroTurn()
            {
                onEndHeroTurn?.Invoke();
            }

            public void SetWinners(HashSet<int> winners)
            {
                onSetWinners?.Invoke(winners);
            }

            public void PlayerActions(int ID, string currentAction)
            {
                onPlayerAction?.Invoke(ID, currentAction);
            }

            public void ClearPlayerActions(List<Player> players)
            {
                onClearPlayerActions?.Invoke(players);
            }
        }
    }
}
