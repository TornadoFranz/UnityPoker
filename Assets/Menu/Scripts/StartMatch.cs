using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Menu
{
    public class StartMatch : MonoBehaviour
    {
        [SerializeField] private GameObject Menu;
        [SerializeField] private TableSettings tableSettings;
        [SerializeField] private TMP_InputField bbTMP;
        [SerializeField] private TMP_InputField sbTMP;
        [SerializeField] private TMP_InputField anteTMP;
        [SerializeField] private PlayerList playerList;


        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        public void StartGame()
        {
            ApplyTableSetting();
            Menu.SetActive(false);

            LeanTween.delayedCall(gameObject, 0.25f, () => {
                ChangeScene(1);
            });
        }

        public void ApplyTableSetting()
        {
            tableSettings.BBAmount = uint.Parse(bbTMP.text);
            tableSettings.SBAmount = uint.Parse(sbTMP.text);
            tableSettings.Ante = uint.Parse(anteTMP.text);
            tableSettings.playerPresets = playerList.playerRowList.Select(row => row.RowToPlayerPreset()).ToList();
        }

        public void ChangeScene(int SceneToChangeTo)
        {
            SceneManager.LoadScene(SceneToChangeTo);
        }

        public void SetInteractable(List<PlayerRow> playerRowList)
        {
            if (playerRowList.Count < 2)
            {
                button.interactable = false;
                return;
            } 
            else
            {
                button.interactable = true;
                return;
            }            
        }
    }
}
