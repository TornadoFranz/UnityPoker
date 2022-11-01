using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Menu;


[CreateAssetMenu(fileName = "TableSettings", menuName = "ScriptableObjects/TableSettings", order = 0)]
public class TableSettings : ScriptableObject
{
    public List<PlayerPreset> playerPresets; 
    public uint SBAmount = 2;
    public uint BBAmount = 4;
    public uint Ante = 0;

    public uint playerCount => (uint)playerPresets.Count;
}
