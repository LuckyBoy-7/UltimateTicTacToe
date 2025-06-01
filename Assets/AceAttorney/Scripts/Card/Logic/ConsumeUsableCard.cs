using System;
using System.Collections;
using Lucky.Kits.Interactive;
using UnityEngine;

namespace AceAttorney.Scripts.Card.Logic
{
    public abstract class ConsumeUsableCard : UsableCard
    {
        protected override bool UseValid() => true;

        protected override void SelectStart()
        {
            UI.ShowSelectedHint();
        }

        protected override void SelectStay()
        {
            transform.position = Vector3.Lerp(transform.position, GameCursor.MouseWorldPosition, MoveToTargetLocalPositionLerpK);
        }

        protected override void CancelSelect()
        {
            UI.HideSelectedHint();
        }

        protected override void Use()
        {
            Discard();
        }
    }
}