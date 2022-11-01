using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PokerGUI
{
    [CreateAssetMenu(fileName = "Blinds", menuName = "ScriptableObjects/Blinds", order = 2)]
    public class Blinds : ScriptableObject
    {
        public Sprite button;
        public Sprite smallBlind;
        public Sprite bigBlind;        
    }
}
