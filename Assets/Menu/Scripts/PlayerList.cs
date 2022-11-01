using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

namespace Menu
{
    public class PlayerList : MonoBehaviour
    {
        [SerializeField] private GameObject PlayerRow;
        [SerializeField] private RectTransform addNewRowRow;
        [SerializeField] private StartMatch startMatchBTN;
        [SerializeField] private List<PlayerPresetSO> PlayerPresetPool;

        public List<PlayerRow> playerRowList { get; private set; } = new List<PlayerRow>();

        private float startPosY = -86;

        public void AddNewPlayerRow()
        {
            if (playerRowList.Count > 9)
            {
                return;
            }

            if (playerRowList.Count > 8)
            {
                addNewRowRow.gameObject.SetActive(false);
            }


            PlayerRow row = Instantiate(PlayerRow, transform).GetComponent<PlayerRow>();
            playerRowList.Add(row);
            row.Initialize(this);
            row.ApplyPlayerPreset(GetRandomPresetFromPool());
            row.rt.anchoredPosition = new Vector2(row.rt.anchoredPosition.x, row.rt.anchoredPosition.y * playerRowList.Count);

            addNewRowRow.anchoredPosition = new Vector2(addNewRowRow.anchoredPosition.x, row.rt.anchoredPosition.y - row.rt.rect.height - 2);
            
            startMatchBTN.SetInteractable(playerRowList);
        }

        public void DeletePlayerRow(PlayerRow row)
        {
            playerRowList.Remove(row);
            Destroy(row.gameObject);
            ReorderPlayerList();
            startMatchBTN.SetInteractable(playerRowList);
        }

        public void ReorderPlayerList()
        {
            if (playerRowList.Count < 10)
            {
                addNewRowRow.gameObject.SetActive(true);
            }

            float posY = 0;
            for (int i = 0; i < playerRowList.Count; i++)
            {
                PlayerRow row = playerRowList[i];
                posY -= row.rt.rect.height + 2;
                row.rt.anchoredPosition = new Vector2(row.rt.anchoredPosition.x, posY);
            }

            addNewRowRow.anchoredPosition = new Vector2(addNewRowRow.anchoredPosition.x, posY + startPosY - 2);
        }

        public PlayerPreset GetRandomPresetFromPool()
        {
            HashSet<int> usedPresetIDs = new HashSet<int>(playerRowList.Select(row => row.presetID));
            HashSet<int> allPresetIDs = new HashSet<int>(PlayerPresetPool.Select(so => so.playerPreset.GetHashCode()));
            allPresetIDs.ExceptWith(usedPresetIDs);
            List<int> openPresetIds = allPresetIDs.ToList();
            int randomHashcode = openPresetIds[UnityEngine.Random.Range(0, openPresetIds.Count)].GetHashCode();
            PlayerPreset randomPreset = PlayerPresetPool.Where(so => so.playerPreset.GetHashCode() == randomHashcode).FirstOrDefault().playerPreset;
            return randomPreset;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                
                
            }
        }
    }
}
