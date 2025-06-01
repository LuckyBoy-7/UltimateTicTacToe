using UnityEngine;

namespace AceAttorney.Scripts.Card.Data
{
    [CreateAssetMenu(fileName = "PropCardData", menuName = "CardData/PropCardData")]
    public class PropCardData : UsableCardData
    {
        public Sprite propSprite;
        [TextArea] public string description;
    }
}