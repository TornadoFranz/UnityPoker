using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoldemHand;
using Poker.Math;
using Newtonsoft.Json;
using System.IO;

namespace Poker.EditorScripts
{
    public static class WinOddsGenerator
    {
        public static void SaveAllPreflopWinningOdds()
        {
            int nopponents = 10;
            for (int i = 1; i < nopponents; i++)
            {
                Hashtable hashtable = new Hashtable();
                foreach (ulong handmask in Hand.Hands(0UL, 0UL, 2))
                {
                    hashtable.Add(handmask, PokerMath.WinOdds(handmask, 0UL, 0UL, i, 0.1));
                }
                string json = JsonConvert.SerializeObject(hashtable, Formatting.Indented);
                File.WriteAllText(Application.dataPath + $"/WinOdds/PreflopWinOdds_{i}.json", json);
                Debug.Log("DONE");
            }
        }

        public static void Test()
        {

        }
    }
}

