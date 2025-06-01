using AceAttorney.Scripts.Card.Data;
using AceAttorney.Scripts.Card.Logic;
using UnityEngine;

namespace AceAttorney.Scripts.Card.UI
{
    [CreateAssetMenu(fileName = "CharacterCardData", menuName = "CardData/CharacterCardData")]
    public class CharacterCardData : CardData
    {
        public string name;
        public Sprite characterSprite;
        public CharacterTypes characterType;
    }
}