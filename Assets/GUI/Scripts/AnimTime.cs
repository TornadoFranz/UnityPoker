using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PokerGUI 
{    
    public static class AnimTime
    {
        private static bool _skipMode;

        public static bool SkipMode
        {           
            set
            {
                if (value == true)
                {
                    //revealCard = 0.05f;
                    //waitforAI = 0.005f;
                    //endGame = 0.05f;

                    revealCard = 0.05f;
                    waitforAI = 0;
                    endGame = 0.05f;

                }
                else if(value == false)
                {
                    revealCard = 0.5f;
                    waitforAI = 0.75f;
                    endGame = 2f;
                }

                _skipMode = value;
            }            

            get
            {
                return _skipMode;
            }
        }

        public static float revealCard { get; private set; }
        public static float waitforAI { get; private set; }
        public static float endGame { get; private set; }
    }
}
