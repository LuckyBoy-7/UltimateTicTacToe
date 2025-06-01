using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace AceAttorney.Scripts.Card.Data
{
    [CreateAssetMenu(fileName = "LogicCardData", menuName = "CardData/LogicCardData")]
    public class LogicCardData : UsableCardData
    {
        [TypeFilter("GetLogicTypes")] public Scripts.Logic logic;
        [TextArea] public string content;
        [TextArea] public string description;

        private static IEnumerable<Type> GetLogicTypes()
        {
            return typeof(Scripts.Logic).Assembly.GetTypes()
                .Where(x => !x.IsAbstract)
                .Where(x => typeof(Scripts.Logic).IsAssignableFrom(x))
                .Where(x => x != typeof(Scripts.Logic));
        }

        public static Scripts.Logic CreateLogicInstance(Scripts.Logic logic)
        {
            return (Scripts.Logic)Activator.CreateInstance(logic.GetType());
        }
    }
}