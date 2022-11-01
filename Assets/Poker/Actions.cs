using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Poker
{
    public enum PokerAction {
        Fold,
        Call,
        Raise,
        Check,
        AllIn,
    }

    public static class PokerActions 
    {
        public static string ToString(PokerAction action)
        {
            switch (action)
            {
                case PokerAction.Fold:
                    return "FOLD";
                case PokerAction.Call:
                    return "CALL";
                case PokerAction.Raise:
                    return "RAISE";
                case PokerAction.Check:
                    return "CHECK";
                case PokerAction.AllIn:
                    return "ALLIN";
                default:
                    return "";
            }
        }
    }
}
