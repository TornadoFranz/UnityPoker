using Poker;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Poker.Events;

namespace PokerGUI
{
    public class ActionsGUI : MonoBehaviour, IPokerEventListener
    {
        public PokerEvents pokerEvents => Main.current.table.pokerEvents;

        private bool IsEnabled = true;

        public void Awake()
        {
            if(Main.current.PlayAnimations) RegisterPokerEvents();
        }

        public void Start()
        {
            DisableUI();
        }

        public void RegisterPokerEvents()
        {
            pokerEvents.onHeroTurn += EnableUI;
            pokerEvents.onEndHeroTurn += DisableUI;
        }

        public void DeregisterPokerEvents()
        {
            pokerEvents.onHeroTurn -= EnableUI;
            pokerEvents.onEndHeroTurn -= DisableUI;
        }

        public void EnableUI()
        {
            if (!IsEnabled)
            {
                callBtn.SetActive(true);
                foldBtn.SetActive(true);
                raiseBtn.SetActive(true);
                slider.gameObject.SetActive(true);
                inputField.gameObject.SetActive(true);
                SetRaiseValues();
                SetCheckOrCall();
                IsEnabled = true;
            }
        }

        public void DisableUI()
        {
            if(IsEnabled)
            {
                callBtn.SetActive(false);
                foldBtn.SetActive(false);
                raiseBtn.SetActive(false);
                slider.gameObject.SetActive(false);
                inputField.gameObject.SetActive(false);
                IsEnabled = false;
            }
        }

        //CALL FUNCTIONS
        [SerializeField] private GameObject callBtn;
        public void Call()
        {
            Main.current.table.PlayerAction(PokerAction.Call, 0);
            DisableUI();
        }

        //FOLD FUNCTIONS
        [SerializeField] private GameObject foldBtn;
        public void Fold()
        {
            Main.current.table.PlayerAction(PokerAction.Fold, 0);
            DisableUI();
        }

        //RAISE FUNCTIONS

        [SerializeField] private GameObject raiseBtn;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Slider slider;

        double MinRaise
        {
            get
            {
                double minRaise = Main.current.table.MinRaise; 
                return minRaise > MaxRaise ? MaxRaise : minRaise;
            }
        }

        double MaxRaise
        {
            get
            {
                return Main.current.table.Hero.Chips + Main.current.table.Hero.CurrentBet;
            }
        }

        double RaiseValue
        {
            get
            {
                return (double)Mathf.Clamp(float.Parse(inputField.text), (float)MinRaise, (float)MaxRaise);
            }
        }

        public void SetSliderToInputField()
        {
            if(inputField.text.Length != 0)
            {
                slider.value = int.Parse(inputField.text);
            }
        }

        public void SetInputFieldToSlider()
        {            
            inputField.text = $"{(int)slider.value}";
        }

        public void ReplaceEmptyInputField()
        {
            if (inputField.text.Length == 0)
            {
                inputField.text = $"{0}";
            }
            else if(inputField.text.Length > 1)
            {
                string text = inputField.text;
                inputField.text = text.TrimStart('0');
            }
        }

        public void ClampInputField()
        {
            inputField.text = $"{RaiseValue}";                        
        }

        public void SetRaiseValues()
        {
            slider.maxValue = (float)MaxRaise;
            slider.minValue = MinRaise > MaxRaise ? (float)MaxRaise : (float)MinRaise;
            slider.value = slider.minValue;
            inputField.text = MinRaise.ToString("N0", CultureInfo.CreateSpecificCulture("de-DE"));
            SetInputFieldToSlider();
            SetSliderToInputField();
        }

        public void SetCheckOrCall()
        {
            //Debug.Log($"CurrentBet: {Main.current.table.Hero.CurrentBet}");
            //Debug.Log($"MinBet: {Main.current.table.MinBet}");

            if(Main.current.table.Hero.CurrentBet >= Main.current.table.MinBet)
            {
                callBtn.GetComponentInChildren<TextMeshProUGUI>().text = "CHECK";
            }
            else
            {
                callBtn.GetComponentInChildren<TextMeshProUGUI>().text = "CALL";
            }
        }

        public void Raise()
        {
            //Main.current.table.Hero.Raise(RaiseValue);
            Main.current.table.PlayerAction(PokerAction.Raise, RaiseValue);
            DisableUI();
        }
    }
}
