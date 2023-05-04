using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPoker
{
    public interface IPlayer
    {
        Cards Hand { get; set; }
        bool IsPlayingThisGame { get; set; }
        bool IsPlayingThisRound { get; set; }
        uint StackSize { get; set; }
    }

    public class Player : IPlayer
    {
        public int Seat { get; set; } = 0;
        public Cards Hand { get; set; } = new Cards();
        public uint StackSize { get; set; } = 500;
        public bool IsPlayingThisRound { get; set; } = true;
        public bool IsPlayingThisGame { get; set; } = true;
    }
}
