using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Poker;

namespace Menu
{
    [CreateAssetMenu(fileName = "PlayerPreset", menuName = "ScriptableObjects/PlayerPreset", order = 0)]
    public class PlayerPresetSO : ScriptableObject
    {
        public PlayerPreset playerPreset;
    }

    [System.Serializable]
    public class PlayerPreset
    {
        public PlayerPreset()
        {

        }

        public PlayerPreset(PlayerPreset playerPreset)
        {
            Name = playerPreset.Name;
            Image = playerPreset.Image;
            Tightness = playerPreset.Tightness;
            Aggression = playerPreset.Aggression;
            Chips = playerPreset.Chips;
            identity = playerPreset.identity;
        }

        public string Name;
        public Sprite Image;
        [Range(0, 1)] public float Tightness;
        [Range(0, 1)] public float Aggression;
        public double Chips = 500;
        public Identity identity = Identity.AI;
    }
}
