using AceAttorney.Scripts.Card.Logic;
using UnityEngine;
using UnityEngine.Serialization;

namespace AceAttorney.Scripts.Card.Data
{

    public enum TestimonyPrerequisiteTypes
    {
        None,
        ConnectedWithRed,
        ConnectedWithBlue,
        ConnectedWithGray
    }

    [CreateAssetMenu(fileName = "TestimonyCardData", menuName = "CardData/TestimonyCardData")]
    public class TestimonyCardData : UsableCardData
    {
        public bool neutral;
        [FormerlySerializedAs("truePointType")] public CharacterTypes truePointFor;
        [FormerlySerializedAs("falsePointType")] public CharacterTypes falsePointFor;
        public int truePoints;
        public int falsePoints;
        public TestimonyPrerequisiteTypes prerequisiteType;
        [TextArea] public string description;
    }
}