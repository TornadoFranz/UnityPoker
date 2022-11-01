using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PokerGUI
{
    [CreateAssetMenu(fileName = "DeckList", menuName = "ScriptableObjects/Decklist", order = 1)]
    public class DeckList : ScriptableObject
    {
        public Sprite cardBack;
        public List<Sprite> sprites;
    }
}
