using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Menu
{
    public class AddPlayerRow : MonoBehaviour
    {
        [SerializeField] private Sprite gradient;
        [SerializeField] private Sprite gradientFilled;

        private Image image;
        private TextMeshProUGUI tmp;

        public void Awake()
        {
            image = GetComponent<Image>();
            tmp = GetComponentInChildren<TextMeshProUGUI>();
        }

        public void Highlight()
        {
            image.sprite = gradientFilled;
            image.color = new Color(0.9f, 0.9f, 0.9f);
            tmp.fontStyle = FontStyles.Bold;
        }

        public void DeHighlight()
        {
            image.sprite = gradient;
            image.color = new Color(1, 1, 1);
            tmp.fontStyle = FontStyles.Normal;
        }
    }
}
