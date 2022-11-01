using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Poker;
using Poker.Events;
using Poker.Extensions;
using UnityEngine.UI;
using TMPro;
using System.Globalization;
using Menu;

namespace PokerGUI
{
    public class TableGUI : MonoBehaviour
    {
        [SerializeField] private TableSettings tableSettings;
        [SerializeField] private DeckList deckList;
        [SerializeField] private Blinds blinds;
        [SerializeField] private TextMeshProUGUI potTMP;
        [SerializeField] private List<Image> board;
        [SerializeField] private List<Image> hand;
        [SerializeField] private List<SeatGUI> seats;

        public PokerEvents pokerEvents => Main.current.table.pokerEvents;

        private Color32 standardColor = new Color32(120, 120, 150, 255);
        private Color32 foldColor = new Color32(55, 55, 55, 255);
        private Color32 turnColor = new Color32(90, 166, 90, 255);
        private Color32 loseColor = new Color32(170, 20, 20, 255);
        private Color32 winColor = new Color32(10, 205, 10, 255);

        void Awake()
        {
            if(Main.current.PlayAnimations) RegisterPokerEvents();
        }

        void RegisterPokerEvents()
        {
            pokerEvents.onSetTable += SetTable;
            pokerEvents.onNewGame += NewGame;
            pokerEvents.onNewRound += NewRound;
            pokerEvents.onNewTurn += NewTurn;
            pokerEvents.onEndTurn += EndTurn;
            pokerEvents.onEndGame += EndGame;

            pokerEvents.onSetHeroHand += SetHeroHand;
            pokerEvents.onSetBoard += ShowBoard;
            pokerEvents.onBet += SetBet;
        }

        void DeregisterPokerEvents()
        {
            pokerEvents.onSetHeroHand -= SetHeroHand;
            pokerEvents.onBet -= SetBet;
        }


        private void SetTable(Table table)
        {
            SetPlayerPresets();
            SetChips(table.Players.List);
            SetPot(0);
            SetPlayersColors(table.Players.List, standardColor);
            RemovePlayers();
        }

        private void NewGame(Table table)
        {
            ClearPlayerActions(table.Players.List);
            SetButtons(table.Dealer.ID, table.SmallBlind.ID, table.BigBlind.ID);
            HideBoard();
            HideAllHands();
        }

        private void NewRound(Table table)
        {
            StartCoroutine(NewRoundCoroutine(table));
        }

        IEnumerator NewRoundCoroutine(Table table)
        {
            SetPot(table.Pot.Amount);
            SetBets(table.Players.ActiveList);
            ShowBoard(table.Board);
            yield return new WaitWhile(IsTweening);
            ClearPlayerActions(table.Players.List);
        }

        private void NewTurn(Table table)
        {
            if(table.CurrentBettingRound != BettingRounds.Showdown)
            {
                SetPlayerColor(table.CurrentPlayer, turnColor);
            }
        }

        private void EndTurn(Table table)        
        {
            SetPlayerAction(table.CurrentPlayer.ID, table.CurrentPlayer.CurrentAction);
            SetPlayerColor(table.CurrentPlayer, standardColor);
        }

        private void EndGame(Table table, bool revealHands)
        {
            StartCoroutine(EndGameCoroutine(table, revealHands));
        }

        IEnumerator EndGameCoroutine(Table table, bool revealHands)
        {
            if (revealHands)
            {
                SetPlayersColors(table.Players.List, standardColor);
                ShowBoard(table.Board);
                yield return new WaitWhile(IsTweening);
                RevealAllHands(table.Players.List, table);
                yield return new WaitWhile(IsTweening);
                SetLosers(table.Players.ActiveList);
                SetWinners(table.Players.ActiveList);
            }
            else
            {
                SetWinners(table.Players.ActiveList);
            }

            SetChips(table.Players.List);
            yield return new WaitForSeconds(AnimTime.endGame);            
            table.StartNewGame();
        }

        private bool IsTweening()
        {
            return LeanTween.isTweening();
        }

        private void RevealCard(List<Image> images, int cardID, int imageID)
        {
            if(images[imageID].sprite != deckList.sprites[cardID])
            {
                var seq = LeanTween.sequence();
                seq.append(LeanTween.rotateY(images[imageID].gameObject, 90, AnimTime.revealCard).setEase(LeanTweenType.easeInCirc));
                seq.append(() => { images[imageID].sprite = deckList.sprites[cardID]; });
                seq.append(LeanTween.rotateY(images[imageID].gameObject, 0, AnimTime.revealCard).setEase(LeanTweenType.easeOutQuint));
            }
        }

        private void ShowBoard(Cards cards)
        {
            var seq = LeanTween.sequence();
            if(cards.list.Count >= 3)
            {
                if(board[0].sprite != deckList.sprites[cards.list[0].ID])
                {
                    seq.append(() => { RevealCard(board, cards.list[0].ID, 0); });
                    seq.append(AnimTime.revealCard);
                    seq.append(() => { RevealCard(board, cards.list[1].ID, 1); });
                    seq.append(AnimTime.revealCard);
                    seq.append(() => { RevealCard(board, cards.list[2].ID, 2); });
                    seq.append(AnimTime.revealCard);
                }
            }
            if(cards.list.Count >= 4)
            {
                if(board[3].sprite != deckList.sprites[cards.list[3].ID])
                {
                    seq.append(() => { RevealCard(board, cards.list[3].ID, 3); });
                    seq.append(AnimTime.revealCard);
                }
            }
            if(cards.list.Count >= 5) seq.append(() => { RevealCard(board, cards.list[4].ID, 4); });
        }

