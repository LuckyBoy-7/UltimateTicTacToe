using System;
using Lucky.Kits.Interactive;
using UnityEngine;

namespace AceAttorney.Scripts.Card.Logic
{
    public abstract class PlaceUsableCard : UsableCard
    {
        protected override void SelectStart()
        {
            PlaceHint.Instance.Show();
        }

        protected override void SelectStay()
        {
            transform.position = Vector3.Lerp(transform.position, GameCursor.MouseWorldPosition, MoveToTargetLocalPositionLerpK);
            PlaceHint.Instance.SetPosition(CardManager.Instance.SnapPosToGridCenter(transform.position));
            PlaceHint.Instance.SetColor(UseValid() ? ValidColor : InvalidColor);
        }

        protected override void CancelSelect()
        {
            PlaceHint.Instance.Hide();
        }
    }
}