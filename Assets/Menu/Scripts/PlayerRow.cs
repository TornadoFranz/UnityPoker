using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Menu
{
    public class PlayerRow : MonoBehaviour
    {
        public int presetID { get; private set; }

        public RectTransform rt => _rt;        

        private RectTransform _rt;
        private PlayerList _playerList;
        public PlayerPreset playerPreset { get; private set; }

        [SerializeField] private Image Portrait;
        [SerializeField] private TMP_InputField Name;
        [SerializeField] private  TMP_Dropdown Identity;
        [SerializeField] private Slider Tightness;
        [SerializeField] private Slider Aggression;
        [SerializeField] private TextMeshProUGUI PlayType;
        [SerializeField] private TMP_InputField Chips;
        [SerializeField] private Image Delete;

        public void Initialize(PlayerList playerList)
        {
            _playerList = playerList;
            _rt = GetComponent<RectTransform>();
        }

        public void DeleteRow()
        {
            _playerList.DeletePlayerRow(this);
        }

        public void ApplyPlayerPreset(PlayerPreset pPreset)
        {
            playerPreset = pPreset;
            presetID = pPreset.GetHashCode();
            Portrait.sprite = pPreset.Image;
            Name.text = pPreset.Name;
            Identity.value = 1;
            Tightness.value = Mathf.Round(pPreset.Tightness * 10);
            Aggression.value = Mathf.Round(pPreset.Aggression * 10);
            Chips.text = pPreset.Chips.ToString();
        }

        public void HiglightDeleteBtn()
        {
            Delete.color = new Color32(160, 5, 15, 255);
        }

        public void DeHiglightDeleteBtn()
        {
            Delete.color = new Color32(230, 5, 15, 255);
        }

        public void ChangeIdentity()
        {
            switch (Identity.value)
            {
                case 0: //HUMAN
                    Aggression.interactable = false;
                    Tightness.interactable = false;
                    break;
                case 1: //DONKEY AI
                    Aggression.interactable = true;
                    Tightness.interactable = true;
                    break;
                default:
                    break;
            }
        }

        public void ChangePlaystyle()
        {
            PlayType.text = GetPlayStyle(Tightness.value, Aggression.value);
        }

        public string GetPlayStyle(float tightness, float aggression)
        {
            float sum = aggression + tightness;
            if (sum < 12 && sum > 8)
            {
                if(tightness == 5 || aggression == 5)
                {
                    return "Balanced";
                }
            }
            
            if(aggression <= 5 && tightness <= 5)
            {
                return "Loose-Passive";
            }
            else if (aggression >= 5 && tightness >= 5)
            {
                return "Tight-Aggressive";
            }
            else if (aggression >= 5 && tightness <= 5)
            {
                return "Loose-Aggressive";
            }
            else if(aggression <= 5 && tightness >= 5)
            {
                return "Tight-Passive";
            }

            return "";
        }

        public PlayerPreset RowToPlayerPreset()
        {
            PlayerPreset preset = new PlayerPreset();
            preset.Aggression = Aggression.value/10;
            preset.Tightness = Tightness.value/10;
            preset.identity = (Poker.Identity)Identity.value;
            preset.Name = Name.text;
            preset.Chips = double.Parse(Chips.text);
            preset.Image = Portrait.sprite;
            return preset;
        }
    }
}