        private void HideBoard()
        {
            for (int i = 0; i < 5; i++)
            {
                board[i].sprite = deckList.cardBack;
            }
        }

        private void SetHeroHand(List<Poker.Card> cards, bool heroOn)
        {
            if(cards != null)
            {
                if(heroOn) hand[0].transform.parent.gameObject.SetActive(true);
                
                for (int i = 0; i < hand.Count; i++)
                {
                    //hand[i].sprite = deckList.sprites[cards[i].ID];
                    RevealCard(hand, cards[i].ID, i);
                }
            } 
        }

        private void SetButtons(int btnID, int sbID, int bbID)
        {
            for (int i = 0; i < seats.Count; i++)
            {
                if(i == sbID)
                {
                    seats[sbID].SetButton(blinds.smallBlind);
                    continue;
                }

                if (i == bbID)
                {
                    seats[bbID].SetButton(blinds.bigBlind); 
                    continue;
                }

                if (i == btnID)
                {
                    seats[btnID].SetButton(blinds.button);
                    continue;
                }

                seats[i].DisableButton();
            }
        }

        private void SetPlayerPresets()
        {
            for (int i = 0; i < tableSettings.playerPresets.Count; i++)
            {
                seats[i].ApplyPlayerPreset(tableSettings.playerPresets[i]);                
            }
        }

        private void SetChips(List<Player> players)
        {
            for (int i = 0; i < players.Count; i++)
            {
                seats[players[i].ID].SetChips(players[i].Chips);
            }
        }

        private void SetBets(List<Player> players)
        {
            foreach (var player in players)
            {
                SetBet(player);
            }
        }

        private void SetBet(Player player)
        {
            if(player.CurrentBet == 0)
            {
                seats[player.ID].bet.gameObject.SetActive(false);
                return;
            }

            seats[player.ID].SetBet(player.CurrentBet);
            seats[player.ID].SetChips(player.Chips);
        }

        private void SetPot(double pot)
        {
            foreach (var seat in seats)
            {
                seat.bet.gameObject.SetActive(false);
            }

            potTMP.text = pot.ToString("N0", CultureInfo.CreateSpecificCulture("de-DE"));
        }

        private void StartTurn(Player player)
        {
            seats[player.ID].ChangeColor(Color.green);
        }

        private void SetPlayerColor(Player player, Color color)
        {
            if (player.IsPlayingThisRound)
            {
                seats[player.ID].ChangePortraitColor(Color.white);
                seats[player.ID].ChangeColor(color);                
            }
            else
            {
                seats[player.ID].ChangePortraitColor(foldColor);                
                seats[player.ID].ChangeColor(foldColor);                
            }
        }

        private void SetPlayersColors(List<Player> players, Color color)
        {
            foreach (var player in players)
            {
                SetPlayerColor(player, color);
            }
        }

        private void RevealHand(Player player)
        {
            seats[player.ID].hand[0].transform.parent.gameObject.SetActive(true);
            RevealCard(seats[player.ID].hand, player.Hand.list[0].ID, 0); //player.Hand[i].ID
            RevealCard(seats[player.ID].hand, player.Hand.list[1].ID, 1); //player.Hand[i].ID         
        }

        private void RevealAllHands(List<Player> players, Table table)
        {
            foreach (var player in players)
            {
                if(player.IsPlayingThisRound)
                {
                    RevealHand(player);
                }
            }
        }

        private void HideAllHands()
        {
            hand[0].sprite = deckList.cardBack;
            hand[1].sprite = deckList.cardBack;
            hand[0].transform.parent.gameObject.SetActive(false);

            foreach (var seat in seats)
            {

                seat.hand[0].sprite = deckList.cardBack;
                seat.hand[1].sprite = deckList.cardBack;
                seat.hand[0].transform.parent.gameObject.SetActive(false);                
            }
        }

        private void RemovePlayers()
        {
            foreach (var seat in seats)
            {
                if (seat.chipCount == 0)
                {
                    seat.RemovePlayer();
                }
            }
        }

        private void SetWinners(List<Player> players)
        {
            foreach (var player in players)
            {
                if(player.AmountWon >= player.AmountInvested)
                {
                    seats[player.ID].ChangeColor(winColor);
                    seats[player.ID].SetActions($"+{player.AmountWon.ToString("N0", CultureInfo.CreateSpecificCulture("de-DE"))}");
                }
            }
        }

        private void SetLosers(List<Player> players)
        {
            foreach (var player in players)
            {
                SetPlayerColor(player, loseColor);
            }
        }

        private void SetPlayerAction(int ID, string currentAction)
        {
            seats[ID].SetActions(currentAction);
        }

        private void ClearPlayerActions(List<Player> players)
        {
            foreach (var player in players)
            {
                seats[player.ID].SetActions("");
            }
        }
    }
}
