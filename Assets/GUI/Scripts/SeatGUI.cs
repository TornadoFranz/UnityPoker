using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Globalization;
using Menu;

namespace PokerGUI
{
    public class SeatGUI : MonoBehaviour
    {
        public Image portrait;
        public TextMeshProUGUI playerName;
        public Image button;
        public Image bet;
        public TextMeshProUGUI playerNameTMP;
        public TextMeshProUGUI chipsTMP;
        public TextMeshProUGUI playerActionTMP;
        public List<Image> hand;

        public Image playerIMG => GetComponent<Image>();

        public double chipCount {get; private set;}
        public bool seatTaken { get; private set; } = true;

        public void RemovePlayer()
        {
            gameObject.SetActive(false);
            seatTaken = false;
        }

        public void SetButton(Sprite spriteBtn)
        {
            button.sprite = spriteBtn;
            EnableButton();
        }

        public void EnableButton()
        {
            button.gameObject.SetActive(true);
        }
        public void DisableButton()
        {
            button.gameObject.SetActive(false);
        }

        public void SetChips(double chipCount)
        {
            this.chipCount = chipCount;
            chipsTMP.text = chipCount.ToString("N0", CultureInfo.CreateSpecificCulture("de-DE"));
        }

        public void SetBet(double betAmount)
        {
            bet.gameObject.SetActive(true);
            bet.GetComponentInChildren<TextMeshProUGUI>().text = betAmount.ToString("N0", CultureInfo.CreateSpecificCulture("de-DE"));
        }

        public void SetActions(string currentAction)
        {
            playerActionTMP.text = currentAction;
        }

        public void ChangeColor(Color color)
        {
            playerIMG.color = color;
        }

        public void ChangePortraitColor(Color color)
        {
            portrait.color = color;
        }

        public void ApplyPlayerPreset(PlayerPreset playerPreset)
        {
            portrait.sprite = playerPreset.Image;
            playerName.text = playerPreset.Name;
        }
    }
}
