using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinOddsDB 
{
    //UNFINISHED

    public double GetWinOdds(ulong key)
    {
        return OneOppopnentWinOddsTable[key];
    }

    Dictionary<ulong, double> OneOppopnentWinOddsTable = new Dictionary<ulong, double>();

    public WinOddsDB()
    {
        OneOppopnentWinOddsTable.Add(1, 0.5);

    }
}
